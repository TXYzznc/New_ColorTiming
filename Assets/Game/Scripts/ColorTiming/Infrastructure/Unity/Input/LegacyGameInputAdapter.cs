using ColorTiming.Input;
using UnityEngine;

namespace ColorTiming.Infrastructure.Unity.Input
{
    [DefaultExecutionOrder(-9000)]
    public sealed class LegacyGameInputAdapter : MonoBehaviour, IGameInput
    {
        private readonly GameInputState state = new GameInputState();

        public Vector2 Move => state.Move;
        public bool DashPressed => state.DashPressed;
        public bool AttackPressed => state.AttackPressed;
        public bool AttackHeld => state.AttackHeld;
        public bool DropPressed => state.DropPressed;
        public bool PausePressed => state.PausePressed;
        public Vector2 PointerScreenPosition => state.PointerScreenPosition;
        public bool AnyPressed => state.AnyPressed;
        public bool ConfirmPressed => state.ConfirmPressed;

        private void Update()
        {
            state.AdvanceFrame(new GameInputFrame(
                new Vector2(UnityEngine.Input.GetAxis("Horizontal"), UnityEngine.Input.GetAxis("Vertical")),
                UnityEngine.Input.GetButtonDown("Jump"),
                UnityEngine.Input.GetButtonDown("Fire1"),
                UnityEngine.Input.GetButton("Fire1"),
                UnityEngine.Input.GetButtonDown("Fire2"),
                UnityEngine.Input.GetKeyDown(KeyCode.Escape),
                UnityEngine.Input.mousePosition,
                UnityEngine.Input.anyKeyDown,
                UnityEngine.Input.GetButtonDown("Submit") || UnityEngine.Input.GetButtonDown("Fire1")));
        }

        public bool ConsumeAnyPressForOverlay()
        {
            return state.ConsumeAnyPressForOverlay();
        }
    }
}
