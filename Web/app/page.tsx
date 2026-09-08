'use client';

/* oxlint-disable next/no-img-element -- Local WebP assets are pre-compressed; sprite strips must preserve their exact dimensions. */

import { type CSSProperties, useEffect, useRef, useState } from 'react';
import { CinematicOpening, ScrollMotion } from './cinematic-opening';
import { BossGameplay } from './boss-gameplay';
import animationData from '../lib/art-animations.json';
import { DOWNLOAD_URL, WISHLIST_URL } from '../lib/site-links';
import { preloadArt, warmArt } from '../lib/preload-art';

const chapters = [
  { id: 'world', label: '失物之地' },
  { id: 'rule', label: '颜色法则' },
  { id: 'boss', label: '首领战' },
  { id: 'artbook', label: '设定手记' },
];
const colours = [
  { id: 'red', label: '红', value: '#f06458' },
  { id: 'purple', label: '紫', value: '#a57aff' },
  { id: 'green', label: '绿', value: '#c7eb36' },
  { id: 'orange', label: '橙', value: '#f5a45b' },
] as const;
type Colour = (typeof colours)[number]['id'];
const weapons = [
  {
    id: 'scissors',
    name: '剪刀',
    en: 'SCISSORS',
    detail: '被遗忘的日常工具，化作小晴手中的利刃。',
    colours: ['red', 'purple', 'green'],
  },
  {
    id: 'hammer',
    name: '锤子',
    en: 'HAMMER',
    detail: '握住沉甸甸的锤柄，留意每一次出手的时机。',
    colours: ['red', 'purple', 'green'],
  },
  {
    id: 'bomb',
    name: '炸弹',
    en: 'BOMB',
    detail: '另一种攻击选择，也是一件带着鲜明颜色的失物。',
    colours: ['red', 'purple', 'green'],
  },
  {
    id: 'axe',
    name: '斧头',
    en: 'AXE',
    detail: '从地面拾起斧头，以对应的颜色迎接下一场战斗。',
    colours: ['red', 'purple', 'green', 'orange'],
  },
  {
    id: 'ringblade',
    name: '戒刀',
    en: 'RING BLADE',
    detail: '细长的刀身划破暗色世界，让颜色成为攻击的线索。',
    colours: ['red', 'purple', 'green', 'orange'],
  },
  {
    id: 'plane',
    name: '纸飞机',
    en: 'PAPER PLANE',
    detail: '一张折起的纸，也能成为失物之地里的武器。',
    colours: ['red', 'purple', 'green', 'orange'],
  },
] as const;
const gallery = [
  {
    src: '/art/hero-study.webp',
    title: '小晴 · 从草图到角色',
    category: '角色设定',
    page: '09–10',
  },
  {
    src: '/art/weaver-study.webp',
    title: '织主 · 线团与针脚',
    category: '第一章 / 首领设定',
    page: '27–28',
  },
  {
    src: '/art/bookworm-study.webp',
    title: '狂躁的书虫 · 纸页生物',
    category: '第二章 / 首领设定',
    page: '57–58',
  },
  {
    src: '/art/weapon-study.webp',
    title: '失物武器 · 形状与颜色',
    category: '武器设定',
    page: '41–42',
  },
  {
    src: '/art/paper-weapons.webp',
    title: '纸的另一种可能',
    category: '第二章 / 动作设定',
    page: '75–76',
  },
  {
    src: '/art/data-arena.webp',
    title: '数据之心',
    category: '第三章 / 场景概念',
    page: '79–80',
  },
];
type Clip = keyof typeof animationData;
const heroClips: Clip[] = ['hero-idle', 'hero-run', 'scissors-purple'];
const weaponClips = Object.keys(animationData).filter((key) => !key.startsWith('hero-')) as Clip[];

