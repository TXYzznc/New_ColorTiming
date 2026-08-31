# ColorTiming 重复实现与类型枚举化审计

## 1. 结论摘要

本次按制作人确认的 **A 标准** 判定：只有结构、生命周期和核心算法一致，真实差异仅为类型、枚举值、资源或配置数据时，才列为“确定错误”；证据不足的项目单列“待决策候选”。

审计结果：

- 确定错误：6 组，其中 P0 1 组、P1 2 组、P2 2 组、P3 1 组。
- 待决策候选：3 组。
- 已核实为合理独立实现：7 类。
- 邻接但不属于本审计主题的问题：1 项未引用 Prefab。

最高优先级错误是 `BattleHud` 的两套 Boss 血条。Boss2 的 Slot 缺少 Boss1 Slot 所具备的 `Image`、`CanvasRenderer` 和 `HorizontalLayoutGroup`，因此运行时生成的血量项会重叠；这不是 Boss2 的产品差异，而是迁移脚本复制 RectTransform 时遗漏组件造成的结构漂移。

## 2. 覆盖范围与方法

静态审计覆盖当前项目中的：

- `Assets/Game` 下 380 个 C# 文件：`ScriptsBuiltin` 163 个、`Scripts` 177 个、Editor 迁移工具 14 个，其余为测试及辅助代码。
- 56 个 Prefab、4 个 Scene、4 个项目 ScriptableObject 配置资产。
- `ProjectSettings`、`Docs`、`openspec` 中与运行时、工具链和既定产品合同有关的内容。
- 原项目 Boss1/Boss2 场景中的旧 `HPBox` 结构，用于确认血条的原始产品意图。

发现候选时使用文件名、相邻类型、继承关系、方法结构、序列化字段、Prefab 组件签名和 Boss 类型分支；最终结论以 Scene/Prefab GUID 引用、运行时绑定、事件入口、资源差异和测试合同为准。代码相似度只作为入口，不独立构成结论。

本次没有修改任何 C#、Scene、Prefab、ScriptableObject、美术资源或框架核心。

## 3. 确定错误

### DUP-001｜P0｜BattleHud Boss 血条被复制成两套结构与脚本（已实施）

**位置**

- `Assets/Game/Prefabs/UI/ColorTiming/Game/BattleHud.prefab`
- `Assets/Game/Scripts/ColorTiming/Presentation/UI/Forms/BattleHudForm.cs`
- `Assets/Game/Scripts/ColorTiming/Presentation/UI/Components/Boss1HealthView.cs`
- `Assets/Game/Scripts/ColorTiming/Presentation/UI/Components/Boss2HealthView.cs`
- `Assets/Game/Scripts/ColorTiming/Presentation/UI/Components/BossWeaknessPipView.cs`
- `Assets/Game/Editor/ColorTimingMigration/ColorTimingBattleHudPrefabMigration.cs`

**证据**

- `Slot_Boss1HP` 与 `Slot_Boss2HP` 的锚点、坐标和尺寸相同。
- 两个 Health View 的订阅、复位、创建 7 个 Item、刷新弱点颜色、前三次显示提示以及销毁退订算法一致；真实差异只有 `BattleKind` 校验和提示样式编号 `1/2`。
- Boss1 Slot 有 `Image`、`CanvasRenderer`、`HorizontalLayoutGroup`；Boss2 Slot 只有 RectTransform 和 View。
- `BattleHudForm` 只切换组件 `enabled`，并未切换整个节点；Boss1 背景仍会出现在 Boss2，Boss2 动态 Item 又因没有布局组件而彼此重叠。
- 迁移工具的 `SeparateBossHealthSlots` 新建 Boss2 Slot 后只复制 RectTransform，没有复制布局和图像组件。
- 原项目 Boss1/Boss2 场景各自只有一套相同的 `HPBox` 视觉结构，未表达“两个 Boss 需要两种血条布局”的需求。

**推荐边界**

