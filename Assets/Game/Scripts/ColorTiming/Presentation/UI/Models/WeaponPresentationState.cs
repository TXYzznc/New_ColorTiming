using ColorTiming.Combat;

namespace ColorTiming.Presentation.UI.Models
{
    /// <summary>
    /// Converts domain weapon identity into the authored HUD array layout.
    /// Keeping this mapping outside MonoBehaviour code makes every color/type combination testable.
    /// </summary>
    public readonly struct WeaponPresentationState
    {
        public const int NormalIconIndex = 18;
        public const int NormalCursorIndex = 0;
        public const int HeldNormalCursorIndex = 5;
        public const int PauseCursorIndex = 6;
        public const int RequiredIconCount = 24;
        public const int RequiredCursorCount = 7;

        private WeaponPresentationState(int iconIndex, int cursorIndex, bool usesChargeHint)
        {
            IconIndex = iconIndex;
            CursorIndex = cursorIndex;
            UsesChargeHint = usesChargeHint;
        }

        public int IconIndex { get; }
        public int CursorIndex { get; }
        public bool UsesChargeHint { get; }

        public static WeaponPresentationState From(WeaponIdentity identity)
        {
            var iconIndex = identity.IsNormal
                ? NormalIconIndex
                : identity.ToLegacyAnimatorIndex() - 1;
            var cursorIndex = identity.IsNormal
                ? NormalCursorIndex
                : (int)identity.Color + 1;
            var usesChargeHint = identity.Type == ColorTiming.Combat.WeaponType.Hammer
                || identity.Type == ColorTiming.Combat.WeaponType.Axe;

            return new WeaponPresentationState(iconIndex, cursorIndex, usesChargeHint);
        }
    }
}
