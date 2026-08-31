using System;
using System.Linq;
using ColorTiming.Application.Battle;
using ColorTiming.Combat;
using NUnit.Framework;
using CombatWeaponType = ColorTiming.Combat.WeaponType;

namespace ColorTiming.Tests.EditMode
{
    public sealed class BattleSessionTests
    {
        [TestCase(BattleKind.Boss1, 11)]
        [TestCase(BattleKind.Boss2, 15)]
        public void Constructor_CreatesExpectedWeaknessCount(BattleKind kind, int expected)
        {
            using var session = new BattleSession(TestConfigurationFactory.Battle(kind), new SeededRandomSource(7));
            Assert.That(session.Snapshot.Weaknesses.Count, Is.EqualTo(expected));
            Assert.That(session.Snapshot.PlayerHealth, Is.EqualTo(5));
            Assert.That(session.Snapshot.Lifecycle, Is.EqualTo(BattleLifecycle.Running));
        }

        [Test]
        public void WrongColor_DoesNotMutateBossState()
        {
            using var session = new BattleSession(TestConfigurationFactory.Battle(BattleKind.Boss1), new SeededRandomSource(3));
            var before = session.Snapshot;
            var wrong = Enum.GetValues(typeof(WeaponColor)).Cast<WeaponColor>()
                .First(color => color != before.Weaknesses[0]);
            var resolution = session.ApplyBossDamage(BossDamage(wrong));
            Assert.That(resolution, Is.EqualTo(BossDamageResolution.RejectedWrongColor));
            Assert.That(session.Snapshot, Is.SameAs(before));
        }

        [Test]
        public void CorrectColors_EmitOneTerminalVictory()
        {
            using var session = new BattleSession(TestConfigurationFactory.Battle(BattleKind.Boss1), new SeededRandomSource(11));
            var wins = 0;
            session.PresentationRequested += message =>
            {
                if (message.Kind == BattlePresentationEventKind.BattleWon) wins++;
            };
            while (session.Snapshot.Weaknesses.Count > 0)
            {
                session.ApplyBossDamage(BossDamage(session.Snapshot.Weaknesses[0]));
            }
            Assert.That(session.Snapshot.Lifecycle, Is.EqualTo(BattleLifecycle.Victory));
            Assert.That(wins, Is.EqualTo(1));
            Assert.That(session.ApplyBossDamage(BossDamage(WeaponColor.Red)), Is.EqualTo(BossDamageResolution.RejectedCompleted));
            Assert.That(wins, Is.EqualTo(1));
        }

        [Test]
        public void PlayerDamage_DropsWeaponAndEndsExactlyOnce()
        {
            using var session = new BattleSession(TestConfigurationFactory.Battle(BattleKind.Boss1, 1), new SeededRandomSource(2));
            Assert.That(session.TryPickup(new WeaponIdentity(WeaponColor.Green, CombatWeaponType.Hammer)), Is.True);
            var losses = 0;
            session.PresentationRequested += message =>
            {
                if (message.Kind == BattlePresentationEventKind.BattleLost) losses++;
            };
            var result = session.ApplyPlayerDamage(new BattleDamage(
                ActorId.BossHead, ActorId.Player,
                new WeaponIdentity(WeaponColor.Red, CombatWeaponType.Normal),
                new CombatPoint(0f, 0f)));
            Assert.That(result, Is.EqualTo(PlayerDamageResolution.Defeated));
            Assert.That(session.Snapshot.Weapon.IsNormal, Is.True);
            Assert.That(session.Snapshot.Lifecycle, Is.EqualTo(BattleLifecycle.Defeat));
            Assert.That(losses, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_IsIdempotentAndRejectsLaterCommands()
        {
            var session = new BattleSession(TestConfigurationFactory.Battle(BattleKind.Boss2), new SeededRandomSource(1));
            session.Dispose();
            session.Dispose();
            Assert.That(session.Snapshot.Lifecycle, Is.EqualTo(BattleLifecycle.Disposed));
            Assert.Throws<ObjectDisposedException>(() => session.TryBeginAttack());
        }

        private static BattleDamage BossDamage(WeaponColor color)
        {
            return new BattleDamage(
                ActorId.Player,
                ActorId.BossHead,
                new WeaponIdentity(color, CombatWeaponType.Scissors),
                new CombatPoint(1f, 2f));
        }
    }
}
