using System;

namespace ColorTiming.Combat
{
    public enum BattleResult
    {
        InProgress,
        Victory,
        Defeat,
    }

    public sealed class Health
    {
        public Health(int maximum)
        {
            if (maximum <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximum));
            }

            Maximum = maximum;
            Current = maximum;
        }

        public int Maximum { get; }
        public int Current { get; private set; }
        public bool IsEmpty => Current == 0;

        public int Damage(int amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            var before = Current;
            Current = Math.Max(0, Current - amount);
            return before - Current;
        }

        public int Heal(int amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            var before = Current;
            Current = Math.Min(Maximum, Current + amount);
            return Current - before;
        }
    }

    public enum PlayerDamageResolution
    {
        RejectedInvulnerable,
        RejectedCompleted,
        Damaged,
        Defeated,
    }

    public sealed class PlayerVitality
    {
        private bool resultEmitted;

        public PlayerVitality(int maximumHealth = 5)
        {
            Health = new Health(maximumHealth);
        }

        public Health Health { get; }
        public BattleResult Result { get; private set; } = BattleResult.InProgress;
        public event Action Defeated;

        public PlayerDamageResolution TakeDamage(int amount, bool invulnerable, bool instantKill = false)
        {
            if (Result != BattleResult.InProgress)
            {
                return PlayerDamageResolution.RejectedCompleted;
            }
            if (invulnerable)
            {
                return PlayerDamageResolution.RejectedInvulnerable;
            }

            Health.Damage(instantKill ? Health.Current : amount);
            if (!Health.IsEmpty)
            {
                return PlayerDamageResolution.Damaged;
            }

            Result = BattleResult.Defeat;
            if (!resultEmitted)
            {
                resultEmitted = true;
                Defeated?.Invoke();
            }
            return PlayerDamageResolution.Defeated;
        }

        public int ResolveSuccessfulDash()
        {
            return Result == BattleResult.InProgress ? Health.Heal(1) : 0;
        }
    }
}
