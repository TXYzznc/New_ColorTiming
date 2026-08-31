## Context

ColorTiming 已完成从源工程到 GF_X 的第一阶段迁移，但业务层仍有 62 个 `Legacy` 文件（约占产品脚本 56%）。`HeroController`、`Boss1_Controller`、`Boss2_Controller` 同时负责输入、物理、生命、状态、动画、声音、生成和胜负；`ColorTimingSceneInputBinder` 与 `BattlePresentationInstaller` 通过全场景 MonoBehaviour 扫描完成服务定位；UI 与表现仍直接持有旧控制器。现有纯 C# 类型（`Health`、`PlayerVitality`、`BossBattleHealth`、`PlayerWeaponInventory`、Boss 选择器等）证明规则可抽离，但还没有形成唯一状态所有者。

约束如下：

- Unity 2022.3.62f3，GF_X 启动入口仍位于历史命名为 `Hotfix` 的程序集；本次把它视为普通 Unity/GF 组合与适配程序集，不设计多热更程序集，也不修改 `Assets/Game/ScriptsBuiltin/`。
- 现有美术资源必须复用。纹理、Sprite、字体、音频、视频、材质、Shader、粒子、Spine、Animator、AnimationClip、Timeline 和可视 Prefab 是受保护输入。
- 现有 129 个 Animation Event、19 个 UnityEvent 与 Spine Event 必须保持可调用和时序等价。
- 用户选择“先完成最终设计，再一次性长任务实施”，不接受长期新旧业务双轨。
- 自动化以代码注释、事件契约、GUID/依赖/序列化清单和测试为主；截图不是阻塞项，最终画面、音效、手感由用户验收。

## Goals / Non-Goals

**Goals:**

- 以唯一 `BattleSession` 管理战斗权威状态与生命周期。
- 形成 Domain → Application → Unity/GF Adapter → Bootstrap 的单向依赖。
- 删除旧业务控制器、`Weapon` 兼容对象、`I_Damage` 和扫描式业务组合。
- 保持源功能审计中的全部可观察行为，并允许已批准的 FIX-001～FIX-004。
- 用纯 C# EditMode 测试覆盖规则，用 PlayMode 覆盖组合、物理适配、GF UI 池复用和三场景生命周期。
- 非破坏式迁移现有序列化资源和事件接线。

**Non-Goals:**

- 不增加新玩法、新数值、新美术、新输入方案或联网能力。
- 不重做 UI 视觉、动画、Spine、音频、粒子或关卡内容。
- 不设计多个 HybridCLR 热更程序集，也不改造 GF_X 的 DLL 加载器。
- 不修改 `ScriptsBuiltin`、第三方包或无关 Editor 工具。
- 不以 ECS、全局事件总线、运行时反射 DI 容器或大量 ScriptableObject 运行时状态替代当前业务。

## Decisions

### 0. 业务调参以 GF DataTable 为唯一权威源

战斗、角色、Boss、武器、技能、声音、关卡流程和 UI 表现中的可调业务参数统一进入
`GameData/DataTables/ColorTiming/`。运行时只通过只读配置仓库取得类型化配置；表中资源字段保存稳定资源名或语义 ID，
不保存 Unity Object、GUID 或场景对象引用。ScriptableObject 不再承担运行时业务数据库职责。

Prefab/Scene 继续作者化组件、骨骼、碰撞器、RectTransform、出生锚点和挂点等结构引用；这些引用不是数值配置，
也不得为追求“全部进表”而转换成脆弱的路径查找。迁移期间配置仓库对缺表、缺行、重复 ID 和非法枚举采取
启动失败策略，不静默回退到代码默认值，从而保证只有一个事实源。

### 1. 选择分层会话架构，而不是继续修补或 ECS 重写

采用：

1. `ColorTiming.Domain`：纯 C# 值对象、实体状态、规则、命令结果和领域事件。
2. `ColorTiming.Application`：`BattleSession`、用例、端口、快照、生命周期与事件分发。
3. `ColorTiming.Presentation`（现由框架既有 `Hotfix.asmdef` 编译）：Unity 物理、Animator/Spine、UGUI、摄像机、音频、瞬态实体薄适配。
4. `ColorTiming.Infrastructure.GF`（同上）：GF Scene/UI/Sound/Entity/Setting/Input 端口实现。
5. `ColorTiming.Bootstrap`（同上）：启动过程、场景 composition root、显式 scene anchors 和销毁顺序。