- Prefab 只保留一个 `Slot_BossHP`。
- 脚本只保留一个 `BossHealthView`，直接绑定当前 `BattleSession`，不校验具体 Boss 类型。
- `BattleHudForm` 只持有一个 Boss 血条引用，不做 Boss1/Boss2 分支。
- 如果两套提示美术仍需保留，把提示样式当作 session/战斗描述数据；不要复制血条节点、View 或更新算法。
- `BossWeaknessPipView` 中 `tip1/tip2 + ShowTip(int)` 应改为一个提示出口与明确的数据选择，避免在 Item 内继续硬编码 Boss 编号。

**迁移风险与验证**

- 保留所有 Sprite、Animator、AnimationClip 与原有 RectTransform 数值，不破坏美术资源。
- 通过迁移脚本或 Unity Prefab API 改引用，避免手写 YAML 导致 GUID/fileID 丢失。
- 验证 Boss1/Boss2 初始 7 段、逐段扣除、颜色顺序、前三次提示、解绑重绑、关闭重开，以及两场景完全一致的布局效果。

### DUP-002｜P1｜武器生成器按 Boss 创建无价值子类和双引用（已实施）

**位置**

- `WeaponSpawnerView.cs`
- `Boss1WeaponSpawnerView.cs`
- `Boss2WeaponSpawnerView.cs`
- `PlayerActorView.cs`
- Boss1/Boss2 Scene 与两个 `WeaponSpawnRuleAsset`

**证据**

- 两个具体子类都只有 4 行有效代码，唯一差异是 `TutorialTipId => 1/2`。
- 生成间隔、位置选择、颜色策略、实体创建、池生命周期、受击计数和提示触发全部已在 `WeaponSpawnerView` 中统一。
- Boss 差异已经正确放入两个 `WeaponSpawnRuleAsset`；提示编号仍通过子类表达，造成 `PlayerActorView` 同时保存两个互斥字段并分别尝试调用。
- `BattleSceneAnchors.GetSupportedWeapons()` 已经按通用 `WeaponSpawnerView` 查询，证明运行时不需要 Boss 专属类型。

**推荐边界**

- 把 `WeaponSpawnerView` 变为可直接挂载的单一组件。
- `TutorialTipId` 改为明确的序列化配置或战斗描述数据；更优方案是传入提示样式/资源标识，而不是裸 `int`。
- `PlayerActorView` 只保存一个 `WeaponSpawnerView`，丢弃武器只调用一次。
- 两个 `WeaponSpawnRuleAsset` 继续独立保留：它们是同一 Schema 的合法关卡数据实例，不是重复实现。

**迁移风险与验证**

- 迁移两个 Scene 的 MonoBehaviour 引用并处理旧脚本 GUID；不得让序列化字段静默丢失。
- 验证两 Boss 的生成间隔、最大数量、可生成武器集合、位置避让、拾取、受击丢弃、提示次数与对象池回收。

### DUP-003｜P1｜战斗场景锚点按具体 Boss 类型保存互斥槽位（已实施）

**位置**

- `BattleSceneAnchors.cs`
- `BattleRuntimeContext.cs`
- Boss1/Boss2 Scene

**证据**

- `BattleSceneAnchors` 同时序列化 `Boss1ActorView boss1` 和 `Boss2ActorView boss2`，但校验明确要求两者只能存在一个。
- `Validate(bool expectBoss1)`、`Boss1?.BindBattleSession`、`Boss2?.BindBattleSession` 都是在对同一“当前 Boss 战斗参与者”角色做类型枚举。
- 新增 Boss3 时必须继续添加字段、属性、布尔校验和绑定分支；差异来自具体组件类型，而非场景组合职责。
- Boss1/Boss2 Actor 本身有真实不同状态机，应独立；错误仅在 Composition/Anchor 层把同一角色复制成类型槽位。

**推荐边界**

- Anchor 保存一个可序列化 `MonoBehaviour bossSessionConsumer`，启动时验证其实现 `IBattleSessionConsumer`；或建立可序列化的公共 Boss View 基类/窄接口适配组件。
- `BattleRuntimeContext` 对“当前 Boss 参与者”只绑定一次。
- 战斗类型与关卡顺序继续由 session/场景描述数据决定，不让 Anchor 认识所有未来 Boss 类型。

