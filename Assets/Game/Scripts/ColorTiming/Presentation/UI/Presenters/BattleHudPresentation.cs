using System;
using ColorTiming.Application.Battle;

namespace ColorTiming.Presentation.UI.Presenters
{
    /// <summary>Session-backed presentation source required by the battle HUD.</summary>
    public sealed class BattleHudPresentation
    {
        public BattleHudPresentation(BattleSession session)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public BattleSession Session { get; }
    }
}
