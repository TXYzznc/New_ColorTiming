# 设计

## 优化原则

- 先修复确定的生命周期浪费，再处理需要主观验收的资源参数。
- 资源优化只使用可逆的 `.meta` Importer 设置和平台覆盖，不修改源资源内容。
- 不对 SpriteRenderer 批量设置 Static，不自动合并 Spine 材质，不以破坏排序、动画或透明效果换取 Draw Call。
- 每次只修改一个可独立验证的资源组或代码问题，保留明确回退边界。

## 已确认分工

| 工作 | 执行方 | 验收方 |
|---|---|---|
| 代码和生命周期修复 | AI | AI 自动验证 + 用户功能验收 |
| Importer 与 Windows 平台覆盖 | AI | 用户视觉/听感验收 |
| 源美术和源音频内容 | 不修改 | 用户保有最终决定权 |
| 性能复测和变更记录 | AI | 用户确认结果 |

## 当前基线

| 场景 | Allocated Memory | Texture Memory | Draw Calls | SetPass |
|---|---:|---:|---:|---:|
| StartMenu | 330.9 MB | 262.1 MB | 19 | 15 |
| Boss1 | 2039.8 MB | 3568.8 MB | 82 | 60 |
| Boss2 | 2647.1 MB | 4814.0 MB | 68 | 35 |

Editor 采样受编辑器自身和 REST 服务影响，只用于同环境前后对比；最终结果还需结合 Player 构建复测。

## MainMenu 视频生命周期决策

- 每次打开或返回 MainMenu，均从头播放开场视频，再切换到循环视频，保持现有产品表现。
- MainMenu 关闭时立即停止 VideoPlayer、解除 `VideoPlayer.targetTexture` 与 `RawImage.texture` 引用并释放运行时 RenderTexture。
- GF.UI 池化表单可以保留运行时 RawImage 节点；再次打开时只重建 RenderTexture 并重新绑定。
- 若重新打开后未重播开场、循环视频无法接管或显示黑屏，则本项验收失败并整体回退。

## Hero 武器动画资源治理（A+B）

### 资源边界

- 常驻基础包只包含玩家待机、移动、Dash、受击、死亡、参数和既有状态行为；不得再直接引用武器逐帧 Animation Clip。
- 每种武器形成独立的动画包，包含攻击状态、该武器专用特效所需的最小资源和现有事件入口。武器即将生成时由其生成器请求预热；拾取、攻击和事件回调路径不得发起同步加载。
- `PlayerActorView` 保持输入、会话和 Animator 参数契约；新增适配层只负责在安全状态安装已就绪的武器动画组合。攻击、Dash、受击、死亡或场景释放期间禁止切换。
- 每项已加载资源由场景运行时上下文持有可取消租约。武器丢弃、实体回收或场景释放会归还租约；仅在没有使用者时调用 GF Resource 卸载。不得在高频 Update 中调用卸载或强制全局 UnloadUnusedAssets。

### 两阶段切换

1. **A：平行运行时组合。** 新建基础/武器 Controller 与运行时映射，保留原 Hero Controller 作为回退；通过资源依赖、状态参数、Animation Event、Spine Event、Prefab 和完整战斗回归证明等价。
2. **B：原 Controller 精简。** 只有 A 的 Player 验收通过后，才从原 Hero Controller 移除武器 Clip 依赖并把场景引用切到精简结构。每次资产编辑前记录依赖清单，失败时回退该批 Controller/Pefab 变更。

### 武器生成规则来源

- 武器生成器不再通过 Boss 类型选择硬编码策略；它只消费 `WeaponSpawnRuleAsset`。
- 每条规则显式列出可用的 `WeaponColor + WeaponType` 组合，并配置生成间隔、同屏上限和弱点保底阈值。显式条目避免请求没有对应美术动画的组合。
- 动画预热器后续也以同一条目作为资源映射键：新增 Boss、小怪或规则变体只替换配置资产，不新增运行时代码分支。

### 验收与回退

- 原始贴图、Animation Clip、Spine 数据与事件名称/时机保持不变；Controller 和 Prefab 的允许差异必须被记录并可由 Unity 序列化审计解释。
- 在 Windows Development Player 的 1920×1080 下采集 StartMenu、Boss1、Boss2 的峰值纹理内存、总分配内存、平均/低分位帧率和关键交互帧时间。
- 任意武器首次生成、拾取或攻击出现可感知卡顿、丢失状态事件或视觉差异时，停止 B 阶段并回退到 A 的已验证 Controller 组合。
