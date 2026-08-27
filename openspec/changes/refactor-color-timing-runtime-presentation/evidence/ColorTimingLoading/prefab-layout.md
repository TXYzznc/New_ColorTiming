# ColorTiming 通用 Loading UI 布局

`Assets/Game/Prefabs/UI/ColorTiming/Game/Loading.prefab` 是独立的 GF.UI Form，
由 `UITable` 中的 `UIViews.Loading`（ID 1004、Overlay 组）打开；它不属于任何
Unity 业务场景，也不持有场景对象引用。

```text
Loading                         Canvas / CanvasScaler / GraphicRaycaster / CanvasGroup
├── Img_Fade                    全屏黑色遮罩，拦截加载期输入
└── Grp_Progress                底部居中的进度组
    └── Sld_Progress
        ├── Img_Background
        └── Grp_Fill
            └── Img_Fill
```

- Root 使用全屏拉伸锚点、`Screen Space - Overlay`、`Scale With Screen Size`
  与 1920×1080 参考分辨率；没有嵌套 Canvas。
- `ColorTimingLoadingForm` 只接收场景流进度和完成信号；完成时以不受
  `Time.timeScale` 影响的 0.2 秒淡出关闭自身。
- 进度条为只读展示，不接收导航或指针输入；遮罩在加载期间阻断输入，淡出时
  释放射线。
- 背景和控件所需美术资源尚未交付，因此使用框架内置 UI Sprite 与颜色占位；
  后续替换 Sprite 时不改变节点名、层级或脚本字段。
