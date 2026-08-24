# ColorTiming 视觉回归对照

日期：2026-08-24

## 结论

- StartMenu 开场视频已经恢复：目标运行时可见实际视频帧、Logo 与三枚主菜单按钮，不再是纯黑背景。
- Boss1、Boss2 的主相机范围、场景边界、角色/Boss 相对位置、Spine 渲染、颜色分段、HUD 层级和动态命中特效均在目标 URP 运行时截图中可见。
- 源静态基准和目标运行时检查点均为 1920×1080。Built-in 到 URP 带来的整体色调/照明差异可见，但没有粉色材质、Fallback、缺图或层级颠倒。
- 源 Boss 基准是编辑态 Main Camera 静态渲染，目标 Boss 证据是运行态截图，因而 HUD、教程、攻击阶段和动画姿势不能做逐像素比较；这些项目继续由运行时截图、GUID/材质映射和 PlayMode 行为测试联合证明。

## 源基准生成方法

源项目保持只读。先复制到隔离目录 `D:\unity\UnityProject\ColorTimeing\New\_ColorTiming_SourceVisualCapture_20260824`，只在隔离副本注入一次性编辑器截图脚本：

- `Boss1`、`Boss2`：打开源场景，使用场景 `Main Camera` 以 1920×1080 渲染。
- `StartMenu`：源 UI 以透明层渲染，再从源 `Assets/Art/Vido/2循环.mp4` 提取第 5 秒原帧并进行 Alpha 合成。
- 批处理日志：`Documentation/Refactor/source-visual-capture-20260824.log`，包含 `SOURCE_VISUAL_CAPTURE_COMPLETE`、成功退出和返回码 0。

截图后再次按迁移前基线计算源项目 SHA-256：3575 个资产缺失 0、差异 0；26 个 ProjectSettings 缺失 0、差异 0。

## 检查点

| 场景 | 源基准 | 目标证据 | 对照结果 |
| --- | --- | --- | --- |
| StartMenu | `Documentation/Refactor/VisualBaseline/Source/source-startmenu.png` | `Assets/Screenshots/color-timing-startmenu-video-runtime-final.png` | 视频背景、Logo、按钮样式/位置和前后层级一致；目标左上角额外显示框架开发态 Debugger。 |
| Boss1 | `Documentation/Refactor/VisualBaseline/Source/source-boss1.png` | `Assets/Screenshots/color-timing-urp-boss1-stacked.png`、`color-timing-urp-boss1-fillphase1.png` | 主相机覆盖完整紫色竞技场；Boss 在上、玩家在中心；运行态 HUD、颜色条、武器与填充阶段正确叠加，Spine/特效无粉色。 |
| Boss2 | `Documentation/Refactor/VisualBaseline/Source/source-boss2.png` | `Assets/Screenshots/color-timing-urp-boss2-stacked.png`、`color-timing-urp-boss2-onhit.png` | 主相机覆盖完整棕色竞技场；Boss/尾部与玩家层级正确；四色条、命中闪白/遮罩及散布物可见，Spine/特效无粉色。 |

## 文件完整性

| 文件 | SHA-256 |
| --- | --- |
| `source-startmenu.png` | `30b6a4798ea3f52f5f43af30f491c8b0004b617389be4217abccefc4c6ee4d6f` |
| `source-boss1.png` | `5bef3de202674d4b1dbfd7f2e4b5a38883d0751196180fd824a638d2bfaa25d9` |
| `source-boss2.png` | `8c2a5b273f6f3f363725467e06e870bcabecd6dcc85c797d263b669573601eaf` |
| `color-timing-startmenu-video-runtime-final.png` | `848be7496a2fd50819178f8163e1250af3ed6ce130a9dc6b0f3a929ae521ab25` |
| `color-timing-urp-boss1-stacked.png` | `6fa6a528d856375763a3c1f5890d8d550f96c65c5357b8c7731be81c98e9b1db` |
| `color-timing-urp-boss1-fillphase1.png` | `22d3e3e91536b7a5824211d812eb6a86b85dd77eddfb6d5e0e616d2e6f423f45` |
| `color-timing-urp-boss2-stacked.png` | `a5d79b76de36ccf9fbb8b3da29a5d1b4cdbffd10465d2fa6017ee35f54329e89` |
| `color-timing-urp-boss2-onhit.png` | `e5e2cf9925a83d211caf135ed4f05574e3535ce9b988712b8dbe0412b2b7aaca` |

## 证据边界

OpenSpec 10.7 可由三张源基准和隔离捕获日志关闭。10.8 仍保持未完成，直到在 UnitySkills Bypass 下补取与源 Boss 静态帧严格对应的目标 Main Camera 静态帧，或取得源/目标同一运行状态的配对帧；现有证据足以证明视频已恢复和目标运行态没有明显渲染回归，但不冒充逐像素同帧比较。

相关证明：`spine-urp-material-mapping.md`、`playmode-color-timing-latest.xml`、`boss-runtime-progression-validation.md`、`source-immutability-and-rollback.md`。
