# Boss1 运行时内存与 SpriteAtlas 试点记录

日期：2026-08-28

## 运行时快照

在 Boss1 六套攻击 PlayMode 测试约 20 秒处采样：

- Unity 统计总分配约 `1917.64 MB`。
- 已加载纹理 `3513` 个，纹理估算总量约 `3452.07 MB`。
- `Texture2D` 数量 `3475`，`Sprite` 数量 `2714`。
- Hero 全武器逐帧动画由一个 AnimatorController 直接引用，是当前最大的资源常驻风险。

对源 PNG 的透明包围盒只读分析显示，锤子攻击等序列存在大量透明留白；这说明资源组织仍有
优化空间，但不能据此直接修改或覆盖原始 PNG。

## SpriteAtlas 试点与崩溃

曾创建一个仅引用锤子序列、启用 Tight Packing 的临时 SpriteAtlas，并在 Unity 2022.3.62f3c1
中调用编辑器打包。Unity 在原生纹理压缩路径崩溃，调用栈集中在：

`GetRowSize -> CompressSingleImageTexture -> CompressTextureWithMultipleImages -> GenerateAtlasTextures -> PackAtlases`

这是 Unity 编辑器的原生 SpriteAtlas 打包崩溃，不是游戏业务异常。崩溃发生在生成试点图集时，
没有修改任何源 PNG、AnimationClip 或 AnimatorController。

## 回退结果与后续约束

- 临时 SpriteAtlas、生成目录的 `.meta`、试点构建代码及临时自动启动服务脚本已全部删除。
- Hero AnimatorController 与原项目 SHA-256 完全一致。
- `Assets/Game/Sprites/ColorTiming` 下没有 Git 可见的美术资源改动。
- 不再使用当前 Unity 2022 原生 SpriteAtlas 打包路径继续本项优化。
- 后续优先设计“按当前武器加载/释放逐帧动画资源”的生命周期方案，在不破坏美术源文件的前提下
  解决单一 AnimatorController 引用全部武器资源的问题，并单独完成方案、回退和运行验证。