备选 A“继续在单一 Hotfix 中整理旧 MonoBehaviour”改动较小，但无法物理阻止 Domain 反向依赖 Unity，拒绝。备选 B“ECS/完全数据驱动重写”会扩大资源迁移和行为偏差，项目规模不需要，拒绝。

### 2. 两个普通运行时 asmdef，保留单一 Hotfix 入口

- `ColorTiming.Domain.asmdef`：`noEngineReferences: true`，不引用任何其他产品程序集。
- `ColorTiming.Application.asmdef`：`noEngineReferences: true`，只引用 `ColorTiming.Domain`。
- `Hotfix.asmdef`：引用 Domain 与 Application，继续承载 `HotfixEntry`、Unity/GF 实现和序列化脚本。
- EditMode 测试优先直接引用 Domain/Application；PlayMode 引用 Hotfix、Domain/Application 与既有 GF/Spine 依赖。

它们是普通 Player/AOT 运行时程序集，不加入 HybridCLR 热更列表。当前架构不以业务热更为目标，也不为可能永远不会发生的多程序集热更新增加 DTO、代理、跨程序集协议或加载边界；`Hotfix` 仅保留框架既有名称和启动职责。

### 3. `BattleSession` 是唯一权威状态所有者

每个 Boss 场景只创建一个 session，持有：

- `PlayerState`：生命、无敌窗口、动作状态、方向与单个手持武器槽。
- `BossState`：生命/弱点队列、阶段、死亡与 Boss 专有运行状态。
- `BattleState`：场景 ID、运行/暂停/结束、终局结果、单调 tick/command 序号。
- 注入的 `IRandomSource`、配置和值类型时钟输入；不读取 Unity 静态时间。

Application 接收 `MoveIntent`、`AttackIntent`、`DashIntent`、`PickupIntent`、`DamageCommand`、`AnimationSignal`、`PauseIntent` 等 typed command，更新 Domain，然后发布不可变 snapshot/event。一次命令同步完成状态变更；延时流程由 Application scheduler/clock token 表示，由 Bootstrap 每帧推进并在销毁时取消。

### 4. 表现适配器保留资源入口，但不保留核心状态

现有序列化脚本尽量保留 `.meta` GUID 并原位改写为新职责，减少 Prefab/Scene 重绑：

| 现有职责 | 最终职责 |
|---|---|
| `HeroController` | `PlayerActorView`：Rigidbody2D 移动、碰撞锚点、session 命令与 snapshot 渲染 |
| `HeroAnimStae` | `PlayerAnimationEventRelay`：保留 `DashWD`、`DashEnd`、`Attack`、`Hit`、`SkillMove`、`Wudi` 等事件签名 |
| `HeroFrireSystem` | `PlayerSkillEmitter`：按表现事件请求 GF Entity 并绑定只读技能 payload |
| `Boss1_Controller` / `Boss2_Controller` | `Boss1ActorView` / `Boss2ActorView`：物理/Spine/Collider 表现和 session 命令 |
| `Boss1Anim` / `Boss2Anim_s` | Boss Spine 事件适配器，负责订阅、转译与成对退订 |
| `Skill_base` / 技能子类 | 统一 `SkillHitboxView` 加少量专有表现策略；命中只提交 `DamageCommand` |
| `Pickup_Weapon` | `WeaponPickupView`：展示 `WeaponIdentity`，触发 pickup intent |
| 旧 HUD inner scripts | GF form 子视图，渲染 snapshot，不持有 Hero/Boss 控制器 |

为了保护 AnimationEvent/UnityEvent，迁移期间允许类型改名但优先保持脚本 GUID；公共事件方法名称和参数保持不变。方法内部只调用当前绑定的 adapter/session port。事件清单会明确“资产 → 组件 GUID → 方法 → 参数 → 新命令”。

### 5. 显式 `BattleSceneAnchors` 替代全场景扫描

