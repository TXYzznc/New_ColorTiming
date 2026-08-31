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

## Hero 安全安装

Hero 维护三个不同状态：

- `requestedWeapon`：战斗库存已确认的真实武器。
- `readyController`：加载上下文已准备的对应候选 Controller。
- `installedWeapon`：当前 Animator 正在呈现的武器 Controller。

武器变化只更新 `requestedWeapon` 并请求安全安装。只有以下条件同时满足才安装：

- PlayerActionStateMachine 处于 Locomotion；
- Base Layer 已进入稳定 `Daiji` 状态且 `Animator.IsInTransition(0)` 为 false；
- 移动输入为零；第 1 层 xuli 权重为零；
- 没有攻击、Dash、受击、死亡或场景释放。

安装步骤固定为：设置 Controller、Rebind、恢复层权重与基础参数、设置 weaponType、触发
switchWeapon、更新 installedWeapon。异步回调只记录“已就绪”，不得直接安装。

每次武器请求带单调递增版本号；安装只接受当前最新请求。异步回调只缓存候选 Controller，
不得覆盖当前 Animator。

## 风险与回退

- 安全边界前会短暂保留旧武器表现；预加载使正常流程中该窗口仅由角色当前动作决定。
- 若设计要求移动中立即显示新武器，则需要后续采用稳定拓扑 + AnimatorOverrideController/
  Playables；本变更不引入该较大重构。
- 预加载失败不得隐式降级；失败可重试，或返回上一加载上下文。
- 保留现有完整 Hero Controller 和候选生成器，作为资源计划或安全安装实现失败时的回退基础。
