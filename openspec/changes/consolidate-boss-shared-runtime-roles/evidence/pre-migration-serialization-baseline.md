# 第一批共享角色迁移前序列化基线

记录时间：2026-08-31。该基线用于证明脚本类型、Scene/Prefab 引用和美术组合在迁移前后的对应关系；不复制或修改美术源资产。

## 目标资产 SHA-256

| 资产 | 迁移前 SHA-256 |
|---|---|
| `BattleHud.prefab` | `3414D1090F5AD2FD148F1EE1F77B0ABD010C1ABB040E393505DBFB60E919CA6F` |
| `Boss1.unity` | `07F6796CD836C65DC6A766CD7E5CC98EDCED2748291412C25D9D33CE61FC82B5` |
| `Boss2.unity` | `665270BBB0F7BCDD2013F1C7FBA08882A48334C4568BBDBFFBE19B36FA281C8B` |

## 旧脚本 GUID 与引用位置

| 类型 | GUID | 资产引用 |
|---|---|---|
| `Boss1HealthView` | `f1f2a4ac3fd25d54ab848adfe84f581d` | `BattleHud.prefab` |
| `Boss2HealthView` | `fd7a939aa0a05004ab3d85017ad27c17` | `BattleHud.prefab` |
| `Boss1WeaponSpawnerView` | `fb81dd1d3e6ab6c4cac26fe62af849c6` | `Boss1.unity` |
| `Boss2WeaponSpawnerView` | `68a801f1217e44a46abac8f95906f4fd` | `Boss2.unity` |
| `WeaponSpawnerView` | `f930d2115ae14c4bbb412f9a50cc2de1` | 迁移前无 Scene/Prefab 直接引用 |
| `BattleHudForm` | `2c65aad324ef23745a46fc57e6b9c791` | `BattleHud.prefab` |
| `BattleSceneAnchors` | `38b422e1bfac7384bb82a2f16ec9d2ac` | `Boss1.unity`、`Boss2.unity` |

## 迁移前合同

- `BattleHud` 同时包含 `Slot_Boss1HP` 和 `Slot_Boss2HP`。
- Boss1 Slot 有 RectTransform、CanvasRenderer、Image、HorizontalLayoutGroup、Boss1HealthView；Boss2 Slot 只有 RectTransform、Boss2HealthView。
- 两个 Health View 的 `HPItem` 都引用同一 `BossHP_Item.prefab`，Item 内的颜色 Sprite、`tip1`、`tip2` 与 Animator 引用必须原样保留。
- Boss1 Scene 的 Player 字段只有 `boss1WeaponSpawner` 非空；Boss2 Scene 只有 `boss2WeaponSpawner` 非空。
- Boss1 Anchor 的 `boss1` 非空、`boss2` 为空；Boss2 Anchor 相反。
- 两个 Scene 分别引用独立 `Boss1WeaponSpawnRule.asset` / `Boss2WeaponSpawnRule.asset`，该数据差异必须保留。

## 迁移成功判据

- 旧四个具体 View/Spawner GUID 在所有 `.prefab`、`.unity`、`.asset` 中引用数为零。
- `BattleHud` 只有一个 `Slot_BossHP`，保留 Boss1 Slot 的布局组件与背景美术，并保留 `BossHP_Item` 的全部美术引用。
- 每个 Boss Scene 只有一个 `WeaponSpawnerView`，Player 只有一个非空生成器引用。
- 每个 Boss Scene 的显式绑定中恰好一个 `IBossBattleSessionConsumer`，BattleKind 与 Scene 匹配。
- Unity 无 Missing Script/Reference，Boss1/Boss2 行为回归通过。
