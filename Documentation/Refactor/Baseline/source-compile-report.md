# 源项目 C# 编译基线

- 日期：2026-08-24
- Unity 版本：`2022.3.62f3c1`
- 入口项目：`Assembly-CSharp.csproj`
- .NET SDK：`10.0.100`
- 结果：成功
- 错误：0
- 警告：13
- 完整日志：`source-dotnet-build.log`

## 命令策略

先使用 `dotnet restore --artifacts-path <目标项目>/Temp/SourceBaselineBuild`，随后以 `--no-restore --no-incremental` 编译。Unity 生成的 `.csproj` 内含绝对 `OutputPath`，因此 DLL 仍由 MSBuild 写入源项目生成目录 `Temp/bin/Debug`；没有写入任何 `Assets`、`Packages` 或 `ProjectSettings` 文件。

## 源事实完整性验证

编译后将 `Baseline/source-assets.csv` 中 3575 个资产的 SHA-256 与源文件重新计算并对比：差异 0。将 `project-snapshot.json` 中全部 ProjectSettings SHA-256 重新计算并对比：差异 0。

## 既有警告分类

- Spine runtime：2 条 `UNT0006`，来自 Spine 3.8 对 Unity message 的旧签名。
- Spine editor：2 条 `CS0618`，使用已弃用的 `TextureImporter.spritesheet`。
- 产品代码：3 条 `CS0108`，旧字段隐藏 Unity Component 成员。
- 产品代码：1 条 `CS0649`，`PlayAnimation.animation` 从未赋值，支持其废弃候选结论。
- 产品代码：5 条 `CS0414`，未使用字段或测试状态，包括两个 Boss `selectedAtk`、`UI_WeaponTip.showTime`、`HeroController.animLayer` 和原型条件类型。

这些警告是源基线事实，不自动允许目标新增同类警告。目标完成门槛仍为零编译错误，并要求迁移相关警告逐项处置。