Boss1/Boss2 场景保留一个作者化锚点组件，字段只引用场景中的 Player view、Boss view、摄像机、音频 cue anchors、生成点和关卡边界。它不是服务、状态或上下文。

GF 场景成功事件后，Bootstrap 动态创建 `BattleRuntimeContext (Clone)`：

1. 定位唯一 `BattleSceneAnchors`（只允许对该已知组件做一次直接查询）。
2. 从 anchors 构造配置、Domain 和 `BattleSession`。
3. 创建 GF 端口实现并显式调用每个 view 的 `Bind(session/port)`。
4. 打开 BattleHud/BattleTutorial GF forms，并传递 view model source。
5. 在 unload 前先停止输入与 session，再关闭 UI、释放实体/声音订阅，最后销毁 root。

不再遍历全部 MonoBehaviour 并按 consumer 接口自动注入。Launch 继续拥有全局 Screen UI roots 与 `WorldUIRoot`；Boss 场景不出现静态 UI 根。

### 6. 输入、物理、声音和生成是端口，不是领域依赖

- Input：框架输入抽象生成 `GameInputFrame`，Bootstrap 按帧转换为意图；Domain 不轮询按键。
- Physics：Rigidbody2D/Collider2D 留在 view；碰撞构造 `ActorId + WeaponIdentity + CombatPoint + HitPolicy`，不传 `GameObject`。
- Time：Unity `deltaTime/unscaledDeltaTime` 只在 composition tick 入口读取；暂停由 `BattleSession` 与 `TimeScaleCoordinator` 协调。
- Sound：用 `SoundCueId + Channel + LoopPolicy` 显式路由，取消基于 GameObject/clip 名称的分类。
- Entity：`ITransientEntityPort` 以 prefab/entity ID 与 payload 创建/释放，owner token 绑定 session；复用时统一重置并维持 `(Clone)` 后缀。

### 7. UI 使用 snapshot/view model，GF 表单生命周期不变

`BattleHudForm`、`BattleTutorialForm`、`BattleResultForm`、`PauseMenuForm`、`MainMenuForm`、`LoadingForm` 继续按 GF 标准打开关闭。Battle UI 接收 `BattleViewModel`：

- 订阅 session snapshot，按版本号刷新生命格、武器、弱点、提示与暂停状态。
- `OnOpen/OnClose` 对称订阅退订；复用时清空动态 item、图标、文本、游标和教程计数。
- Button/Toggle UnityEvent 的现有公开方法暂时作为序列化入口，内部转发 typed intent；后续可在不改美术的独立任务中统一命名。

### 8. 资源保护采用清单与结构化差异，不依赖截图

实施前生成 protected manifest：路径、GUID、文件 SHA-256、`.meta` SHA-256、Importer 关键字段、依赖 GUID、AnimationEvent、UnityEvent、Spine handler。分类规则：

- Raw art：内容、GUID、Importer 全部不可变化。
- Animator/Animation/Spine/Timeline：内容默认不可变化；通过保留方法签名避免修改。
- Prefab/Scene：允许脚本字段/GUID 接线及功能节点变化；视觉节点名、层级、RectTransform、渲染组件顺序和资源引用纳入结构化 diff。
- C# 与测试：正常重写，不属于美术保护对象。

截图只在定位具体视觉问题时按需使用。最终用户实机验收仍是视觉、音效与手感完成条件。

### 9. 注释用于解释边界与资源契约，不重复代码

公开端口、session 生命周期、Animation/Spine/UnityEvent 入口必须有 XML/summary 注释，明确调用方、所有权、是否可在 dispose 后调用。普通算法不堆叠逐行注释；复杂行为用“源行为 ID / 事件资产来源 / 不变量”说明原因。

## Dependency Direction

```text
ColorTiming.Domain
        ↑
ColorTiming.Application
        ↑
Existing Unity/GF adapter assembly: Presentation + Infrastructure
        ↑
Existing Unity/GF adapter assembly: Bootstrap / HotfixEntry
```

