using System.Linq;
using ColorTiming.Combat;
using ColorTiming.Presentation.UI;
using ColorTiming.Presentation.Audio;
using NUnit.Framework;
using CombatWeaponType = ColorTiming.Combat.WeaponType;

namespace ColorTiming.Tests.EditMode
{
    public sealed class CombatDomainTests
    {
        [Test]
        public void AllLegacyWeaponPresentationIndicesRoundTrip()
        {
            for (var index = 1; index <= 24; index++)
            {
                Assert.That(WeaponIdentity.FromLegacyAnimatorIndex(index).ToLegacyAnimatorIndex(), Is.EqualTo(index));
            }
        }

        [Test]
        public void WeaponPresentationMapsEveryColorAndWeaponToAuthoredSlots()
        {
            foreach (WeaponColor color in System.Enum.GetValues(typeof(WeaponColor)))
            {
                foreach (CombatWeaponType type in System.Enum.GetValues(typeof(CombatWeaponType)))
                {
                    var identity = new WeaponIdentity(color, type);
                    var presentation = WeaponPresentationState.From(identity);

                    Assert.That(presentation.IconIndex, Is.EqualTo(
                        identity.IsNormal ? WeaponPresentationState.NormalIconIndex : identity.ToLegacyAnimatorIndex() - 1));
                    Assert.That(presentation.CursorIndex, Is.EqualTo(
                        identity.IsNormal ? WeaponPresentationState.NormalCursorIndex : (int)color + 1));
                    Assert.That(presentation.UsesChargeHint, Is.EqualTo(
                        type == CombatWeaponType.Hammer || type == CombatWeaponType.Axe));
                    Assert.That(presentation.IconIndex, Is.InRange(0, WeaponPresentationState.RequiredIconCount - 1));
                    Assert.That(presentation.CursorIndex, Is.InRange(0, WeaponPresentationState.RequiredCursorCount - 1));
                }
            }
        }

        [Test]
        public void FrameworkSoundNamesIncludeFolderAndWaveExtension()
        {
            Assert.That(
                GfColorTimingSoundService.BuildRelativeName("第一章bgm0526", ColorTimingSoundChannel.BGM),
                Is.EqualTo("BGM/第一章bgm0526.wav"));
            Assert.That(
                GfColorTimingSoundService.BuildRelativeName("amb_cave", ColorTimingSoundChannel.Environment),
                Is.EqualTo("amb_cave.wav"));
            Assert.That(
                GfColorTimingSoundService.BuildRelativeName("ui_click.wav", ColorTimingSoundChannel.UI),
                Is.EqualTo("ui_click.wav"));
        }

        [Test]
        public void Boss1WeaknessDistributionIsStableAcrossShuffle()
        {
            var queue = WeaknessQueue.CreateBoss1(new SeededRandomSource(17));

            Assert.That(queue.Count, Is.EqualTo(11));
            Assert.That(queue.CountOf(WeaponColor.Red), Is.EqualTo(4));
            Assert.That(queue.CountOf(WeaponColor.Green), Is.EqualTo(3));
            Assert.That(queue.CountOf(WeaponColor.Purple), Is.EqualTo(4));
            Assert.That(queue.CountOf(WeaponColor.Orange), Is.Zero);
            Assert.That(queue.Upcoming().Count, Is.EqualTo(7));
        }

        [Test]
        public void Boss2WeaknessDistributionIncludesOrange()
        {
            var queue = WeaknessQueue.CreateBoss2(new SeededRandomSource(23));

            Assert.That(queue.Count, Is.EqualTo(15));
            Assert.That(queue.CountOf(WeaponColor.Red), Is.EqualTo(4));
            Assert.That(queue.CountOf(WeaponColor.Green), Is.EqualTo(4));
            Assert.That(queue.CountOf(WeaponColor.Purple), Is.EqualTo(4));
            Assert.That(queue.CountOf(WeaponColor.Orange), Is.EqualTo(3));
        }

        [Test]
        public void BossRejectsWrongColorAndInvulnerabilityWithoutMutation()
        {
            var queue = WeaknessQueue.CreateBoss1(new SeededRandomSource(2));
            var boss = new BossBattleHealth(queue);
            var current = queue.Current;
            var wrong = current == WeaponColor.Red ? WeaponColor.Green : WeaponColor.Red;

            var wrongResult = boss.Apply(Request(wrong));
            boss.IsDamageable = false;
            var invulnerableResult = boss.Apply(Request(current));

            Assert.That(wrongResult, Is.EqualTo(BossDamageResolution.RejectedWrongColor));
            Assert.That(invulnerableResult, Is.EqualTo(BossDamageResolution.RejectedInvulnerable));
            Assert.That(queue.Count, Is.EqualTo(11));
        }

        [Test]
        public void BossVictoryIsSingleShot()
        {
            var queue = WeaknessQueue.CreateBoss1(new SeededRandomSource(7));
            var boss = new BossBattleHealth(queue);
            var victories = 0;
            boss.Victory += () => victories++;

            while (!queue.IsEmpty)
            {
                boss.Apply(Request(queue.Current));
            }

            var afterVictory = boss.Apply(Request(WeaponColor.Red));
            Assert.That(boss.Result, Is.EqualTo(BattleResult.Victory));
            Assert.That(victories, Is.EqualTo(1));
            Assert.That(afterVictory, Is.EqualTo(BossDamageResolution.RejectedCompleted));
        }

        [Test]
        public void PlayerHealthClampsHealsAndDefeatIsSingleShot()
        {
            var player = new PlayerVitality();
            var defeats = 0;
            player.Defeated += () => defeats++;

            Assert.That(player.TakeDamage(1, false), Is.EqualTo(PlayerDamageResolution.Damaged));
            Assert.That(player.ResolveSuccessfulDash(), Is.EqualTo(1));
            Assert.That(player.ResolveSuccessfulDash(), Is.Zero);
            Assert.That(player.Health.Current, Is.EqualTo(5));
            Assert.That(player.TakeDamage(10, true), Is.EqualTo(PlayerDamageResolution.RejectedInvulnerable));
            Assert.That(player.TakeDamage(1, false, true), Is.EqualTo(PlayerDamageResolution.Defeated));
            Assert.That(player.TakeDamage(1, false), Is.EqualTo(PlayerDamageResolution.RejectedCompleted));
            Assert.That(defeats, Is.EqualTo(1));
        }

        [Test]
        public void UpcomingProjectionDoesNotMutateQueue()
        {
            var queue = WeaknessQueue.CreateBoss2(new SeededRandomSource(5));
            var before = queue.Upcoming(15).ToArray();

            var projection = queue.Upcoming(7).ToArray();

            Assert.That(projection, Is.EqualTo(before.Take(7).ToArray()));
            Assert.That(queue.Count, Is.EqualTo(15));
        }

        private static DamageRequest Request(WeaponColor color)
        {
            return new DamageRequest(null, new WeaponIdentity(color, CombatWeaponType.Scissors), new CombatPoint(1f, 2f));
        }
    }
}
