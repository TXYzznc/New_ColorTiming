# ColorTiming 项目文档

> 文档基线：源工程功能迁移与重构期
> 更新日期：2026-08-26
> 当前产品范围：仅完成源工程功能等价重构；暂无新增玩法需求。

本目录是 ColorTiming 的长期可读文档入口。它说明项目已经承诺要具备什么、如何以 GF_X 重构、当前完成到哪里、以及如何验收；它不替代原始测试日志、资源盘点或 OpenSpec。

## 推荐阅读

1. [项目与重构总览](GameDesign/00-项目与重构总览.md)
2. [功能与重构矩阵](GameDesign/04-重构实施/01-功能与重构矩阵.md)
3. [系统关系与场景流程](GameDesign/01-核心框架/00-系统关系与场景流程.md)
4. [重构架构与实施原则](GameDesign/04-重构实施/00-重构架构与实施原则.md)
5. [设计状态总表](GameDesign/90-设计管理/设计状态总表.md)
6. [源功能验收清单](../openspec/changes/migrate-color-timing-to-ai-friendly-framework/evidence/source-feature-acceptance-checklist.md)

## 文档层次

- `Development/`：ColorTiming 制作和协作规范。当前已包含 [GF UI 全流程规范](Development/GF-UI-Standards/README.md)。
- `GameDesign/`：ColorTiming 的功能合同、体验流程、重构实施与管理记录。
- `openspec/`：中大型变更的提案、设计、规格、任务与经筛选的验收证据；是变更过程的权威记录。

## 维护规则

1. 功能规则只在一个系统页作为正式定义；其他页面通过功能 ID 和链接引用。
2. 当前功能基线来自源工程的可观察行为；任何差异必须先记录为缺陷或经 OpenSpec 批准的修复。
3. 每项重构完成后，必须同步功能矩阵、状态总表和验收证据入口。
4. 新功能先进入[开放问题与未来需求入口](GameDesign/90-设计管理/开放问题与未来需求入口.md)，经决策与 OpenSpec 后才可写入基线。
