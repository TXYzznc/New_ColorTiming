// 文件职责：定义 Game输入状态 数据及其状态语义。
// 所属模块：ColorTiming / Input。

using UnityEngine;

namespace ColorTiming.Input
{
    public class GameInputState : IGameInput
    {
        private GameInputFrame frame;
        private bool gameplaySuppressed;
        private bool anyPressConsumed;

        public Vector2 Move => gameplaySuppressed ? Vector2.zero : frame.Move;
        public bool DashPressed => !gameplaySuppressed && frame.DashPressed;
        public bool AttackPressed => !gameplaySuppressed && frame.AttackPressed;
        public bool AttackHeld => !gameplaySuppressed && frame.AttackHeld;
        public bool DropPressed => !gameplaySuppressed && frame.DropPressed;
        public bool PausePressed => frame.PausePressed;
        public Vector2 PointerScreenPosition => frame.PointerScreenPosition;
        public bool AnyPressed => !anyPressConsumed && frame.AnyPressed;
        public bool ConfirmPressed => !gameplaySuppressed && frame.ConfirmPressed;
        public bool DebugBoss1Attack5PrimaryPressed => !gameplaySuppressed && frame.DebugBoss1Attack5PrimaryPressed;
        public bool DebugBoss1Attack5SecondaryPressed => !gameplaySuppressed && frame.DebugBoss1Attack5SecondaryPressed;

        // 执行Advance帧对应的主要流程。
        public void AdvanceFrame(GameInputFrame nextFrame)
        {
            frame = nextFrame;
            gameplaySuppressed = false;
            anyPressConsumed = false;
        }

        // 执行ConsumeAnyPressForOverlay对应的主要流程。
        public bool ConsumeAnyPressForOverlay()
        {
            if (anyPressConsumed || !frame.AnyPressed)
            {
                return false;
            }

            anyPressConsumed = true;
            gameplaySuppressed = true;
            return true;
        }
    }

    public sealed class FakeGameInput : GameInputState
    {
        // 设置帧，并使后续流程使用最新状态。
        public void SetFrame(GameInputFrame frame)
        {
            AdvanceFrame(frame);
        }
    }
}