**迁移风险与验证**

- Unity 不直接序列化接口，必须使用受检的 `MonoBehaviour` 字段、公共基类或显式适配器，不能为了抽象而放弃 Inspector 校验。
- 验证 Scene 加载时缺失、重复和错误类型均能给出明确异常；Boss1/Boss2 正常建立并释放 Session。

### DUP-004｜P2｜多碰撞子节点使用两份仅父类型不同的转发脚本

**位置**

- `Skill_Bo1_Atk5_I.cs`，在 `sk_Boss1_atk5.prefab` 的多个子碰撞节点重复挂载。
- `Skill_futou_child.cs`，在 `sk_futou.prefab` 的多个子碰撞节点重复挂载。
- `Skill_Bo1_Atk5.cs` 与 `Skill_futou.cs` 中相同的 `ChildTrigger`。

**证据**

- 两个 child 脚本都在 `Start` 通过 `GetComponentInParent<T>()` 找父组件，并在 `OnTriggerEnter2D` 原样转发 Collider；真实差异只有父组件具体类型。
- 两个父技能的 `ChildTrigger` 都进行同样的 tag 判断并调用基类 `OnHit`。
- Boss1 Attack5 Prefab 需要多处重复挂载该组件，扩大未来修复的同步面。

**推荐边界**

- 建立一个通用的子碰撞转发组件，通过窄接口或 `Skill_base` 的受控公开入口转发。
- 删除两个类型专属 child relay 和父类中重复的转发方法。
- `Skill_futou` 的朝向翻转仍留在自身 `ChildStart`；这是其真实独立行为。

**迁移风险与验证**

- 保持每个 PolygonCollider2D 的几何、Trigger 状态、Layer/Tag、伤害位置和“一次命中”语义。
- 验证斧头与 Boss1 Attack5 所有子碰撞区均可命中且不重复结算。

### DUP-005｜P2｜`Skill_Nor` 是空类型标记

**位置**

- `Skill_Nor.cs`
- `sk_nor.prefab`

**证据**

- `Skill_Nor` 没有字段、方法或生命周期差异，唯一内容是已注释掉的旧实现。
- 全项目没有 `typeof/is/GetComponent<Skill_Nor>` 等依赖具体类型身份的代码。
- Prefab 只依赖从 `Skill_base` 继承的字段和行为。

**推荐边界**

- Prefab 直接挂载 `Skill_base`，删除空子类；如未来普通攻击产生真实行为，再以职责命名实现策略，而不是预留空类型。

**迁移风险与验证**

- 迁移 Prefab 脚本引用时保持 `life`、`HitFX`、`cTag`、`damageParm` 原值。
- 验证普通攻击命中、伤害负载、特效、寿命结束与实体回收。

### DUP-006｜P3｜框架 Editor 工具存在两套近同构宿主/子面板体系

**位置**

- `ScriptsBuiltin/Editor/EditorTools/UtilityToolEditorBase.cs`
- `ScriptsBuiltin/Editor/EditorTools/UtilitySubToolBase.cs`
- `ScriptsBuiltin/Editor/EditorTools/CompressTool/CompressToolEditor.cs`
- `ScriptsBuiltin/Editor/EditorTools/CompressTool/CompressToolSubPanel.cs`

**证据**

- 两套体系重复了子面板反射扫描、Toolbar 切换、ReorderableList、资源选择、拖放、文件夹展开、类型过滤、设置保存和面板进入/退出生命周期。
- 真实差异主要是选择列表存放位置、说明文本和少量 UI 样式；这些都是宿主可配置数据或扩展钩子。
- `BatchOperateTool` 已使用通用 Utility 体系，压缩工具仍维护一套早期专用体系。

**推荐边界**

- 在框架独立 change 中给 `UtilityToolEditorBase/UtilitySubToolBase` 增加选择列表存储策略、Readme 和布局钩子，让 Compress 子面板迁入通用宿主。
- 不在 ColorTiming 业务修复 change 中直接修改框架核心；先验证这是否也是上游 AI-Friendly-Project 的现状，再决定项目修复还是回馈框架仓库。

