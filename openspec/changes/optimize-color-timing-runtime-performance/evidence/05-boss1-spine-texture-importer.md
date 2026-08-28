# Boss1 Spine 图集导入优化证据

日期：2026-08-28

## 结论

Boss1 关卡背景与逐帧环境动画已经采用 `2048` 上限、压缩格式且关闭 Mipmap，
本轮没有继续降低它们的分辨率。高收益项来自 Boss1 本体的 POT Spine 图集：
在不改变尺寸、不修改源 PNG 的前提下，仅增加 Standalone 平台 BC7 覆盖。

## 已修改资源

| 资源 | 修改前 | 修改后 | 估算节省 |
|---|---:|---:|---:|
| `Boss/Spine/BOSS拆分2.png` | 2048×2048 RGBA32，16.00 MB | 2048×2048 BC7，4.00 MB | 12.00 MB |
| `Boss/Spine/BOSS拆分3.png` | 2048×2048 RGBA32，16.00 MB | 2048×2048 BC7，4.00 MB | 12.00 MB |
| `Boss/Spine2/BOSS拆分.png` | 2048×2048 RGBA32，16.01 MB | 2048×2048 BC7，4.01 MB | 12.00 MB |
| `Boss/Spine2/BOSS拆分2.png` | 2048×1024 RGBA32，8.00 MB | 2048×1024 BC7，2.00 MB | 6.00 MB |

合计约 `56.01 MB -> 14.01 MB`，减少约 `42.00 MB（75%）`。

Standalone 覆盖参数：

- `overridden: 1`
- `maxTextureSize: 2048`
- `textureFormat: BC7 (25)`
- `compressionQuality: 100`
- 不启用 Mipmap，不启用 Read/Write

## 保护与回退验证

- 原始 PNG 没有被重采样、覆盖或重新编码；本轮只改 `.meta` 平台导入参数。
- `Boss/Spine/BOSS拆分.png`（2048×1921）曾做 NPOT BC7 样本验证，Unity 实际仍回退为
  RGBA32，因此已恢复原始 Standalone 设置，没有保留无收益覆盖。
- `Boss/第二关BOSS拆分3.png` 同样是 NPOT，并且不属于本轮 Boss1 主体资源，保持不变。
- `Scene/B1` 的 1106 张资源理论压缩占用约 162.03 MB；主要逐帧组已经是压缩、无 Mipmap，
  本轮不以降低分辨率换取收益。

## 自动验证

- 四张目标图集重新导入均成功，Unity 离线导入进程退出码为 `0`。
- Unity 重新读取后的实际格式均为 BC7，尺寸与修改前一致。
- PlayMode：
  `ColorTiming.Tests.PlayMode.BossAttackExecutionPlayModeTests.Boss1_AllSixAttacksPlayAndDispatchTheirAuthoredSpineEvents`
  结果 `1/1 Passed`，耗时 `25.836963 s`。
- 结果文件：
  `openspec/changes/reimplement-color-timing-business-architecture/evidence/TestResults/playmode-color-timing-latest.xml`

## 人工验收

2026-08-28，用户已在 1080p 下完成 Boss1 画面检查并确认验收通过；未发现明显色带、
边缘脏点或图集串图，任务 6.1 已勾选。
