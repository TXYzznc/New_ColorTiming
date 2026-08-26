## Why

ColorTiming 的重构证据分散在 `Documentation/Refactor`、OpenSpec 与代码中，尚缺少可供开发、验收和后续新增需求共同使用的产品功能基线。当前没有新玩法需求，因此需要先将源工程的已观察行为、批准的修复差异和重构路径收敛为可维护的 Docs 体系。

## What Changes

- 在 `Docs` 建立面向 ColorTiming 的功能基线、系统规格、玩家体验、重构实施与设计管理目录。
- 为每个功能领域记录源项目行为、目标架构／重构方式、实现状态、验收方法及证据链接。
- 建立功能 ID、状态总表、术语表、决策归档与未来需求入口，使后续新需求能够在不改变既有功能合同的前提下扩展。
- 保留现有 GF UI 全流程规范，并在项目文档中明确其适用边界。

## Capabilities

### New Capabilities

- `color-timing-refactor-documentation`: ColorTiming 功能基线、重构可追溯性、验收索引及后续需求扩展的文档合同。

### Modified Capabilities

无。

## Impact

- 新增和维护 `Docs/` 下的 Markdown 文档；不修改运行时代码、Unity 场景或内容资产。
- 以 `Documentation/Refactor/source-feature-acceptance-checklist.md`、已有审计证据和现有 OpenSpec 为输入，避免复制或改写源工程行为。
- 后续重构任务与新增需求应引用本次建立的功能 ID、状态与验收入口。
