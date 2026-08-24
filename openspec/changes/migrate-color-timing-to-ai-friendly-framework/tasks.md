## 1. 固化源基线与追踪证据

- [ ] 1.1 记录源项目和目标项目的 Unity、Packages、渲染管线、ProjectSettings、Build Settings 与 Git 状态快照
- [ ] 1.2 生成源项目自有脚本清单，记录路径、类型、GUID、代码引用和场景/Prefab 序列化引用
- [ ] 1.3 生成三场景对象、组件、MonoBehaviour、UnityEvent 按钮绑定和持久对象清单
- [ ] 1.4 生成 Prefab、Animator Controller、Animation Clip、Animation Event、Spine 数据/事件、材质与 Shader 引用清单
- [ ] 1.5 生成图片、音频、视频、字体及其他产品资源的路径、GUID、大小和内容哈希 manifest
- [ ] 1.6 将已盘点的 StartMenu、全局流、玩家、输入、战斗、武器、Boss1、Boss2、UI、媒体/世界/相机功能写入追踪矩阵
- [ ] 1.7 为每个功能行分配目标模块、目标资产、自动化检查和人工回归步骤，禁止存在未分配行
- [ ] 1.8 建立废弃候选清单并为每个空脚本、原型和测试热键记录全范围零引用证据
- [ ] 1.9 建立 `behavior-fixes.md`，先登记 Boss2 橙色生命段索引缺陷及其同槽位不变量
- [ ] 1.10 在源项目执行干净的 Unity/C# 基线编译并保存零错误与既有警告报告

## 2. 建立产品架构与框架入口

- [ ] 2.1 创建 `Assets/Game/Scripts/ColorTiming/` 下的 Bootstrap、Input、Combat、Player、Bosses、UI、Presentation 目录与 meta
- [ ] 2.2 配置产品运行时与 EditMode/PlayMode 测试程序集边界，验证产品代码不进入 `ScriptsBuiltin`
- [ ] 2.3 实现显式 `ColorTimingCompositionRoot`，定义服务创建、绑定、释放顺序且不新增 Service Locator
- [ ] 2.4 实现 `ColorTimingStartupProcedure : IFrameworkStartupProcedure` 并接入 `FrameworkReadyProcedure`
- [ ] 2.5 实现场景标识与场景流服务，封装 `ChangeSceneProcedure` 参数及防重复切换
- [ ] 2.6 配置 `Launch` 为唯一标准入口，并以最小 StartMenu 占位链验证框架启动成功
- [ ] 2.7 运行框架纯度审计，证明 `ScriptsBuiltin` 无 ColorTiming 产品逻辑

## 3. 迁移资产、包与工程配置

- [ ] 3.1 制定并验证 GUID 安全的源→目标路径映射，排除四个已知 folder meta 路径冲突
- [ ] 3.2 在不覆盖框架 meta 的前提下迁移三场景及其 `.meta`，并登记资源规则
- [ ] 3.3 迁移 48 个 Prefab 及其完整依赖，按 UI、Entity、World 等产品目录分类但保留 GUID
- [ ] 3.4 迁移 27 个 Animator Controller、148 个 Animation Clip 及相关 Avatar/状态行为，保留 GUID
- [ ] 3.5 迁移 Spine 3.8 runtime、SkeletonData、atlas、纹理和材质，暂保留源 Shader 以便后续受控替换
- [ ] 3.6 迁移图片、粒子、音频、两个 MP4、字体与其他产品资源并核对源/目标哈希
- [ ] 3.7 合并 `Enemy` Tag 与 UI、Camera、LimitPlayer、Boss Layer，验证所有 LayerMask 序列化值
- [ ] 3.8 合并 Physics2D、Sorting Layer、分辨率 1920×1080、输入轴及其他功能必需 ProjectSettings
- [ ] 3.9 从框架 Packages 基线确认 Cinemachine/UGUI/TMP/UniTask/GF 依赖，移除业务未使用的 Input System 依赖前先通过引用审计
- [ ] 3.10 配置 Scene、UI、Entity、Sound、DataTable/Config 的框架资源收集规则和产品标识表
- [ ] 3.11 导入目标项目并修复所有包、GUID、脚本和序列化错误，保存首次导入报告

## 4. 输入、领域模型与时间服务

- [ ] 4.1 定义 `IGameInput` 及 Move、Dash、AttackPressed/Held、Drop、Pause、Pointer、AnyKey、Confirm 语义
- [ ] 4.2 实现 Legacy Input Manager 适配器和显式 gameplay-camera 指针世界坐标适配器
- [ ] 4.3 提供确定性 Fake Input，并为按下/持续/释放、暂停和教程输入消费编写测试
- [ ] 4.4 静态扫描产品代码，建立“仅适配器可直接调用 Unity Input”的门槛
- [ ] 4.5 实现 WeaponColor、WeaponType、DamageRequest、Health 与 BattleResult 纯 C# 类型及旧索引映射
- [ ] 4.6 实现 Boss 匹配色伤害、无敌拒绝、单段扣除与单次结果发射规则及测试
- [ ] 4.7 实现 Boss1 11 段和 Boss2 15 段弱点队列、洗牌、当前/未来七段投影及颜色计数测试
- [ ] 4.8 实现玩家 5 HP、伤害、无敌、治疗上限、死亡规则及测试
- [ ] 4.9 实现可组合的暂停/慢动作时间服务，覆盖 0.45 倍 Dash 效果、死亡与恢复次序测试
- [ ] 4.10 实现武器生成上限、计时与弱点颜色保证的纯逻辑策略及测试

