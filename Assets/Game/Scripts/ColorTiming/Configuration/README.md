# Configuration

只读业务配置边界。`GfColorTimingConfiguration` 将 GF DataTable 转换为强类型规则并在启动时校验跨表引用；业务模块不得自行读取 Excel、ScriptableObject 或提供隐藏默认值。

Excel 权威源和各表职责见 `Docs/GameDesign/04-重构实施/05-GF业务配置表实施说明.md`。

把 GF DataTable 行映射为运行时只读配置，并在启动阶段验证跨表 ID、枚举、权重与资源名称。该目录不保存运行时状态。
