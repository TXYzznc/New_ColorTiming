## Context

项目是长期维护的小型游戏，目前只有两个 Boss。音效播放算法已经完全重复，而两个 Boss 的攻击数量、头尾结构、潜地流程和 Spine 事件合同真实不同。设计必须只复用稳定机制，不建立按 Boss 类型分支的大一统 Relay。

## Decisions

### 1. Catalog 是创作期配置，View 是运行时播放器

`BossSoundCueCatalogAsset` 保存稳定 Cue ID、可选动画事件键和 AudioClip。`BossSoundView` 在绑定时建立两个 Dictionary，后续播放不遍历数组、不使用 LINQ、不产生每次调用分配。Catalog 不保存任何运行时状态。

### 2. Boss 专属语义通过稳定 ID 常量保留

Boss1/Boss2 各自保留静态 Cue ID 定义，Relay 使用语义化常量调用通用 View。动画资源产生的旧字符串仅由 Catalog 的 `animationEventKey` 映射，不扩散到通用播放算法。

### 3. 受击闪烁是独立窄组件

`BossHitFlashView` 接受 Renderer 数组，缓存 Shader Property ID 与单个 MaterialPropertyBlock。空闲时禁用 Update；`Play()` 时启用并驱动现有 `_FillPhase` 往返闪烁，结束后复位并再次禁用。

### 4. Relay 保持独立

Boss1/Boss2 Relay 只移除音效播放器重复和闪烁算法；技能事件 switch、挂点、目标参数、头尾/潜地逻辑继续保留。禁止新增 Boss 类型分支或公共 Relay 基类。

### 5. Unity 序列化通过迁移器保护

迁移器在旧组件仍可反射读取时创建/更新两个 Catalog，添加通用组件并复制 AudioClip/Renderer 引用。验证新配置后才删除旧组件和类型。迁移器最终保留为幂等校验/修复入口。

## Non-Goals

- 不合并 Animation Event Relay、Boss Actor 或战斗状态机。
- 不引入 BattleDescriptor/SceneDescriptor。
- 不增加随机 Cue、音量曲线、延迟、冷却等当前不存在的音频需求。
- 不修改音频、美术、动画、材质或 Shader 内容。

## Risks / Trade-offs

- Cue ID 字符串配置错误：初始化校验空值、重复值和缺失 AudioClip，EditMode 覆盖全部既有 Cue。
- 删除旧组件造成引用丢失：先执行迁移并生成 GUID/对象引用报告，再删除旧类型。
- Hit Flash 行为漂移：沿用现有时长、速度与 `_FillPhase` 属性，只改变状态所有者和分配方式。
- Boss2 尾部声源位置：通用 View 支持显式世界坐标，尾部调用使用尾部 Transform 位置。
