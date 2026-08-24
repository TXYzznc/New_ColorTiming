# 功能追踪矩阵逐行审计

日期：2026-08-24

## 结果

`feature-traceability.csv` 的 57 行均已逐行复核，实现路径没有发现新的空行或未迁移功能，但原 `AutomatedCheck` 列部分使用了计划式名称，不能全部视为已经执行的直接证据。审计结果写入 `feature-traceability-audit.csv`：

- `DIRECT`：30 行。已有直接自动化或已保存的运行时功能证据；仍需规格要求的最终人工手感/视听确认。
- `PARTIAL`：19 行。纯逻辑、部分运行时或部分画面证据成立，但不足以证明完整表现链。
- `STATIC_MANUAL`：8 行。实现、序列化引用或资源审计成立，但功能必须在运行时人工触发后才能关闭。

所有 57 行继续保留 `manual pending`，没有用编译通过、静态扫描或单张截图替代完整人工回归。

## 本轮补强

新增 `StartMenuNavigation_AndAllSettingsPersistThroughGfSetting` PlayMode 测试，直接覆盖：

- 主菜单、关卡选择、设置页及返回导航的显隐状态；
- BGM、SFX、key-tip 三项设置写入 GF.Setting 后重新实例化仍可读取；
- 三组按钮状态与持久化值一致；
- finally 恢复进入测试前的真实设置，避免污染用户配置。

Unity 已完成脚本编译且该文件编译错误为 0。由于 8093 当前仍是 Auto，PlayMode 执行被 UnitySkills 拒绝，因此 `FLOW-004` 与 `SET-003` 保持 `PARTIAL`，不得提前升级为 `DIRECT`。

随后新增 `BossAttackExecutionPlayModeTests`：强制实际播放 Boss1 六攻击并监听各自 Spine Event→GF.Entity，检查攻击 5 无敌窗口及恢复；Boss2 则覆盖头部近战/投射、完整潜地换位出土、12→11 尾部激活、尾部初次潜地和两种尾部攻击。脚本编译错误为 0，但同样因尚未执行而只作为“已准备的直接测试”，相关行仍维持原证据等级。

## 回到实现/验收的弱证据项

- StartMenu：退出应用、每条加载淡入淡出、设置新测试执行。
- 玩家：真实移动/瞄准、Dash 奖励、受击/强制丢弃、死亡相机与重开。
- 武器/动画：拾取轮廓与淡出、首次提示、每个 Animation/Spine Event 的实际触发。
- Boss1：六攻击全量、攻击 5 无敌与弱点变暗、全部事件和声音。
- Boss2：潜地表现、全部投射物/落点标记、完整头尾协作和声音。
- 世界/相机/音频：草地、视差、Cinemachine 行为及全部听觉 cue。
- 渲染：OpenSpec 10.8 要求的源/目标同状态配对帧。

因此，本文件证明 OpenSpec 12.5 的“逐行审计动作”已经完成，但同时明确证明 10.8、12.1–12.4 以及 Boss 专项人工验收仍未完成。
