# 第一批共享战斗角色实施证据

记录时间：2026-08-31。

## 实施结果

- `BattleHud.prefab` 仅保留一个 `Slot_BossHP` 和一个 `BossHealthView`。
- `Slot_BossHP` 沿用原 Boss1 血条的 RectTransform、Image、HorizontalLayoutGroup 和背景 Sprite；`hpItem` 仍引用原 `BossHP_Item.prefab`。
- Boss1/Boss2 Scene 均改为一个具体 `WeaponSpawnerView`；分别保留原 `Boss1WeaponSpawnRule.asset`、`Boss2WeaponSpawnRule.asset`，教程提示参数分别为 1、2。
- `PlayerActorView` 仅持有一个 `weaponSpawner`，主动丢弃和受击丢弃走同一生成入口。
- `BattleSceneAnchors` 不再保存 Boss1/Boss2 互斥字段，而是在 `explicitBindings` 中校验恰好一个 `IBossBattleSessionConsumer`，并校验其 `BattleKind`。
- 已删除四个仅用于枚举 Boss 类型的脚本及 `.meta`：`Boss1HealthView`、`Boss2HealthView`、`Boss1WeaponSpawnerView`、`Boss2WeaponSpawnerView`。

## 序列化与资源保护检查

| 检查项 | 结果 |
|---|---|
| 通用 `BossHealthView` GUID 在 BattleHud 中的引用数 | 1 |
| 通用 `WeaponSpawnerView` GUID 在 Boss1 Scene 中的引用数 | 1 |
| 通用 `WeaponSpawnerView` GUID 在 Boss2 Scene 中的引用数 | 1 |
| `Slot_BossHP` 数量 | 1 |
| `Slot_Boss1HP` / `Slot_Boss2HP` 数量 | 0 |
| 四个旧脚本 GUID 在 `.prefab` / `.unity` / `.asset` 中的引用数 | 均为 0 |
| Boss 血条 Item Prefab GUID | 仍为 `ad4e1c1d08ebbec4e93653269f8831ef` |
| Boss 血条背景 Sprite GUID | 仍为 `741b0f5c99e986c478bda203beaea01d` |
| Boss1 / Boss2 生成规则 GUID | 仍为 `c1a46ffb45cb2304db211dbd7dc652f9` / `c1d02422e0a162249af48363443bfce1` |

未修改 Sprite、Animator、AnimationClip、Spine、AudioClip、Material 或原 Item Prefab 内容。

## 迁移后目标资产 SHA-256

| 资产 | SHA-256 |
|---|---|
| `BattleHud.prefab` | `930F910B0323D46E9B5C1FEBB20CF0315497F5E969847D843ED6EA0C58E3ED3C` |
| `Boss1.unity` | `8BC103B3F6393DE0E5FA45DC2036D7B2A39EDCE15A8A1F7887BD1582EB968D55` |
| `Boss2.unity` | `FE54FD84816F5E52367CBE5FBB86A1179E6F1B885DEF4022E011A3E91CC8125D` |

## 工具与验证状态

- 幂等迁移菜单 `Tools/ColorTiming/Migration/Consolidate Shared Battle Roles` 已执行成功，并在最终类型收口后重复执行通过。
- 迁移后的 Unity 编译诊断为 0 error / 0 warning。
- 全量 EditMode：222/222 通过；包含新增 `BattleSceneAnchorsTests` 与既有武器生成规则测试。
- 本变更定向 PlayMode：`ColorTimingBattleHudPlayModeTests` 1/1 通过；同一测试实际进入 Boss1、击败 Boss1 后进入 Boss2，并验证两场景共用一个 `Slot_BossHP`、一个具体 `WeaponSpawnerView`，且分别保留 9/12 项已创作武器规则。
- Boss1、Boss2 逐场景 `validate_scene`：各 0 issue；`validate_missing_references`：各 0；全项目 Scene/Prefab `validate_find_missing_scripts`：0。
- OpenSpec strict validation 通过。
- `python tools/audit_framework_purity.py` 通过。

## 后续失败收敛

对最初 15/19 的四项失败继续追踪后，确认并处理了两项实际业务缺陷和两项测试契约问题：

- `Skill_Bo2_atk2_s` 生成的子技能现通过 `Skill_base.ConfigureNestedSkill<T>()` 继承攻击者、武器、朝向和动画事件参数，不再产生 `missing-payload`。
- `PlayerActorView` 补充实现 `IBattleSessionConsumer`，组合根会实际调用其既有 `BindBattleSession()`，修复玩家状态机与武器库存未绑定。
- `BattleRuntimeContext.IsReady` 成为资源准备和显式依赖绑定完成后的公开测试边界，战斗测试不再把 Active Scene 误当作玩法已就绪。
- Loading 测试改为验证表单覆盖切换生命周期并在完成后关闭；不再于下一帧错误要求已在上一帧末派发卸载的旧场景仍保持加载。

最终验证：ColorTiming 专用 EditMode 82/82、完整 PlayMode 19/19，Unity 编译诊断 0 error / 0 warning。