function Sprite({
  clip,
  label,
  paused,
  group,
}: {
  clip: Clip;
  label: string;
  paused: boolean;
  group: Clip[];
}) {
  const ref = useRef<HTMLDivElement>(null);
  const [visible, setVisible] = useState(false);
  const [loaded, setLoaded] = useState(false);
  const [displayed, setDisplayed] = useState<{ clip: Clip; label: string } | null>(null);
  const [failedClip, setFailedClip] = useState<Clip | null>(null);
  const error = failedClip === clip;
  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) setLoaded(true);
      },
      { rootMargin: '800px' },
    );
    const visibility = new IntersectionObserver(([entry]) => setVisible(entry.isIntersecting));
    if (ref.current) {
      observer.observe(ref.current);
      visibility.observe(ref.current);
    }
    return () => { observer.disconnect(); visibility.disconnect(); };
  }, []);
  useEffect(() => {
    if (!loaded) return;
    let cancelled = false;
    preloadArt(animationData[clip].src)
      .then(async (image) => {
        await image.decode();
        if (!cancelled) {
          setDisplayed({ clip, label });
          setFailedClip(null);
        }
      })
      .catch(() => { if (!cancelled) setFailedClip(clip); });
    return () => { cancelled = true; };
  }, [clip, label, loaded]);
  useEffect(() => {
    if (!loaded) return;
    return warmArt(group.flatMap((key) => [
      animationData[key].src,
      ...(!key.startsWith('hero-') ? [`/art/${key}.webp`] : []),
    ]));
  }, [loaded, group]);
  const data = animationData[displayed?.clip ?? clip];
  return (
    <div
      ref={ref}
      className="sprite"
      aria-busy={!error && displayed?.clip !== clip}
      style={
        {
          '--frames': data.frames,
          '--duration': `${data.duration}s`,
        } as CSSProperties
      }
    >
      {displayed && (
        <img
          key={displayed.clip}
          src={data.src}
          alt={displayed.label}
          width={data.frames * 256}
          height={256}
          className="sprite-strip"
          style={{
            animationPlayState: paused || !visible ? 'paused' : 'running',
          }}
        />
      )}
      {(error || displayed?.clip !== clip) && (
        <output className="sprite-status">
          {error ? '动作暂不可用，请切换后重试' : '动作准备中…'}
        </output>
      )}
    </div>
  );
}

function ExternalActions({ large = false }: { large?: boolean }) {
  return (
    <div className="cta-actions">
      <a
        className={`button button-light ${large ? 'large' : ''}`}
        href={WISHLIST_URL}
        target="_blank"
        rel="noopener noreferrer"
      >
        加入愿望单 <span aria-hidden="true">↗</span>
      </a>
      <a
        className={`button button-outline ${large ? 'large' : ''}`}
        href={DOWNLOAD_URL}
        target="_blank"
        rel="noopener noreferrer"
      >
        下载试玩 Demo <span aria-hidden="true">↗</span>
      </a>
    </div>
  );
}

