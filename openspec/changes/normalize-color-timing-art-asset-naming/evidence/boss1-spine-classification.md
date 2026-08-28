# Boss1 Spine 套件分类证据

日期：2026-08-28

## 主套件 `Boss/Spine`

- Boss1 场景对象 `Spine` 默认启用。
- JSON 包含待机、两套受击和第一至第六招等 10 个动画。
- 定义 `atk_end`、`atk_read`、`attack` 事件。
- `Boss1ActorView` 默认使用该套件播放待机、受击以及第五招以外的攻击。

结论：正式主套件，目标目录 `Boss1/Core`。

## 第五招套件 `Boss/Spine2`

- Boss1 场景对象 `Spine2` 默认关闭。
- JSON 仅包含第五招两个版本和无动作动画。
- `attack_5_test1_60fps2` 含正式攻击事件。
- `Boss1ActorView` 播放第五招时关闭主套件与预警套件、启用 Spine2；完成后恢复主套件。
- 原项目 `Boss1_Controller.cs` 使用同一切换流程，证明它不是重构阶段产生的临时资源。

结论：第五招专用正式套件，目标目录 `Boss1/Attack5`，不得删除。

## 预警套件 `Boss/tip`

- 场景对象名为 `Spine tip`。
- 与主套件使用相同的攻击动画名称。
- Slot 内容为攻击范围图形，运行时随主套件同步播放，并在第五招切换时暂时关闭。

结论：攻击预警套件，目标目录 `Boss1/Telegraph`。
