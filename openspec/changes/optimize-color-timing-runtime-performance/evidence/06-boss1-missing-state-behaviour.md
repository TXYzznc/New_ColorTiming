# Boss1 Missing Script 根因与修复证据

日期：2026-08-28

## 根因

Boss1 与 Boss2 场景静态扫描均未发现缺失组件。运行 Boss1 时出现的 `175` 条
`The referenced script on this Behaviour is missing!` 来自玩家 AnimatorController，
并非 Boss 场景对象。

`Hero Animator Controller.controller` 序列化了大量 `StateMachineBehaviour`，共同引用
GUID `a248c9c6fd6d4ee45949c4a43ad8e435`。该 GUID 在原项目对应
`EnterAnimStateEvent.cs`，迁移时脚本被遗漏；当前 `PlayerActorView` 仍依赖状态进入、退出通知
驱动冲刺、攻击和武器表现，因此不能简单删除这些行为。

## 修复

- 新增 `Presentation/Actors/Player/EnterAnimStateEvent.cs`，保留原类型名和原 GUID。
- 适配器从 Animator 父级缓存 `PlayerActorView`，转发状态进入与退出通知。
- `PlayerActorView` 新增内部通知入口，继续使用现有 `OnAnimStateEnter` 事件链。
- 强制重新导入 Hero AnimatorController，使 Unity Library 缓存重新解析该脚本引用。

## 验证

- 修复前：Boss1 运行日志中 Missing Script 警告 `175` 条。
- 修复后：同一 PlayMode 测试运行日志中 Missing Script 警告 `0` 条。
- 测试：
  `ColorTiming.Tests.PlayMode.BossAttackExecutionPlayModeTests.Boss1_AllSixAttacksPlayAndDispatchTheirAuthoredSpineEvents`
  结果 `1/1 Passed`。
- 当前与原项目 Hero AnimatorController 的 SHA-256 均为
  `25E14874B7218E11D69E103685649307919FD439D747C1A981E843708EC99B7E`，确认没有修改美术动画资产。

## 已知原始数据问题

控制器包含两个未被任何 AnimatorState 引用的孤立 Transition 子资源，它们的目标状态
fileID `3973208461528983912` 已不存在。重新导入时 Unity 会记录两条 Broken PPtr 导入信息，
但该数据同样存在于原项目，且不产生新的运行时 Missing Script 警告。为保护原始动画资产，
本轮记录问题但不清理控制器序列化数据。
