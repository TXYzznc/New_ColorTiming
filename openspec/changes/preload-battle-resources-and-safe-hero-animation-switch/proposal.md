# 变更：预加载战斗资源并安全切换 Hero 动画

## 背景

Hero 当前按武器异步加载候选 Animator Controller。资源完成回调会立即替换 Controller 并调用
`Animator.Rebind()`；当回调发生在移动、攻击、Dash 或受击中时，会清空当前 Animator 状态并
造成首帧待机或状态事件中断。

同时，资源预热仅在拾取后发起，无法将可预期的加载成本纳入场景或同场景关卡切换 Loading
流程。

## 已确认决策

- 加载单位定义为可配置的加载上下文，而非仅 Unity Scene 名称；同场景内切换关卡、波次或
  战斗配置时同样可以请求并释放对应上下文。
- Loading UI 展示场景加载、必需战斗资源预加载和运行时组合初始化的聚合进度。
- 本场景/上下文可能生成的 Hero 武器候选 Controller 必须在进入 `BattleLifecycle.Running` 前
  完成预加载；战斗中的拾取、攻击和受击路径不得发起 Controller 加载。
- Animator Controller 可在稳定的 Locomotion（待机或移动）边界安装；攻击、Dash、受击、死亡、
  技能位移、场景释放与 Animator 过渡期间禁止 `Rebind()`。移动中拾取或丢弃后必须立即更新
  对应武器表现，不再等待输入归零。
- 资源计划由配置描述；加载与释放由上下文拥有者管理，`PlayerActorView` 只消费已就绪资源。
- 玩家武器库存是业务状态唯一真相；动画 Driver 维护 requested/presented 两种表现状态。当前使用
  Mecanim Driver，后续可在不改变战斗规则的前提下替换为 Spine Driver。
- 攻击进行中忽略玩家发起的拾取/丢弃交互；有效受击仍会中断攻击并强制掉落武器，已被中断攻击
  的迟到 Animation Event 不得造成伤害或消耗武器。

## 范围

- ColorTiming 场景/关卡加载流程、Loading 进度聚合和战斗上下文资源租约。
- Hero 候选 Controller 的预加载、查询、取消和释放。
- PlayerActorView 的请求武器与已安装武器分离，以及安全安装队列。

不改变武器数值、动画帧、Animation Event、玩法规则或美术资源内容。
