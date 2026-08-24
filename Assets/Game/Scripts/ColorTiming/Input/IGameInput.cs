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

        bool ConsumeAnyPressForOverlay();
    }
}
