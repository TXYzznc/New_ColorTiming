# 功能追踪矩阵逐行审计

日期：2026-08-24

## 结果

`feature-traceability.csv` 的 57 行均已逐行复核，实现路径没有发现新的空行或未迁移功能，但原 `AutomatedCheck` 列部分使用了计划式名称，不能全部视为已经执行的直接证据。审计结果写入 `feature-traceability-audit.csv`：

- `DIRECT`：30 行。已有直接自动化或已保存的运行时功能证据；仍需规格要求的最终人工手感/视听确认。
- `PARTIAL`：19 行。纯逻辑、部分运行时或部分画面证据成立，但不足以证明完整表现链。
- `STATIC_MANUAL`：8 行。实现、序列化引用或资源审计成立，但功能必须在运行时人工触发后才能关闭。

所有 57 行继续保留 `manual pending`，没有用编译通过、静态扫描或单张截图替代完整人工回归。

Unity Test Runner 的最新发现结果单独记录在 `test-discovery-20260824.md`：63 个 ColorTiming EditMode 与 14 个 ColorTiming PlayMode 用例均为 `Runnable`，但新增用例尚未执行，因此不会据此增加通过数。

## 本轮补强

新增 `StartMenuNavigation_AndAllSettingsPersistThroughGfSetting` PlayMode 测试，直接覆盖：

- 主菜单、关卡选择、设置页及返回导航的显隐状态；
- BGM、SFX、key-tip 三项设置写入 GF.Setting 后重新实例化仍可读取；
- 三组按钮状态与持久化值一致；
- finally 恢复进入测试前的真实设置，避免污染用户配置。

Unity 已完成脚本编译且该文件编译错误为 0。由于 8093 当前仍是 Auto，PlayMode 执行被 UnitySkills 拒绝，因此 `FLOW-004` 与 `SET-003` 保持 `PARTIAL`，不得提前升级为 `DIRECT`。

随后新增 `BossAttackExecutionPlayModeTests`：强制实际播放 Boss1 六攻击并监听各自 Spine Event→GF.Entity，检查攻击 5 无敌窗口及恢复；Boss2 则覆盖头部近战/投射、完整潜地换位出土、12→11 尾部激活、尾部初次潜地和两种尾部攻击。脚本编译错误为 0，但同样因尚未执行而只作为“已准备的直接测试”，相关行仍维持原证据等级。

新增 `PlayerRuntimeExecutionPlayModeTests`：通过 `IGameInput` 语义边界驱动真实 Hero，覆盖双向移动/朝向、武器拾取、受击强制丢弃、受击拒绝窗口、Dash 移动、成功 Dash 回血与 0.45 时间脉冲、连续伤害、死亡镜头及 `DeathOver` Animation Event 强制重开。脚本编译错误为 0；执行前不提升对应证据等级。

新增 `WeaponAnimationEventExecutionPlayModeTests`：从正式 `HeroAnimStae.Attack` 接收器进入，覆盖普通攻击、Boss1 三武器×三颜色、Boss2 三武器×四色和剪刀第二段，共 23 条实际事件路径；每条都等待对应 GF.Entity，读取 `Skill_base` 的武器身份、攻击者和炸弹/飞机指针参数。脚本编译错误为 0；执行及最终视觉观察前，`WEAPON-006`/`ANIM-003` 继续保持非完成证据。

新增 `GrassWorldInteractionPlayModeTests`：对正式 Boss1 草地对象调用 Unity 触发器接收路径，验证草地 Animator 合同、环境 rustle cue、HeroSoundManager 覆盖集合、修复后的草地脚步列表以及退出后恢复普通脚步。脚本编译错误为 0；执行和实际听感/动画观察前，`MEDIA-002` 仍保持 `STATIC_MANUAL`。

新增 `CameraRuntimeExecutionPlayModeTests`：从正式 Boss1 进入，执行 `CameraShow` 的视差位移公式，验证 `HeroCamera_` 在近距、阈值和超距时写入的正交尺寸，并检查 Brain、VirtualCamera、Confiner2D、ImpulseSource 与 ImpulseListener 的运行时装配；死亡相机禁用路径继续由玩家生命周期测试覆盖。脚本编译错误为 0；实际执行和相机手感观察前，`MEDIA-003`/`MEDIA-004` 仍保持 `STATIC_MANUAL`。

## 回到实现/验收的弱证据项

- StartMenu：退出应用、每条加载淡入淡出、设置新测试执行。
- 玩家：真实移动/瞄准、Dash 奖励、受击/强制丢弃、死亡相机与重开。
- 武器/动画：拾取轮廓与淡出、首次提示、每个 Animation/Spine Event 的实际触发。
- Boss1：六攻击全量、攻击 5 无敌与弱点变暗、全部事件和声音。
- Boss2：潜地表现、全部投射物/落点标记、完整头尾协作和声音。
- 世界/相机/音频：草地、视差、Cinemachine 行为及全部听觉 cue。
- 渲染：OpenSpec 10.8 要求的源/目标同状态配对帧。

因此，本文件证明 OpenSpec 12.5 的“逐行审计动作”已经完成，但同时明确证明 10.8、12.1–12.4 以及 Boss 专项人工验收仍未完成。
