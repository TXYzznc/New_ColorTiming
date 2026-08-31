// 文件职责：定义由 GF DataTable 注入领域层的只读业务规则。
// 所属模块：ColorTiming / Domain / Configuration。

using System;
using System.Collections.Generic;
using System.Linq;
using ColorTiming.Bosses.Boss1;
using ColorTiming.Bosses.Boss2;
using ColorTiming.Combat;

namespace ColorTiming.Configuration
{
    public readonly struct WeaknessComposition
    {
        public WeaknessComposition(int red, int green, int purple, int orange, int upcomingLimit)
        {
            if (red < 0 || green < 0 || purple < 0 || orange < 0)
                throw new ArgumentOutOfRangeException(nameof(red), "Weakness counts cannot be negative.");
            if (red + green + purple + orange <= 0)
                throw new ArgumentException("A battle needs at least one weakness segment.");
            if (upcomingLimit <= 0) throw new ArgumentOutOfRangeException(nameof(upcomingLimit));
            Red = red;
            Green = green;
            Purple = purple;
            Orange = orange;
            UpcomingLimit = upcomingLimit;
        }

        public int Red { get; }
        public int Green { get; }
        public int Purple { get; }
        public int Orange { get; }
        public int UpcomingLimit { get; }
        public int Total => Red + Green + Purple + Orange;
    }

    public sealed class PlayerCombatRules
    {
        public PlayerCombatRules(int maximumHealth, int damagePerHit, int dashHeal, float hitInvulnerabilitySeconds)
        {
            if (maximumHealth <= 0) throw new ArgumentOutOfRangeException(nameof(maximumHealth));
            if (damagePerHit <= 0) throw new ArgumentOutOfRangeException(nameof(damagePerHit));
            if (dashHeal < 0) throw new ArgumentOutOfRangeException(nameof(dashHeal));
            if (hitInvulnerabilitySeconds < 0f) throw new ArgumentOutOfRangeException(nameof(hitInvulnerabilitySeconds));
            MaximumHealth = maximumHealth;
            DamagePerHit = damagePerHit;
            DashHeal = dashHeal;
            HitInvulnerabilitySeconds = hitInvulnerabilitySeconds;
        }

        public int MaximumHealth { get; }
        public int DamagePerHit { get; }
        public int DashHeal { get; }
        public float HitInvulnerabilitySeconds { get; }
    }

    public sealed class BattleRulesConfiguration
    {
        public BattleRulesConfiguration(
            BattleKind kind,
            PlayerCombatRules player,
            WeaknessComposition weaknesses,
            int tailActivationRemaining)
        {
            Kind = kind;
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Weaknesses = weaknesses;
            if (kind == BattleKind.Boss2 && (tailActivationRemaining < 0 || tailActivationRemaining >= weaknesses.Total))
                throw new ArgumentOutOfRangeException(nameof(tailActivationRemaining));
            TailActivationRemaining = tailActivationRemaining;
        }

        public BattleKind Kind { get; }
        public PlayerCombatRules Player { get; }
        public WeaknessComposition Weaknesses { get; }
        public int TailActivationRemaining { get; }
    }

    public readonly struct WeightedBoss1Attack
    {
        public WeightedBoss1Attack(Boss1DistanceZone zone, Boss1Attack attack, float weight, bool disallowRepeat, Boss1Attack fallback)
        {
            if (weight <= 0f) throw new ArgumentOutOfRangeException(nameof(weight));
            Zone = zone;
            Attack = attack;
            Weight = weight;
            DisallowRepeat = disallowRepeat;
            Fallback = fallback;
        }

        public Boss1DistanceZone Zone { get; }
        public Boss1Attack Attack { get; }
        public float Weight { get; }
        public bool DisallowRepeat { get; }
        public Boss1Attack Fallback { get; }
    }

    public sealed class Boss1AttackRules
    {
        private readonly Dictionary<Boss1DistanceZone, WeightedBoss1Attack[]> rules;

        public Boss1AttackRules(IEnumerable<WeightedBoss1Attack> entries)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));
            rules = entries.GroupBy(entry => entry.Zone)
                .ToDictionary(group => group.Key, group => group.ToArray());
            foreach (Boss1DistanceZone zone in Enum.GetValues(typeof(Boss1DistanceZone)))
            {
                if (!rules.TryGetValue(zone, out var zoneRules) || zoneRules.Length == 0)
                    throw new ArgumentException($"Boss1 attack rules are missing zone '{zone}'.", nameof(entries));
                var total = zoneRules.Sum(entry => entry.Weight);
                if (Math.Abs(total - 1f) > 0.001f)
                    throw new ArgumentException($"Boss1 zone '{zone}' weights must total 1, actual {total}.", nameof(entries));
            }
        }

        public IReadOnlyList<WeightedBoss1Attack> For(Boss1DistanceZone zone) => rules[zone];
    }

    public sealed class Boss2ActionRules
    {
        public Boss2ActionRules(float headFarDistance, float headMeleeDistance, float headBurrowWeight,
            float tailFarDistance, float tailMeleeWeight)
        {
            if (headFarDistance <= headMeleeDistance || headMeleeDistance < 0f)
                throw new ArgumentOutOfRangeException(nameof(headFarDistance));
            if (tailFarDistance < 0f) throw new ArgumentOutOfRangeException(nameof(tailFarDistance));
            if (headBurrowWeight < 0f || headBurrowWeight > 1f)
                throw new ArgumentOutOfRangeException(nameof(headBurrowWeight));
            if (tailMeleeWeight < 0f || tailMeleeWeight > 1f)
                throw new ArgumentOutOfRangeException(nameof(tailMeleeWeight));
            HeadFarDistance = headFarDistance;
            HeadMeleeDistance = headMeleeDistance;
            HeadBurrowWeight = headBurrowWeight;
            TailFarDistance = tailFarDistance;
            TailMeleeWeight = tailMeleeWeight;
        }

        public float HeadFarDistance { get; }
        public float HeadMeleeDistance { get; }
        public float HeadBurrowWeight { get; }
        public float TailFarDistance { get; }
        public float TailMeleeWeight { get; }
    }
}