禁止 Domain/Application 引用 UnityEngine、GF、Spine、Cinemachine、UGUI、Resources、SceneManager、`GameObject` 或任何 Presentation 类型。架构 EditMode 测试扫描程序集引用和命名空间，发现逆向引用即失败。

## Data and Event Flow

```text
GF Input / Physics / Serialized Art Event
                ↓ typed intent/command
          BattleSession
                ↓ Domain rules
     state transition + domain events
                ↓ immutable snapshot/instruction
 Unity Actor / Animator-Spine / GF UI-Sound-Entity
```

同一帧内命令按递增序号处理；战斗进入 terminal 后只允许查询、结果 UI 和 scene-flow 命令。表现回调不得直接修改另一 view。

## Risks / Trade-offs

- [一次性替换范围大，编译中间态较长] → 按 Domain/Application、views、scene wiring、删除旧实现的内部批次实施，但只在全部契约通过后交付，不保留长期双轨。
- [普通 Domain/Application 程序集不能独立热更] → 当前产品不以业务热更为目标；保持简单、稳定的本地程序集依赖优先于预留未确认的热更能力。
- [序列化类型改名可能导致 Missing Script] → 优先原位改写并保留 `.meta` GUID；必须换脚本时通过 Unity API 重绑并运行 Missing Script/GUID 审计。
- [Animation/Spine 事件可能在特殊时序调用已销毁 session] → adapter 维护可空绑定 token，`OnDisable/OnDestroy` 对称退订，dispose 后事件安全忽略并记录一次诊断。
- [纯 Domain 与 Unity 物理存在行为偏差] → Domain 只决定规则，位移和碰撞仍由原 Rigidbody2D/Collider2D 参数驱动；PlayMode 覆盖接触到命令的适配边界。
- [资源清单过严阻止必要脚本重绑] → 清单按 Raw/Serialized 两类比较；只允许 migration table 中的脚本/功能接线差异。
- [音频语义化后遗漏原 playOnAwake] → 先建立所有场景 AudioSource cue 表，再替换名称推断；逐项验证 channel、loop、spatial 与触发时机。

## Migration Plan

1. 固化当前 Git 状态、源功能审计与 protected asset/event manifest；记录现有 dirty 文件归属，不覆盖无关修改。
2. 新建 Domain/Application asmdef、数据模型、`BattleSession`、端口与纯 C# 测试；保持现有运行路径暂时可编译。
3. 建立 `BattleSceneAnchors`、动态 composition root、GF ports 和显式 dispose；移除扫描式注入路径。
4. 按 Player → shared skill/pickup → Boss1 → Boss2 顺序原位改写表现脚本，保留脚本 GUID 和美术事件公开签名。
5. 将 GF UI forms 改为 snapshot/view model，完成池复用状态重置；将音频和 transient entity 改为显式 cue/payload。
6. 用 Unity Editor/API 更新 Boss1/Boss2 scene anchors 与必要 Prefab 序列化字段，不手工破坏 YAML；Launch UI roots 保持既有规范。
7. 运行编译、Domain EditMode、全 EditMode、目标 PlayMode、全 PlayMode、三场景生命周期、Missing Script、event manifest、asset manifest 和 framework purity。
8. 所有自动化与资源契约通过后删除 `Legacy` 旧业务类型、兼容 adapter 和无用扫描器；再次运行全量验证。
9. 更新功能 ID 映射、OpenSpec evidence 与用户人工验收清单，由用户验证画面、音效和手感。
10. 建立 ColorTiming 业务 DataTable、只读配置仓库和启动校验；依次替换 SO、代码常量和重复序列化数值，
    删除运行时业务配置双轨，同时保持场景/Prefab 结构引用和受保护美术资源不变。

### Rollback

- 不重写历史、不删除源工程；由 Git 集成窗口创建实施前检查点提交。
- protected manifest 保存重构前基线，任何资源差异可定位到 GUID/文件/事件。
- 若某一表现迁移失败，回退该脚本/Prefab/Scene 到检查点并重新做显式映射；不通过重建美术资源修复。

## Open Questions

已确认的本轮边界内无阻塞问题。是否从框架层完全移除 HybridCLR 属于独立框架决策；本次业务架构不依赖它，也不为它预留多热更程序集方案。
