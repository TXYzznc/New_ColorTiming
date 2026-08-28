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
