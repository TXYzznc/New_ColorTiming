## Context

源项目 `D:\unity\UnityProject\ColorTimeing\ColorTimeing` 使用 Unity `2022.3.62f3c1`，包含 `StartMenu`、`Boss1`、`Boss2` 三个场景、64 个自有运行时脚本、48 个 Prefab、27 个 Animator Controller、148 个 Animation Clip、Spine 3.8 运行时以及大量图片、音频和视频资源。当前实现能够编译，但业务状态、输入读取、场景跳转、UI、音频、动画事件和 Boss 行为高度耦合在 MonoBehaviour 与场景序列化引用中。

目标项目以 AI-Friendly-Project 提交 `3d67f22bd20c762329978e9c8fff0a6a74ec8559` 为框架来源，并已在目标仓库提交迁移基线 `0cfa53b`。框架提供 `LaunchProcedure → 资源/更新/热更 → PreloadProcedure → FrameworkReadyProcedure → IFrameworkStartupProcedure` 启动链，以及 UnityGameFramework 的 UI、Entity、Sound、DataTable、Config、Language、资源和场景能力。产品代码不得进入 `Assets/Game/ScriptsBuiltin/`。

约束如下：源项目保持不动；目标项目必须保留所有玩家可见功能；明确缺陷可以修复但必须记录；空脚本、未引用原型和测试热键可以在引用审计后移除；渲染管线统一迁移到 URP；完成声明需要资源与引用对账、零编译错误、EditMode、PlayMode 和三个场景的完整人工回归证据。

## Goals / Non-Goals

**Goals:**

- 在 AI-Friendly-Project 的标准启动、资源、UI、实体、声音和异步约定上重建 ColorTiming 的全部功能。
- 保持菜单、关卡、玩家、七类武器、技能、Boss1、Boss2、HUD、教程、音视频、世界交互和相机的行为等价。
- 将可独立验证的游戏规则从 MonoBehaviour、Animator 与 Spine 表现中分离为纯 C# 领域模型和显式状态机。
- 建立语义化输入、明确的组合根、可取消异步和对称生命周期，消除业务代码中的隐式全局访问。
- 以追踪矩阵证明每个源功能、资源、序列化引用和 Animation Event 都有迁移去向与验证证据。

**Non-Goals:**

- 不重新设计关卡、美术、玩法节奏、数值平衡或玩家操作。
- 不升级 Spine 数据到 4.x，也不要求重新从 Spine Editor 导出源数据。
- 不维护 Built-in 与 URP 双渲染分支。
- 不修改框架核心来容纳产品特例；若需要扩展，优先在产品层通过接口、适配器或派生类型实现。
- 不引入新的 DI 容器、输入框架或全局事件总线来替代框架和标准 C# 已能满足的能力。

## Decisions

### 1. 旁路迁移与不可变源基线

所有实现发生在 `D:\unity\UnityProject\ColorTimeing\New\_ColorTiming`。源项目只用于读取、运行对照和生成审计清单，不执行写入。迁移按可提交阶段推进，每个阶段保留可编译或可审计的回退点。

选择旁路迁移而不是原地重构，是因为框架 ProjectSettings、Packages、启动场景、资源规则和渲染管线均与源项目不同；原地替换会让功能回归失去可信对照，也会扩大回滚范围。

### 2. 产品层目录与程序集边界

产品运行时代码放在 `Assets/Game/Scripts/ColorTiming/`，按以下模块组织：

- `Bootstrap`：组合根、启动 Procedure、场景流和框架桥接。
- `Input`：`IGameInput` 语义契约与 Unity Legacy Input 适配器。
- `Combat`：颜色、武器、伤害、生命、弱点队列、战斗结果和时间效果等纯 C# 规则。
- `Player`：玩家状态机、武器、技能、投射物和动画事件适配。
- `Bosses`：Boss1/Boss2 的状态机、行为上下文与 Spine 适配。
- `UI`：框架 UIForm、Presenter、HUD、菜单、教程、暂停和结果。
- `Presentation`：声音、视频、特效、世界交互和相机。

产品代码编入现有 `Hotfix` 程序集；纯领域测试放入独立 EditMode 测试程序集。`ScriptsBuiltin` 保持框架纯净。相比把旧脚本直接复制到框架核心，此结构能保留框架升级边界，并使依赖方向可审计。

### 3. 标准启动链与显式组合根

新增 `ColorTimingStartupProcedure : ProcedureBase, IFrameworkStartupProcedure` 作为产品入口和长期应用 Procedure。它由 `FrameworkReadyProcedure` 发现，创建项目级组合根，注册显式服务引用，并通过 GF.Scene、框架加载事件与 Builtin loading view 进入 `StartMenu` 及处理后续产品场景切换。

