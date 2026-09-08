"""Export project-owned art for the website without changing Unity source assets.

Run with Python + Pillow. PDF pages are rendered with Poppler, not re-authored.
All source references in the manifest are relative to the repository root.
"""
from pathlib import Path
import json
import shutil
import subprocess
from PIL import Image

WEB = Path(__file__).resolve().parents[1]
ROOT = WEB.parent
SOURCE = ROOT / 'Assets/Game/Sprites/ColorTiming'
OUT = WEB / 'public/art'
WORK = WEB / 'work/art-review'
OUT.mkdir(parents=True, exist_ok=True)
WORK.mkdir(parents=True, exist_ok=True)
records = []


def save_image(image, name, source, max_size=(1600, 1600), **extra):
    image.thumbnail(max_size, Image.Resampling.LANCZOS)
    path = OUT / (name + '.webp')
    image.save(path, 'WEBP', quality=85, method=6)
    records.append(dict(file='art/' + path.name, source=source,
                        width=image.width, height=image.height, bytes=path.stat().st_size, **extra))


def project_image(relative, name, trim=False, **kwargs):
    path = SOURCE / relative
    image = Image.open(path).convert('RGBA')
    if trim:
        bounds = image.getchannel('A').getbbox()
        if bounds:
            image = image.crop(bounds)
    save_image(image, name, path.relative_to(ROOT).as_posix(), **kwargs)


project_image('UI/MainMenu/标题.png', 'title', trim=True, max_size=(900, 500))
project_image('UI/MainMenu/封面.png', 'cover', max_size=(1800, 1100))

# Keep every available colour variant. Not all weapons have orange artwork.
weapon_names = {'剪刀': 'scissors', '戒刀': 'ringblade', '斧头': 'axe',
                '炸弹': 'bomb', '锤子': 'hammer', '飞机': 'plane'}
colours = {'01': 'red', '02': 'purple', '03': 'green', '04': 'orange'}
for path in sorted((SOURCE / 'Weapons/Icons').glob('*_默认.png')):
    _, weapon, colour, _ = path.stem.split('_')
    project_image(path.relative_to(SOURCE), f'{weapon_names[weapon]}-{colours[colour]}', trim=True)

# Retain frame order and timing; crop the union of all frames so the origin stays fixed.
clips = {'hero-idle': 'Idle', 'hero-run': 'Move/Front',
         'scissors': 'Scissors/Attack', 'axe': 'Axe/Idle', 'hammer': 'Hammer/Idle',
         'bomb': 'Bomb/Idle', 'plane': 'Plane/Idle', 'ringblade': 'RingBlade/Idle'}
animation_data = {}
for name, folder in clips.items():
    all_paths = sorted((SOURCE / 'Hero/Sequences' / folder).glob('*.png'))
    prefixes = sorted({p.stem.rsplit('_', 1)[0] for p in all_paths})
    if name.startswith('hero-'):
        prefixes = [all_paths[0].stem.rsplit('_', 1)[0]]
    for prefix in prefixes:
        colour = next((en for cn,en in [('红色','red'),('紫色','purple'),('绿色','green'),('橙色','orange')] if cn in prefix), None)
        clip_name = name if name.startswith('hero-') else f'{name}-{colour}'
        paths = [p for p in all_paths if p.stem.rsplit('_', 1)[0] == prefix]
        stride = 2 if len(paths) > 24 else 1
        duration = round(len(paths) / 30, 3)
        paths = paths[::stride]
        images = [Image.open(p).convert('RGBA') for p in paths]
        boxes = [im.getchannel('A').getbbox() for im in images]
        boxes = [b for b in boxes if b]
        box = (min(b[0] for b in boxes)-12, min(b[1] for b in boxes)-12,
               max(b[2] for b in boxes)+12, max(b[3] for b in boxes)+12)
        frame_size = 256
        strip = Image.new('RGBA', (frame_size * len(images), frame_size))
        for index, image in enumerate(images):
            image = image.crop(box)
            image.thumbnail((frame_size-16, frame_size-16), Image.Resampling.LANCZOS)
            strip.alpha_composite(image, (index*frame_size+(frame_size-image.width)//2,
                                          (frame_size-image.height)//2))
        save_image(strip, clip_name + '-strip', (SOURCE / 'Hero/Sequences' / folder).relative_to(ROOT).as_posix(),
                   max_size=strip.size, frames=len(images), duration=duration, sourcePrefix=prefix)
        animation_data[clip_name] = dict(src='/art/' + clip_name + '-strip.webp', frames=len(images), duration=duration)

def book_crop(page, name, rect, max_size=(1600, 1200)):
    """Coordinates are fractions of the verified page render."""
    path = WORK / f'detail-{page}.png'
    if not path.exists():
        poppler = shutil.which('pdftoppm')
        if not poppler:
            raise RuntimeError('pdftoppm must be available to render the art book')
        subprocess.run([poppler, '-f', str(page), '-l', str(page), '-scale-to', '1800',
                        '-png', '-singlefile', str(ROOT/'Arts/失物语设定集.pdf'), str(path.with_suffix(''))], check=True)
    image = Image.open(path).convert('RGB')
    box = tuple(round(v * (image.width if i % 2 == 0 else image.height)) for i, v in enumerate(rect))
    save_image(image.crop(box), name, 'Arts/失物语设定集.pdf', max_size=max_size,
               pdfPage=page, crop=list(rect), kind='concept-art')


book_crop(5, 'lost-world', (.198, 0, 1, .97))
book_crop(13, 'weaver-arena', (.201, .01, .970, .98))
book_crop(27, 'bookworm-arena', (.20, .01, .972, .98))
book_crop(44, 'data-arena', (.199, .01, .97, .98))
book_crop(18, 'weaver-study', (0, 0, 1, 1))
book_crop(33, 'bookworm-study', (0, 0, 1, 1))
book_crop(9, 'hero-study', (0, 0, 1, 1))
book_crop(25, 'weapon-study', (0, 0, 1, 1))
book_crop(42, 'paper-weapons', (0, 0, 1, 1))
book_crop(55, 'comic-awakening', (.052, .008, .46, .493), max_size=(1000, 650))
book_crop(55, 'comic-weaver', (.555, .008, .968, .50), max_size=(1000, 650))
book_crop(56, 'comic-colour', (.574, .022, .94, .477), max_size=(1000, 650))

# Existing site images are left in place; the page uses these compressed derivatives.
for name in ['boss2-scene', 'boss2-fight']:
    source = WEB / 'public/images' / (name + '.jpg')
    save_image(Image.open(source), name, source.relative_to(ROOT).as_posix())

(WEB/'lib/art-animations.json').write_text(json.dumps(animation_data, indent=2), encoding='utf-8')
(WEB/'art-assets.json').write_text(json.dumps(records, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
print(f'Exported {len(records)} assets, {sum(r["bytes"] for r in records)/1024/1024:.2f} MiB total')
print(json.dumps(animation_data, indent=2))
