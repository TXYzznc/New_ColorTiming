# Cursor 与 WeaponSpawner 高频路径优化

## 修改

### BattlePlayerInfoView

- 新增已应用 Cursor 索引与 `Texture2D` 引用缓存。
- `Update()` 仍按输入状态计算应显示的 Cursor，保证业务行为不变。
- 目标索引和纹理引用未变化时，不再重复调用 `Cursor.SetCursor`。

### WeaponSpawnerView

- 新增 `WeaponPickupView` 组件缓存。
- 仅在 `weaponT.childCount` 变化时重新扫描子节点并调用 `TryGetComponent`。
- GF.Entity 新生成或复用武器时，由生成回调立即登记 Pickup。
- 活跃颜色统计、出生点占用检测、伤害后提示隐藏和弱点武器提示均复用缓存。
- GF.Entity 回收时 `WeaponPickupView.HasWeapon` 会重置；回收对象移出 `weaponT` 后，子节点数量变化会刷新缓存。

## 收益

- 消除普通武器状态下每帧重复进入 Unity 原生 Cursor 设置接口。
- 消除 WeaponSpawner 每帧按当前武器数量执行的 `GetComponent<WeaponPickupView>()` 查询。
- 没有引入 LINQ、闭包或逐帧集合分配；复用现有 List 容量。

## 验证

- 两个修改脚本 Compile Feedback：0 errors。
- `EveryAuthoredWeaponColorExecutesAnimationEventThroughGfEntity`
  - total=1, passed=1, failed=0, duration=6.641s
- `BattleHud_IsRuntimeOwned_Unique_AndPlayerItemsUseExpectedLayout`
  - total=1, passed=1, failed=0, duration=4.966s
- 测试后 `unity_diagnose`
  - healthy=true
  - consoleErrorCount=0
  - consoleWarningCount=0
  - isCompiling=false

## 回退

- Cursor：删除 `appliedCursorIndex` / `appliedCursor` 与 `SetCursor` 的提前返回即可。
- WeaponSpawner：恢复遍历 `weaponT` 并逐项 `GetComponent<WeaponPickupView>()` 的旧实现即可。
