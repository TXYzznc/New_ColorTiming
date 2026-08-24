# Unity Test Runner 发现证据（2026-08-24）

## 环境

- UnitySkills：`http://localhost:8093/`
- 实例：`GameDesinger_189B1E1A`
- Unity：`2022.3.62f3c1`
- 发现方式：Unity Test Runner async discovery cache

## 结果

| 模式 | 工程总数 | ColorTiming 数量 | ColorTiming 非 Runnable |
|---|---:|---:|---:|
| EditMode | 203 | 63 | 0 |
| PlayMode | 14 | 14 | 0 |

上表本身只证明测试程序集成功编译、被 Unity Test Runner 发现且状态为 `Runnable`，不单独证明通过。发现之后已在包含根目录 `Samples~`、`GameData` 与 `AB` 的独立完整工程副本中执行：PlayMode `14/14` passed、0 failed、0 skipped、0 inconclusive；EditMode 在最终运行修复后再次执行，最终结果见 `editmode-color-timing-203.xml/.log`。

## PlayMode 可运行合同

1. `Boss1_AllSixAttacksPlayAndDispatchTheirAuthoredSpineEvents`
2. `Boss2_HeadTailBurrowAndAttackEventsExecuteThroughFrameworkEntities`
3. `FormalFlow_ConsumesEveryBossColor_ActivatesTailAndShowsFinalResult`
4. `Boss1_ParallaxDistanceZoomAndCinemachineRuntimeWiringExecute`
5. `PauseForm_ReopensAndSceneExitReleasesPauseLease`
6. `SoundGroups_PersistMutePolicyAndResetTrackedSceneSounds`
7. `StartMenuNavigation_AndAllSettingsPersistThroughGfSetting`
8. `StartMenuVideo_RendersAndSwitchesFromIntroToLoop`
9. `GrassEnterExit_AnimatesAndSwitchesFrameworkFootstepCueSet`
10. `SkillAnimationEnd_ReleasesFrameworkEntityExactlyOnce`
11. `UnityGameTimeAdapter_ComposesAndReleasesRequests`
12. `SemanticInput_PickupHitDashDeathAndAnimationRestartExecuteInBoss1`
13. `LaunchBootsFrameworkAndLoadsStartMenuOnce`
14. `EveryAuthoredWeaponColorExecutesAnimationEventThroughGfEntity`

## EditMode 增量

63 个 ColorTiming EditMode 用例全部为 `Runnable`。相对已执行的 `201/201` 工程基线，本次发现的工程总数增加到 203；新增的两项是：

- `Boss1_MapsEveryAuthoredCueToBossSoundChannel`
- `Boss2_MapsEveryAuthoredCueToBossSoundChannel`

随后执行的完整 EditMode 结果已包含这两项，二者现已计入 `203/203` 通过数。原始证据：

- `editmode-color-timing-203.xml`
- `editmode-color-timing-203.log`

首次临时副本未复制仓库根目录 `Samples~`，因此框架的两个“可选包可发现”用例按预期失败；补齐完整仓库结构后的重跑为上述有效结果。首次运行不作为产品失败或通过证据。

## 同时复核的编辑器健康状态

- Unity 未在编译或刷新；
- 当前 Console error/exception：0；
- 场景及 Prefab missing script：0；
- shader compilation error：0。

PlayMode 原始执行证据：`playmode-color-timing-14.xml`、`playmode-color-timing-14.log`。以上健康检查不替代执行结果，也不替代最终人工画面/听感对比。
