// 文件职责：定义 Game输入帧，承担 输入 模块中的对应职责。
// 所属模块：ColorTiming / Input。

using UnityEngine;

namespace ColorTiming.Input
{
    public readonly struct GameInputFrame
    {
        public GameInputFrame(
            Vector2 move,
            bool dashPressed,
            bool attackPressed,
            bool attackHeld,
            bool dropPressed,
            bool pausePressed,
            Vector2 pointerScreenPosition,
            bool anyPressed,
            bool confirmPressed)
        {
            Move = move;
            DashPressed = dashPressed;
            AttackPressed = attackPressed;
            AttackHeld = attackHeld;
            DropPressed = dropPressed;
            PausePressed = pausePressed;
            PointerScreenPosition = pointerScreenPosition;
            AnyPressed = anyPressed;
            ConfirmPressed = confirmPressed;
        }

        public Vector2 Move { get; }
        public bool DashPressed { get; }
        public bool AttackPressed { get; }
        public bool AttackHeld { get; }
        public bool DropPressed { get; }
        public bool PausePressed { get; }
        public Vector2 PointerScreenPosition { get; }
        public bool AnyPressed { get; }
        public bool ConfirmPressed { get; }

        public static GameInputFrame Empty => new GameInputFrame(
            Vector2.zero, false, false, false, false, false, Vector2.zero, false, false);
    }
}
