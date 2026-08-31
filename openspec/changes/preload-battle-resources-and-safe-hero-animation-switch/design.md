# 设计

## 加载上下文

`BattleLoadContext` 是一次内容加载请求，包含唯一上下文 ID、由关卡配置导出的武器 Controller
需求及完成/失败状态。它不假设一次上下文等于一次 Unity Scene。

首批实现直接复用 `WeaponSpawnRuleAsset` 的武器集合；`BattleSceneAnchors` 将场景内 Spawner
配置聚合为 `BattleLoadContext`，因此 Boss、关卡与波次不会在 Bootstrap 中硬编码。后续若需要
音频、VFX 等额外资源，可扩展该上下文而不改变场景流程。

上下文拥有所有 GF Resource 加载句柄。新上下文替代旧上下文或场景卸载时，取消未完成请求，
并对没有其它租约的资源执行对称释放。

## Loading 流程

1. `ChangeSceneProcedure` 创建目标 `BattleLoadContext`，显示 Loading UI。
2. 场景加载与资源计划预加载并行开始；UI 显示聚合进度：场景 55%、资源 35%、组合 10%。
3. 两项均成功后，组合根绑定预加载资源；之后才允许 BattleSession 进入 `Running`。
4. 必需资源失败时保持 Loading，提供重试或返回；不得以运行时懒加载继续进入战斗。

同场景关卡切换跳过场景 55% 阶段，通过 `BattleRuntimeContext.TryPrepareResourceContext` 使用
同一 `BattleLoadContext` 契约；调用方接收 0–1 的资源进度并只在完成回调中激活新关卡。

## Hero 动画表现边界

Hero 维护三个不同状态：

- `requestedWeapon`：战斗库存已确认的真实武器。
- `readyController`：加载上下文已准备的对应候选 Controller。
- `presentedWeapon`：当前动画渲染器正在呈现的武器。

`PlayerWeaponInventory` 是武器业务状态的唯一真相；其 Changed 事件只更新 `requestedWeapon` 并请求
表现同步。`IPlayerAnimationDriver` 隔离业务层与具体动画技术，当前
`MecanimPlayerAnimationDriver` 管理 Controller、参数和层权重，未来 Spine 实现遵守同一语义接口。

Mecanim Controller 只有以下条件同时满足才安装：

- PlayerActionStateMachine 处于 Locomotion；
- Base Layer 已进入稳定 `Daiji` 或 `Move` 状态且 `Animator.IsInTransition(0)` 为 false；
- 没有攻击、Dash、受击、死亡或场景释放。

安装步骤固定为：设置 Controller、Rebind、恢复移动/攻击参数、按 `presentedWeapon` 重算层权重、
设置 weaponType、触发 switchWeapon、更新 presentedWeapon。移动输入不得阻塞安装。
异步回调只记录“已就绪”，不得直接安装。

每次武器请求带单调递增版本号；安装只接受当前最新请求。异步回调只缓存候选 Controller，
不得覆盖当前 Animator。

## 交互与攻击一致性

- Locomotion 中允许移动、拾取、丢弃和武器切换并立即更新表现。
- Attacking 中拒绝玩家发起的拾取/丢弃交互，不排队、不延迟执行。
- 受击是强制规则：即使处于 Attacking，也立即切换为 HitStun、清空库存并请求空手表现。
- Animation Event 只有在领域状态仍为 Attacking 时才能发射攻击；攻击退出只有在本次攻击未被
  受击中断时才能消耗一次性武器。

## 风险与回退

- 攻击、Dash 或受击期间会短暂保留旧武器表现；领域武器仍即时生效，回到 Locomotion 后同步。
- 移动中的 Controller Rebind 会恢复移动参数和层权重；若未来资源结构统一，可再评估
  AnimatorOverrideController/Playables，但当前保留分武器 Controller 以控制常驻内存。
- 预加载失败不得隐式降级；失败可重试，或返回上一加载上下文。
- 保留现有完整 Hero Controller 和候选生成器，作为资源计划或安全安装实现失败时的回退基础。
