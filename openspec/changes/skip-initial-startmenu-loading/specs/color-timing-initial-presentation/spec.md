## ADDED Requirements

### Requirement: 首次 StartMenu 不展示项目 Loading

系统 SHALL 将尚无当前业务场景时发起的 StartMenu 过渡标记为首次过渡，并 SHALL 跳过项目 `Loading` GF.UI 表单。MainMenu SHALL 仅在 StartMenu 场景成功加载并完成绑定后打开。

#### Scenario: Launch 首次进入 StartMenu

- **WHEN** ColorTiming 启动流程在尚无当前业务场景时请求 StartMenu
- **THEN** SceneFlow 发布 `IsInitialTransition=true` 的上下文
- **AND** 项目 Loading 表单不被请求
- **AND** StartMenu 成功绑定后打开 MainMenu

### Requirement: 后续产品场景切换继续展示 Loading

系统 SHALL 对所有非首次过渡继续展示项目 Loading，包括返回 StartMenu。

#### Scenario: Boss 返回 StartMenu

- **WHEN** 当前业务场景为 Boss1 或 Boss2，并请求 StartMenu
- **THEN** SceneFlow 发布带来源场景的非首次上下文
- **AND** 项目 Loading 表单按原生命周期打开、更新和关闭

#### Scenario: StartMenu 进入 Boss

- **WHEN** 当前业务场景为 StartMenu，并请求 Boss1 或 Boss2
- **THEN** 项目 Loading 表单按原生命周期展示

### Requirement: 首次加载不使用延迟 Loading 兜底

系统 SHALL 在首次 StartMenu 加载耗时较长时仍保持跳过项目 Loading，不得基于时间阈值重新打开进度表单。

#### Scenario: 首次加载较慢

- **WHEN** 首次 StartMenu 加载超过普通帧时长
- **THEN** 系统保持 Launch 当前背景直到 MainMenu 可用
- **AND** 不显示项目 Loading 或 BuiltinView Loading
