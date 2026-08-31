## ADDED Requirements

### Requirement: 所有 Boss 共享单一血条结构与刷新实现
项目 SHALL 使用一个 Boss 血条 Slot 和一个 Boss Health View 显示当前 `BattleSession` 的弱点队列。Boss 类型、关卡或提示主题的差异 MUST NOT 复制血条容器、布局组件、订阅生命周期或刷新算法。

#### Scenario: Boss1 与 Boss2 显示相同结构
- **WHEN** BattleHud 分别绑定 Boss1 和 Boss2 的 BattleSession
- **THEN** 两场战斗使用同一个 `Slot_BossHP`、同一个 Health View 和相同 RectTransform/Layout 结构显示弱点项

#### Scenario: 提示美术作为数据保留
- **WHEN** 当前战斗需要显示既有 Boss 专属提示美术
- **THEN** 通用血条根据战斗数据选择对应提示主题，且不得创建第二套血条 Slot 或 View

#### Scenario: 表单关闭与重绑
- **WHEN** BattleHud 关闭、解绑或绑定新的 BattleSession
- **THEN** 通用 Health View 对称退订旧 Session、复位 Item/提示状态并只订阅一次新 Session

### Requirement: 武器生成器使用单一组件和关卡配置
项目 SHALL 使用一个可挂载的 `WeaponSpawnerView` 负责所有战斗的生成、位置选择、提示、实体生命周期和回收。关卡差异 MUST 由 `WeaponSpawnRuleAsset` 与提示主题配置提供，不得通过 Boss 专属空壳子类表达。

#### Scenario: 两个 Boss 使用同一生成器类型
- **WHEN** Boss1 Scene 和 Boss2 Scene 初始化武器生成系统
- **THEN** 两个 Scene 均挂载 `WeaponSpawnerView`，分别读取各自的 WeaponSpawnRuleAsset 和提示配置

#### Scenario: 玩家丢弃武器
- **WHEN** 玩家主动丢弃或受击强制丢弃当前武器
- **THEN** PlayerActorView 只调用一个当前 WeaponSpawnerView，且只生成一个掉落实体

#### Scenario: 场景退出
- **WHEN** 战斗场景卸载或对象池回收武器
- **THEN** 通用生成器和瞬态实体服务释放所有订阅与运行时对象，不跨场景保留 Boss 专属状态

### Requirement: 场景装配只声明一个当前 Boss 会话角色
每个战斗 Scene SHALL 在显式绑定中提供且只提供一个 `IBossBattleSessionConsumer`。Scene Anchor MUST 通过公共角色和 BattleKind 校验装配，不得保存 Boss1/Boss2 等具体类型的互斥字段。

#### Scenario: 合法战斗场景装配
- **WHEN** BattleRuntimeContext 初始化 Boss1 或 Boss2 Scene
- **THEN** Anchor 找到恰好一个 BattleKind 匹配的 Boss 会话参与者，并由统一显式绑定流程绑定一次 Session

#### Scenario: 缺失或重复 Boss 角色
- **WHEN** Scene 显式绑定中不存在 Boss 会话参与者或存在多个该角色
- **THEN** 初始化在玩法开始前失败并报告可定位的装配错误

#### Scenario: BattleKind 不匹配
- **WHEN** Scene 资源标识对应的 BattleKind 与 Boss 参与者声明不一致
- **THEN** 初始化拒绝启动该战斗，且不得静默绑定错误 Boss

### Requirement: Unity 资产迁移保持内容和引用完整
类型和结构迁移 MUST 通过可重复的 Unity Editor 迁移与校验流程完成。项目 MUST 保留现有 Sprite、Animator、AnimationClip、Spine、AudioClip、Material、RectTransform 和提示对象引用，不得在删除旧脚本后遗留 Missing Script 或 Missing Reference。

#### Scenario: 迁移 BattleHud Prefab
- **WHEN** 迁移器处理旧的双 Boss Slot Prefab
- **THEN** 它保留正确布局与全部美术引用，生成一个通用 Slot/View，并可重复执行而不新增重复节点或组件

#### Scenario: 迁移 Boss Scene
- **WHEN** 迁移器处理 Boss1/Boss2 Scene 中的旧 WeaponSpawner、Player 和 Anchor 引用
- **THEN** 所有非空引用和序列化配置被迁入通用字段，旧脚本 GUID 引用清零且 Scene 可正常加载

#### Scenario: 迁移后完整性验证
- **WHEN** 第一批重构完成
- **THEN** Unity 编译、Missing Script/Reference、Prefab/Scene 静态合同、EditMode 与两 Boss PlayMode 回归全部通过
