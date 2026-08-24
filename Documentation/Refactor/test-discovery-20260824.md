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

这份证据只证明测试程序集成功编译、被 Unity Test Runner 发现且状态为 `Runnable`；它不证明测试已经执行或通过。此前保存的执行基线仍是新增专项合同之前的 EditMode `201/201`、PlayMode `7/7`，不得用本次发现数量改写通过数量。

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

它们同样只完成发现，尚未计入通过数。

## 同时复核的编辑器健康状态

- Unity 未在编译或刷新；
- 当前 Console error/exception：0；
- 场景及 Prefab missing script：0；
- shader compilation error：0。

以上是本次发现后的实时只读检查，不替代 PlayMode 执行结果。
