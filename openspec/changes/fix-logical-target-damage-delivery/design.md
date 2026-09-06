## Context

`Skill_base` 目前会把 Collider 解析为其自身或父节点的 `IBattleDamageReceiver`。该兼容行为是正确的，但一次物理攻击可进入同一角色的多个 Collider；所有回调最终投递到同一个 `ActorId`，造成同一击多扣血。

## Confirmed Decisions

### 1. 攻击阶段由有效攻击事件或明确的新波次创建

一次普通攻击只有一个阶段。二段攻击的每个有效攻击事件各创建一个阶段；因此只要每次结算时弱点颜色满足，允许依次造成两次伤害。范围攻击在一个阶段内可伤害多个不同逻辑目标。

### 2. 子技能默认继承父攻击阶段

分裂、特效、多个碰撞区和同步 AoE 表现默认继承父阶段。同一阶段内对相同 `ActorId` 仅结算一次；不同 `ActorId` 独立结算。只有后续段、波次、回程或持续伤害 tick 被显式声明时，才创建新阶段。

### 3. 使用独立的伤害投递管线与声明式攻击配置

不再让 `Skill_base` 同时承担攻击编排、物理命中解析和伤害去重。新增独立的伤害投递管线：

- `AttackExecution` 表示一次完整出招；每个有效动画攻击事件创建一个新的执行实例。
- `DamagePhase` 表示该出招内的一次可独立结算的伤害段，并携带唯一运行时标识。
- `DamageDeliveryService` 是唯一允许投递 `BattleDamage` 的入口；它按“阶段 + 逻辑目标实例”判定是否首次命中。
- `Skill_base` 只解析 Collider、申请投递并触发表现；不得自己保存分散的目标去重集合。

逻辑目标身份不得使用 `ActorId`，因为多个同类型小怪可共享该枚举。每个
`IBattleDamageReceiver` 必须暴露战斗期唯一的 `DamageTargetHandle`；同一 Boss 根节点和子碰撞体
解析到同一个 Handle，不同小怪解析到不同 Handle。

攻击触发时机继续由 Animation Event 决定。建立专用的 `CombatAttackDefinition`
（ScriptableObject 配置载体，而非运行时数据库），以语义化的攻击事件键描述根阶段和生成关系。
子技能生成关系使用 `InheritParent` 或 `CreateNewPhase` 声明；默认继承。不得复用现有
`parm` 字符串，因为它已经承担武器分段、颜色和目标坐标等不兼容数据。

`Skill_base` 以及所有派生技能只通过统一的 `SpawnChild` 请求创建子效果；请求显式接受阶段策略，
避免任一新脚本绕过阶段传递。对象池回收时，阶段引用和命中状态由投递服务按执行生命周期释放。

## Acceptance Criteria

- 任一攻击阶段命中同一 Boss 的根/子 Collider 任意次数，只产生一条成功伤害投递。
- 二段攻击的两个有效动画事件产生两个阶段；若两次弱点颜色均匹配，可分别结算。
- 一个 AoE 阶段对同一 `DamageTargetHandle` 的任意多个 Collider 仅结算一次；多目标能力作为接口合同与自动化夹具覆盖，当前 Boss1/Boss2 单敌人关卡不把它列为手工玩法验收。
- 子效果默认不重复伤害父阶段已经命中的目标；标记为 `CreateNewPhase` 的波次、回程或 tick 可以再次结算。
- 命中日志包含执行 ID、阶段 ID、目标 Handle、来源技能、投递/抑制原因；对象池复用后不得继承旧阶段的命中记录。
