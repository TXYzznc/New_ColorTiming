## ADDED Requirements

### Requirement: 所有 Boss 共享单一音效播放实现
项目 SHALL 使用一个 `BossSoundView` 和每 Boss 一份 `BossSoundCueCatalogAsset` 播放 Boss 音效。Boss 类型差异 MUST 由 Cue 数据和语义化 Cue ID 表达，不得复制声音服务绑定、动画键映射和 Cue→AudioClip 播放算法。

#### Scenario: Boss1 与 Boss2 播放各自 Cue
- **WHEN** Boss1 或 Boss2 Relay 请求一个已配置的语义 Cue
- **THEN** 通用 View 从对应 Catalog 解析现有 AudioClip，并通过 GF.Sound 的 Boss Channel 播放一次

#### Scenario: 动画事件键映射
- **WHEN** Spine/Animation Event 传入现有动画事件字符串
- **THEN** Catalog 将其映射到对应 Cue，且未知键不播放任何音频

#### Scenario: 多部位 Boss 指定声源位置
- **WHEN** Boss2 尾部请求潜地或攻击 Cue
- **THEN** 通用 View 使用尾部提供的世界坐标播放，而不是强制使用 Boss 根节点坐标

### Requirement: Cue Catalog 在进入玩法前可验证
每个 Boss Cue Catalog SHALL 拒绝空 Cue ID、重复 Cue ID、重复的非空动画事件键和缺失 AudioClip。运行时查询 MUST 使用初始化缓存，不得在每次播放时遍历配置或制造非必要分配。

#### Scenario: 合法 Catalog 初始化
- **WHEN** Scene 组合根绑定包含完整唯一 Cue 的 Catalog
- **THEN** View 建立查询缓存并可立即接受语义 Cue 和动画事件键请求

#### Scenario: 非法 Catalog
- **WHEN** Catalog 存在空值、重复键或缺失 Clip
- **THEN** 初始化在播放前报告可定位配置错误

### Requirement: Boss 共享窄职责受击闪烁组件
项目 SHALL 使用 `BossHitFlashView` 管理 Renderer 的 `_FillPhase` 受击闪烁。Boss1/Boss2 Relay MUST 保持各自攻击事件合同，不得为共享闪烁建立 Boss 类型分支或统一 Relay。

#### Scenario: Boss1 多 Renderer 闪烁
- **WHEN** Boss1 受击
- **THEN** 一个 Hit Flash 组件同步驱动现有两个 Renderer，并在结束后复位属性

#### Scenario: Boss2 单 Renderer 闪烁
- **WHEN** Boss2 受击
- **THEN** 同一组件类型驱动现有 Renderer，空闲时不持续执行 Update

### Requirement: 迁移保持现有资源内容与引用
迁移 MUST 保持当前所有 AudioClip、Renderer、Spine、Material、Shader 和动画引用，不得修改源资源内容，不得在 Scene 中遗留旧 Sound View、Missing Script 或 Missing Reference。

#### Scenario: 迁移两个 Boss Scene
- **WHEN** Editor migration 处理 Boss1/Boss2 Scene
- **THEN** 旧组件的每个 AudioClip 都进入对应 Catalog，Renderer 进入 Hit Flash 组件，通用 View 绑定正确 Catalog

#### Scenario: 重复执行迁移
- **WHEN** 已迁移项目再次运行同一 migration
- **THEN** 不创建重复 Catalog、组件或 Cue，且所有现有引用保持不变
