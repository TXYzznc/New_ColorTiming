# Prefab 与 RectTransform 规范

## 结构原则

- 每个独立 GF 页面一个 UIForm Prefab；根节点直接持有该页视觉结构。
- 只在两个以上页面复用、具备稳定职责与生命周期的组件才拆子 Prefab，并记录复用者。
- 根节点不保留停用的旧结构、第二套视觉节点或运行时生成兜底；迁移完成后删除旧路径。
- 动态列表使用已存在且默认 inactive 的 `Item_*Template`，由所属 Form 管理对象池。

## 命名与层级

Prefab、脚本和公开类型用 PascalCase；节点采用 `前缀_语义`，同一 Form 内唯一且不含本地化文字、路径分隔符或无意义序号。

`Bg_` 背景、`Overlay_` 模态根、`Panel_` 可视容器、`Grp_` 无视觉分组、`Txt_` TMP 文本、`Img_` 图片、`Icon_` 图标、`Btn_` 按钮、`Tgl_` 开关、`Sld_` 滑条、`List_` 列表、`Item_` 模板。

### 无可视节点

无 `Graphic` 的节点不是一律使用 `Grp_`；前缀必须表达它在页面中的功能，而不是
挂载脚本的类型。禁止使用 `GameObject`、`Box`、`Container`、`Area`、`Copy`、中文或
组件类型作为节点名。

- `Grp_语义`：仅组织同一逻辑区域的静态子节点，不承担运行时注入或定位契约。
- `Slot_语义`：页面预留的运行时内容宿主；动态 Item、子页面或可替换内容只从这里
  创建/挂接。例如 `Slot_HeroHP` 挂 `UI_HeroHPBox`，其子级为 `HeroHP_Item(Clone)`。
- `Anchor_语义`：无视觉定位参照；供动画、特效、跟随物或弹出物读取位置，不承载内容。
- `Hook_语义`：无视觉功能宿主；挂载页面局部控制器、事件桥接或触发组件，名称说明功能
  （如 `Hook_InputBlocker`），不写脚本类名。

同一节点只选择一个主要职责前缀。具有 Image、Button、Slider 等可视/交互组件时，优先
使用其可视组件前缀；不要因同时挂有辅助脚本改成 `Hook_`。运行时创建对象使用资产名加
精确 `(Clone)` 后缀；动态原型资产以 `*_Item` 命名，只有池模板才使用 `Item_*Template`。

## Canvas 与根节点

- UIForm 根使用全屏 Stretch：anchorMin `(0,0)`、anchorMax `(1,1)`、offsets 为 `0`。
- 使用统一 `CanvasScaler / Scale With Screen Size` 和项目参考分辨率；不得每页私设不同缩放策略。
- 根只放 UIForm、项目 Form 脚本、Canvas、CanvasGroup、GraphicRaycaster 等生命周期组件，避免重复 Canvas/GraphicRaycaster。
- 页面层级、Sorting Order、UI Layer 和打开组由 GF 配置统一管理，不在子节点硬编码对抗。

## 锚点、轴心与尺寸

- 全屏背景/遮罩：Stretch/Stretch；模态框：Center/Center；HUD：按语义锚定对应边缘或中心。
- 元素相对于哪条屏幕边稳定，就锚定哪条边；禁止用 1920 坐标模拟另一侧对齐。
- 左上定位必须同时使用左上 anchor 与左上 pivot；锚点改变后重新核对 `anchoredPosition`。
- 内部连续行、按钮列和列表优先 Layout Group；不要同时以 Layout Group 和手写绝对坐标争夺同一子节点。
- 每个节点在 layout 中记录 anchor、pivot、sizeDelta、anchoredPosition、组件、颜色、资源、文本样例与状态。

## UGUI 组件

- 正式文本只用 `TextMeshProUGUI`，禁止 Legacy Text。
- 可视面板使用 Image；装饰 Graphic 的 raycastTarget 为 false，遮罩和可交互 Graphic 为 true。
- Button 的 targetGraphic 指向自身 Image；优先 Sprite Swap，缺资源时才使用统一 Tint。
- ScrollRect 必须具备 Viewport 和 Content；Content 的锚点、布局和滚动方向一致。
- 每个可交互控件有清晰 target、导航和 disabled 状态；关闭按钮是模态框最后一个可选中节点。

## Prefab 制作步骤

1. 根据确认的 `prefab-layout.md` 创建完整节点树和 RectTransform。
2. 设置组件、锚点、轴心、导航与射线规则。
3. 绑定已验收资源；缺失项保留命名明确的占位 Image。
4. 在 Form 脚本声明 SerializeField 并显式绑定。
5. 逐节点复核 layout，打开页面检查多宽高比、焦点和遮挡。
