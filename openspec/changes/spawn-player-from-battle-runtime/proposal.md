## Why

Boss1、Boss2 场景各自静态保存完整 Hero，导致角色结构、武器能力和场景引用重复，并让玩家生命周期依赖场景序列化。需要把玩家改为通用 Prefab，由动态战斗上下文统一创建、注入依赖和释放，同时原样保留两关不同的音效与镜头表现。

## What Changes

- 创建完整 `Player.prefab`，保留现有角色美术、Animator、碰撞、挂点、音效和七武器引用。
- 新增由 `BattleRuntimeContext` 持有的 `BattlePlayerManager`，每场创建一个 `Player(Clone)` 并负责释放。
- 新增 Player Scene Profile，保存 Boss1/Boss2 既有音效集合和镜头调节参数。
- `BattleSceneAnchors` 改为声明玩家 Prefab、Profile、出生位置及场景侧依赖，不再引用静态 `PlayerActorView`。
- 动态配置 WeaponSpawner、DeathShow、Cinemachine Follow、Boss Target，并绑定 Player Prefab 内所有依赖消费者。
- 从 Boss1/Boss2 场景删除静态 Hero，清除其他场景对象对其 Transform/Component 的引用。

## Impact

- 运行时：战斗初始化顺序、玩家创建/销毁与依赖绑定。
- 资产：新增一个 Player Prefab、两个 Player Scene Profile，修改 Boss1/Boss2 Scene。
- 测试：场景合同、玩家运行时生命周期、两关完整 PlayMode 回归。
- 美术：只迁移和复用引用，不修改 Sprite、动画、音频、材质或其他源资产。
