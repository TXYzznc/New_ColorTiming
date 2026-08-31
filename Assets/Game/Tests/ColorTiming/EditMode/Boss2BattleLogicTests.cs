using ColorTiming.Bosses.Boss2;
using NUnit.Framework;

namespace ColorTiming.Tests.EditMode
{
    public sealed class Boss2BattleLogicTests
    {
        [TestCase(13f, false, 0.1f, Boss2Action.Burrow)]
        [TestCase(13f, false, 0.9f, Boss2Action.Projectile)]
        [TestCase(5f, true, 0.1f, Boss2Action.Burrow)]
        [TestCase(5f, true, 0.9f, Boss2Action.Projectile)]
        [TestCase(8f, false, 0.4f, Boss2Action.Melee)]
        [TestCase(10f, false, 0.4f, Boss2Action.Projectile)]
        public void HeadSelector_PreservesDistanceAndFacingRules(
            float distance,
            bool facingAway,
            float sample,
            Boss2Action expected)
        {
            Assert.That(Boss2ActionSelector.SelectHead(distance, facingAway, sample, TestConfigurationFactory.Boss2Actions()), Is.EqualTo(expected));
        }

        [TestCase(11f, false, 0.9f, Boss2Action.Burrow)]
        [TestCase(5f, true, 0.9f, Boss2Action.Burrow)]
        [TestCase(5f, false, 0.2f, Boss2Action.Melee)]
        [TestCase(5f, false, 0.8f, Boss2Action.Projectile)]
        public void TailSelector_PreservesBurrowAndAttackRules(
            float distance,
            bool facingAway,
            float sample,
            Boss2Action expected)
        {
            Assert.That(Boss2ActionSelector.SelectTail(distance, facingAway, sample, TestConfigurationFactory.Boss2Actions()), Is.EqualTo(expected));
        }

        [Test]
        public void TailActivation_IsSingleShotOnlyOnTwelveToElevenTransition()
        {
            var phase = new Boss2PhaseCoordinator(15, 11);

            Assert.That(phase.ObserveRemaining(14), Is.False);
            Assert.That(phase.ObserveRemaining(13), Is.False);
            Assert.That(phase.ObserveRemaining(12), Is.False);
            Assert.That(phase.ObserveRemaining(11), Is.True);
            Assert.That(phase.ObserveRemaining(10), Is.False);
            Assert.That(phase.IsTailActive, Is.True);
        }

        [Test]
        public void BurrowFlow_RejectsOutOfOrderTransitions_AndCanBeInterrupted()
        {
            var flow = new Boss2BurrowFlow();

            Assert.That(flow.EnterHiddenMovement(), Is.False);
            Assert.That(flow.BeginEntering(), Is.True);
            Assert.That(flow.EnterHiddenMovement(), Is.True);
            Assert.That(flow.BeginEmerging(), Is.True);
            Assert.That(flow.CompleteEmerging(), Is.True);
            flow.BeginEntering();
            flow.Interrupt();
            Assert.That(flow.State, Is.EqualTo(Boss2BurrowState.AboveGround));
        }
    }
}
