# 设计

## 候选方案

| 方案 | 优点 | 缺点 |
|---|---|---|
| 显式 `SceneTransitionContext`（采用） | 初次切换语义明确；不依赖事件触发时机；后续可扩展启动动画、存档直达等入口 | 需要调整事件签名和订阅方 |
| UI 层读取 `HasCurrentScene` | 改动最小 | UI 决策隐式依赖事件发出时 SceneFlow 尚未提交当前场景 |
| 延迟注册 UI 订阅 | 首次事件天然不会触发 Loading | 生命周期隐蔽，容易遗漏进度/失败事件，初始化顺序更难测试 |

## 决策

新增不可变值对象 `SceneTransitionContext`，包含可空来源场景、目标场景和 `IsInitialTransition`。`ColorTimingSceneFlow` 是上下文的唯一创建者；订阅方只消费事实，不自行推断。

UI 服务使用一个可测试的纯函数决定是否展示 Loading：仅当 `IsInitialTransition && TargetScene == StartMenu` 时返回 false，其他情况均返回 true。

## 时序

```text
Launch
  → TryLoad(StartMenu)
  → TransitionStarted(initial=true, target=StartMenu)
  → UI decision=SkipLoading
  → GF.Scene 加载并绑定 StartMenu
  → PresentScene(StartMenu)
  → MainMenu.Open
```

## 异常策略

首次加载不设置超时后补 Loading。加载失败继续走现有错误日志、TransitionFailed 和框架重启路径。
