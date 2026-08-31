## 1. 基线与兼容实现

- [x] 1.1 记录 BattleHud、Boss1/Boss2 Scene 的旧脚本 GUID、字段与美术引用基线
- [x] 1.2 新增通用 Boss 会话角色接口并让两个 Boss Actor 实现
- [x] 1.3 新增通用 BossHealthView 与单一 WeaponSpawnerView 兼容实现
- [x] 1.4 为 Player、Anchor 和迁移工具增加可迁移的新引用边界并完成第一次 Unity 编译

## 2. Unity 资产迁移

- [x] 2.1 实现幂等 Editor migration，合并 BattleHud Boss Slot/View 并保留布局与美术引用
- [x] 2.2 迁移 Boss1/Boss2 Scene 的 WeaponSpawner、Player 和 Anchor 显式绑定
- [x] 2.3 执行迁移并保存 BattleHud Prefab、Boss1 Scene、Boss2 Scene
- [x] 2.4 生成迁移后引用报告，确认新字段有效且旧脚本/双 Slot 引用清零

## 3. 最终代码收口

- [x] 3.1 删除 Boss1/Boss2 Health View，更新 BattleHudForm 与 UI 测试为通用 View
- [x] 3.2 删除 Boss1/Boss2 WeaponSpawner 子类与 Player 双引用，更新生成规则迁移器
- [x] 3.3 删除 Anchor Boss1/Boss2 字段和 RuntimeContext 双绑定，完成公共角色校验
- [x] 3.4 更新旧迁移器与校验器，确保重复执行不会恢复错误结构

## 4. 自动化与人工验收

- [x] 4.1 运行 Unity 编译及相关 EditMode 测试并修复失败
- [x] 4.2 运行 Boss1/Boss2 HUD、武器生成和场景装配 PlayMode 测试
- [x] 4.3 校验 BattleHud/Boss Scene 无 Missing Script、Missing Reference、旧 GUID 和重复 Slot
- [x] 4.4 记录制作人可执行的两 Boss 画面与玩法验收清单

## 5. 变更验证

- [x] 5.1 更新 Docs/OpenSpec 状态和实施证据
- [x] 5.2 运行 OpenSpec strict validation、框架纯度、git diff check 和最终范围审计
