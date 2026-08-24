## Why

当前 ColorTiming 是一个功能完整但高度耦合的 Unity 2022.3 项目，启动、输入、战斗、Boss、UI、音视频与场景流程直接绑定在场景对象和旧脚本中，难以在不遗漏功能的前提下持续维护。现在需要以 AI-Friendly-Project 为新基线进行旁路重构，在保留全部玩家可见行为与资源的同时获得标准启动链、资源管理、UI、实体、声音、异步和测试能力。

## What Changes

- 在独立目标目录中建立 AI-Friendly-Project 基线，源项目保持只读并作为功能、资源和行为对照。
- 将 `StartMenu`、`Boss1`、`Boss2` 接入框架启动 Procedure、GF.Scene 生命周期与统一产品场景流，保留加载进度、淡入淡出、暂停、重开、前后关卡及返回菜单流程。
- 引入语义化游戏输入边界，完整覆盖移动、冲刺、攻击、蓄力、丢弃、暂停、指针、教程确认与结果确认，业务代码不再直接读取 Unity `Input`。
- 将颜色弱点、武器、伤害、生命、时间减速、治疗、胜负与 Boss 阶段规则拆分为可测试的纯 C# 领域逻辑；MonoBehaviour、Animator、Spine 和 Cinemachine 只承担 Unity 适配与表现。
- 重构玩家、七类武器、技能、投射物、拾取/丢弃、生成保证逻辑，以及 Boss1、Boss2（含尾部与阶段行为）的全部运行时功能。
- 使用框架 UI、Entity、Sound、DataTable/Config 与异步约定承载菜单、HUD、设置、教程、暂停、结果、特效、音频、视频、世界交互和相机表现。
- **BREAKING**：目标项目统一采用 URP；补齐 Spine 3.8 URP Shader 支持并迁移 11 个现有 Boss 材质，不保留 Built-in 长期分支。
- 移除经引用审计证明为空或未使用的原型脚本和测试热键；修复证据明确的既有缺陷，所有行为变化均记录并验证，不进行玩法或数值再设计。
- 建立功能—代码—资源—测试追踪矩阵，要求资源/序列化引用对账、零编译错误、EditMode 测试、PlayMode 冒烟测试与三场景完整人工回归全部通过后才允许宣称迁移完成。

## Capabilities

### New Capabilities

- `color-timing-startup-flow`: 框架启动、预加载、场景切换、加载表现、暂停与关卡/结果流程。
- `color-timing-input`: 与具体 Unity 输入 API 解耦的语义输入契约及兼容适配器。
- `color-timing-combat-domain`: 颜色弱点、武器、伤害、生命、无敌、治疗、时间效果和胜负规则。
- `color-timing-player-weapons`: 玩家移动与战斗、七类武器、技能、投射物、拾取丢弃和武器生成保证。
- `color-timing-bosses`: Boss1 与 Boss2 的完整阶段、攻击、弱点、Spine 事件、尾部和胜利逻辑。
- `color-timing-ui-presentation`: 菜单、设置、HUD、教程、暂停、加载、胜负和结果界面。
- `color-timing-media-world`: 音频、开场/循环视频、特效、草地交互、视差与 Cinemachine 相机行为。
- `color-timing-migration-verification`: 功能与资源追踪、引用完整性、自动化测试和三场景人工验收证据。

### Modified Capabilities

无。此次变更在框架既有通用能力之上新增项目能力，不修改 `openspec/specs/` 中框架级需求。

## Impact

- 主要代码位于 `Assets/Game/Scripts/ColorTiming/`，框架核心 `Assets/Game/ScriptsBuiltin/` 保持不承载产品逻辑。
- 影响场景、Prefab、Animator/Animation Event、Spine 3.8 运行时与材质、Cinemachine、音频、视频、输入、DataTable、UI/Entity/Sound 配置及 Build Settings。
- 目标项目继续使用 Unity `2022.3.62f3c1`、URP `14.0.12`、Cinemachine `2.10.3`、UniTask 与 UnityGameFramework；增加与现有 Spine 3.8 数据兼容的 URP Shader 模块。
- 源项目不做实现性修改，所有迁移与验证证据均进入目标仓库。
