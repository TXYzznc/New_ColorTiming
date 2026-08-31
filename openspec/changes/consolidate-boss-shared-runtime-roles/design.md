## Context

ColorTiming 是长期维护的小型项目，运行时已经按 Domain/Application/Presentation/Bootstrap 分层，并使用显式 `BattleSession`、GF.UI、GF.Entity 和 Scene Anchor 装配。当前问题不是缺少框架，而是迁移时在 Presentation/Bootstrap 层为 Boss1/Boss2 复制了相同角色：两套 HUD View、两个空壳 WeaponSpawner 子类和两个互斥 Anchor 字段。

本次必须保持 Unity 2022.3.62f3、GF_X 生命周期、现有 Scene/Prefab GUID 合同以及所有美术资源内容。Unity 序列化类型变更存在 Missing Script 和字段丢失风险，因此不能先删除旧脚本再人工补引用。

## Goals / Non-Goals

**Goals:**

- 共享 Boss HUD、武器生成和 Scene Boss 绑定都只有一个运行时实现与一个引用角色。
- Boss1/Boss2 的玩法状态机、攻击、资源和生成规则继续独立。
- 迁移过程可重复、可校验、可回退，不出现 Missing Script 或美术引用丢失。
- 新增 Boss 时不再修改 HUD Prefab、Player 的生成器字段或 Anchor 的具体 Boss 字段。

**Non-Goals:**

- 不处理 Boss Sound View、Animation Event Relay 和 BattleDescriptor 待决策项。
- 不合并两个 `WeaponSpawnRuleAsset`。
- 不重制、重导入或破坏性修改 Sprite、Animator、AnimationClip、Spine、AudioClip、Material。
- 不引入新包、事件总线、反射式运行时扫描或多热更程序集。

## Decisions

### 1. 共享角色使用组合与窄接口，不建立 Boss 大基类

新增 `IBossBattleSessionConsumer : IBattleSessionConsumer`，只暴露其 `BattleKind`。Boss1/Boss2 Actor 各自实现该角色，仍保留完全独立的 MonoBehaviour 和状态机。

`BattleSceneAnchors` 不再保存 `boss1/boss2`；它从显式绑定数组中校验恰好一个 `IBossBattleSessionConsumer`，并校验其 `BattleKind` 与待加载战斗一致。`BattleRuntimeContext.BindExplicit` 统一完成一次 Session 绑定。

未选择公共 Boss Actor 基类，因为两个 Actor 的状态、部件和生命周期差异明显；为了一个绑定方法建立继承树会产生错误耦合。

### 2. HUD 只保留一个 View，Boss 差异仅作为提示数据

`BattleHud.prefab` 只保留 `Slot_BossHP`，沿用当前 Boss1 Slot 的正确 RectTransform、Image、CanvasRenderer 和 HorizontalLayoutGroup。`BossHealthView` 只消费 `BattleSession.Snapshot`，不按 Boss 类型复制订阅与刷新算法。

Boss1/Boss2 现有提示节点和美术继续由 `BossWeaknessPipView` 保存；通用 View 根据当前 `BattleKind` 选择提示主题。这是数据选择，不创建第二套 Slot/View。后续若采用 BattleDescriptor，可再把映射迁到描述数据，本批不提前扩展。

### 3. WeaponSpawner 变为一个可挂载组件

`WeaponSpawnerView` 从 abstract 改为 sealed/concrete，提示主题作为 Inspector 配置保存在每个 Scene 实例中。两个 `WeaponSpawnRuleAsset` 继续提供真正不同的生成数据。

`PlayerActorView` 只持有一个 `WeaponSpawnerView`，主动和受击丢弃均只调用一次。

### 4. 使用分阶段 Unity Editor 迁移保持序列化合同

迁移分为兼容阶段和收口阶段：

1. 先让新旧类型/字段短暂共存，并编译通过。
2. 由 Editor migration 通过 Prefab/Scene API 创建通用组件、复制所有序列化字段和对象引用、修改 `BattleHudForm`/Player/Anchor 引用并保存资产。
3. 校验新引用完整且旧组件为零后，删除旧类型和兼容字段。
4. 更新迁移器为最终幂等结构，防止再次生成双 Slot/双类型。

不直接手写 Scene/Prefab YAML；只有 REST/Unity API 无法表达且已确认 fileID 合同时才使用最后手段，并必须先做静态引用校验。

### 5. 测试边界

- 纯业务规则和 `WeaponSpawnRuleAsset` 不改，不新增无价值抽象测试。
- EditMode 覆盖 Anchor 单 Boss 角色校验、BattleKind 不匹配、通用提示主题映射。
- PlayMode 覆盖 Boss1/Boss2 同一 HUD 结构、解绑重绑、WeaponSpawner 丢弃与场景退出清理。
- Prefab/Scene 静态校验覆盖旧脚本 GUID 清零、单 Slot、布局组件、美术引用和 Missing Script。

## Risks / Trade-offs

- [脚本类型删除导致 Missing Script] → 必须先执行兼容迁移并证明所有旧 GUID 引用清零，再删除旧 `.cs/.meta`。
- [多个 `FormerlySerializedAs` 遇到两个旧字段时覆盖非空值] → 不依赖自动字段重命名；由迁移器明确选择非空互斥引用并写入新字段。
- [通用 Anchor 失去 Inspector 类型约束] → 使用 `MonoBehaviour[] explicitBindings` + `IBossBattleSessionConsumer` 早期校验，错误在战斗初始化前失败。
- [Boss2 HUD 美术被 Boss1 Slot 覆盖] → 产品已确认所有 Boss 血条布局一致；保留 Boss2 提示节点/资源，只统一容器结构与算法。
- [一次修改 Scene、Prefab 和脚本回归面较大] → 分阶段迁移、独立 change、静态验证与两 Boss PlayMode 回归，并保持单独提交以便回退。

## Migration Plan

1. 建立新接口、通用 HUD/Spawner 和兼容字段，完成第一次编译。
2. 执行幂等 Editor migration，更新 `BattleHud.prefab`、Boss1/Boss2 Scene。
3. 执行引用报告，确认所有新字段有效、旧 MonoBehaviour GUID/双 Slot 均已清零。
4. 删除旧类型和兼容字段，更新迁移工具、校验器与测试。
5. 完成 Unity 编译、EditMode/PlayMode、Missing Script/Reference、OpenSpec 和框架纯度验证。

回退时按同一提交恢复脚本与 Unity 资产；美术源资产从未被修改，无需重新制作或导入。

## Open Questions

无。本批范围与不处理项已经由制作人确认；音效等候选在本批完成后讨论。
