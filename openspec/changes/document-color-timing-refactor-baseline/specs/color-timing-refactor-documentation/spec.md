## ADDED Requirements

### Requirement: 可追溯的功能基线
项目 SHALL 在 `Docs` 中定义以源工程可观察行为为基础的功能合同，并为每个功能领域提供稳定功能 ID、目标重构方式、状态、验收方法和证据入口。

#### Scenario: 定位一个待验收功能
- **WHEN** 协作者从 Docs 中选择任一功能 ID
- **THEN** 文档 MUST 能定位该功能的源行为、目标实现、当前状态和验收依据。

### Requirement: 分层且可扩展的文档结构
项目 SHALL 将 Docs 组织为核心框架、系统设计、玩家体验、重构实施和设计管理，并保留未来需求的受控入口。

#### Scenario: 提出未来功能需求
- **WHEN** 有新的玩法、功能或产品需求被提出
- **THEN** 该需求 MUST 先记录为待确认事项，且不得在未决策前改变当前功能基线或验收结论。

### Requirement: 单一证据来源与同步规则
项目 SHALL 将原始重构日志、截图、CSV 和自动化测试结果保留在 `Documentation/Refactor`；Docs MUST 通过链接引用这些证据，而不得制造第二份原始证据。每个完成的重构变更 MUST 同步受影响的 Docs 状态和验收入口。

#### Scenario: 完成一个重构批次
- **WHEN** 一个 OpenSpec 重构批次完成实现和验证
- **THEN** 受影响功能页、状态总表和证据入口 MUST 在同一变更中更新。

### Requirement: 当前范围的诚实表达
项目 Docs MUST 将当前没有新增玩法需求的事实表达为范围约束；未实现或尚未验证的项目 MUST 标识为待实施、实施中或已实现待验收，而不得标注为已验证。

#### Scenario: 全量 PlayMode 验证尚未完成
- **WHEN** 某功能只有编译或 EditMode 证据，未完成要求的 PlayMode 验证
- **THEN** 该功能状态 MUST 不得标为已验证，并 MUST 指向后续验证动作。
