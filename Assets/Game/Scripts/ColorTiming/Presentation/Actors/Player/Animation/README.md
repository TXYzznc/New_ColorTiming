# Animation

负责把玩家业务动作和武器状态转换为具体动画表现。

`IPlayerAnimationDriver` 是 Mecanim 与未来 Spine 实现共享的语义边界；`MecanimPlayerAnimationDriver` 管理当前 Sprite Animator 的参数、层权重和武器 Controller 安全切换。

本目录不持有业务库存，不加载或修改美术资源。
