# Player

负责玩家动画、镜头、技能、死亡和音效表现。

直接脚本：`PlayerActorView.cs`、`PlayerAnimationEventRelay.cs`、`PlayerCameraLifecycleView.cs`、`PlayerChargeHintView.cs`、`PlayerDeathSequenceView.cs`、`PlayerSkillEmitter.cs`、`PlayerSoundView.cs`。

`Animation/` 隔离玩家业务动作与 Mecanim/未来 Spine 的具体实现。

`Player.prefab` 只保存角色内部结构和美术引用；数值、关卡音效和镜头参数来自 GF DataTable，场景对象引用由 `BattlePlayerManager` 在运行时配置。

修改本目录代码后，应执行 Unity 编译和对应测试。
