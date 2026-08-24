# 功能追踪矩阵逐行审计

日期：2026-08-24

## 结果

`feature-traceability.csv` 的 57 行均已逐行复核，实现路径没有发现新的空行或未迁移功能，但原 `AutomatedCheck` 列部分使用了计划式名称，不能全部视为已经执行的直接证据。审计结果写入 `feature-traceability-audit.csv`：

- `DIRECT`：30 行。已有直接自动化或已保存的运行时功能证据；仍需规格要求的最终人工手感/视听确认。
- `PARTIAL`：19 行。纯逻辑、部分运行时或部分画面证据成立，但不足以证明完整表现链。
- `STATIC_MANUAL`：8 行。实现、序列化引用或资源审计成立，但功能必须在运行时人工触发后才能关闭。

所有 57 行继续保留 `manual pending`，没有用编译通过、静态扫描或单张截图替代完整人工回归。

Unity Test Runner 的最新发现与执行结果单独记录在 `test-discovery-20260824.md`：63 个 ColorTiming EditMode 与 14 个 ColorTiming PlayMode 用例均为 `Runnable`。独立完整工程副本随后执行 PlayMode `14/14` 通过，并在最终运行修复后重跑完整 EditMode；新增 Boss、玩家、武器、草地、相机、视频与生命周期合同均已成为直接运行证据。

另以 Roslyn 从 64 个源产品脚本反向扫描方法面；57 个保留/重构脚本的目标路径缺失为 0，21 个存在签名变化的脚本均在 `method-surface-reverse-audit.md` 中逐项落到领域层、框架服务、实体生命周期或有引用证据的不可达入口。该结果补强“无功能遗漏”的静态证据，但不替代人工回归。

同一反向策略也应用于 Unity 序列化字段：54 个源组件类展开为 241 个含继承字段合同，目标组件缺失、字段名缺失和字段类型变化均为 0；详见 `serialized-field-surface-reverse-audit.md`。这排除了“代码能编译但场景/Prefab 因字段改名静默丢值”的一类迁移风险。

## 本轮补强

新增 `StartMenuNavigation_AndAllSettingsPersistThroughGfSetting` PlayMode 测试，直接覆盖：

- 主菜单、关卡选择、设置页及返回导航的显隐状态；
- BGM、SFX、key-tip 三项设置写入 GF.Setting 后重新实例化仍可读取；
- 三组按钮状态与持久化值一致；
- finally 恢复进入测试前的真实设置，避免污染用户配置。

该测试已在完整隔离副本中执行通过。`FLOW-004` 与 `SET-003` 仍保持 `PARTIAL`，因为自动状态合同不能替代最终鼠标操作、可见 key-tip 与按钮手感观察。

随后新增并执行 `BossAttackExecutionPlayModeTests`：强制实际播放 Boss1 六攻击并监听各自 Spine Event→GF.Entity，检查攻击 5 无敌窗口及恢复；Boss2 覆盖头部近战/投射、完整潜地换位出土、12→11 尾部激活、尾部初次潜地和两种尾部攻击。两项均通过；相关行仍保留人工画面/听感缺口。

新增并执行 `PlayerRuntimeExecutionPlayModeTests`：通过 `IGameInput` 语义边界驱动真实 Hero，覆盖双向移动/朝向、武器拾取、受击强制丢弃、受击拒绝窗口、Dash 移动、成功 Dash 回血与 0.45 时间脉冲、连续伤害、死亡镜头及 `DeathOver` Animation Event 同场景重开。测试通过；移动手感、受击闪烁和死亡演出仍需人工观察。

新增并执行 `WeaponAnimationEventExecutionPlayModeTests`：从正式 `HeroAnimStae.Attack` 接收器进入，覆盖普通攻击、Boss1 三武器×三颜色、Boss2 三武器×四色和剪刀第二段，共 23 条实际事件路径；每条都等待对应 GF.Entity，读取 `Skill_base` 的武器身份、攻击者和炸弹/飞机指针参数。测试通过；`WEAPON-006`/`ANIM-003` 仍保留最终攻击画面与非武器事件的人工/部分缺口。

新增并执行 `GrassWorldInteractionPlayModeTests`：对正式 Boss1 草地对象调用 Unity 触发器接收路径，验证草地 Animator 合同、环境 rustle cue、HeroSoundManager 覆盖集合、修复后的草地脚步列表以及退出后恢复普通脚步。测试通过；实际听感/动画观察仍保留在人工清单。

新增并执行 `CameraRuntimeExecutionPlayModeTests`：从正式 Boss1 进入，执行 `CameraShow` 的视差位移公式，验证 `HeroCamera_` 在近距、阈值和超距时写入的正交尺寸，并检查 Brain、VirtualCamera、Confiner2D、ImpulseSource 与 ImpulseListener 的运行时装配；死亡相机禁用路径由玩家生命周期测试覆盖。测试通过；相机手感与最终画面对比仍保留在人工清单。

## 回到实现/验收的弱证据项

- StartMenu：退出应用、每条加载淡入淡出、真实鼠标操作与可见设置反馈。
- 玩家：物理键鼠手感、瞄准、Dash/受击可见反馈与死亡演出。
- 武器/动画：拾取轮廓与淡出、首次提示、最终攻击画面及未覆盖的非武器 Animation Event。
- Boss1：六攻击、攻击 5 弱点变暗及全部声音的最终视听确认。
- Boss2：潜地、投射物/落点标记、完整头尾协作和声音的最终视听确认。
- 世界/相机/音频：草地、视差、Cinemachine 行为及全部听觉 cue。
- 渲染：OpenSpec 10.8 要求的源/目标同状态配对帧。

因此，本文件证明 OpenSpec 12.5 的“逐行审计动作”已经完成，但同时明确证明 10.8、12.1–12.4 以及 Boss 专项人工验收仍未完成。