export default function Home() {
  const [active, setActive] = useState('');
  const [weaponIndex, setWeaponIndex] = useState(0);
  const [colour, setColour] = useState<Colour>('purple');
  const [heroClip, setHeroClip] = useState<Clip>('hero-idle');
  const [heroPaused, setHeroPaused] = useState(false);
  const [weaponPaused, setWeaponPaused] = useState(false);
  const [selectedArt, setSelectedArt] = useState<number | null>(null);
  const dialog = useRef<HTMLDialogElement>(null);
  const selectedWeapon = weapons[weaponIndex];
  const selectedColour = colours.find((item) => item.id === colour)!;
  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) setActive(entry.target.id);
        });
      },
      { rootMargin: '-15% 0px -60% 0px' },
    );
    document
      .querySelectorAll('[data-chapter]')
      .forEach((section) => observer.observe(section));
    return () => observer.disconnect();
  }, []);
  useEffect(() => {
    if (selectedArt === null) return;
    const element = dialog.current;
    const oldOverflow = document.body.style.overflow;
    element?.showModal();
    document.body.style.overflow = 'hidden';
    return () => {
      element?.close();
      document.body.style.overflow = oldOverflow;
    };
  }, [selectedArt]);
  function chooseWeapon(index: number) {
    setWeaponIndex(index);
    if (!(weapons[index].colours as readonly string[]).includes(colour))
      setColour('purple');
  }

  return (
    <main className="site-shell">
      <ScrollMotion />
      <a className="skip-link" href="#world">
        跳到正文
      </a>
      <header className="topbar">
        <a className="brand" href="#top" aria-label="失物语首页">
          <img
            src="/art/title.webp"
            width="180"
            height="110"
            alt="The Lost Tales"
          />
        </a>
        <nav className="nav-tabs" aria-label="页面导航">
          {chapters.map((chapter) => (
            <a
              key={chapter.id}
              className={active === chapter.id ? 'active' : ''}
              href={`#${chapter.id}`}
              aria-current={active === chapter.id ? 'location' : undefined}
            >
              {chapter.label}
            </a>
          ))}
        </nav>
        <a
          className="wish-link"
          href={WISHLIST_URL}
          target="_blank"
          rel="noopener noreferrer"
        >
          加入愿望单 ↗
        </a>
      </header>

      <section id="top" className="hero panel-dark" data-chapter>
        <CinematicOpening />
        <div className="hero-copy">
          <p className="eyebrow">THE LOST TALES / COLOR TIMING</p>
          <h1>失物语</h1>
          <p className="hero-line">
            被遗忘的东西，
            <br />
            <em>还记得你。</em>
          </p>
          <p className="hero-desc">
            醒来时，你变成了自己丢弃的娃娃。
            <br />
            拾起失物，以颜色为线索，找回属于你的故事。
          </p>
          <ExternalActions />
          <a className="read-link" href="#world">
            向下翻开失物之地 <span aria-hidden="true">↓</span>
          </a>
        </div>
        <span className="hero-edition">一场关于颜色与记忆的冒险</span>
      </section>

      <section id="world" className="chapter panel-paper" data-chapter>
        <div className="section-heading">
          <p className="eyebrow ink">01 / THE LOST WORLD</p>
          <span>失物之地</span>
        </div>
        <div className="chapter-grid">
          <div className="chapter-copy">
            <h2>
              遗忘之后，
              <br />
              <span>故事才刚开始。</span>
            </h2>
            <p>
              小晴原本是一个人类。一觉醒来，他却变成了自己丢弃的破旧娃娃，困在了失物之地。
            </p>
            <p>
              为了找回自己的身体，他走进一个个被失物统治的地盘。那些旧物承载的梦想、情感，以及它们与“我”的往事，正等待被重新发现。
            </p>
          </div>
          <figure className="world-illustration">
            <img
              src="/art/lost-world.webp"
              alt="小晴坠入被彩色光点与巨大失物环绕的世界"
              width="1444"
              height="794"
              loading="lazy"
            />
            <figcaption>失物之地 / 世界观概念图</figcaption>
          </figure>
        </div>
        <div className="character-feature">
          <div className="character-copy">
            <p className="eyebrow">MEET XIAOQING</p>
            <h3>小晴</h3>
            <p>
              一只破旧娃娃，
              <br />
              一段找回自己的旅程。
            </p>
            <span className="small-note">看看他如何在失物之地行动。</span>
          </div>
          <div className="character-animation">
            <Sprite
              clip={heroClip}
              label="小晴的游戏动作演示"
              paused={heroPaused}
              group={heroClips}
            />
            <span className="animation-caption">
              游戏原始动作 /{' '}
              {heroClip === 'hero-idle'
                ? '待机'
                : heroClip === 'hero-run'
                  ? '奔跑'
                  : '剪刀挥击'}
            </span>
          </div>
          <div className="character-controls">
            <fieldset className="control-stack" aria-label="选择主角动作">
              {(
                [
                  { id: 'hero-idle', label: '01 / 待机' },
                  { id: 'hero-run', label: '02 / 奔跑' },
                  { id: 'scissors-purple', label: '03 / 挥击' },
                ] as const
              ).map((action) => (
                <button
                  key={action.id}
                  onPointerEnter={() => { void preloadArt(animationData[action.id].src).catch(() => undefined); }}
                  onFocus={() => { void preloadArt(animationData[action.id].src).catch(() => undefined); }}
                  onClick={() => setHeroClip(action.id)}
                  aria-pressed={heroClip === action.id}
                >
                  {action.label}
                  <span aria-hidden="true">↗</span>
                </button>
              ))}
            </fieldset>
            <button
              className="motion-toggle"
              onClick={() => setHeroPaused((previous) => !previous)}
              aria-pressed={heroPaused}
            >
              {heroPaused ? '播放动作' : '暂停动作'}
            </button>
          </div>
        </div>
        <div className="comic-heading">
          <p className="eyebrow ink">A PAGE FROM THE STORY</p>
          <p>醒来。相遇。拾起颜色。</p>
        </div>
        <div className="comic-strip">
          {[
            {
              src: 'comic-awakening',
              text: '01 / 一场意外的醒来',
              alt: '小晴从高处坠下，惊讶地发现自己变成娃娃',
            },
            {
              src: 'comic-weaver',
              text: '02 / 藏在线团里的身影',
              alt: '织主以巨大的针刃出现在小晴面前',
            },
            {
              src: 'comic-colour',
              text: '03 / 握紧手中的颜色',
              alt: '小晴手持紫色剪刀面对织主',
            },
          ].map((panel) => (
            <figure key={panel.src}>
              <img
                src={`/art/${panel.src}.webp`}
                alt={panel.alt}
                width="750"
                height="410"
                loading="lazy"
              />
              <figcaption>{panel.text}</figcaption>
            </figure>
          ))}
        </div>
        <p className="source-note">过场漫画节选 · 《失物语》美术设定集</p>
      </section>

      <section id="rule" className="rule-section panel-purple" data-chapter>
        <div className="section-heading">
          <p className="eyebrow">02 / COLOR IS THE CLUE</p>
          <span>颜色法则</span>
        </div>
        <div className="rule-heading">
          <h2>
            看清颜色。
            <br />
            <em>拾起武器。</em>
          </h2>
          <div>
            <p>
              怪物身上的颜色，就是弱点的线索。
              <br />
              使用对应颜色的武器，击破相同颜色的部分。
            </p>
            <p className="muted">
              失物有不同的形状，也有不同的攻击方式。
              <br />
              先选一件武器，看看它的模样。
            </p>
          </div>
        </div>
        <fieldset className="weapon-tabs" aria-label="选择武器种类">
          {weapons.map((weapon, index) => (
            <button
              key={weapon.id}
              onClick={() => chooseWeapon(index)}
              aria-pressed={weaponIndex === index}
            >
              <img
                src={`/art/${weapon.id}-purple.webp`}
                alt=""
                width="112"
                height="100"
                loading="lazy"
              />
              <span>{weapon.name}</span>
              <small>{weapon.en}</small>
            </button>
          ))}
        </fieldset>
        <div
          className="weapon-detail"
          style={{ '--weapon-colour': selectedColour.value } as CSSProperties}
        >
          <div className="weapon-info">
            <p className="eyebrow">
              0{weaponIndex + 1} / {selectedWeapon.en}
            </p>
            <h3>{selectedWeapon.name}</h3>
            <p>{selectedWeapon.detail}</p>
            <fieldset className="colour-options" aria-label="查看武器颜色">
              {colours
                .filter((item) =>
                  (selectedWeapon.colours as readonly string[]).includes(
                    item.id,
                  ),
                )
                .map((item) => (
                  <button
                    key={item.id}
                    style={{ '--swatch': item.value } as CSSProperties}
                    onClick={() => setColour(item.id)}
                    aria-pressed={colour === item.id}
                  >
                    <span />
                    {item.label}
                  </button>
                ))}
            </fieldset>
            <p className="weapon-status" aria-live="polite">
              正在查看：{selectedColour.label}色{selectedWeapon.name}
            </p>
          </div>
          <div className="weapon-object">
            <img
              key={`${selectedWeapon.id}-${colour}`}
              src={`/art/${selectedWeapon.id}-${colour}.webp`}
              alt={`${selectedColour.label}色${selectedWeapon.name}游戏图标`}
              width="180"
              height="180"
              loading="lazy"
            />
            <span>武器图鉴</span>
          </div>
          <div className="weapon-motion">
            <Sprite
              clip={`${selectedWeapon.id}-${colour}` as Clip}
              label={`${selectedColour.label}色${selectedWeapon.name}动作样例`}
              paused={weaponPaused}
              group={weaponClips}
            />
            <span className="animation-caption">
              动作样例 / {selectedColour.label}色 ·{' '}
              {selectedWeapon.id === 'scissors' ? '挥击' : '待机'}
            </span>
            <button
              className="motion-toggle"
              onClick={() => setWeaponPaused((previous) => !previous)}
              aria-pressed={weaponPaused}
            >
              {weaponPaused ? '播放动作' : '暂停动作'}
            </button>
          </div>
        </div>
        <div className="rule-steps">
          <p>
            <b>01</b>
            <span>
              观察弱点<small>记住怪物身上的颜色。</small>
            </span>
          </p>
          <p>
            <b>02</b>
            <span>
              拾取武器<small>移动到对应颜色的武器旁。</small>
            </span>
          </p>
          <p>
            <b>03</b>
            <span>
              把握时机<small>躲开攻击，再寻找出手机会。</small>
            </span>
          </p>
        </div>
      </section>

      <section id="boss" className="boss-section panel-ink" data-chapter>
        <div className="section-heading">
          <p className="eyebrow">03 / THE THINGS THAT REMAIN</p>
          <span>首领相遇</span>
        </div>
        <div className="boss-title">
          <h2>
            它们也曾，
            <br />
            <span>被人珍惜。</span>
          </h2>
          <p>
            针线、纸页与旧日记忆，
            <br />
            在失物之地长出了新的模样。
          </p>
        </div>
        <article className="boss-entry">
          <figure>
            <img
              src="/art/weaver-arena.webp"
              alt="织主盘踞在巨大的线团与针线织成的战场中"
              width="1384"
              height="794"
              loading="lazy"
            />
            <figcaption>第一章 / 织主的缝补场 · 场景设定</figcaption>
          </figure>
          <div className="boss-entry-copy">
            <span className="boss-index">I</span>
            <p className="eyebrow">THE WEAVER</p>
            <h3>织主</h3>
            <p>
              缝补场的领主与编织者。她背负着巨大的毛线团，永不停歇地缝补旧物。
            </p>
            <button className="text-link" onClick={() => setSelectedArt(1)}>
              翻开角色设定 ↗
            </button>
          </div>
        </article>
        <article className="boss-entry reverse">
          <figure>
            <img
              src="/art/bookworm-arena.webp"
              alt="狂躁的书虫在废纸堆叠成的荒漠中出现"
              width="1390"
              height="794"
              loading="lazy"
            />
            <figcaption>第二章 / 废纸荒漠 · 场景设定</figcaption>
          </figure>
          <div className="boss-entry-copy">
            <span className="boss-index">II</span>
            <p className="eyebrow">THE BOOKWORM</p>
            <h3>狂躁的书虫</h3>
            <p>
              吞下的纸张与文具，逐渐成为它身体的一部分。它在废纸荒漠里不断进食，等待下一位来客。
            </p>
            <button className="text-link" onClick={() => setSelectedArt(2)}>
              翻开角色设定 ↗
            </button>
          </div>
        </article>
        <div className="gameplay-frame">
          <div className="section-heading">
            <p className="eyebrow">INSIDE THE GAME</p>
            <span>狂躁的书虫 · 28 秒实机战斗</span>
          </div>
          <BossGameplay />
          <div className="boss-footer">
            <p>
              移动、观察、出手。
              <br />
              进入游戏，亲自找到下一次攻击的时机。
            </p>
            <a
              className="button button-outline"
              href={DOWNLOAD_URL}
              target="_blank"
              rel="noopener noreferrer"
            >
              前往下载试玩 ↗
            </a>
          </div>
        </div>
      </section>

      <section
        id="artbook"
        className="artbook-section panel-paper"
        data-chapter
      >
        <div className="section-heading">
          <p className="eyebrow ink">04 / FROM THE SKETCHBOOK</p>
          <span>设定手记</span>
        </div>
        <div className="artbook-heading">
          <h2>
            每件失物，
            <br />
            <span>都有来处。</span>
          </h2>
          <p>
            翻看角色、武器与场景的最初模样。
            <br />
            以下为美术设定稿，不代表试玩版全部可玩内容。
          </p>
        </div>
        <div className="art-grid">
          {gallery.map((art, index) => (
            <button
              key={art.src}
              className="art-card"
              onClick={() => setSelectedArt(index)}
              aria-label={`放大查看：${art.title}`}
            >
              <div className="art-thumbnail">
                <img
                  src={art.src}
                  alt={art.title}
                  width="1600"
                  height="730"
                  loading="lazy"
                />
                <span aria-hidden="true">↗</span>
              </div>
              <span className="art-category">{art.category}</span>
              <strong>{art.title}</strong>
            </button>
          ))}
        </div>
        <p className="source-note">
          选自《失物语》美术设定集 · 点击图片查看大图
        </p>
      </section>

      <section id="cta" className="cta-section panel-red" data-chapter>
        <div className="cta-inner">
          <p className="eyebrow">YOUR STORY STARTS HERE</p>
          <h2>
            找回失物，
            <br />
            <em>也找回自己。</em>
          </h2>
          <p>带上你的颜色，走进失物之地。</p>
          <ExternalActions large />
          <p className="cta-note">PC Demo · 百度网盘下载 · 提取码：2npx</p>
        </div>
      </section>
      <footer className="footer">
        <a className="brand" href="#top" aria-label="返回顶部">
          <img
            src="/art/title.webp"
            width="180"
            height="110"
            alt="The Lost Tales"
          />
        </a>
        <p>
          失物语 / THE LOST TALES
          <br />
          <span>一个关于颜色、选择与失物的故事。</span>
        </p>
        <span>© 2026 COLOR TIMING</span>
      </footer>
      <dialog
        ref={dialog}
        className="art-dialog"
        aria-labelledby="art-dialog-title"
        onClose={() => setSelectedArt(null)}
      >
        {selectedArt !== null && (
          <div className="art-dialog-content">
            <div className="art-dialog-header">
              <div>
                <p>{gallery[selectedArt].category}</p>
                <h2 id="art-dialog-title">{gallery[selectedArt].title}</h2>
              </div>
              <button
                autoFocus
                className="close-dialog"
                onClick={() => setSelectedArt(null)}
                aria-label="关闭大图"
              >
                关闭 ×
              </button>
            </div>
            <img
              src={gallery[selectedArt].src}
              alt={gallery[selectedArt].title}
            />
            <p className="dialog-caption">
              《失物语》美术设定集 · 书内页码 {gallery[selectedArt].page} ·
              设定画面
            </p>
          </div>
        )}
      </dialog>
    </main>
  );
}
