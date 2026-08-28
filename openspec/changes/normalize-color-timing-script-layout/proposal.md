## Why

ColorTiming 的第一次功能迁移与第二次分层重构留下了两套并存的目录语义：旧的 `Player/Bosses/UI` 空目录仍被跟踪，Unity/GF 适配器散落在 `Presentation`、`Input`、`Combat` 和 `Settings`，GF UI Form、组件、Presenter 与模型也混在同一级或笼统的 `Views` 目录。当前实现可运行，但代码发现性、职责边界和后续扩展成本已经偏离已确认的 Domain → Application → Presentation/Infrastructure → Bootstrap 架构。

## What Changes

- 清理第一次迁移遗留的空目录及其 `.meta`。
- 在 `ColorTiming` 下落实 `Infrastructure/GF` 与 `Infrastructure/Unity` 物理目录，把现有端口实现迁入对应边界。
- 将项目 UI 按 `Forms`、`Components`、`Presenters`、`Models` 分类，消除 Form 与叶子 View 混放。
- 在不改变 MonoScript GUID、Prefab/Scene 美术结构和序列化资源内容的前提下统一受影响代码的命名空间与引用。
- 保留 `Assets/Game/Scripts/UI/Core` 作为 GF UI 通用接入边界；生成的 `UIViews` 注册表暂不脱离现有生成流程。
- 增加目录、程序集依赖、GUID、Missing Script 和回归测试验收证据。

## Capabilities

### New Capabilities

- `color-timing-script-layout`: 规定 ColorTiming 产品代码的分层目录、UI 子职责、基础设施适配器归属、命名空间和 Unity 序列化安全迁移合同。

### Modified Capabilities

无。

## Impact

- 影响 `Assets/Game/Scripts/ColorTiming/` 下的物理路径、命名空间和内部引用。
- 不修改 `Assets/Game/ScriptsBuiltin/`、Prefab/Scene 层级、美术资源内容、UI 配置 ID 或运行时业务行为。
- 移动脚本必须通过 Unity AssetDatabase 并保留 `.meta` GUID；命名空间变化需要完整编译、Missing Script、EditMode、PlayMode 和受保护资源审计。
