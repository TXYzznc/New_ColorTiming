// 文件职责：定义 武器展示状态 数据及其状态语义。
// 所属模块：ColorTiming / Presentation / UI / Models。

using ColorTiming.Combat;

namespace ColorTiming.Presentation.UI.Models
{
    /// <summary>
    /// Converts domain weapon identity into the authored HUD array layout.
    /// Keeping this mapping outside MonoBehaviour code makes every color/type combination testable.
    /// </summary>
    public readonly struct WeaponPresentationState
    {
        private WeaponPresentationState(int iconIndex, int cursorIndex, bool usesChargeHint)
        {
            IconIndex = iconIndex;
            CursorIndex = cursorIndex;
            UsesChargeHint = usesChargeHint;
        }

        public int IconIndex { get; }
        public int CursorIndex { get; }
        public bool UsesChargeHint { get; }

        // 执行From对应的主要流程。
        public static WeaponPresentationState From(ColorTimingWeaponTable row)
        {
            return new WeaponPresentationState(row.IconIndex, row.CursorIndex, row.UsesChargeHint);
        }
    }
}
