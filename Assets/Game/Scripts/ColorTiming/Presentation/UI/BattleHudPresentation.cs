using System;
using ColorTiming.Combat;

namespace ColorTiming.Presentation.UI
{
    /// <summary>Explicit scene-owned actor references required by the battle HUD.</summary>
    public sealed class BattleHudPresentation
    {
        public BattleHudPresentation(HeroController hero, Boss1_Controller boss1, Boss2_Controller boss2)
        {
            Hero = hero ?? throw new ArgumentNullException(nameof(hero));
            if ((boss1 == null) == (boss2 == null))
            {
                throw new ArgumentException("A battle HUD requires exactly one supported boss.");
            }

            Boss1 = boss1;
            Boss2 = boss2;
        }

        public HeroController Hero { get; }
        public Boss1_Controller Boss1 { get; }
        public Boss2_Controller Boss2 { get; }
    }
}
