# 明确缺陷修复记录

本文件只登记依据已确认策略允许的明确缺陷修复。未登记的玩家可见差异默认视为迁移回归。

## FIX-001：Boss2 橙色生命段使用错误槽位索引

- 状态：implemented，待运行态验收
- 源证据：Boss2 的 `DamageHPColor` 橙色分支以 `HP_zi[0]` 读取当前槽位名称，却从 `HP_chen` 集合移除生命段。
- 违反不变量：一次有效命中必须读取、显示并移除同一弱点队列槽位；颜色集合不能交叉索引。
- 目标行为：当当前弱点为橙色时，仅使用当前橙色段对应的逻辑槽位和显示项；扣除一次后推进同一全局弱点队列。
- 自动化证据：`Assets/Game/Tests/ColorTiming/EditMode/WeaknessSlotLedgerTests.cs` 证明橙色消费只读取/移除橙色队列且紫色队列不变；`Boss2BattleLogicTests` 证明 15 段与尾部阶段边界。
- 人工证据：`MANUAL-BOSS2-HP`、`MANUAL-BOSS2-RESULT`（执行后补报告）。
- 数值/玩法影响：不改变 15 段总量或红4/绿4/紫4/橙3 分布，仅修正错误索引。

## 新增修复规则

每个新条目必须包含源证据、被违反的不变量、目标行为、自动化证据、人工证据和数值/玩法影响。没有这些字段的差异不得合入。

## FIX-002：Boss1 攻击 5 的 Spine 视图切换比较了字符串字面量

- 状态：implemented，待运行态验收
- 源证据：源 `Boss1_Controller.AnimPlay` 使用 `animName == "animName_Atk5"`，而实际参数值为 `attack_5_test1_60fps`，该分支恒为 false。
- 运行证据：Spine2 同时包含无事件的 `attack_5_test1_60fps` 草稿和带 4 个 `attack/atk5` 事件的 `attack_5_test1_60fps2`；迁移后选择后者，确保第 5 招的无敌窗口与攻击实体均实际执行。
- 违反不变量：攻击 5 必须切换到专用 SkeletonAnimation，并在结束后恢复主体、提示与弱点显示。
- 目标行为：使用 `animName_Atk5` 常量作比较；只对攻击 5 切换专用 Spine 视图，其余攻击保持主体视图。
- 自动化证据：`Boss1BattleLogicTests` 覆盖攻击 5 的远距选择和禁止连续选择；产品与测试程序集编译为 0 错误。
- 人工证据：`MANUAL-BOSS1-ATK5`（待最终回归补报告）。
- 数值/玩法影响：不改变攻击 5 权重、冷却或伤害，只恢复原注释和资源结构明确要求的视图切换。

## FIX-003：Boss1 受击动画随机上界导致 Hit1 永不出现

- 状态：implemented，待运行态验收
- 源证据：源代码调用整数重载 `Random.Range(0, 1)`；Unity 整数上界不包含，因此结果恒为 0，`Hit1` 分支不可达。
- 违反不变量：两条已配置受击动画均应能被选择，且每次有效同色命中只播放其中一条。
- 目标行为：将整数上界改为 2，使 0/1 两个结果均可达。
- 自动化证据：产品程序集编译为 0 错误；最终 PlayMode 将记录两条受击表现可达性。
- 人工证据：`MANUAL-BOSS1-HIT`（待最终回归补报告）。
- 数值/玩法影响：不改变生命段、伤害或无敌时序，只恢复第二条已配置视觉分支。

## FIX-004：武器生成计时跨过阈值后额外延迟一帧

