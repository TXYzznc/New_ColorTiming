## Why

当前迁移实现把相同的 Boss 血条、武器生成器和战斗场景 Boss 绑定角色按 Boss1/Boss2 复制，已经造成 Boss2 HUD 结构缺失、互斥引用扩散和新增 Boss 时必须修改组合根。需要在不破坏既有美术资源与玩法差异的前提下，把这三类共享运行时角色收敛为单一实现。

## What Changes

- 将 `BattleHud` 的 Boss1/Boss2 双 Slot 与双 Health View 合并为一个 Boss 血条 Slot/View，复用原版布局和全部提示美术。
- 将 Boss1/Boss2 武器生成器空壳子类合并为一个可配置 `WeaponSpawnerView`，玩家只保存一个当前生成器引用。
- 将 `BattleSceneAnchors` 的 Boss1/Boss2 互斥字段合并为一个受校验的当前 Boss 会话参与者引用，运行时只绑定一次。
- 同步更新 Boss1/Boss2 Scene、BattleHud Prefab、迁移工具、校验器、测试和稳定文档。
- **BREAKING**：移除 `Boss1HealthView`、`Boss2HealthView`、`Boss1WeaponSpawnerView`、`Boss2WeaponSpawnerView` 及其序列化类型合同；所有 Unity 资产引用必须在同一迁移中完成替换。
- 不合并 Boss Actor、Boss 战斗状态机、动画事件 Relay、攻击 Skill 或两份 `WeaponSpawnRuleAsset`。

## Capabilities

### New Capabilities

- `color-timing-shared-battle-roles`: 规定所有 Boss 共享一套 HUD 血条、武器生成生命周期和场景当前 Boss 绑定角色，同时保留配置数据与 Boss 玩法策略差异。

### Modified Capabilities

无。

## Impact

- 业务代码：ColorTiming UI、武器生成、玩家表现、场景装配和运行时上下文。
- Unity 资产：`BattleHud.prefab`、Boss1/Boss2 Scene 及相关脚本 `.meta` 引用。
- 工具与测试：ColorTiming 迁移器、校验器、EditMode/PlayMode 回归。
- 美术资产：只复用、重绑或重组，不修改 Sprite、Animator、AnimationClip、Spine、AudioClip、Material 的内容。
