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

        public void AdvanceFrame(GameInputFrame nextFrame)
        {
            frame = nextFrame;
            gameplaySuppressed = false;
            anyPressConsumed = false;
        }

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
        public void SetFrame(GameInputFrame frame)
        {
            AdvanceFrame(frame);
        }
    }
}
