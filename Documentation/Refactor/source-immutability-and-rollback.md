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
- 回滚原则：保留上述提交作为迁移前只读回滚点；迁移提交序列在创建后追加到本节。

OpenSpec 12.6 只在迁移提交创建并复核后关闭。
