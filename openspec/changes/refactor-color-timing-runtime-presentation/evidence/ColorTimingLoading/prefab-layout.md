# ColorTiming 通用 Loading UI 布局

`Assets/Game/Prefabs/UI/ColorTiming/Game/Loading.prefab` 是独立的 GF.UI Form，
由 `UITable` 中的 `UIViews.Loading`（ID 1004、Overlay 组）打开；它不属于任何
Unity 业务场景，也不持有场景对象引用。

它的视觉树以旧项目 `LoadScene_s.prefab` 为基线重建；仅节点命名规范化，保留原有
层级、隐藏节点、绘制顺序、RectTransform 和业务美术资源。

```text
Loading                              Canvas / CanvasScaler / GraphicRaycaster / ColorTimingLoadingForm
├── Grp_Progress                     2029 × 1300，中心
│   ├── Img_Background               加载时背景图.png
│   └── Img_ProgressBar              进度条.png
│       └── Grp_Slider               (328.80347, -481)，1005.6069 × 60
│           └── Sld_Progress
│               ├── Grp_Fill                 inactive
│               │   └── Img_Fill             旧版滑块 Sprite
│               ├── Img_SliderBackground     inactive（保留旧版默认 Sprite）
│               └── Grp_Handle
│                   └── Img_Handle           旧版滑块 Sprite
└── Img_FadeOverlay                  最后绘制的全屏黑色遮罩
```

- 根节点承载 `Canvas`、`CanvasScaler`、`GraphicRaycaster` 与 UI Form；这符合 GF.UI
  预制体结构，UI Form 的打开/关闭直接管理根对象，不再额外套用 `Canvas_Loading` 子节点。
- `Grp_Fill`、`Img_Fill` 与 `Img_SliderBackground` 保持旧版的 inactive 状态，不因当前
  展示逻辑暂未使用而删除。
- `ColorTimingLoadingForm` 通过已序列化的 `progressRoot`、`progressSlider`、`fadeImage`
  字段管理该视觉树；完成时依次黑色淡入、淡出并关闭。
  所有 tween 使用不受 `Time.timeScale` 影响的时间。
- `ColorTimingRuntimePresentationMigration.RunLoadingVisualHierarchyBatch()` 是限定于
  此预制体的自动结构校验入口；不会重建/覆盖美术预制体、刷新资源规则或修改 StartMenu。
- 已保留旧版三个业务 Sprite 的 GUID 引用：背景 `f8a2a4a8b5afa774b9c90cab2a62c7a3`、
  进度条 `0302e3eca1486a041af96c28b4c2bf08`、滑块 `581414a5691b14244a714af973cbd51f`。
  内置 Slider 背景采用当前 Unity 2022.3 的等价 UGUI 内置 Sprite。