## 5. 玩家、武器、技能与实体生命周期

- [ ] 5.1 实现玩家 locomotion 状态机与视图适配，恢复移动、朝向、速度和状态限制
- [ ] 5.2 实现 Dash 状态、事件定义的无敌窗口、成功 Dash 治疗/慢动作和音效
- [ ] 5.3 实现受击、击退、强制掉落、Hit 动画与受击无敌流程
- [ ] 5.4 实现死亡状态、输入封锁、Death/DeathOver 事件、相机序列与重开解锁
- [ ] 5.5 实现武器世界实体、拾取、切换、掉落、淡入淡出、描边与回收状态
- [ ] 5.6 实现 normal、scissors、hammer、bomb、knife、axe、airplane 七种类型与四色展示映射
- [ ] 5.7 实现标准攻击与 charge/held 攻击状态，恢复 `Atk`、`Atk_x` 和 charge 阈值
- [ ] 5.8 实现近战、技能、投射物创建与 DamageRequest 适配，接入 GF.Entity/对象池
- [ ] 5.9 实现 HitFX、技能移动、落点/命中反馈和 Cinemachine impulse 请求
- [ ] 5.10 创建 Animation Event 兼容桥，覆盖 Attack、PlayAuido、PlayAuido_Random、DashWD、DashEnd、SkillMove、Wudi、Hit、DeathOver
- [ ] 5.11 创建技能事件兼容桥，覆盖 EventEnd_Destroy、OnFXEnd、Cerate、End 并验证回收清理
- [ ] 5.12 迁移 EnterAnimStateEvent、RestXuli、Xuli StateMachineBehaviour 并核对 Animator 参数完整性
- [ ] 5.13 实现 Boss1/Boss2 武器生成器、活动计数和第一批弱点提示，执行 PlayMode 生命周期测试

## 6. Boss1 完整迁移

- [ ] 6.1 建立 Boss1 纯 C# 状态上下文、三距离区域判定和攻击选择策略测试
- [ ] 6.2 迁移 Boss1 六种攻击的状态、进入/退出条件和 Spine 动画请求
- [ ] 6.3 逐攻击恢复移动、Hitbox、技能/特效生成、声音和 Spine 事件时间点
- [ ] 6.4 实现攻击 5 的临时无敌与弱点变暗/恢复，并编写匹配色命中回归测试
- [ ] 6.5 接入 11 段弱点、受击表现、HP/HUD 事件和未来七段投影
- [ ] 6.6 实现 Boss1 最后一击、胜利表现和单次跳转 Boss2 流程
- [ ] 6.7 为六种攻击、三距离区域、无敌、全部颜色和胜利运行 Boss1 PlayMode/人工专项检查

## 7. Boss2 完整迁移

- [ ] 7.1 建立 Boss2 头部与尾部状态上下文、共享战斗数据和单向协调接口
- [ ] 7.2 迁移基于距离/朝向的近战与投射物攻击选择及状态转移测试
- [ ] 7.3 迁移潜地、隐藏、轨迹、合法位置重定位、出土与中断清理流程
- [ ] 7.4 迁移全部 Boss2 技能投射物模式、速度/方向和落点标记生命周期
- [ ] 7.5 实现剩余段数从 12 到 11 时尾部阶段单次激活及专项测试
- [ ] 7.6 接入 15 段四色弱点、受击表现、HP/HUD 与未来七段投影
- [ ] 7.7 修复并测试橙色生命段同槽位索引，更新 `behavior-fixes.md`
- [ ] 7.8 建立全部 Boss2 Spine 事件与声音映射，验证无重复订阅和缺失事件
- [ ] 7.9 实现最终胜利时头/尾/投射物/重定位停止、结果 UI 与返回菜单流程
- [ ] 7.10 为潜地、近战/远程、全部模式、尾部阈值、四色伤害和最终结果运行 Boss2 PlayMode/人工专项检查

## 8. 菜单、HUD、教程与全局 UI

- [ ] 8.1 将 StartMenu 主界面、关卡选择、设置与返回导航迁移为 GF.UI Form/Presenter
- [ ] 8.2 恢复 StartGameBtnDown、BackStartBtnDown、SettingBtnDwon、BackSettingBtnDwon、GoTest1、GoTest2、ExitGameBtn 绑定
- [ ] 8.3 实现项目设置服务并恢复 SetBGM、SetSFX、OffKeyTip、OpenKeyTip 持久行为
- [ ] 8.4 将玩家五格 HP、Boss 当前/未来七段弱点和当前提示动画迁移到战斗 HUD
- [ ] 8.5 实现每个颜色/武器组合的图标、光标与 charge 提示同步
- [ ] 8.6 迁移第一批武器/弱点教程，使用实时等待、最短显示保护和输入边缘消费
- [ ] 8.7 迁移暂停 Form 与 Open/OffKeyTip、重开、下一关、返回菜单按钮绑定
- [ ] 8.8 迁移伤害闪烁、胜利、失败和最终结果 Form 及底层输入封锁
- [ ] 8.9 迁移加载进度和淡入淡出表现，验证非递减进度与防重复切换
- [ ] 8.10 为反复打开/关闭、场景退出和暂停时交互编写 UI 生命周期 PlayMode 测试

