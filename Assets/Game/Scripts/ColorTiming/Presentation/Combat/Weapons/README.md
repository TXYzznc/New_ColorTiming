# Weapons

负责武器生成、拾取及关卡规则驱动的生成表现。

直接脚本：`WeaponPickupView.cs`、`WeaponSpawnerView.cs`。Boss 差异通过 `ColorTimingWeaponSpawnRuleTable` 配置，不新增 Boss 专属生成器类型或 ScriptableObject 配置副本。

修改本目录代码后，应执行 Unity 编译和对应测试。
