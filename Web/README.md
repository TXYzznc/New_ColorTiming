# 失物语官网

保留深紫、纸白、荧光色的视觉方向，以项目原有美术补充世界观、主角动作、武器图鉴、首领介绍与设定手记。

## 部署到腾讯云 EdgeOne Pages（直接上传）

请上传 **`outputs/colortiming-edgeone.zip`**，或者选择 **`deploy-edgeone` 文件夹**。不要上传整个 `Web` 开发目录，也不要上传 vinext 的 `dist` 目录。

本次导出包含 64 个文件，展开约 11.62 MiB，ZIP 约 11.27 MiB；`index.html` 位于 ZIP 根目录。包含全部在用图片、序列帧、首页两段视频和修正方向后的 Boss2 视频，不含依赖、Unity 工程、录制原片、调试日志、源码和服务端程序。所有交互复用现有页面组件，HTML 预渲染后由浏览器接管，不需要 Node.js 或 Workers 服务端。

上传步骤：在 EdgeOne Pages 选择“直接上传”，移除先前选中的 `Web`，上传 ZIP，点击“开始部署”。直接上传不需要填构建命令。部署完成后使用平台给出的访问地址。

官方上传要求：https://pages.edgeone.ai/zh/document/direct-upload 。压缩包根目录必须有 `index.html`，不能再套一层 `deploy-edgeone/`。当前文档的文件数量上限可能与账户界面不同，此包同时满足截图中更严格的 1000 文件上限；单个文件均小于 25 MiB。

以后改完网站，在 `Web` 目录重新执行：

```powershell
npm run build:edgeone
```

需本机已有 Node.js、项目依赖和 Python（用于生成 ZIP）。该命令更新 `deploy-edgeone` 和 `outputs/colortiming-edgeone.zip`，不改变现有 `npm run dev` / `npm run build`。请重新上传新 ZIP，不要手工改部署包。`node_modules` 是开发依赖，`work` 是录制及调试中间文件，`dist` 是原服务端构建产物，它们均不需要上传，源目录保留用于维护。

验证包括：静态 HTML 内容、ZIP 入口层级、文件数和单文件体积、所有 HTML/序列帧资源引用、最新下载链接与提取码、静态服务器下的武器配色切换、视频加载、浏览器错误检查。

## 本地预览

在本目录执行：

```powershell
npm ci
npm run dev -- --host 127.0.0.1
```

以终端输出的本地地址为准。构建：`npm run build`。

## 入口链接

统一配置在 `lib/site-links.ts`：

- 愿望单按钮按本次明确要求指向 `https://chatgpt.com/`，不是 Steam 商店地址。拿到正式 Steam 地址后只需修改这一处。
- 下载入口使用用户提供的百度网盘地址，保留 `pwd=2npx`，页面同时显示提取码。
- 所有外链在新标签页打开，并带 `noopener noreferrer`。

## 素材来源与用途

`art-assets.json` 逐项记录仓库相对来源、PDF 物理页码、裁切区域、图片尺寸与导出大小。

| 来源 | 用途 |
| --- | --- |
| `Assets/Game/Sprites/ColorTiming/UI/MainMenu` | 原版 Logo、官网主视觉 |
| `Assets/Game/Sprites/ColorTiming/Weapons/Icons` | 剪刀、锤子、炸弹、斧头、戒刀、纸飞机，共 21 个配色图标 |
| `Assets/Game/Sprites/ColorTiming/Hero/Sequences` | 待机、奔跑，以及六类武器的 21 组配色动作，共 23 组序列帧 |
| `Arts/失物语设定集.pdf` 第 5、9 页 | 世界观、小晴的来历与角色设计 |
| PDF 第 13、18、27、33 页 | 织主、狂躁的书虫及场景设定 |
| PDF 第 25、42、44 页 | 武器设计、动作设定、第三章数据之心概念图 |
| PDF 第 55、56 页 | 三段过场漫画选图 |
| 原网站 `public/images` | 战斗画面的压缩副本 |

本轮共导出 60 份 WebP，约 3.02 MiB；不是首屏一次性下载的体积。原始 PDF、Unity 素材与原网站图片保持不变。

图标配色与动作配色同步；没有橙色素材的武器不显示橙色选项。从橙色武器切回这类武器时自动选择紫色。动画进入视口才载入，离开视口暂停，支持手动暂停和系统减少动态效果设置。图鉴只演示素材，不模拟正式战斗。

Boss 原件位于 `Boss1/Core` 和 `Boss2/Core`，为 Spine 3.8 骨骼与图集。本轮官网使用原设定图展示 Boss，没有加入 Spine 浏览器运行时或把骨骼图集当作完整角色图片。

设定画廊可以点击放大，以关闭按钮或 Escape 关闭。第三章及草稿明确标为美术设定，不宣称是本次 Demo 的可玩内容。

## 重新导出素材

需要 Python、Pillow，以及可调用的 Poppler `pdftoppm`：

```powershell
python scripts/prepare-art-assets.py
```