- 状态：implemented，EditMode 已覆盖，待运行态验收
- 源证据：旧计时逻辑在 `remaining > 0` 分支中先减去 `deltaTime` 后无条件返回 false；即使本帧已跨过零点，也必须等下一帧才生成。
- 违反不变量：生成周期应由配置间隔决定，不应随帧时间或低帧率额外变长。
- 目标行为：跨过阈值的同一帧生成，并保留超出的时间量用于下一周期；单帧最多产生一次生成决策。
- 自动化证据：`TimeAndSpawnPolicyTests.SpawnClockIsReadyImmediatelyAndFiresOnThresholdCrossing` 与 `PlayerWeaponRuntimeTests.SpawnerRuntime_UsesClock_AndGuaranteesCurrentWeakness`。
- 人工证据：`MANUAL-BOSS1-SPAWN`、`MANUAL-BOSS2-SPAWN`（待最终回归补报告）。
- 数值/玩法影响：不修改配置间隔、场上数量限制或弱点保底策略，只消除最多一帧的非确定性延迟。

# Migration compatibility fixes

## EventSystem input module replacement

The source Boss1 and Boss2 scenes serialized `InputSystemUIInputModule` plus the package's `DefaultInputActions`, while all authored gameplay input uses the Legacy Input Manager. The framework baseline intentionally does not carry the unused Input System dependency. During migration, each missing Input System UI component is replaced with `StandaloneInputModule`; the existing `EventSystem` component and UI objects are retained. This restores pointer/navigation processing without broadening the package surface.
# FIX-005 — Framework group-enum generator output path

- Source behavior: the framework editor constant targeted the nonexistent path `Assets/Game/Scripts/Common/Core/Const.Groups.cs`.
- Target behavior: the generator targets the existing canonical file `Assets/Game/Scripts/Common/Const.Groups.cs`.
- Invariant: refreshing all framework data tables completes without a `DirectoryNotFoundException` and preserves generated group enums.

# FIX-006 — Gameplay camera discovery

- Fault: the scene binder searched for `Camera` instances inside a `MonoBehaviour[]`; `Camera` derives from `Behaviour`, so Boss scenes with pointer/camera consumers always failed binding.
- Fix: scan loaded scene roots directly with `GetComponentsInChildren<Camera>(true)` and select an enabled main/fallback camera.
- Invariant: both Boss scenes bind pointer and camera consumers without using `GameObject.Find` and without reporting a missing camera when an enabled scene camera exists.

# FIX-007 — GF.Sound resource names and stale serial cleanup

- Fault: the sound adapter omitted the `.wav` extension required by the framework asset path, so migrated scene AudioSources were stopped but no GF.Sound agent played their clips. Completed one-shots also remained in the pause-tracking set and caused `Can not find sound` exceptions.
- Fix: build canonical `ColorTiming/*.wav` and `ColorTiming/BGM/*.wav` names, track scene sounds separately, stop them on scene exit, and prune completed gameplay serials while retaining still-loading serials.
- Invariant: legacy play-on-awake sources are stopped, the corresponding GF.Sound agent owns playback, pause never addresses a completed serial, and outgoing loop sounds do not survive a scene transition.

# FIX-008 — Scene-parented GF.Entity recycling race

- Fault: transient GF entities can be parented to authored scene transforms. A fast scene unload destroyed the Unity `Entity` object before EntityManager processed its next-frame recycle queue.
- Fix: request release at `TransitionStarted` and move tracked transient entities back under the persistent GF.Entity root before hiding them.
- Invariant: scene-parented effects preserve authored transforms during play but remain alive long enough for framework unspawn/recycle during every scene exit.

# FIX-009 — Pooled main-menu navigation state

- Fault: reopening pooled `MainMenu` retained the previous selected/settings panel, unlike loading the original authored menu scene.
- Fix: `UI_ButtonAction.OnOpen` restores the start panel and closes the level/settings panels before each pooled form open.
- Invariant: every entry to StartMenu begins on the authored start panel while persistent settings toggles are refreshed independently.

# FIX-010 — URP gameplay/UI camera composition

