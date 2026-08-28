// 文件职责：定义 Combat生命值，承担 Combat 模块中的对应职责。
// 所属模块：ColorTiming / Domain / Combat。

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
        // 初始化生命值实例及其核心依赖。
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

        // 执行伤害对应的主要流程。
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

        // 执行Heal对应的主要流程。
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

        // 初始化玩家Vitality实例及其核心依赖。
        public PlayerVitality(int maximumHealth = 5)
        {
            Health = new Health(maximumHealth);
        }

        public Health Health { get; }
        public BattleResult Result { get; private set; } = BattleResult.InProgress;
        public event Action Defeated;

        // 执行Take伤害对应的主要流程。
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

        // 解析成功冲刺并返回可供上层使用的结果。
        public int ResolveSuccessfulDash()
        {
            return Result == BattleResult.InProgress ? Health.Heal(1) : 0;
        }
    }
}