**迁移风险与验证**

- 这是 Editor-only、低运行风险但高回归面的框架改动。
- 验证所有压缩子面板的发现顺序、设置持久化、拖拽/选择、图片/动画/图集处理结果，以及 BatchOperateTool 不受影响。

## 4. 待决策候选

### CAND-001｜P2｜Boss 音效 View 合并为通用 Cue 播放器（已实施）

`Boss1SoundView` 与 `Boss2SoundView` 的绑定、字符串 Animation Event 映射、Cue→AudioClip 映射和播放算法完全一致，差异只有 Cue 集合、字符串键和 Clip 数据。按纯结构证据已接近确定错误；但现有 Boss Actor/Animation Relay 直接依赖两个强类型枚举，强类型接口能在编译期防止 Boss1 调用 Boss2 Cue。

建议决策：

- **推荐**：合并 Unity 播放组件与序列化数据结构，使用通用 `BossSoundView` + Cue Catalog；Boss 各自保留语义化 Cue 常量/适配入口。这样复用生命周期与算法，同时不把 Animation Event 字符串散落到 Actor 中。
- 保守方案：只抽取一个共享 Cue 播放内核，保留两个薄的强类型适配器。
- 不建议：继续维护两套字段和双 switch；新 Boss 会完整复制第三份。

制作人已确认并完成实施：运行时代码统一为 `BossSoundView`，Boss1/Boss2 分别使用独立 `BossSoundCueCatalogAsset` 保存创作数据，并保留各自语义化 Cue 常量。旧的两个 Sound View、Cue enum 及 Scene GUID 引用已经清零。

### CAND-002｜P3｜Boss Animation Event Relay 的命中特效抽公共组件（窄范围已实施）

两个 Relay 的完整职责不能合并：Boss1 有六种攻击、两个 MeshRenderer 和独有的攻击流程；Boss2 有头尾、潜地、目标绑定和不同事件时序，属于真实独立状态机。

二者的受击颜色闪烁已抽为 `BossHitFlashView`：复用 `_FillPhase` 表现参数，缓存 Shader Property ID 与 MaterialPropertyBlock，并在空闲时禁用 Update。两个 Relay 的攻击事件、挂点、头尾与潜地时序继续独立；瞬态实体生成仍通过既有 `ITransientEntityService`，没有增加大一统 Relay 或 Boss 类型分支。

### CAND-003｜P3｜场景流是否进一步改为 Battle/Scene Descriptor

`ColorTimingSceneId.ToResourceName()`、`BattleRuntimeContext` 的 Scene→BattleKind 映射、Boss1 胜利后进入 Boss2，以及菜单的两个显式入口都认识具体场景。当前产品只有固定的 StartMenu→Boss1→Boss2 流程，因此这些分支是可读且可验收的有限产品流程，不判定为错误。

如果近期会新增 Boss、关卡、波次或非线性选关，建议建立 `BattleDescriptor`，统一资源名、BattleKind、下一节点、提示样式和生成规则引用；如果仍保持固定两关，暂不引入额外配置层。

制作人已确认暂缓。当前只保留扩展边界，不提前增加配置层；新增 Boss、分支选关或波次需求出现时再重新评估。

## 5. 合理独立实现

以下候选经运行职责和资源差异核实，不应为了“减少文件数”强行合并：

