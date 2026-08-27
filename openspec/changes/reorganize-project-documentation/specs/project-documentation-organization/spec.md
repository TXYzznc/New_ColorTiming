## ADDED Requirements

### Requirement: Docs 的稳定目录职责
项目 MUST 将项目制作规范存放在 `Docs/Development/`，将游戏设计、功能基线和开发任务存放在 `Docs/GameDesign/`。`Docs` MUST NOT 存放完整工作日志、控制台流水或重复机器快照。

#### Scenario: 定位制作规范
- **WHEN** 协作者需要 FSR、协作调度或 Git 集成规则
- **THEN** 它 MUST 能从 `Docs/Development/` 和项目入口直接定位规范。

### Requirement: OpenSpec 证据归属与精简
项目 MUST 将保留的重构证据放入对应变更的 `evidence/` 目录。证据 MUST 限于功能合同、批准修复、功能追踪、最新测试结果、审计摘要、视觉基线或未解决问题记录；完整可再生日志和重复快照 MUST NOT 保留。

#### Scenario: 定位运行时表现验证
- **WHEN** 协作者审查 Loading、BGM 或运行时命名的验证结果
- **THEN** 它 MUST 能从 `refactor-color-timing-runtime-presentation/evidence/` 找到对应布局、测试或未解决验证记录。

### Requirement: 迁移完整性与删除可审计性
项目 MUST 在删除旧 `Documentation/` 内容前记录保留与删除清单，更新所有内部路径引用，并在迁移后通过链接、纯度审计和 OpenSpec 校验。删除只允许作用于清单中明确列出的可再生产物。

#### Scenario: 完成文档迁移
- **WHEN** `Documentation/` 被移除
- **THEN** 项目 MUST 不存在指向旧路径的有效引用，且保留／删除清单 MUST 可在本迁移变更中审查。