- Fault: the persistent framework `UICamera` and each product `Main Camera` were both URP Base cameras. The higher-depth UI camera therefore replaced the gameplay camera output, leaving Boss scenes black except for UI.
- Fix: on every product-scene bind, configure the scene camera as Base, configure the framework UI camera as Overlay, and add it exactly once to the scene camera stack. Restore the UI camera to Base when the ColorTiming composition root is disposed. StartMenu explicitly retains its authored black clear background.
- Invariant: StartMenu, Boss1, and Boss2 render one gameplay frame with GF.UI composited above it; scene transitions never retain a destroyed camera in the stack.
- Evidence: `Assets/Screenshots/color-timing-urp-startmenu-final.png`, `color-timing-urp-boss1-stacked.png`, and `color-timing-urp-boss2-stacked.png`.
- Gameplay impact: none; this restores visibility and does not modify camera tracking, framing, confiner, or impulse parameters.

# FIX-011 — Broken grass-footstep references in source Boss1 scene

- Fault: the source Boss1 scene serialized four nonexistent AudioClip GUIDs in `HeroSoundManager.rMove_Overwrite_Audio`; `XiaoCao` selects this list while the hero walks through grass.
- Fix: map the four entries, in order, to the existing `grass_walk_001.WAV` through `grass_walk_004.WAV` assets.
- Invariant: entering grass selects four valid grass-footstep clips and leaving grass restores the blanket/default movement set; no null clip is sent to GF.Sound.
- Evidence: the source and pre-fix target scene both contained the same four unresolved GUIDs; the post-fix `validate_missing_references` result for Boss1 is zero.
- Gameplay impact: restores intended grass movement audio only; movement timing and random selection behavior are unchanged.

# FIX-012 — Formal product scenes omitted from live Editor Build Settings

- Fault: applying the domain-neutral framework-template purity rule to the installed ColorTiming product disabled StartMenu, Boss1, and Boss2. In GF EditorResource mode those scenes are loaded through `SceneManager.LoadSceneAsync`, which requires them to be enabled in Build Settings.
- Fix: declare the four formal Launch-flow scenes as the product repository's explicit Build Settings allowlist and provide `Game Framework/GameTools/Sync ColorTiming Build Scenes` to synchronize the live editor state deterministically.
- Invariant: Launch remains entry index 0; StartMenu, Boss1, and Boss2 are enabled exactly once; undeclared scenes still fail the framework-foundation purity audit.
- Evidence: formal Launch reaches StartMenu with zero console errors; `python tools/audit_framework_purity.py` passes; the persisted PlayMode suite exercises StartMenu → Boss1 → Boss2 → StartMenu.
- Gameplay impact: restores formal editor startup and scene transitions; no gameplay values or scene contents change.

# FIX-013 — Product scenes carried duplicate EventSystems under the persistent framework UI

- Fault: Boss1 and Boss2 retained their authored root EventSystems while Launch already owns the persistent GF.UI EventSystem. Additive product flow could therefore activate two input event dispatchers.
- Fix: retain the serialized product EventSystem objects and `StandaloneInputModule` compatibility data, but disable both roots; the persistent Launch EventSystem remains the sole active dispatcher.
- Invariant: exactly one runtime EventSystem dispatches UI input across StartMenu, Boss1, Boss2, pause and result forms; leaving a scene cannot retain a product dispatcher.
- Evidence: both product scene YAML roots are inactive; the post-fix PlayMode suite passes 5/5 through all product scenes with zero console errors.
- Gameplay impact: removes duplicate UI dispatch risk without changing button bindings or navigation data.

# FIX-014 — Weapon entities bypassed GF.Entity despite implementing its lifecycle contract

