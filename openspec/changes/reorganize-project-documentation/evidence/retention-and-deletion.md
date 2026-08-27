# Documentation 迁移保留与删除清单

> 决策依据：2026-08-27 用户确认。`Docs` 只保留 Development 与 GameDesign；OpenSpec 仅保留变更合同和精选证据。

## 迁入 Docs/Development

- `Documentation/Development/AI-Team-Collaboration-Initialization.md`
- `Documentation/Development/FastScriptReload.md`
- `Documentation/Development/Dispatch/` 全部 12 个规范、模板和示例文件。

## 迁入迁移 OpenSpec evidence

迁入 `openspec/changes/migrate-color-timing-to-ai-friendly-framework/evidence/`：

- 功能合同与决策：`source-feature-acceptance-checklist.md`、`behavior-fixes.md`、`decision-checkpoint-01.md`、`decision-checkpoint-02.md`、`deprecated-candidates.md`。
- 验收与追踪：`feature-traceability.csv`、`feature-traceability-audit.md`、`feature-traceability-audit.csv`、`final-manual-regression-runbook.md`、`validation-gate-latest.md`、`editmode-color-timing-203.xml`、`playmode-color-timing-latest.xml`。
- 架构与资源：`architecture-validation.md`、`animation-contract.json`、`asset-path-mapping.md`、`asset-reconciliation.json`、`asset-validation.json`、`inputs/source-assets.csv`、`inputs/migrated-assets.csv`、`method-surface-audit.json`、`method-surface-reverse-audit.md`、`serialized-field-surface-audit.json`、`serialized-field-surface-reverse-audit.md`、`runtime-risk-audit.json`、`runtime-risk-and-lifecycle-audit.md`、`spine-listener-audit.json`、`spine-urp-material-mapping.md`、`cinemachine-audit.json`。
- 行为／UI／场景：`boss-runtime-progression-validation.md`、`player-boss1-implementation-coverage.md`、`scene-lifecycle-smoke.md`、`source-immutability-and-rollback.md`、`weapon-presentation-mapping-audit.md`、`gf-ui-main-menu-smoke.md`、`gf-ui-pause-smoke.md`、`gf-ui-battle-result-smoke.md`、`fresh-library-validation.md`。
- 视觉：`visual-regression-comparison.md` 和 `VisualBaseline/` 全部 6 张源／目标截图。

## 迁入运行时表现 OpenSpec evidence

迁入 `openspec/changes/refactor-color-timing-runtime-presentation/evidence/ColorTimingLoading/`：

- `ColorTimingLoading/prefab-layout.md`
- `ColorTimingLoading/editmode-results.xml`

## 删除

删除 `Documentation/Refactor/` 中未列于以上保留清单的所有内容，包括：

- 26 份完整 Console、资源导入、测试重跑和刷新 `.log` 文件；
- 已被保留的最新 XML 替代的旧测试 XML；
- `Baseline/`、`PostMigrationAudit/`、`asset-validation-latest.json/`、`migration-audit-final.md/` 等重复资产快照目录；
- 未提供独立验收结论的原始 CSV、JSON、资源清单和临时文件。

删除后 `Documentation/` 应为空并整体移除。删除不影响已迁入 OpenSpec 的功能合同、测试结论、视觉基线或未完成 PlayMode 记录。
