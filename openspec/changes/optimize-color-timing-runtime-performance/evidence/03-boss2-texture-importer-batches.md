# Boss2 纹理导入优化证据

## 约束

- 只修改 Windows/Standalone 的 TextureImporter 覆盖参数。
- 未修改、覆盖或重新编码任何 PNG/JPG 源文件。
- 未修改 Sprite、材质、Spine Atlas、场景和预制体中的美术引用。
- 目标分辨率为 1920×1080；最终画面仍需用户在 Game View/Player 中人工验收。

## 已保留的有效调整

| 资源组 | 原导入状态 | 最终导入状态 | 近似内存变化 |
|---|---|---|---:|
| `Boss2/Shenti/第二章boss.png` | 8192×4096 RGBA32 | 4096×2048 BC7 | 128 MB → 8 MB |
| `Boss2/Weiba/第二章boss.png` | 8192×4096 RGBA32 | 4096×2048 BC7 | 128 MB → 8 MB |
| `Scene/B2/层级1～4` | 6830×3783 RGBA32 | 4096×2269 RGBA32 | 约 493 MB → 142 MB |
| `Scene/B2/暗角.png` | 6830×3783 RGBA32 | 4096×2269 RGBA32 | 约 98.6 MB → 35.45 MB |
| `Scene/B2/摆放位置（注意前后位置和左右方向）.jpg` | 6830×3783 RGBA32 | 4096×2269 RGBA32 | 约 98.6 MB → 35.45 MB |

以上十二张候选纹理（含下述四张纸片）的合计运行时纹理内存由约 1.07 GB 降至约 357 MB，减少约 717 MB。最终保留的六张背景层合计为 212.7 MB，两张 Spine 纹理合计为 16 MB。

## 已回滚的无效调整

`纸片1～4.png` 原始尺寸为 7800×4320，默认导入已经是 BC7，单张约 32.14 MB。尝试设置 4096 + BC7 后，Unity 按比例生成 4096×2269；由于高度不满足 BC 块压缩条件，实际回退为 RGBA32，单张反而上升到 35.45 MB。

该试验属于负优化，已经逐张恢复原导入设置。最终四张纸片保持 7800×4320 BC7，合计约 128.6 MB，同时保留完整原画质。

## 导入稳定性处理

在运行中的 Editor 通过 UnitySkills 直接重导入 6.8K 纹理时，Unity 2022.3 原生纹理导入器连续退出；崩溃记录只保留 `Start importing ...`，没有 C# 异常或编译错误。后续改为：

1. 每次只准备一张 `.meta`；
2. 使用 `-batchmode -nographics -quit` 离线导入；
3. 每批等待 Unity 进程退出并检查退出码；
4. 最终重新启动 Editor，通过 UnitySkills 查询实际尺寸、格式和内存。

所有后续单张批次退出码均为 0，没有再次发生异常退出。

## 自动化验证

- `BossAttackExecutionPlayModeTests.Boss2_HeadTailBurrowAndAttackEventsExecuteThroughFrameworkEntities`
  - total=1
  - passed=1
  - failed=0
  - duration=28.395s
- 测试后 `unity_diagnose`
  - healthy=true
  - consoleErrorCount=0
  - consoleWarningCount=0
  - isCompiling=false

## 待人工验收与同步

- 2026-08-28：用户确认 1920×1080 画面验收通过。
- 这些纹理位于被 Git 忽略的美术目录；另一台设备需要重新导出并同步“美术资源”资源组，不能只依赖普通 Git 提交。

## 回退

- Spine 两张纹理：关闭 Standalone override，即恢复 8192×4096 RGBA32。
- 六张背景层：关闭 Standalone override，即恢复 6830×3783 默认导入。
- 四张纸片已经处于原配置，无需回退。
