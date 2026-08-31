using System.Collections.Generic;
using ColorTiming.Bosses.Boss1;
using ColorTiming.Bosses.Boss2;
using ColorTiming.Combat;
using ColorTiming.Configuration;

namespace ColorTiming.Tests.EditMode
{
    internal static class TestConfigurationFactory
    {
        public static BattleRulesConfiguration Battle(BattleKind kind, int maximumHealth = 5)
        {
            var weaknesses = kind == BattleKind.Boss1
                ? new WeaknessComposition(4, 3, 4, 0, 7)
                : new WeaknessComposition(4, 4, 4, 3, 7);
            return new BattleRulesConfiguration(kind, new PlayerCombatRules(maximumHealth, 1, 1, 1f),
                weaknesses, kind == BattleKind.Boss2 ? 11 : 0);
        }

        public static Boss1AttackRules Boss1Attacks()
        {
            return new Boss1AttackRules(new List<WeightedBoss1Attack>
            {
                new WeightedBoss1Attack(Boss1DistanceZone.Near, Boss1Attack.Attack2, .35f, false, Boss1Attack.Attack2),
                new WeightedBoss1Attack(Boss1DistanceZone.Near, Boss1Attack.Attack1, .25f, false, Boss1Attack.Attack1),
                new WeightedBoss1Attack(Boss1DistanceZone.Near, Boss1Attack.Attack3, .27f, false, Boss1Attack.Attack3),
                new WeightedBoss1Attack(Boss1DistanceZone.Near, Boss1Attack.Attack4, .13f, false, Boss1Attack.Attack4),
                new WeightedBoss1Attack(Boss1DistanceZone.Middle, Boss1Attack.Attack6, .35f, false, Boss1Attack.Attack6),
                new WeightedBoss1Attack(Boss1DistanceZone.Middle, Boss1Attack.Attack1, .25f, false, Boss1Attack.Attack1),
                new WeightedBoss1Attack(Boss1DistanceZone.Middle, Boss1Attack.Attack3, .27f, false, Boss1Attack.Attack3),
                new WeightedBoss1Attack(Boss1DistanceZone.Middle, Boss1Attack.Attack4, .13f, false, Boss1Attack.Attack4),
                new WeightedBoss1Attack(Boss1DistanceZone.Far, Boss1Attack.Attack3, .4f, false, Boss1Attack.Attack3),
                new WeightedBoss1Attack(Boss1DistanceZone.Far, Boss1Attack.Attack4, .35f, false, Boss1Attack.Attack4),
                new WeightedBoss1Attack(Boss1DistanceZone.Far, Boss1Attack.Attack5, .25f, true, Boss1Attack.Attack3),
            });
        }

        public static Boss2ActionRules Boss2Actions() => new Boss2ActionRules(12f, 9f, .7f, 10f, .5f);
    }
}