## 9. 音频、视频、世界交互与相机

- [ ] 9.1 配置 GF.Sound 的 BGM、UI、Player、Boss、Environment 分组和 DataTable/资源条目
- [ ] 9.2 迁移 BGM、UI hover/click、移动、Dash、拾取/掉落、受击、Boss 攻击/潜地与环境声音映射
- [ ] 9.3 实现声音分组持久静音、同类重叠/单例策略和场景退出清理测试
- [ ] 9.4 迁移 `1开头.mp4` 一次播放与 `2循环.mp4` 循环切换、取消和场景退出清理
- [ ] 9.5 迁移 Grass trigger 动画与脚步声覆盖，验证禁用/退出时恢复
- [ ] 9.6 迁移 CameraShow 视差与背景层行为
- [ ] 9.7 迁移 Cinemachine 距离尺寸/构图、Confiner、Impulse 与死亡相机行为
- [ ] 9.8 运行多次进入/退出 Boss 场景的媒体、声音、Entity 与相机泄漏检查

## 10. URP 与 Spine 视觉迁移

- [ ] 10.1 记录 Spine 3.8 URP Shader 模块候选的来源、精确版本/提交、许可证和哈希
- [ ] 10.2 在隔离样板中验证候选模块可在 Unity 2022.3.62f3c1 + URP 14.0.12 编译和渲染 Spine 3.8 数据
- [ ] 10.3 建立 11 个 Boss 材质的源 Shader、纹理、关键属性与目标 Shader 映射表
- [ ] 10.4 迁移 8 个 Spine/Skeleton 材质并逐项验证 PMA、透明度、顶点色、遮罩和排序
- [ ] 10.5 迁移 3 个 Spine/Skeleton Fill 材质并逐项验证填充色、透明度、遮罩和排序
- [ ] 10.6 升级其他 Sprite、粒子与自定义材质到受支持 URP Shader，确保无粉色/Fallback 输出
- [ ] 10.7 固定 StartMenu、Boss1、Boss2 的相同分辨率/相机视觉检查点并生成源基准截图
- [ ] 10.8 生成目标截图并逐检查点比较角色、Boss、UI、特效、遮罩、颜色和层级，记录差异处理
- [ ] 10.9 执行全项目 missing/unsupported shader、missing material 与渲染器引用扫描

## 11. 自动化验证与工程质量

- [ ] 11.1 完成并运行所有 Combat/Input/Time/Spawn/StateMachine EditMode 测试，保存 XML 与日志
- [ ] 11.2 完成并运行 Launch→StartMenu、三场景切换、暂停、UI、Entity、Sound PlayMode 冒烟测试
- [ ] 11.3 在全新 Library 条件下运行 Unity 批处理导入与编译，要求零错误和零包解析失败
- [ ] 11.4 运行 missing script、missing GUID、Animation Event receiver、Spine listener 和序列化引用审计
- [ ] 11.5 运行资源 manifest 对账，要求每个源资产有唯一处置且无新增 GUID 冲突
- [ ] 11.6 运行输入边界、框架纯度、静态单例/Find/Resources 路径和 Update 分配风险扫描
- [ ] 11.7 检查所有事件订阅、CancellationToken、Entity/Form 回收和场景退出清理的对称性
- [ ] 11.8 复核废弃脚本和测试热键移除证据，确保没有误删运行路径
- [ ] 11.9 复核 `behavior-fixes.md`，确保所有可见差异均已记录并有回归检查

## 12. 三场景人工回归与完成审计

- [ ] 12.1 从 Launch 执行 StartMenu 完整清单：双视频、所有菜单/设置/返回/退出按钮、音频/key tip、加载和测试关卡入口
- [ ] 12.2 从 Launch 执行 Boss1 完整清单：移动/Dash/受击/死亡、三武器三颜色、教程、六攻击、无敌、HUD、暂停、重开和胜利
- [ ] 12.3 从 Launch 执行 Boss2 完整清单：三武器四色、潜地、全部投射物/标记、尾部阈值、HUD、暂停、死亡/重开和最终结果
- [ ] 12.4 对 StartMenu、Boss1、Boss2 的画面、音频、视频、输入、Animation Event 与 Spine Event 保存配对证据
- [ ] 12.5 对照功能追踪矩阵逐行审计，任何缺失、弱证据或失败项保持未完成并回到对应实现任务
- [ ] 12.6 复核源项目未被迁移操作修改，记录目标分支提交序列和可回滚点
- [ ] 12.7 运行最终 OpenSpec validate/verify，仅在全部任务和直接证据通过后归档变更