| 类别 | 结论 | 依据 |
|---|---|---|
| `Boss1BattleLogic` / `Boss2BattleLogic` | 保留 | Boss1 是距离分区与六攻击选择；Boss2 是头尾协调、潜地状态机与不同攻击选择。 |
| `Boss1ActorView` / `Boss2ActorView` / `Boss2TailActorView` | 保留 | 角色部件、状态、攻击入口、位移和阶段生命周期真实不同。可共享基础设施，不共享状态机。 |
| Boss1/Boss2 Animation Event Relay 整体 | 保留 | 动画事件合同、攻击数量、头尾/潜地时序不同；只考虑 CAND-002 的窄组件。 |
| Boss 专属攻击 Skill/Prefab | 保留 | `Skill_Bo1_Atk5_b`、Boss2 投射/落点/潜地等拥有不同运动、递归生成和回收时序。 |
| 两个 `WeaponSpawnRuleAsset` | 保留 | 同一配置 Schema 的不同关卡数据实例，Boss1/Boss2 的间隔、数量和武器集合不同；这正是应有的数据驱动。 |
| 视觉/特效 Prefab 变体 | 保留 | `sk_jiandao`/`sk_jiandao 2`、三种 HitFX、Boss1 Attack3 变体、草丛 Prefab 等引用不同 Sprite、AnimatorController、碰撞或视觉资源。可以整理命名或采用 Prefab Variant，但不是类型枚举化代码错误。 |
| DataTable/Network/GF Helper 类型族 | 保留 | DataTable Processor 分别生成/解析不同 C# 类型和数组维度；CS/SC Packet 是协议方向合同；JSON、Sound、UI Helper 是不同第三方/框架接口适配器。相似模板代码来自静态类型和插件发现约束。 |

Editor 迁移器和校验器中显式列出 Boss1/Boss2 Scene/旧类型，是一次性迁移与回归合同，不独立判错。实施 DUP-001～003 时必须同步更新这些工具，防止旧工具再次生成错误结构。

## 6. 邻接观察项

`Assets/Game/Prefabs/Entity/ColorTiming/Weapon_Pickup.prefab` 只有 Transform 和 SpriteRenderer，项目内无 Scene、Prefab、ScriptableObject 引用。真正由 Boss1/Boss2 Scene 引用的是 `PickUPWeapon_.prefab`。前者不是本次“枚举化重复实现”的一部分，但建议在后续资源清理中确认是否为弃用占位资源；未确认前不删除。

## 7. 推荐实施批次

### 批次 1：可见错误与同链路结构收敛

实施状态：已由 `consolidate-boss-shared-runtime-roles` 完成；自动化、序列化引用与人工验收清单见该 change 的 `evidence/`。

1. DUP-001：单一 Boss 血条 Slot/View。
2. DUP-002：单一 WeaponSpawner 与 Player 引用。
3. DUP-003：单一当前 Boss 锚点/绑定角色。
4. 同步 Scene、BattleHud Prefab、迁移器、校验器、EditMode/PlayMode 测试和文档。

这三项共享 Session/Scene/Prefab 序列化边界，适合在一个业务 OpenSpec change 中整体迁移和回归。

### 批次 2：低风险技能结构清理

1. DUP-004：通用子碰撞转发。
2. DUP-005：移除空 `Skill_Nor` 类型。

该批只改技能 Prefab 与组件引用，应独立于 HUD/场景组合以便回退。

### 批次 3：待决策音效与可选公共表现

- 制作人先决定 CAND-001 的通用播放器边界。
- CAND-002 仅在能保持 Boss 动画合同独立时抽窄组件。

### 批次 4：框架核心

- DUP-006 建立独立框架 change，并先与上游框架仓库对照。
- 不与 ColorTiming 业务提交混合。

## 8. 总体验收范围

- 静态：Unity 编译零错误、Prefab/Scene 无 Missing Script/Reference、旧类型 GUID 引用清零、资源组与 UI 配置有效。
- EditMode：Boss Health、Sound Cue、WeaponSpawnPolicy、Session 绑定、Scene 描述/校验测试。
- PlayMode：Launch→StartMenu→Boss1→Boss2→结果→菜单；两 Boss HUD 完全同布局；拾取/丢弃/提示；技能所有子碰撞；退出场景后的订阅与临时实体清理。
- 人工：不修改或重制现有 Sprite、AnimationClip、AnimatorController、Spine、音频和材质；只允许移动、改名、重绑和非破坏式 Prefab 组合。
- 回退：每个批次独立提交；Prefab/Scene 迁移前保留可重复执行的校验器，禁止依赖手工逐对象修复。
