# 源脚本方法面反向审计

日期：2026-08-24

## 目的与方法

本审计从源工程反向出发，防止 `feature-traceability.csv` 只覆盖“已经想到的功能”，却遗漏源脚本中的入口。

- 源：`D:\unity\UnityProject\ColorTimeing\ColorTimeing\Assets\Game\Scripts`
- 目标：`Assets/Game/Scripts/ColorTiming/Legacy` 及拆出的 ColorTiming 框架模块
- 解析器：Unity 2022.3 自带 `Microsoft.CodeAnalysis.CSharp`，比较方法名和参数类型，不使用文本正则猜测 C# 结构。
- 交叉证据：资源处置表、UnityEvent/Animation Event 清单、代码引用、场景/Prefab missing-script 审计、现有 EditMode/PlayMode 合同。
- 可重复执行：`powershell -File tools/audit_color_timing_method_surface.ps1`；机器可读结果为 `method-surface-audit.json`。

## 汇总

| 项目 | 数量 | 结果 |
|---|---:|---|
| 源产品脚本 | 64 | 全部有唯一处置 |
| 有证据移除的无效/调试脚本 | 7 | 与 `deprecated-candidates.md`、`asset-reconciliation.json` 一致 |
| 应保留或重构的脚本 | 57 | 目标路径缺失 0 |
| 源方法签名全部仍存在的脚本 | 36 | 无需例外映射 |
| 存在方法签名变化的脚本 | 21 | 逐项复核如下 |
| 复核后无目标实现/无理由的源方法 | 0 | 未发现新的功能遗漏 |

“未发现新的功能遗漏”仅指静态方法入口全部有去向；动画时序、画面、声音和手感仍必须通过 OpenSpec 6.7、7.10、10.8、12.1～12.4 的运行时证据证明。

## 21 个签名变化脚本的逐项处置

