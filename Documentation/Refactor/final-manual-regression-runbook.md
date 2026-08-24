# ColorTiming 最终人工回归与留证脚本

日期：2026-08-24  
适用目标：`D:\unity\UnityProject\ColorTimeing\New\_ColorTiming`

本文只定义最终人工验收步骤和证据要求。未实际执行并保存证据前，不得据此勾选 OpenSpec 6.7、7.10、10.8、12.1～12.4。

## 1. 执行前条件

1. Unity 打开目标工程，Console 无编译错误，Game View 使用 `1920x1080`、`1x` 或明确记录的固定缩放。
2. 从 `Launch` 场景开始，不从 Boss 场景直接启动。
3. 键鼠基线：`WASD/方向键` 移动、`Space` Dash、`鼠标左键/Left Ctrl` 攻击、`鼠标右键/Left Alt` 丢弃、`Escape` 暂停。
4. 每个场景至少保存一段连续录像；截图不能证明动画、视频、音频或事件时序。
5. 证据放入 `Documentation/Refactor/ManualEvidence/<日期>/`，文件名使用本文给出的前缀。
6. 缺陷记录必须包含：步骤、期望、实际、场景、时间戳、截图/录像文件名、是否为源工程差异。

## 2. StartMenu（OpenSpec 12.1）

从 `Launch` 进入后连续执行：

- [ ] `Launch` 只加载一次 StartMenu，过渡期间有淡入淡出和单调进度；无重复 UI、无黑屏卡死。
- [ ] 开头视频有实际画面并播放一次，结束后自动进入循环视频；循环期间不回到开头片段。
- [ ] BGM 正常；关闭/开启 BGM 后按钮状态、声音和重新进入设置页后的状态一致。
- [ ] SFX 关闭/开启后按钮点击音和重新进入设置页后的状态一致。
- [ ] key-tip 关闭/开启后状态持久化，并在战斗 HUD/暂停提示中生效。
- [ ] 设置页、关卡选择页和主菜单的全部返回按钮回到正确父页面；页面不叠加。
- [ ] “开始”经过 Loading 进入 Boss1。
- [ ] 关卡选择 Boss1、Boss2 分别经过 Loading 进入正确场景。
- [ ] 返回 StartMenu 后双视频、BGM 和按钮仍只有一套实例。
- [ ] 在 Player 构建中验证退出按钮确实退出；Editor 中只确认无异常，不把 Editor 无法退出判失败。

证据：`startmenu-full.mp4`、`startmenu-intro.png`、`startmenu-loop.png`、`startmenu-settings.png`、`startmenu-loading.png`。

## 3. Boss1（OpenSpec 6.7、12.2）

### 玩家与武器

- [ ] 四方向移动、朝向和移动动画一致；鼠标指针改变攻击方向。
- [ ] Dash 有位移和无敌窗口；成功穿过可伤害判定后恢复 1 HP，并出现约 `0.45` 时间脉冲后恢复正常速度。
- [ ] 受击扣血、击退、短暂无敌并强制丢弃武器；同一受击窗口不重复扣血。
- [ ] 5 格 HP 与 HUD 同步；死亡动画、死亡相机、失败 UI 和同场景重开均正常。
- [ ] 拾取、切换限制、主动丢弃、掉落淡出和轮廓表现正常。
- [ ] 红/绿/紫三色各验证剪刀、锤、炸弹三类正式攻击；普通攻击也能执行。
- [ ] 前三次弱点/武器提示出现且任意键只关闭一次，不把同一按键泄漏成攻击。

### Boss 与世界

- [ ] 初始 11 段弱点包含红 4、绿 3、紫 4；HUD 当前项和后 7 项顺序正确。
- [ ] 错色不掉段，正确颜色只掉一段；受击闪烁、命中特效和镜头冲击可见。
- [ ] 在近/中/远三个距离区域观察到选择变化，并覆盖六种攻击。
- [ ] 六攻击的 Spine Event、实体/投射物、落点或范围、动画结束回收均完整；无残留实体。
- [ ] 攻击 5 期间 Boss 无敌且弱点变暗，结束后伤害与弱点亮度恢复。
- [ ] Boss、玩家、武器、环境和 UI 声音均来自正确组；草地进入有摆动/沙沙声并切换脚步，离开后恢复。
- [ ] 视差层随相机移动，距离缩放平滑，Confiner 不穿界，受击 Impulse 正常。
- [ ] Escape 暂停/继续，重开、返回菜单路径正确；暂停时游戏时间停止且 UI 可操作。
- [ ] 最后一段正确伤害只结算一次，胜利表现完整并进入 Boss2。

