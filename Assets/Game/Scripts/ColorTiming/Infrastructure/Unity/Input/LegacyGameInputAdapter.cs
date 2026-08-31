// 文件职责：把旧版 Unity Input 读取转换为语义化游戏输入。
// 所属模块：ColorTiming / Infrastructure / Unity / Input。

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
        public bool DebugBoss1Attack5PrimaryPressed => state.DebugBoss1Attack5PrimaryPressed;
        public bool DebugBoss1Attack5SecondaryPressed => state.DebugBoss1Attack5SecondaryPressed;

        // 逐帧推进需要实时刷新的业务或表现状态。
        private void Update()
        {
            var debugBoss1Attack5PrimaryPressed = false;
            var debugBoss1Attack5SecondaryPressed = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            debugBoss1Attack5PrimaryPressed = UnityEngine.Input.GetKeyDown(KeyCode.Alpha1);
            debugBoss1Attack5SecondaryPressed = UnityEngine.Input.GetKeyDown(KeyCode.Alpha2);
#endif

            state.AdvanceFrame(new GameInputFrame(
                new Vector2(UnityEngine.Input.GetAxis("Horizontal"), UnityEngine.Input.GetAxis("Vertical")),
                UnityEngine.Input.GetButtonDown("Jump"),
                UnityEngine.Input.GetButtonDown("Fire1"),
                UnityEngine.Input.GetButton("Fire1"),
                UnityEngine.Input.GetButtonDown("Fire2"),
                UnityEngine.Input.GetKeyDown(KeyCode.Escape),
                UnityEngine.Input.mousePosition,
                UnityEngine.Input.anyKeyDown,
                UnityEngine.Input.GetButtonDown("Submit") || UnityEngine.Input.GetButtonDown("Fire1"),
                debugBoss1Attack5PrimaryPressed,
                debugBoss1Attack5SecondaryPressed));
        }

        // 执行ConsumeAnyPressForOverlay对应的主要流程。
        public bool ConsumeAnyPressForOverlay()
        {
            return state.ConsumeAnyPressForOverlay();
        }
    }
}