脚本以仓库根目录定位输入，只写入 `public/art`、`lib/art-animations.json`、`art-assets.json` 和已忽略的 `work/art-review`。它不改动 Unity 项目源素材。保留整个动作序列统一裁切范围，避免逐帧裁切导致角色抖动；生成精灵条后由 CSS 播放。

## 验证

本次检查包括桌面与手机显示、21 种武器配色与动作对应关系、主角动作切换、图鉴放大与 Escape 关闭、外链配置、构建与 TypeScript 检查。

新增页面与链接文件通过定向 lint。仓库完整 lint 仍包含原有 `components/ui`、`hooks/use-mobile.ts` 的问题；本次未修改这些通用组件。

当前保留本地预览与项目原有构建方式，未发布或更改域名配置。国内外公网访问质量需要在正式托管环境单独验证。

## 片头与首页视频

片头幕布随静态 HTML 预渲染输出，动画在客户端接管前暂停，避免脚本下载较慢时先露出正文。客户端接管后才启动约 3.1 秒的片头计时；禁用 JavaScript 时通过 noscript 隐藏幕布，正文仍可访问。已用延迟脚本加载、章节锚点、减少动态效果及禁用脚本场景验证。

进入无锚点的首页时，播放约 3.1 秒的片名动画：细线揭字、停留、上下分幕，然后播放原项目开场视频，结束后衔接循环画面。带章节锚点的链接直接进入内容。片头自动播放且不显示跳过按钮，视频可暂停、重播、切换声音；默认静音，离开首屏或切到后台时暂停。系统开启减少动态效果时跳过片名且视频默认暂停。自动播放被拦截时显示播放按钮，加载失败保留画面和正文入口。

- `public/media/opening.mp4`：源自 `Assets/Game/Video/ColorTiming/1开头.mp4`，4.8 秒。
- `public/media/idle.mp4`：源自 `Assets/Game/Video/ColorTiming/2循环.mp4`，11.2 秒。
- 网页副本采用 H.264 / AAC、1600×900、30 fps、CRF 25、AAC 96k、faststart，共 797,897 字节；源视频未修改。
- 动画逻辑位于 `app/cinematic-opening.tsx`，动效样式位于 `app/motion.css`。正文包括视口入场、漫画与画廊错落展示、武器浮动、图片悬停缩放，无新增浏览器动画依赖。

## Boss2 实机视频

`public/media/boss2-gameplay.mp4` 为本项目 Unity Editor Play Mode 中录制的 Boss2 战斗，1280×720、30 fps、28 秒、无音轨。通过 `MainMenuForm.GoTest2()` 进入正常战斗，以项目 `IGameInput` 接口自动驱动移动、冲刺与攻击，拾取通过原有碰撞逻辑发生；没有修改角色或 Boss 血量、伤害与出招规则。它是自动操作的实机演示，不是人工游玩录像或性能基准。

网页将首领战截图替换为原生视频播放器，点击播放，独立暂停、进度拖动与全屏；离开画面或切到后台自动暂停。不使用静态 poster，不自动播放，不影响主角图鉴、武器图鉴或首页视频的控制状态。

录制源保存在已忽略的 `work/boss2-capture/boss2-raw.mp4`；从第 1 秒截取 28 秒，以 H.264 CRF 24 和 faststart 导出网站副本。首次试录 `boss2-take1.mp4` 未用于官网。抽帧图与状态也保留在该目录供复查。

`Web/scripts/WebBoss2Capture.cs` 保留录制辅助源码（仅 `UNITY_EDITOR`）。需要重录时，将其临时复制到 `Assets/Game/Scripts/ColorTiming/WebBoss2Capture.cs`，编译后从 Launch 进入运行，待主菜单出现后执行 `Game Framework/GameTools/ColorTiming/Web Capture/Record Boss2`。录制会运行自动输入、以固定 30 fps 模拟并采集 Game View、完成后暂停编辑器。退出运行时还原录制时修改的提示开关、后台运行和 captureDeltaTime。操作前确保输出目录中的上一份源录像已另存。此次已退出 Play Mode，并移除 Assets 中的临时脚本及 meta；Unity 场景与游戏代码没有保留改动。

### 视频方向与网页黑屏修复

当前网页使用 `public/media/boss2-gameplay-upright.mp4`，以新文件名避开旧视频缓存；旧 `boss2-gameplay.mp4` 同步为修正版本。原始 Game View 采集存在上下颠倒，导出时使用 `vflip` 校正（等价于旋转 180 度后再校正左右镜像），并通过 WASD 和中文提示检查方向。采用 H.264 Main / Level 3.1、yuv420p、faststart。

视频容器不再参与 `section-enter` 滚动裁剪动画，也不保留动画结束后的 clip-path 或 transform，以减少视频合成层黑屏问题；标题和其余正文动效保留。网页播放仍不使用静态封面。

### 动画加载优化

角色与武器展示区接近视口 800px 时，按组提前加载动画与武器图标，每组最多两个后台请求。共享请求缓存避免重复下载；实际选中动作解码完成后再替换旧画面，快速连续切换不会被旧请求覆盖。离屏动画仍暂停，角色和武器暂停状态保持独立。片头等待脚本时完全隐藏标题，标题初始裁剪收拢到 50%，移除跳过片头按钮。