现有 `ChangeSceneProcedure` 在加载完成后保持为当前 Procedure，未提供返回产品 Procedure 的通用续接机制，因此只适合一次性终端加载，不适合作为三场景长期导航控制器。产品 Procedure 将复用其资源路径、声音/Entity 清理、卸载、进度与失败处理约定，但不修改框架核心来加入 ColorTiming 专用续接。加载成功后，Procedure 通过已知场景根契约注入组合根上下文，不使用全局 `Find` 或静态 Service Locator。

组合根使用构造参数、序列化引用和窄接口连接已知依赖；跨系统通知使用 C# 事件，只有真正跨模块且需要框架生命周期的广播才使用 Game Framework Event。不会新增 Service Locator 或静态单例替代当前 `GameManager`。

### 4. 语义输入边界保持现有手感

定义 `IGameInput`，暴露 Move、Dash、AttackPressed/AttackHeld、Drop、Pause、PointerScreen/World、AnyKey 和 Confirm 等语义。第一阶段适配器使用框架已配置且与源项目轴定义一致的 Legacy Input Manager，以减少行为偏差；除适配器外禁止直接调用 `UnityEngine.Input`。

保留按下、持续、释放的帧语义和暂停时的 UI 输入；指针世界坐标由显式相机适配器转换。源项目已安装但业务未使用的新 Input System 不作为迁移依赖。相比立即改用 Input System，此决定更能满足操作等价，未来仍可在不改业务层的情况下替换适配器。

### 5. 代码状态机拥有规则，Animator/Spine 拥有表现

玩家、Boss1 和 Boss2 使用纯 C# 状态或层级状态机拥有阶段、转移条件、无敌窗口、攻击选择与结果规则。Animator 和 Spine 继续负责动画播放、事件时间点和视觉混合，但不得成为唯一业务真相。

既有 Animation Event 方法名（包括历史拼写如 `PlayAuido`、`Cerate`）通过薄适配器保留，以避免修改 148 个动画资源造成遗漏；适配器将事件转发给领域/应用服务。Animator 参数 `moveSpeed`、`moveV`、`weaponType`、`switchWeapon`、`Dash`、`Atk`、`Atk_x`、`Hit`、`Death` 保持兼容。

### 6. 框架服务承载 Unity 对象生命周期

- 菜单、HUD、设置、暂停、教程、加载和结果界面迁移为 GF.UI Form/Presenter。
- 武器、投射物、技能和瞬态特效接入 GF.Entity 或对象池；回收时必须清理事件、计时器和异步任务。
- BGM、UI、玩家、Boss 和环境声音通过 GF.Sound 分组与配置播放，不由业务脚本直接管理 AudioSource 生命周期。
- 场景、UI、Entity、Sound 与配置条目使用框架 DataTable/Config 和资源规则，不在业务代码中拼接 AssetDatabase 或 Resources 路径。

静态场景对象可以继续作为场景组成部分；不会为了“使用框架”而强制把所有对象变成 Entity。只有确实需要生成、隐藏、回收或统一生命周期的对象进入 Entity/对象池。

### 7. 数据与运行时状态分离

武器类型/颜色、Boss 生命段分布、场景流、UI、Sound 和 Entity 标识进入 DataTable 或静态配置；Unity 对象引用可用项目层 ScriptableObject 配置。当前生命、弱点队列、手持武器、状态机状态和战斗结果只存在于运行时上下文，不写回资产。

Boss1 弱点队列固定为 11 段（红 4、绿 3、紫 4），Boss2 固定为 15 段（红 4、绿 4、紫 4、橙 3），每场战斗按现有规则洗牌。数据化不能改变概率、数量或关卡可用武器集合。

### 8. 异步与时间语义

框架与产品层异步优先使用 UniTask，并绑定场景、UIForm、Entity 或组件生命周期的 CancellationToken。订阅必须在退出/禁用/回收时对称注销。加载和教程等需要在暂停时继续的流程使用实时/不受 `timeScale` 影响的等待；战斗计时继续遵循游戏时间。

Dash 命中后的慢动作保持 `timeScale = 0.45` 和恢复语义，但由专用时间效果服务仲裁，避免暂停、死亡和慢动作互相覆盖后无法恢复。

### 9. URP 与 Spine 3.8 兼容门槛

