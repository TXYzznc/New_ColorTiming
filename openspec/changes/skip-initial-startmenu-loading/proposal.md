# 变更：首次启动直接进入 MainMenu

## 背景

当前 `Launch → StartMenu` 首次加载会发送普通 `TransitionStarted` 事件，UI 服务因此短暂打开项目 Loading。产品要求首次启动直接呈现 MainMenu；项目 Loading 只服务于已有业务场景之间的切换。

## 已确认边界

- 首次 `Launch → StartMenu` 始终不显示项目 Loading，也不设置延迟兜底。
- `StartMenu → Boss1/Boss2`、`Boss1 → Boss2` 继续显示 Loading。
- Boss、暂停或结果界面返回 StartMenu 继续显示 Loading。
- 场景仍由 GF.Scene 异步加载；MainMenu 只在 StartMenu 成功绑定后打开。
- 不启用 Launch BuiltinView Loading，不修改框架核心。

## 影响范围

- ColorTiming SceneFlow 事件契约及其两个订阅方。
- 项目 UI Loading 决策。
- EditMode 场景流与 UI 决策测试。
- 现有启动诊断日志。
