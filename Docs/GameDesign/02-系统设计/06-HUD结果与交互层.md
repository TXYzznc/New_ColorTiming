# HUD、结果与交互层

| 功能 ID | 行为合同 | 实现与验收 |
|---|---|---|
| CT-HUD-01 | Hero HUD 固定 1920×1080 根尺寸，五格 HP 的创建、失去、填充与受击闪烁正确。 | HUD 订阅玩家状态；页面或 Item 池复用时解绑旧事件。 |
| CT-HUD-02 | Boss1／Boss2 仅显示当前场景对应 HUD；当前弱点、后续七段、颜色／武器图标同步。 | HUD 控制器按场景战斗上下文启用，不以名称猜测。 |
| CT-HUD-03 | 鼠标光标、蓄力提示、拾取提示、伤害提示和教程随状态更新。 | 输入封锁时禁止战斗交互穿透。 |
| CT-HUD-04 | 暂停、胜利、失败、最终结果显示层级正确，打开时封锁底层输入。 | 使用 GF.UI 层与统一关闭回调；结果只在 Boss2 最终结束显示。 |
| CT-HUD-05 | 失败后可同场景重开；最终结果可返回 StartMenu。 | 先清理旧 HUD／实体／声音，再发起 SceneFlow。 |

正式 UI 制作必须遵循 [GF UI 全流程规范](../../Development/GF-UI-Standards/README.md)：一个独立页面对应一个 UIForm Prefab；先定义 Prefab 布局，再绑定逻辑；输入经 InputModule；正式页面不可运行时拼装。

参照：[验收清单 §6](../../../openspec/changes/migrate-color-timing-to-ai-friendly-framework/evidence/source-feature-acceptance-checklist.md)。
