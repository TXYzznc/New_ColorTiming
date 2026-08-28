# 02 — 长音频 Streaming 与战斗音频生命周期

## 修改对象

### 代码与场景

- `Assets/Game/Scripts/ColorTiming/Bootstrap/BattleSceneAnchors.cs`
- `Assets/Game/Scripts/ColorTiming/Bootstrap/BattleRuntimeContext.cs`
- `Assets/Game/Editor/ColorTimingMigration/ColorTimingBattleArchitectureMigration.cs`
- `Assets/Game/Scene/Boss1.unity`
- `Assets/Game/Scene/Boss2.unity`

### 音频 Importer

- `Assets/Game/Audio/ColorTiming/BGM/菜单第三版.wav.meta`
- `Assets/Game/Audio/ColorTiming/BGM/第一章bgm0526.wav.meta`
- `Assets/Game/Audio/ColorTiming/BGM/第二章bgm0526.wav.meta`
- `Assets/Game/Audio/ColorTiming/amb_cave.wav.meta`

## 音频 Importer 修改

| 参数 | 修改前 | 修改后 |
|---|---|---|
| Load Type | Decompress On Load | Streaming |
| Compression Format | Vorbis | Vorbis |
| Quality | 1.0 | 0.8 |
| Load In Background | false | true |
| Force To Mono | false | false |
| Sample Rate | Preserve Sample Rate | Preserve Sample Rate |

源 `.wav` 文件内容、声道和采样率均未修改。Streaming 主要降低运行时音频驻留内存；Vorbis 质量 0.8 同时降低导入后资源体积，最终听感由用户验收。

## 战斗音频结构修改

### 修改前

- Boss1 场景包含两个独立根对象：一条章节 BGM 和一条洞穴环境音。
- Boss2 场景包含一个独立 BGM 根对象。
- 三个对象均通过 `AudioSource.playOnAwake=true` 提供配置，场景加载后再由 `BattleRuntimeContext` 停止并转交 GF.Sound。
- 场景加载到运行时绑定之间存在短暂抢播或重复播放的可能。

### 修改后

- `BattleSceneAnchors.SoundCue` 直接保存 `AudioClip`、GF 声道、循环标志和原位置。
- `BattleRuntimeContext` 只通过 `IColorTimingSoundService` 请求播放。
- Boss1 的两个、Boss2 的一个纯 AudioSource 根对象已删除。
- 切场景时继续由 `GfColorTimingSoundService.ResetTrackedSounds()` 统一停止和清理。
- 暂停、BGM/SFX 静音以及声音分组继续沿用现有 GF.Sound 策略。

Unity 保存场景时还清理了若干当前脚本已不存在的旧序列化字段，并把使用 `FormerlySerializedAs` 的武器生成器字段写成当前名称；未修改 Sprite、材质、动画、音频源文件或其他美术资源引用。

## 预期收益

- StartMenu 原约 25.25 MB 的解压 BGM 改为流式缓冲。
- Boss1 原约 18.37 MB 的 BGM + 环境音解压数据改为流式缓冲。
- Boss2 原约 11.79 MB 的解压 BGM 改为流式缓冲。
- 实际 Streaming 仍有解码缓冲和 AudioClip 元数据，精确收益应在 Player 中复测。
- 战斗场景不再存在先于 GF 生命周期播放的静态章节音频。

## 自动验证

- 修改脚本编译：0 error。
- 专项 PlayMode：`BattleSceneAudio_UsesGfSoundWithoutAuthoredAudioSources`，1/1 passed。
- 验证链路：StartMenu → Boss1 → Boss2。
- Boss1 的两条、Boss2 的一条章节声音均收到 GF `PlaySoundSuccessEventArgs`，确认不只是提交请求，而是资源加载成功并开始播放。
- 两个战斗场景中带剪辑的静态 AudioSource 数量均为 0。
- Unity Console：0 error，0 warning。

## 运行时诊断日志

`GfColorTimingSoundService` 只在状态变化时输出 `[ColorTiming.Audio]` 日志，不在 `Update()` 中刷屏：

- `action=PlayRequested result=Accepted`：GF 已接受请求，并记录 serialId、clip、channel、loop。
- `action=PlayStarted result=Success`：GF 已加载资源并开始播放，同时记录加载耗时。
- `action=PlayStarted result=Failure`：资源加载或播放失败，以 Error 记录错误码和原因。
- `action=Stop`：业务主动停止一条受管声音。
- `action=ResetTrackedSounds`：切场景时批量清理受管声音并记录数量。

## 用户验收步骤

1. 在 MainMenu 停留至少 30 秒，确认 BGM 启动及时、循环正常、无爆音和断流。
2. 进入 Boss1，分别确认章节 BGM 和洞穴环境音存在，音量关系与修改前一致。
3. 暂停和恢复游戏，确认战斗声音按原规则暂停/恢复。
4. 进入 Boss2，确认 Boss1 的两条背景声音已停止，Boss2 BGM 正常播放。
5. 返回 MainMenu，确认 Boss2 BGM 已停止，菜单 BGM 正常重新播放。
6. 切换 BGM/SFX 开关，确认静音状态仍正确。

## 跨设备同步注意事项

`Assets/Game/Audio/` 被 `.gitignore` 忽略，因此四个 `.meta` 的 Importer 修改不会进入 Git 提交。用户验收通过后，需要在资源导出工具中只选择“美术资源”组并重新导出资源包；另一台设备重新导入该资源包后才能获得相同的 Streaming 设置。

## 回退方式

- 听感不接受：先把 Quality 从 0.8 调高至 0.9 或 1.0，保留 Streaming。
- 出现平台性断流：仅把对应音频改为 Compressed In Memory，不恢复场景 AudioSource。
- 生命周期异常：回退 `SoundCue` 结构、两个场景和运行时播放代码；源音频不受影响。
