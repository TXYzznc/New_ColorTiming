# 01 — MainMenu 视频 RenderTexture 生命周期

## 修改对象

- `Assets/Game/Scripts/ColorTiming/Presentation/UI/Components/MainMenuIntroSequence.cs`
- `Assets/Game/Tests/ColorTiming/PlayMode/ColorTimingUiAndSoundLifecyclePlayModeTests.cs`

## 修改前

- `RestartSequence()` 首次打开表单时创建 1920×1080 ARGB32 RenderTexture。
- `MainMenuForm.OnClose()` 只调用 `StopSequence()` 停止视频。
- RenderTexture 仅在 `MainMenuIntroSequence.OnDestroy()` 中释放。
- GF.UI 会缓存已关闭表单，关闭 MainMenu 后通常不会触发 `OnDestroy()`，因此视频输出纹理会继续占用运行时内存。

## 修改后

- `StopSequence()` 停止开场和循环 VideoPlayer 后立即释放视频输出纹理。
- 释放前清空两个 `VideoPlayer.targetTexture` 和 `RawImage.texture`，避免悬空引用。
- `OnDestroy()` 复用同一释放方法，释放操作可重复调用且保持安全。
- 再次打开 MainMenu 时，`RestartSequence()` 通过既有 `EnsureVideoOutput()` 创建并绑定新的 RenderTexture。
- 运行时创建的 `VideoOutput (Clone)` RawImage 节点继续随 GF.UI 表单复用，不反复创建节点。

## 预期收益

- 离开 MainMenu 后释放约 14.4 MB 的运行时视频纹理占用。
- Boss 场景不再无意义保留 StartMenu 视频输出纹理。
- 不改变视频源文件、分辨率、播放顺序或 UI 结构。

## 自动验证

- Unity 编译：两个修改脚本均为 0 error。
- 专项 PlayMode：`StartMenuVideo_StopReleasesAndRestartRecreatesOutput`，1/1 passed。
- 验证内容：旧纹理引用全部解除、旧 RenderTexture 被销毁、重启后创建不同的新纹理、开场对象重新激活。
- Unity Console：0 error，0 warning。

## 用户验收步骤

1. 从 Launch 启动游戏，确认 MainMenu 开场视频正常播放并切换至循环视频。
2. 进入 Boss1 或 Boss2，确认场景加载与显示正常。
3. 返回 MainMenu，确认重新从开场视频开始播放，而不是黑屏或直接停在循环画面。
4. 再重复一次“进入 Boss → 返回 MainMenu”，确认行为稳定。

## 回退方式

若出现黑屏、循环视频无法接管或返回 MainMenu 不重播开场，只需回退 `MainMenuIntroSequence.StopSequence()` 中的 `ReleaseVideoOutput()` 调用以及对应的释放测试；源视频和 Prefab 均未修改。
