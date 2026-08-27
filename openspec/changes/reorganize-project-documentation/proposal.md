## Why

项目同时维护 `Documentation/`、`Docs/` 与 `openspec/`，其中 `Documentation` 混合了项目制作规范、重构结论和大量可再生日志，导致入口重复、链接分散且难以判断长期保留价值。用户已确认以 `Docs` 作为稳定项目文档入口，并将筛选后的变更证据归入对应 OpenSpec。

## What Changes

- 将 `Documentation/Development/` 迁入 `Docs/Development/`，并更新所有入口、审计和 OpenSpec 中的硬编码引用。
- 将 `Documentation/Refactor/` 按证据价值筛选：保留的验收、追踪、修复、摘要、测试结果和视觉基线迁入所属 OpenSpec 的 `evidence/`；删除重复、可再生或无独立结论的日志、快照和旧结果。
- 保持 `Docs/` 仅包含 `Development/` 与 `GameDesign/` 两类长期内容；OpenSpec 仅保存变更合同及精炼证据。
- 更新 Docs 的证据链接与索引，移除空的 `Documentation/` 根目录。

## Capabilities

### New Capabilities

- `project-documentation-organization`: Docs、OpenSpec 证据和可删除工作日志之间的目录职责与迁移合同。

### Modified Capabilities

- `ai-team-collaboration`: 协作规范与入口路径从 `Documentation/Development/` 迁移到 `Docs/Development/`。
- `framework-collaboration-purity`: 纯度审计改为检查新的 Docs 开发规范路径。

## Impact

- 修改项目入口指令、纯度审计、协作文档引用、现有 Docs 证据链接和 OpenSpec 证据引用。
- 不修改 Unity 运行时代码、场景、Prefab、资源或数据表。
- 删除的内容仅是已识别为重复或可再生的工作日志／快照；保留清单和删除摘要会记录在本变更证据中。