- Fault: `Pickup_Weapon` implemented `IFrameworkEntityParticipant`, but `WeaponSpawnerView` still created and destroyed world weapons directly. Scene exit could therefore bypass the central transient-entity tracker.
- Fix: make the spawner an `ITransientEntityConsumer` and create weapons through `ITransientEntityService`; pickup initialization runs in the GF.Entity show callback and pickup release returns the entity to the framework pool.
- Invariant: formal Boss scene weapon creation requires the Launch composition root, is tracked by the Effect entity group, and participates in `ReleaseAll` during scene transitions.
- Evidence: Unity recompiles with zero errors/warnings; EditMode passes 201/201 and the all-scene PlayMode lifecycle suite passes 5/5.
- Gameplay impact: preserves spawn positions, interval, color/type policy, fade, pickup and tutorial behavior; only ownership/recycling changes.

# FIX-015 — Hero charge-tip listeners were not removed

- Fault: `Hero_XuliTip` subscribed to two `HeroController` UnityEvents in `Start` but had no matching removal.
- Fix: retain the controller reference and remove both listeners in `OnDestroy`.
- Invariant: every runtime-added UnityEvent listener owned by a scene view is removed when that view is destroyed.
- Evidence: lifecycle inventory and manual source review show matching `AddListener`/`RemoveListener`; EditMode 201/201 and PlayMode 5/5 pass.
- Gameplay impact: none; prevents retained callbacks during scene teardown.

# FIX-016 — Pooled menu VideoPlayers lost their scene-camera output

- Fault: the source StartMenu VideoPlayers rendered to the authored scene camera. Converting the menu into a GF.UI prefab correctly removed the cross-scene object reference, leaving both `CameraFarPlane` players with `targetCamera = null`; the loop decoded and played but produced no visible frame. Pool reopening also retained the completed intro state.
- Fix: `StartVido` creates one pooled-form-owned RenderTexture/RawImage background, routes both players through that URP-safe output, resets the intro→loop sequence on every GF.UI form open, stops both players on form close, and releases the runtime texture on destruction.
- Invariant: every StartMenu entry visibly starts `1开头.mp4`, switches once to looping `2循环.mp4`, and releases playback when the pooled form closes.
- Evidence: runtime diagnosis showed `2循环` prepared/playing with a decoded texture while `targetCamera` was null; `Assets/Screenshots/color-timing-video-rt-fixed-late.png` and `color-timing-startmenu-video-runtime-final.png` show the restored stable loop frame beneath the menu. Runtime property inspection records `renderMode=RenderTexture`, `targetTexture=ColorTiming StartMenu Video`, `isPrepared=true`, `isPlaying=true`, and `isLooping=true`; `StartMenuVideo_RendersAndSwitchesFromIntroToLoop` verifies shared RenderTexture output, intro playback, completed handoff and looping playback. Latest PlayMode result: 6/6 passed.
- Gameplay impact: restores the authored menu background video; menu controls and transition timing are unchanged.

## 11.9 统一复核矩阵

复核日期：2026-08-24。`Fault`/`Source behavior` 等同“源证据”，`Fix`/`Target behavior` 等同“目标行为”。下表补齐旧条目未显式写出的自动化、人工和玩法影响字段；人工列为 `pending` 时不得据此关闭最终人工回归任务。

