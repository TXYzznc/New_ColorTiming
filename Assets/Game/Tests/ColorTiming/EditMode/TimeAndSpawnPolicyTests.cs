using System;
using ColorTiming.Combat;
using ColorTiming.Player;
using NUnit.Framework;
using CombatWeaponType = ColorTiming.Combat.WeaponType;

namespace ColorTiming.Tests.EditMode
{
    public sealed class TimeAndSpawnPolicyTests
    {
        [Test]
        public void TimeRequestsRestoreCorrectlyOutOfOrder()
        {
            var time = new TimeScaleCoordinator();
            var dash = time.Acquire(0.45f);
            var pause = time.Acquire(0f);

            Assert.That(time.EffectiveScale, Is.Zero);
            dash.Dispose();
            Assert.That(time.EffectiveScale, Is.Zero);
            pause.Dispose();
            Assert.That(time.EffectiveScale, Is.EqualTo(1f));
            pause.Dispose();
            Assert.That(time.EffectiveScale, Is.EqualTo(1f));
        }

        [Test]
        public void BattleEndResetRestoresTimeAndInvalidatesOutstandingScopes()
        {
            var time = new TimeScaleCoordinator();
            var hitStop = time.Acquire(0.1f);
            var pause = time.Acquire(0f);

            time.Reset();
            hitStop.Dispose();
            pause.Dispose();

            Assert.That(time.EffectiveScale, Is.EqualTo(1f));
        }

        [Test]
        public void Boss1SpawnGuaranteesMissingCurrentColorAtThreshold()
        {
            var policy = WeaponSpawnPolicy.Boss1();
            var decision = policy.Decide(
                new[] { WeaponColor.Red, WeaponColor.Red, WeaponColor.Green },
                WeaponColor.Purple,
                new SeededRandomSource(1));

            Assert.That(decision.ShouldSpawn, Is.True);
            Assert.That(decision.Weapon.Color, Is.EqualTo(WeaponColor.Purple));
            Assert.That(
                new[] { CombatWeaponType.Scissors, CombatWeaponType.Hammer, CombatWeaponType.Bomb },
                Does.Contain(decision.Weapon.Type));
        }

        [Test]
        public void Boss2SpawnSupportsOrangeAndOnlyBoss2Families()
        {
            var policy = WeaponSpawnPolicy.Boss2();
            var decision = policy.Decide(
                new[] { WeaponColor.Red, WeaponColor.Green, WeaponColor.Purple },
                WeaponColor.Orange,
                new SeededRandomSource(4));

            Assert.That(decision.Weapon.Color, Is.EqualTo(WeaponColor.Orange));
            Assert.That(
                new[] { CombatWeaponType.Knife, CombatWeaponType.Axe, CombatWeaponType.Airplane },
                Does.Contain(decision.Weapon.Type));
        }

        [Test]
        public void SpawnAtLimitProducesNoDecision()
        {
            var policy = WeaponSpawnPolicy.Boss1(activeLimit: 2);
            var decision = policy.Decide(
                new[] { WeaponColor.Red, WeaponColor.Green },
                WeaponColor.Purple,
                new SeededRandomSource(3));

            Assert.That(decision.ShouldSpawn, Is.False);
        }

        [Test]
        public void SpawnClockIsReadyImmediatelyAndFiresOnThresholdCrossing()
        {
            var clock = new WeaponSpawnClock(5f);

            Assert.That(clock.Tick(0.016f), Is.True);
            Assert.That(clock.Tick(4f), Is.False);
            Assert.That(clock.Tick(1.1f), Is.True);
            Assert.That(clock.Tick(0f), Is.False);
        }
    }
}
