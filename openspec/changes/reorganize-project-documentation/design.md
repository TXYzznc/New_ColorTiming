## Context

`Documentation/Development` 有 14 份项目制作规范，当前被 AGENTS、Claude 指令、纯度审计和协作规格硬编码引用；`Documentation/Refactor` 有 120 份、约 45.6 MB 的混合证据，其中 26 份完整日志和多组重复导入／验证快照不适合作为长期项目 Docs。`Docs` 已包含稳定的制作规范入口与游戏设计／重构规格，`openspec` 已包含迁移和运行时表现两个对应变更。

## Goals / Non-Goals

**Goals:**

- `Docs/Development` 成为项目制作规范的唯一稳定路径；`Docs/GameDesign` 只存游戏设计、功能基线和开发任务。
- 每项保留的重构证据归入其所属 OpenSpec 的 `evidence/`，保持原始格式和可追溯名称。
- 用一份删除清单记录所有移除的可再生或重复工作产物。
- 更新所有内部链接、审计路径和 OpenSpec 引用，迁移后移除 `Documentation/`。

**Non-Goals:**

- 不把完整控制台流水、重复导入日志或所有原始 CSV 快照重新安置到 Docs/OpenSpec。
- 不因文件整理改变功能合同、测试结论、OpenSpec 任务状态或 Unity 内容。
- 不归档尚未完成验证的运行时表现变更。

## Decisions

### 1. Docs 只保留 Development 与 GameDesign

`Docs/Development` 接收项目制作规范（FSR、协作、Git 集成）；`Docs/GameDesign` 保留项目设计、功能矩阵、验收策略和开发任务。两者均不承载原始执行流水。

### 2. OpenSpec evidence 是保留证据的唯一归宿

迁移总证据归入 `migrate-color-timing-to-ai-friendly-framework/evidence/`；Loading/BGM/命名等运行时表现证据归入 `refactor-color-timing-runtime-presentation/evidence/`。证据只保留源功能合同、批准修复、功能追踪、最新测试结果、审计摘要、视觉基线和未解决问题记录。

### 3. 删除可再生和重复产物

删除完整导入／控制台日志、重复测试重跑、旧 `latest` 快照、重复资产清单目录和没有独立结论的临时记录。迁移变更自身保存删除清单、保留清单及删除前汇总，不保留被删除文件副本。

## Risks / Trade-offs

- [链接失效] → 在删除前全库替换与校验 Markdown 链接、AGENTS 引用、审计和 OpenSpec 严格验证。
- [删除有价值的证据] → 先将保留／删除清单写入本变更的 evidence，并只删除清单内的精确文件／目录。
- [OpenSpec 变成日志仓库] → evidence 只接收精选摘要、最新机器结果和视觉基线；完整日志一律删除。
- [协作框架入口失效] → 同步更新 AGENTS、Claude 指令、纯度审计及其测试。

## Migration Plan

1. 写入保留／删除清单并建立 OpenSpec evidence 目录。
2. 移动 Development 规范和精选 Refactor 证据，保留对应变化的语义归属。
3. 更新所有引用、Docs 索引、协作规格和审计路径。
4. 删除清单内剩余产物和空目录。
5. 运行 Markdown 链接、agent 同步、纯度审计、OpenSpec 严格校验及 Git 差异检查。

## Open Questions

- 无。删除依据和迁移边界已由用户确认；未完成的 PlayMode 状态会作为证据保留。