| 源脚本 | 源侧消失/变化的方法 | 目标去向与证据 | 结论 |
|---|---|---|---|
| `Boss1_Controller.cs` | `CreateHP_ctp` | `CreateBossHP` 改用 `WeaknessQueue.CreateBoss1`，仍生成红4/绿3/紫4；分布与完整 11 段运行合同已存在 | 行为抽到领域层 |
| `Boss2_Controller.cs` | `CreateHP_ctp` | `WeaknessQueue.CreateBoss2` 生成红4/绿4/紫4/橙3；15 段、四色与尾部阈值合同已存在 | 行为抽到领域层 |
| `Boss1SoundManager.cs` | `Start`、`Update` | `BindSoundService` 取代本地 AudioSource；`GfColorTimingSoundService` 根据 `IGameTime.ScaleChanged` 暂停/恢复 Boss 组，cue 映射测试覆盖 | 框架声音生命周期替代 |
| `Boss2SoundManager.cs` | `Start`、`Update` | 同上，Boss2 全 cue 映射测试覆盖 | 框架声音生命周期替代 |
| `HeroSoundManager.cs` | `Start` | 本地 AudioSource 初始化改为 `BindSoundService`；Player/草地覆盖声道合同已存在 | 框架声音绑定替代 |
| `HeroAnimStae.cs` | `StartSupHit` | 源内只有定义、无代码引用、无 Animation Event，且只把 `Time.timeScale` 设为 0 而无恢复路径；不属于可达正式行为 | 删除不可达原型方法 |
| `HeroController.cs` | `BulletTime` | 成功 Dash 的 `Invoke`/全局 timescale 改为 `IGameTime.Pulse(0.45f, 0.3f)`，可组合且自动恢复；运行时玩家合同覆盖 | 框架时间服务替代 |
| `HeroController.cs` | `GetIntType` | 该方法实际属于同文件 `Weapon` 类型，迁入 `LegacyWeaponCompatibility.cs`，签名和 Animator 索引语义保留 | 文件拆分，API 保留 |
| `Pickup_Weapon.cs` | `Start`、`TrayPickUP` | 订阅与初始化拆成 `Awake`/`OnEnable`/`OnDisable` 和 `TryPickup`，增加 GF.Entity spawn/despawn 重置 | 生命周期重构 |
| `Pickup_Weapon.cs` | `OnTriggerStay2D` | 源方法只有被注释的 `TrayPickUP` 调用，无运行效果；正式拾取仍由进入范围后的玩家拾取事件触发 | 删除空接收器 |
| `WeaponControSystem.cs` | `Start/Update/OnBossDamage/CheckWeapon/CheckWeaponTip/CreateWeapon/GetRandomPos/CreateWeapon_dis` | 共同行为移入 `WeaponSpawnerView`；Boss1 子类只提供弱点、策略和订阅。`CreateWeapon_dis` 作为继承的 public 方法继续供 Hero 丢弃调用 | 基类/策略抽取 |
| `WeaponControSystem_2.cs` | 同上 | 同一基类承载，Boss2 子类提供四色与 Boss2 武器族策略 | 基类/策略抽取 |
| `sk_bo2_luodian.cs` | `End` | 改为 `ReleaseEntity` 和 GF.Entity release callback，避免 Destroy 绕过实体池 | 框架实体生命周期替代 |
| `Skill_Bo1_Atk5_b.cs` | `Start`、空 `Update` | `Skill_base.InitializeForSpawn` 调用 `ChildStart`；16 个子技能走 `SpawnTransient`，空 Update 删除 | 框架实体生命周期替代 |
| `Skill_Bo2_atk2_s.cs` | `Start` | 原 5×4 投射物生成体迁入 `ChildStart`，由 `Skill_base` 在每次实体生成时调用 | 框架实体生命周期替代 |
| `Skill_Bo2_Atk2.cs` | `SetSkill_Atk2` | 源内只有定义、无调用；正式入口一直是 `Set(Vector3)`，目标保留并改用落点 GF.Entity | 删除未接入备用入口 |
| `Skill_Bo2w_Atk.cs` | `GetW2` | 按名字 `GameObject.Find` 改为 `BindTail(SkeletonAnimation)`；由 `Boss2_Controller_w` 生成实体时显式注入 | 显式依赖替代查找 |
| `Skill_Zhadan.cs` | `Start` | 生成期逻辑改为 `ChildStart`，爆炸声通过框架 Sound 绑定；`FixedUpdate` 轨迹保留 | 框架实体/声音替代 |
| `LoadScenes.cs` | `FixedUpdate/LodOK/Fead/LoadScenesSync` | `IColorTimingSceneFlow` 的 transition started/progress/changed/failed 事件承载加载、进度和淡入淡出；所有原静态调用点已改为场景流服务 | 框架场景流替代 |
| `StartVido.cs` | `Start/Update/Startred` | `OnEnable`、`RestartSequence`、`SwitchToLoop`、`StopSequence` 明确实现池化 UI 每次打开的 intro→loop；RenderTexture 输出修复已覆盖 | GF.UI/视频生命周期替代 |
| `UI_Game.cs` | `Awake/StartFead/GOBoss2/GoStart` | 单例与静态加载改为 `IBattleResultSink`、GF.UI result form、`LoadBoss2AfterDelay`、`IColorTimingSceneFlow`；源 `GoStart` 仅来自注释的 Invoke，最终胜利返回由结果按钮负责 | 框架 UI/场景流替代 |
| `UI_SoundManager.cs` | `Awake`、`Start` | 单例 AudioSource 改为 `IUiSoundSink` + `IColorTimingSoundService`，click/hover public 入口保留 | 框架 UI 声音替代 |
| `UI_WeaponTip.cs` | `TimeOK`、`WaitForTime` | `IGameTime` lease + unscaled deadline + `IGameInput.ConsumeAnyPressForOverlay` 取代全局 timescale/coroutine；`OnDestroy` 保证释放 | 框架时间/输入替代 |

## 序列化入口专项结论

- `Baseline/unityevent-methods.csv` 中的持久 UnityEvent 方法在目标审计中均有接收器；
- 13 组 Animation Event 方法族全部有接收器；`StartSupHit` 不在源 Animation Event 清单中；
- Spine Event/Complete/End 订阅均有配对解除，缺失解除路径为 0；
- 继承后仍可调用的 `CreateWeapon_dis` 不能被“当前文件未声明”误判为删除；
- `SetSkill_Atk2`、`StartSupHit` 和 `OnTriggerStay2D` 均经源代码引用与序列化清单证明不可达或无行为。

因此，方法签名变化均属于领域抽取、框架边界替换、实体池生命周期适配、显式依赖注入或有证据的无效入口删除，没有发现尚未映射的源行为入口。
