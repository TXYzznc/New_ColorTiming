## ADDED Requirements

### Requirement: Product scripts follow the confirmed layered layout
ColorTiming 产品脚本 SHALL 按 Domain、Application、Presentation、Infrastructure 与 Bootstrap 分层；第一次迁移遗留的空 `Player`、`Bosses` 和 `UI` 顶层目录 MUST NOT 保留为活动结构。

#### Scenario: Developer browses the ColorTiming script root
- **WHEN** 开发者查看 `Assets/Game/Scripts/ColorTiming`
- **THEN** 目录结构与已确认的分层架构一致，且不存在仅含 `.meta` 的旧功能目录

### Requirement: Business UI remains outside GF UI Core
ColorTiming 业务 UI Form、组件、Presenter、模型和合同 SHALL 位于 `ColorTiming/Presentation/UI` 的职责子目录；`Assets/Game/Scripts/UI/Core` SHALL 只承载 GF UI 通用接入机制与现有生成注册表。

#### Scenario: A new ColorTiming form is located
- **WHEN** 开发者查找具体的 ColorTiming GF UI Form
- **THEN** 该 Form 位于 `Presentation/UI/Forms`，而不是通用 `UI/Core` 或笼统的 `Views` 目录

### Requirement: Platform and framework adapters have explicit ownership
GF UI、Sound、Entity、Setting 端口实现 SHALL 位于 `Infrastructure/GF`；Unity Input 与 Time adapter SHALL 位于 `Infrastructure/Unity`，且上层接口和业务状态不得反向依赖具体实现。

#### Scenario: A GF adapter is reviewed
- **WHEN** 开发者定位类名以 `Gf` 开头的 ColorTiming 端口实现
- **THEN** 其物理路径和命名空间明确表达 `ColorTiming.Infrastructure.GF` 所有权

### Requirement: Unity serialized references survive script relocation
所有 MonoBehaviour 脚本迁移 MUST 保持原 `.meta` GUID，且迁移后 Prefab、Scene 和 UnityEvent 中不得出现由本次变更造成的 Missing Script 或断裂引用。

#### Scenario: Scripts are moved to normalized directories
- **WHEN** Unity 完成资产移动、重新导入和脚本编译
- **THEN** 每个迁移脚本的 GUID 与迁移前一致，项目 Missing Script 数量为零，受保护美术资产合同通过

### Requirement: Runtime behavior remains unchanged
目录和命名空间整理 MUST NOT 改变 UI ID、Prefab 结构、场景结构、资源键、战斗规则、输入语义或场景流行为。

#### Scenario: Regression suites run after normalization
- **WHEN** 执行 ColorTiming EditMode、PlayMode 和资源审计
- **THEN** 所有既有通过项继续通过，且没有新增编译错误或资源差异