| 修复 | 自动化/静态证据 | 人工证据 | 数值/玩法影响 |
|---|---|---|---|
| FIX-001 | `WeaknessSlotLedgerTests`、`Boss2BattleLogicTests` | `MANUAL-BOSS2-HP`、`MANUAL-BOSS2-RESULT` pending | 不改变 15 段和四色分布 |
| FIX-002 | `Boss1BattleLogicTests`、201/201 EditMode | `MANUAL-BOSS1-ATK5` pending | 不改变权重、冷却和伤害 |
| FIX-003 | 分支可达性静态复核、201/201 EditMode | `MANUAL-BOSS1-HIT` pending | 不改变伤害和无敌时序 |
| FIX-004 | `TimeAndSpawnPolicyTests`、`PlayerWeaponRuntimeTests` | `MANUAL-BOSS1-SPAWN`、`MANUAL-BOSS2-SPAWN` pending | 仅消除最多一帧延迟 |
| FIX-005 | 全量数据表刷新与 201/201 EditMode 编译链 | `MANUAL-FRAMEWORK-DATATABLE-REFRESH` pending | 无玩法影响 |
| FIX-006 | Boss1/Boss2 场景绑定 PlayMode、相机序列化审计 | Boss1/Boss2 目标截图已保存 | 仅恢复相机绑定 |
| FIX-007 | Sound 生命周期 PlayMode、场景往返 6/6 | 菜单音频开关已观察；Boss 音频专项 pending | 不改变音量与触发时点 |
| FIX-008 | Entity 生命周期 PlayMode、场景往返 6/6 | `MANUAL-SCENE-ENTITY-EXIT` pending | 不改变实体位置与存活时长 |
| FIX-009 | MainMenu 反复开关 PlayMode | 菜单→设置/关卡→返回已观察 | 仅恢复每次进入的初始面板 |
| FIX-010 | 三场景相机栈检查、missing/shader 审计 | 三张目标运行态截图已保存 | 不改变跟随、构图与冲击参数 |
| FIX-011 | Boss1 missing reference=0、资源 GUID 对账 | `MANUAL-BOSS1-GRASS-AUDIO` pending | 仅恢复草地脚步音 |
| FIX-012 | Launch→三场景→StartMenu PlayMode 6/6、框架纯度审计 | 正式 Launch 入口已观察 | 仅恢复正式场景可加载性 |
| FIX-013 | 场景 YAML/EventSystem 静态审计、PlayMode 6/6 | 三场景运行态 UI 已观察 | 仅消除重复输入派发 |
| FIX-014 | Entity 生命周期 PlayMode、EditMode 201/201 | `MANUAL-WEAPON-SPAWN-PICKUP` pending | 不改变生成、拾取和淡出规则 |
| FIX-015 | listener 对称性审计、EditMode 201/201 | `MANUAL-HERO-CHARGE-TIP-EXIT` pending | 无玩法影响 |
| FIX-016 | `StartMenuVideo_RendersAndSwitchesFromIntroToLoop`、完整 PlayMode 14/14 | `color-timing-video-rt-fixed-late.png` | 仅恢复原菜单双视频背景 |
| FIX-017 | Boss 全血条进度 PlayMode、完整 PlayMode 14/14 | `MANUAL-BOSS2-RESULT` pending | 仅确保胜利同帧完成尾部清理 |
| FIX-018 | Boss1 六攻击直接执行、Spine 事件清单、PlayMode 14/14 | `MANUAL-BOSS1-ATK5` pending | 选择 Spine2 中实际带事件的攻击 5 版本 |
| FIX-019 | GF.Entity 自回收与切场景直接执行、PlayMode 14/14 | `MANUAL-SCENE-ENTITY-EXIT` pending | 仅保证回收前脱离场景父节点 |
| FIX-020 | 草地触发/音频 PlayMode、Console 无参数警告 | `MANUAL-BOSS1-GRASS-AUDIO` pending | 无 Animator 参数时跳过动画触发，声音/脚步不变 |
| FIX-021 | 玩家死亡 Animation Event 同场景重载、PlayMode 14/14 | `MANUAL-BOSS1-DEATH-RELOAD` pending | 同资源先卸载完成再加载，并拒绝重复重启请求 |

复核结论：当前已知且可见的迁移差异均对应本文件中的 FIX 条目；未发现未登记的刻意行为变化。所有 `pending` 人工项继续由 6.7、7.10、12.1～12.5 持有，不以本次文档复核替代运行态验收。

## FIX-017 — Boss2 最终结果暂停阻止尾部延迟清理

