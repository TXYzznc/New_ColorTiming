// 文件职责：定义 Game输入 的依赖契约，供模块间解耦使用。
// 所属模块：ColorTiming / Input。

using UnityEngine;

namespace ColorTiming.Input
{
    public interface IGameInput
    {
        Vector2 Move { get; }
        bool DashPressed { get; }
        bool AttackPressed { get; }
        bool AttackHeld { get; }
        bool DropPressed { get; }
        bool PausePressed { get; }
        Vector2 PointerScreenPosition { get; }
        bool AnyPressed { get; }
        bool ConfirmPressed { get; }
        bool DebugBoss1Attack5PrimaryPressed { get; }
        bool DebugBoss1Attack5SecondaryPressed { get; }

        // 执行ConsumeAnyPressForOverlay对应的主要流程。
        bool ConsumeAnyPressForOverlay();
    }
}
