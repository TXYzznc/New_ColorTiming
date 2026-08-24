# 源项目不变性与目标回滚点

日期：2026-08-24

## 源项目只读复核

源项目：`D:\unity\UnityProject\ColorTimeing\ColorTimeing`

以迁移前 `Baseline/source-assets.csv` 和 `Baseline/project-snapshot.json` 为权威基线，重新读取并计算 SHA-256：

- 资产基线：3575；缺失：0；哈希差异：0。
- ProjectSettings 基线：26；缺失：0；哈希差异：0。

结论：迁移和重构操作未修改源项目的任何已盘点资产或工程设置。

## 目标分支与回滚

- 目标分支：`codex/color-timing-framework-migration`
- 迁移前框架提交：`0abebfe05f947718e5b1dcd34c303cd25591e3ec`
- 迁移实现检查点：`503f23a26eeee97a7144239cfb73c6333eaf83a5`（`refactor: migrate ColorTiming onto AI-friendly framework`）
- 回滚原则：保留 `0abebfe05f947718e5b1dcd34c303cd25591e3ec` 作为迁移前只读回滚点；`503f23a26eeee97a7144239cfb73c6333eaf83a5` 是完整迁移成果的首个可运行检查点。

OpenSpec 12.6 已由源哈希复核、目标分支记录和上述提交序列共同证明。