- 状态：implemented，PlayMode 已通过，待人工最终结果验收。
- 源证据：迁移后的尾部控制器只在下一次 `FixedUpdate` 观察头部 `death` 后停止潜地和碰撞；最终结果 Form 在同一胜利调用中立即取得 `timeScale=0` 租约，因此该物理帧不保证发生。
- 违反不变量：最终胜利必须在显示结果前同步停止头、尾、投射物、标记和重定位，不能依赖暂停之后的帧回调。
- 目标行为：头部确认最终胜利时直接调用尾部幂等 `StopForBattleEnd`，清除攻击/移动状态、中断潜地流程并关闭碰撞、轨迹提示和出土提示；随后释放全部 GF.Entity 临时实体，再打开结果 Form。
- 自动化证据：`BossRuntimeProgressionPlayModeTests.FormalFlow_ConsumesEveryBossColor_ActivatesTailAndShowsFinalResult` 验证最终一击同帧 `IsStoppedForBattleEnd=true` 且尾部 Collider 已关闭；持久证据 `playmode-color-timing-14.log/xml`，完整 PlayMode 14/14 passed。
- 人工证据：`MANUAL-BOSS2-RESULT` pending。
- 数值/玩法影响：不改变 Boss2 生命、攻击、阶段阈值或结果时序，仅使已要求的胜利清理确定发生。

## FIX-018 — Boss1 攻击 5 选择了 Spine2 的无事件草稿

- 源证据：Spine2 同时导出 `attack_5_test1_60fps`（无 Event）与 `attack_5_test1_60fps2`（4 个 `attack/atk5` Event）；旧代码的字符串字面量错误又让 Spine2 分支不可达。
- 目标行为：进入攻击 5 专用 Spine2 视图并播放带事件版本，四次生成攻击环；无敌窗口与弱点恢复仍由原 Event/Complete 合同控制。
- 自动化证据：`Boss1_AllSixAttacksPlayAndDispatchTheirAuthoredSpineEvents` 与完整 PlayMode 14/14。
- 数值/玩法影响：不改变伤害、冷却或选择权重，只恢复素材已创作的攻击实体事件。

## FIX-019 — 场景父节点销毁早于 GF.Entity 回收队列

- 故障：瞬态实体为跟随出生点而挂到场景 Transform；实体自回收后已移出活动 ID 集合，但仍在 GF 的下一帧 recycle queue，切场景会先销毁其 `Entity` 组件。
- 目标行为：`ColorTimingTransientEntity.OnHide` 在进入框架回收队列前移回持久 `GF.Entity` 根节点。
- 自动化证据：Boss1 攻击 5 大量嵌套实体、自回收、后续攻击与返回菜单连续通过；完整 PlayMode 14/14 无 `MissingReferenceException`。
- 数值/玩法影响：不改变实体位置、运动和寿命。

## FIX-020 — 部分草地 Animator 没有 Trigger 参数

- 故障：所有草地进入都无条件 `SetTrigger("Trigger")`，没有该参数的控制器持续写 Console 警告。
- 目标行为：启动时缓存参数合同，仅对确有 Trigger 参数的草地触发摆动；玩家脚步覆盖与环境声仍按原逻辑执行。
- 自动化证据：`GrassEnterExit_AnimatesAndSwitchesFrameworkFootstepCueSet`、完整 PlayMode 14/14，相关参数警告为 0。
- 数值/玩法影响：无参数的对象原本就无法播放该 Trigger 动画；声音与移动规则不变。

## FIX-021 — 同场景死亡重载在卸载期间立即加载

- 故障：强制重载当前 Boss1 时，启动流程同帧请求 Unload 与 Load 同一资源，GF 正确抛出“scene is being unloaded”；旧死亡动画对象还可能重复请求。
- 目标行为：目标资源与当前资源相同时等待 `UnloadSceneSuccess` 后再 Load；`Death_sc_Over` 每个死亡对象只接受一次重启请求。
- 自动化证据：`SemanticInput_PickupHitDashDeathAndAnimationRestartExecuteInBoss1` 单项与完整 PlayMode 14/14。
- 数值/玩法影响：不改变死亡动画与重启结果，只使正式 GF 场景时序确定化。
