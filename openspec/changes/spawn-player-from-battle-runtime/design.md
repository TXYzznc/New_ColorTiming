## Context

当前两个静态 Hero 的根组件和本地结构大体一致，但 Boss2 包含后期 Knife/Axe/Airplane 挂点与技能 Prefab；两关的 `PlayerSoundView` AudioClip 集合及 `PlayerCameraLifecycleView` 参数不同。Player 还持有 WeaponSpawner、DeathShow、Cinemachine 和 Boss Transform 等场景引用。

## Decisions

### 1. 一个完整 Prefab，关卡差异由 Profile 表达

以能力完整的 Boss2 Hero 为 Prefab 结构基线，同时保留所有既有本地引用。Boss1/Boss2 音效集合和镜头参数迁入两个 `PlayerSceneProfileAsset`，运行时覆盖 Prefab 的场景差异，避免维护两个 Player Prefab 变体。

### 2. Manager 是 BattleRuntimeContext 的运行时所有者对象

`BattlePlayerManager` 是普通 C# 生命周期对象，不是全局单例或静态场景 GameObject。它只负责 Instantiate、场景依赖配置、本地 binding 枚举和 Dispose。角色玩法、输入和战斗状态仍由现有 Player View 与 BattleSession 负责。

### 3. Player 不接入瞬态 GF.Entity 池

Player 每场只创建一次，生命周期与战斗 Scene 一致，不存在高频复用收益。使用直接 Prefab Instantiate，并由 `BattleRuntimeContext.OnDestroy` 对称释放；技能、武器和特效继续通过 GF.Entity。

### 4. 场景只声明显式锚点

`BattleSceneAnchors` 保存 Player Prefab、Profile、出生位、WeaponSpawner、VirtualCamera、Boss Target 和 DeathSequence。禁止运行时 `Find`；Manager 创建 Player 后立即完成一次性注入，再由 BattleRuntimeContext 绑定接口。

### 5. 创建后名称保留 Unity Clone 后缀

Prefab 根命名为 `Player`，运行时实例保持 `Player(Clone)`，不覆盖 Unity 自动名称，符合项目动态对象命名合同。

## Lifecycle

1. CompositionRoot 创建 `BattleRuntimeContext (Clone)`。
2. Anchors 校验静态关卡配置。
3. BattlePlayerManager 创建并配置 `Player(Clone)`。
4. RuntimeContext 预载该关卡需要的武器 Animator Controller。
5. 创建 BattleSession，依次绑定场景 bindings 与 Player Prefab 内 bindings。
6. Scene 卸载或 Context 销毁时，Manager 清理相机/死亡表现引用并销毁 Player。

## Non-Goals

- 不动态生成 Boss、Camera、WeaponSpawner 或静态关卡几何。
- 不改变玩家规则、动画事件、碰撞、美术或音频内容。
- 不引入全局角色注册表、多角色选择或角色池。
