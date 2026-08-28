## Why

当前业务虽然已经迁移到 GF_X 框架，但核心玩法仍由无命名空间的 `Legacy` MonoBehaviour、场景扫描和相互持有引用驱动，生命周期、状态所有权与资源事件边界不清晰。现有功能规模允许在完整设计后一次性重建业务架构，从而消除长期兼容层，同时保持源项目全部可观察行为与既有美术资源。

## What Changes

- **BREAKING**：以 Domain、Application、Unity Presentation、GF Infrastructure、Bootstrap 的单向依赖替代现有 `Legacy` 业务实现。
- **BREAKING**：删除旧 `HeroController`、Boss 控制器、`Weapon`、`I_Damage` 等核心业务类型；序列化资源事件改由无状态薄适配器承接。
- 建立唯一 `BattleSession` 与显式组合根，统一战斗状态、命令、事件、取消和销毁顺序，不再依赖全场景 MonoBehaviour 扫描进行业务服务定位。
- 将玩家、武器、Boss1、Boss2、伤害、颜色克制、胜负与暂停逻辑迁入可独立测试的纯 C# 模块。
- 保留 GF 的场景、UI、声音、输入、实体和设置能力作为基础设施端口实现；不修改 `Assets/Game/ScriptsBuiltin/`。
- 建立资源保护清单与自动化契约，保证原始美术内容、GUID、导入设置、129 个 Animation Event、19 个 UnityEvent 和 Spine Event 不被破坏。
- 以注释、事件清单、序列化引用和自动化测试作为主要验收证据；截图仅作可选辅助，最终画面、手感与音效由制作人实机验收。

## Capabilities

### New Capabilities

- `color-timing-battle-domain`: 规定战斗领域状态、规则、命令、事件和确定性生命周期。
- `color-timing-runtime-composition`: 规定 GF_X 下的场景组合、输入/UI/声音适配与运行时对象所有权。
- `color-timing-presentation-contracts`: 规定 Animation、Spine、UnityEvent、物理和 UI 薄适配器的表现边界。
- `color-timing-asset-preservation`: 规定既有美术资源、GUID、导入设置、序列化事件与视觉结构的非破坏式迁移和验收。

### Modified Capabilities

<!-- 本次保持既有产品行为，不修改框架基线中的正式能力需求。 -->

## Impact

- 主要影响 `Assets/Game/Scripts/ColorTiming/`、其测试程序集，以及包含业务脚本接线的 ColorTiming Scene/Prefab。
- 不把业务热更作为设计目标，也不扩展 HybridCLR 热更程序集清单；新增普通 `ColorTiming.Domain` 与 `ColorTiming.Application` 运行时程序集，历史命名的 `Hotfix` 仅承载启动入口及 Unity/GF 适配并单向引用它们，测试程序集继续独立。
- 资源本体视为受保护输入；允许保留 `.meta` 的改名/迁移和可审计的脚本重绑，不重新制作或破坏美术资产。
- 旧业务类型和场景扫描式组合属于删除范围；GF_X 框架核心、第三方包与无关工具配置不属于本次修改范围。
