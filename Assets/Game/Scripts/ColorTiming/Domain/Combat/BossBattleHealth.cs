using System;

namespace ColorTiming.Combat
{
    public enum BossDamageResolution
    {
        RejectedInvulnerable,
        RejectedWrongColor,
        RejectedCompleted,
        Damaged,
        Victory,
    }

    public sealed class BossBattleHealth
    {
        private bool victoryEmitted;

        public BossBattleHealth(WeaknessQueue weaknesses)
        {
            Weaknesses = weaknesses ?? throw new ArgumentNullException(nameof(weaknesses));
            if (weaknesses.IsEmpty)
            {
                throw new ArgumentException("A boss must start with at least one weakness.", nameof(weaknesses));
            }
        }

        public WeaknessQueue Weaknesses { get; }
        public bool IsDamageable { get; set; } = true;
        public BattleResult Result { get; private set; } = BattleResult.InProgress;
        public event Action<WeaponColor> DamageAccepted;
        public event Action Victory;

        public BossDamageResolution Apply(BattleDamage request)
        {
            if (Result != BattleResult.InProgress)
            {
                return BossDamageResolution.RejectedCompleted;
            }
            if (!IsDamageable)
            {
                return BossDamageResolution.RejectedInvulnerable;
            }
            if (request.Weapon.Color != Weaknesses.Current)
            {
                return BossDamageResolution.RejectedWrongColor;
            }

            var removed = Weaknesses.RemoveCurrent();
            DamageAccepted?.Invoke(removed);
            if (!Weaknesses.IsEmpty)
            {
                return BossDamageResolution.Damaged;
            }

            Result = BattleResult.Victory;
            if (!victoryEmitted)
            {
                victoryEmitted = true;
                Victory?.Invoke();
            }
            return BossDamageResolution.Victory;
        }
    }
}
