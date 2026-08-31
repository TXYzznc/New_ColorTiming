## Why

Boss1 与 Boss2 当前各自维护一套 Sound View、Cue 枚举、动画事件字符串映射和 Cue→AudioClip switch；差异仅是创作数据。两个 Animation Event Relay 还重复实现受击闪烁并在 Update 中反复创建 MaterialPropertyBlock。需要消除确认的表现层重复，同时保持两个 Boss 的动画事件与攻击合同独立。

## What Changes

- 新增通用 `BossSoundView` 与 `BossSoundCueCatalogAsset`，由每个 Boss 的 Catalog 保存现有 Cue、动画事件键和 AudioClip 引用。
- Boss1/Boss2 保留语义化 Cue ID 常量；Actor/Relay 继续表达各自事件合同。
- 新增窄职责 `BossHitFlashView`，缓存 MaterialPropertyBlock 并管理一个或多个 Renderer 的受击闪烁。
- 通过幂等 Unity Editor 迁移把两个 Scene 的旧 Sound View 字段迁入 Catalog，并把 Relay 的 Renderer 引用迁入 Hit Flash 组件。
- 删除 `Boss1SoundView`、`Boss2SoundView` 与两个 Boss 专属 Cue enum。
- 不合并 Boss1/Boss2 Animation Event Relay，不引入 Battle/Scene Descriptor。

## Capabilities

### New Capabilities

- `color-timing-boss-audio-presentation`: 所有 Boss 共享音效播放生命周期与受击闪烁机制，Boss 差异由 Cue Catalog 和独立动画事件合同表达。

## Impact

- 业务代码：Boss1/Boss2 Actor、Animation Event Relay、通用 Audio/Presentation 组件。
- Unity 资产：Boss1/Boss2 Scene、新增两个 Cue Catalog ScriptableObject。
- 工具与测试：迁移器、映射测试、Scene/PlayMode 回归。
- 美术资源：只迁移引用；不修改 AudioClip、Spine、材质、Shader、动画或 Renderer 内容。