证据：`boss1-full.mp4`、`boss1-six-attacks.mp4`、`boss1-attack5.mp4`、`boss1-weapons-colors.mp4`、`boss1-grass-camera-audio.mp4`、`boss1-victory.png`。

## 4. Boss2（OpenSpec 7.10、12.3）

- [ ] 初始 15 段包含红 4、绿 4、紫 4、橙 3；HUD 当前项与后续顺序正确。
- [ ] 红/绿/紫/橙四色均验证错色拒绝、正确颜色单段消耗。
- [ ] 三种武器覆盖四色攻击，橙色资源、动画、特效和 UI 不借用紫色槽。
- [ ] 头部在距离/朝向条件下覆盖潜地、近战、远程选择。
- [ ] 潜地流程完整：入地、碰撞关闭、轨迹、换位、出土、碰撞恢复、回 idle；过程中无瞬移残影或永久失去碰撞。
- [ ] 头部近战与远程的 Spine Event、实体、落点标记、命中和回收正确。
- [ ] 仅在血段 `12→11` 时激活尾部一次；尾部首次潜地、近战、远程均可完成。
- [ ] 头尾同时存在时攻击、受击、实体回收和声音互不串用。
- [ ] 玩家受击、死亡相机、失败 UI、重开、暂停/继续/返回菜单完整。
- [ ] 最后一段只触发一次最终结果；头尾和战斗实体同步清理，结果按钮返回 StartMenu。

证据：`boss2-full.mp4`、`boss2-burrow.mp4`、`boss2-head-tail.mp4`、`boss2-weapons-colors.mp4`、`boss2-projectiles-markers.mp4`、`boss2-result.png`。

## 5. 源—目标同状态视觉对比（OpenSpec 10.8）

源工程：`D:\unity\UnityProject\ColorTimeing\ColorTimeing`  
目标工程：`D:\unity\UnityProject\ColorTimeing\New\_ColorTiming`

每组使用相同分辨率、Game View 缩放、场景状态、角色/Boss 位置、HP 段和 UI 状态；源、目标各保存一张原始 PNG，再保存一张并排图。至少包含：

- [ ] StartMenu：开头视频帧、循环视频帧、主菜单、设置页、关卡选择页。
- [ ] Boss1：初始帧、持武器 HUD、命中闪烁、攻击 5 无敌/弱点变暗、胜利帧。
- [ ] Boss2：初始帧、潜地轨迹、出土、尾部激活、投射物/落点、最终结果。
- [ ] 逐组检查角色、Boss、UI、特效、遮罩、颜色、排序层、画面裁切和字体。
- [ ] 每个可见差异在 `visual-regression-comparison.md` 中标为“修复”“源本身如此”或“经批准的差异”，不得只写“看起来差不多”。

证据文件前缀：`pair-start-*`、`pair-boss1-*`、`pair-boss2-*`。

## 6. 自动合同执行后的合并判定

人工观察之外，最终还需执行 `test-discovery-20260824.md` 中的全部 ColorTiming 测试：

- EditMode：当前发现 63 个 ColorTiming 用例；
- PlayMode：当前发现 14 个 ColorTiming 用例；
- 失败、Skipped、Inconclusive、Other 均需逐项解释，不能只报告 Passed。

只有以下条件同时满足，才可关闭 OpenSpec 12.4 和 12.7：

1. 本文所有复选框都有对应原始证据；
2. 57 行 `feature-traceability.csv` 的人工证据 ID 均能映射到录像时间戳或截图；
3. 自动测试完整执行且无未解释结果；
4. Console、missing script、missing reference、shader、GUID/资源审计保持通过；
5. `openspec validate migrate-color-timing-to-ai-friendly-framework --strict` 通过；
6. `tasks.md` 无未完成任务后才归档变更。
