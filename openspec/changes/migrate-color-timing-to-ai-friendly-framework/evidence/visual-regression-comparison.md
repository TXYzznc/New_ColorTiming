# ColorTiming 视觉回归对照

日期：2026-08-24

## 结论

- StartMenu 开场视频已经恢复：目标运行时可见实际视频帧、Logo 与三枚主菜单按钮，不再是纯黑背景。
- Boss1、Boss2 的主相机范围、场景边界、角色/Boss 相对位置、Spine 渲染、颜色分段、HUD 层级和动态命中特效均在目标 URP 运行时截图中可见。
- 源静态基准和目标运行时检查点均为 1920×1080。Built-in 到 URP 带来的整体色调/照明差异可见，但没有粉色材质、Fallback、缺图或层级颠倒。
- 已补齐目标 Boss1、Boss2 的编辑态 Main Camera 静态渲染；两侧均为 1920×1080、同场景、同相机、无 HUD/Debugger 的可配对帧。Boss1 的 Spine idle 刀刃是编辑器预览子帧，源/目标内部子帧不同；场景、角色、Boss 主体、颜色、遮罩和层级可直接逐点比较，但不宣称逐像素相同。

## 源基准生成方法

源项目保持只读。先复制到隔离目录 `D:\unity\UnityProject\ColorTimeing\New\_ColorTiming_SourceVisualCapture_20260824`，只在隔离副本注入一次性编辑器截图脚本：

- `Boss1`、`Boss2`：打开源场景，使用场景 `Main Camera` 以 1920×1080 渲染。
- `StartMenu`：源 UI 以透明层渲染，再从源 `Assets/Art/Vido/2循环.mp4` 提取第 5 秒原帧并进行 Alpha 合成。
- 批处理以 `SOURCE_VISUAL_CAPTURE_COMPLETE`、成功退出和返回码 0 完成；完整捕获流水已按文档保留策略清理。

截图后再次按迁移前基线计算源项目 SHA-256：3575 个资产缺失 0、差异 0；26 个 ProjectSettings 缺失 0、差异 0。

## 目标静态基准生成方法

目标主实例保持打开且未切场景。以提交 `40343f3` 创建 detached 隔离工作树 `D:\unity\UnityProject\ColorTimeing\New\_ColorTiming_TargetVisualCapture_20260824`，只在隔离工作树注入一次性编辑器捕获脚本：

- 从零重建隔离 AssetDatabase，仅复用主项目 `Library/PackageCache`，避免不同绝对路径下复用 AssetDatabase 导致脚本映射失效。
- 打开 `Boss1`、`Boss2`，显式初始化场景内 Spine SkeletonAnimation（Boss1 3 个、Boss2 4 个），再由场景 `Main Camera` 渲染 1920×1080 PNG。
- Unity 批处理返回码 0，并记录两条 `TARGET_VISUAL_CAPTURE_SAVED` 与 `TARGET_VISUAL_CAPTURE_COMPLETE`；完整捕获流水已按文档保留策略清理。
- 另对 Boss1 idle 归一化时间采样 31 帧；`target-boss1-static-samples-20260824.log` 包含 `TARGET_VISUAL_CAPTURE_BOSS1_SAMPLES_COMPLETE count=31`。采样确认源基准中的刀刃位置来自编辑器预览缓存子帧，不是材质、骨骼或层级缺失。

## 检查点

| 场景 | 源基准 | 目标证据 | 对照结果 |
| --- | --- | --- | --- |
| StartMenu | `VisualBaseline/Source/source-startmenu.png` | `Assets/Screenshots/color-timing-startmenu-video-runtime-final.png` | 视频背景、Logo、按钮样式/位置和前后层级一致；目标左上角额外显示框架开发态 Debugger。 |
| Boss1 | `VisualBaseline/Source/source-boss1.png` | `VisualBaseline/Target/target-boss1-static.png`，辅以 `Assets/Screenshots/color-timing-urp-boss1-stacked.png`、`color-timing-urp-boss1-fillphase1.png` | 静态帧的相机、紫色竞技场、玩家中心位置、Boss 主体、彩色针/珠、前后景遮挡和排序一致；仅 idle 刀刃子帧不同。运行帧补证 HUD、武器、填充和特效，无粉色/Fallback。 |
| Boss2 | `VisualBaseline/Source/source-boss2.png` | `VisualBaseline/Target/target-boss2-static.png`，辅以 `Assets/Screenshots/color-timing-urp-boss2-stacked.png`、`color-timing-urp-boss2-onhit.png` | 静态帧的相机、棕色竞技场、玩家/Boss 位置、Boss 四色分块、遮罩和排序一致；运行帧补证尾部、四色条、命中闪白及散布物，无粉色/Fallback。 |

## 量化比较

比较源/目标静态 PNG 的 RGB 像素；这里的误差包括 Built-in→URP 的预期整体色调差异和 Spine 抗锯齿/子帧差异，不作为“必须为零”的阈值：

| 场景 | RGB MAE | RMSE | PSNR | 解释 |
| --- | ---: | ---: | ---: | --- |
| Boss1 | 3.234783 | 11.441257 | 26.9613 dB | 主要集中于全局轻微色调与 idle 刀刃子帧；主体、位置、颜色槽和遮挡关系保持。 |
| Boss2 | 7.633455 | 9.817866 | 28.2905 dB | 主要是 Built-in→URP 的整体浅色调变化；构图、Boss 姿势/四色、玩家与层级保持。 |

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
| `target-boss1-static.png` | `012d96e34ebfb0086cbc4ffcc5e56f52df8bf636c8014c79e2bddcfcf2f84aec` |
| `target-boss2-static.png` | `6a2b04310647f54c0e7298c061e3fde09b3c05b767f500cedae9ee6efe8d54e2` |

## 证据边界

OpenSpec 10.7 由三张源基准和源隔离捕获日志证明；10.8 由两张目标静态 Main Camera 帧、目标隔离捕获日志、StartMenu 配对帧和三场景运行态补充帧共同关闭。结论限于视觉检查点等价，不把 Built-in→URP 的预期色调变化或 Boss1 idle 子帧差异冒充逐像素一致，也不替代 12.1～12.4 的音频、输入、手感和完整人工路径验收。

相关证明：`spine-urp-material-mapping.md`、`playmode-color-timing-latest.xml`、`boss-runtime-progression-validation.md`、`source-immutability-and-rollback.md`。