保留框架 URP 14.0.12、线性色彩和质量档配置。Spine runtime 保持 3.8 数据兼容；在引入任何模块前固定来源、版本、许可证和文件哈希。迁移当前 8 个 `Spine/Skeleton` 与 3 个 `Spine/Skeleton Fill` Boss 材质到对应 URP Shader，并逐一比较纹理、PMA 混合、遮罩、填充、顶点色、层级和相机输出。

若兼容模块无法在 Unity 2022.3 + URP 14 下达到视觉等价，停止渲染阶段并保留可运行前一提交，不以粉色材质、Fallback Shader 或隐藏效果作为“完成”。

### 10. 明确缺陷与废弃代码处理

只在以下证据成立时移除脚本：类为空或仅为测试、没有场景/Prefab/Animator/代码引用、且追踪矩阵记录搜索证据。明确缺陷必须同时具备源代码证据、预期不变量和回归测试，例如 Boss2 橙色生命段使用错误索引。所有修复单列在本变更的 `evidence/behavior-fixes.md`，不得夹带玩法调整。

### 11. 验收是迁移产物的一部分

建立机器可读资产/引用清单和人工功能追踪矩阵。每个能力规格映射到实现文件、源资源、EditMode/PlayMode 测试或人工步骤。零编译错误只是一项门槛，不能替代三个场景从启动到结束的完整回归。

## Risks / Trade-offs

- [Spine 3.8 很旧且当前未安装 URP 模块] → 在迁移美术前建立独立兼容性样板，只通过固定版本和 11 个材质逐项对比后再批量应用。
- [约 2928 个 PNG 和大量序列化引用使人工复制易遗漏] → 保留 `.meta` GUID，先生成源/目标 manifest，再移动资源；每阶段运行 GUID、缺失脚本和引用审计。
- [重写状态机可能改变动画时序] → 保留 Animation Event 名称和动画资源，以录制的事件/状态追踪与人工手感回归对照纯逻辑转移。
- [GF.Entity/UI/Sound 生命周期与旧场景对象不同] → 先用薄适配器接通功能，再逐类迁移生命周期；回收与取消行为必须有 PlayMode 测试。
- [URP 线性色彩与源项目 Built-in/Gamma 外观不同] → 使用同一相机位置、分辨率和场景检查点进行截图对比，显式记录允许的管线差异，不能静默接受明显偏差。
- [源项目存在隐式 Bug，行为等价与修复可能冲突] → 默认保留玩家可见行为；只有“证据明确缺陷”走单独修复记录和测试。
- [大规模一次性迁移导致问题难定位] → 以基线、资源导入、启动流、领域、玩家、Boss、UI/媒体、URP、验收为独立提交阶段，每阶段通过门槛后继续。
- [没有可用 Unity MCP] → 文件与代码阶段使用静态工具；需要场景/Prefab 序列化写入和运行验证时使用 Unity Editor/批处理模式，并保存日志和测试报告。

## Migration Plan

1. 固化源项目 manifest、功能清单、序列化引用、Animation Event、按钮绑定、包和 ProjectSettings 差异；验证源项目基线零编译错误。
2. 在目标框架基线上创建产品目录、程序集/测试边界、组合根和长期启动/场景流 Procedure；保持框架 Launch 为唯一启动入口。
3. 按 GUID 安全策略迁移场景、Prefab、动画、Spine、图片、音频、视频和字体，合并 Tag/Layer、物理、分辨率和输入设置；不覆盖框架核心 folder meta。
4. 先实现输入、战斗领域、弱点队列、生命和状态机测试，再接入玩家、武器、技能和实体生命周期。
5. 分别迁移 Boss1、Boss2 与 Spine 事件桥，逐 Boss 建立行为/生命段/攻击回归。
6. 迁移 GF.UI、GF.Sound、视频、世界交互、Cinemachine 和加载表现，恢复所有持久设置和按钮绑定。
7. 引入固定的 Spine 3.8 URP Shader 模块，迁移 11 个 Boss 材质，完成逐材质和逐场景视觉门槛。
8. 运行资源/引用对账、Unity 编译、EditMode、PlayMode 与三场景人工回归；只在追踪矩阵所有项都有直接证据后归档 OpenSpec。

回滚策略：每个阶段使用独立提交；任何阶段失败时回到最近通过验证的提交，并保留失败日志。源项目始终保持可运行，因此不会以覆盖源项目的方式回滚。

## Open Questions

- 在渲染阶段开始前，需要从可审计来源确定 Spine 3.8 URP Shader 模块的精确版本、许可证和哈希；如果无法获得，渲染阶段保持阻塞而不是自行升级 Spine 数据格式。
- 人工视觉证据的最终截图基准将在源项目运行时生成，并与目标项目相同分辨率和相机检查点配对；检查点命名在验证清单中固定。
