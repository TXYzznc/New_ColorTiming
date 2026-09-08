import { build } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/postcss';
import { readdir, readFile, writeFile, mkdir, copyFile, realpath } from 'node:fs/promises';
import { resolve, relative, dirname } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { execFileSync } from 'node:child_process';

const web = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const output = resolve(web, 'deploy-edgeone');
const renderOutput = resolve(web, 'work/edgeone-render');
// Vite empties only these known generated directories, never the source/public tree.
for (const directory of [output, renderOutput]) {
  await mkdir(directory, { recursive: true });
  const actual = await realpath(directory);
  const base = await realpath(web);
  if (relative(base, actual).startsWith('..') || actual === base) {
    throw new Error(`Unsafe build destination: ${directory}`);
  }
}
const common = {
  configFile: false,
  root: resolve(web, 'static-site'),
  publicDir: false,
  plugins: [react()],
  css: { postcss: { plugins: [tailwindcss()] } },
};

await build({ ...common, build: { outDir: output, emptyOutDir: true } });
await build({
  ...common,
  build: {
    ssr: resolve(web, 'static-site/render.tsx'),
    outDir: renderOutput,
    emptyOutDir: true,
    rolldownOptions: { output: { entryFileNames: 'render.mjs' } },
  },
});
const { render } = await import(pathToFileURL(resolve(renderOutput, 'render.mjs')).href);
const htmlPath = resolve(output, 'index.html');
const html = (await readFile(htmlPath, 'utf8')).replace('<!--site-content-->', render());
await writeFile(htmlPath, html);

// Collect literal public URLs from the shared page/components and animation manifests.
// Dynamic sprite URLs are fully enumerated in lib/art-animations.json.
async function walk(directory) {
  const result = [];
  for (const item of await readdir(directory, { withFileTypes: true })) {
    const path = resolve(directory, item.name);
    if (item.isDirectory()) result.push(...await walk(path));
    else if (item.isFile()) result.push(path);
  }
  return result;
}
const assets = new Set();
for (const match of html.matchAll(/(?:src|href)="(\/(?:art|media|images)\/[^"?#]+)"/g)) assets.add(match[1]);
const sourceFiles = [...await walk(resolve(web, 'app')), ...await walk(resolve(web, 'lib'))];
for (const path of sourceFiles) {
  if (!/\.(tsx?|json|css)$/.test(path)) continue;
  const source = await readFile(path, 'utf8');
  for (const match of source.matchAll(/["'](\/(?:art|media|images)\/[^"'`{}]+)["']/g)) assets.add(match[1]);
}
// The icon names are composed from the weapon and colour selections in the UI.
for (const name of await readdir(resolve(web, 'public/art'))) {
  if (/^(scissors|hammer|bomb|axe|ringblade|plane)-(red|purple|green|orange)\.webp$/.test(name)) assets.add(`/art/${name}`);
}
for (const asset of assets) {
  const destination = resolve(output, `.${asset}`);
  await mkdir(dirname(destination), { recursive: true });
  await copyFile(resolve(web, `public${asset}`), destination);
}
const files = await walk(output);
if (files.length > 1000) throw new Error('Deployment contains more than 1000 files.');
let total = 0;
for (const path of files) {
  const size = (await readFile(path)).length;
  if (size > 25 * 1024 * 1024) throw new Error(`File exceeds 25 MiB: ${path}`);
  total += size;
}
const archive = resolve(web, 'outputs/colortiming-edgeone.zip');
await mkdir(dirname(archive), { recursive: true });
execFileSync('python', ['-c',
  'import pathlib,sys,zipfile; root=pathlib.Path(sys.argv[1]); archive=zipfile.ZipFile(sys.argv[2],"w",zipfile.ZIP_DEFLATED); [archive.write(p,p.relative_to(root).as_posix()) for p in sorted(root.rglob("*")) if p.is_file()]; archive.close()',
  output, archive,
], { stdio: 'inherit' });
console.log(`EdgeOne upload: ${files.length} files, ${(total / 1024 / 1024).toFixed(2)} MiB`);
console.log(`Folder: ${output}\nZIP: ${archive}`);
