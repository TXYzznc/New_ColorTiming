using ColorTiming.Bosses.Boss1;
using NUnit.Framework;

namespace ColorTiming.Tests.EditMode
{
    public sealed class Boss1BattleLogicTests
    {
        [TestCase(true, true, Boss1DistanceZone.Near)]
        [TestCase(true, false, Boss1DistanceZone.Near)]
        [TestCase(false, true, Boss1DistanceZone.Middle)]
        [TestCase(false, false, Boss1DistanceZone.Far)]
        public void DistanceZones_HaveStableNearToFarPrecedence(
            bool near,
            bool middle,
            Boss1DistanceZone expected)
        {
            Assert.That(Boss1DistanceZones.Resolve(near, middle), Is.EqualTo(expected));
        }

        [TestCase(Boss1DistanceZone.Near, 0.1f, Boss1Attack.Attack2)]
        [TestCase(Boss1DistanceZone.Near, 0.5f, Boss1Attack.Attack1)]
        [TestCase(Boss1DistanceZone.Near, 0.7f, Boss1Attack.Attack3)]
        [TestCase(Boss1DistanceZone.Near, 0.95f, Boss1Attack.Attack4)]
        [TestCase(Boss1DistanceZone.Middle, 0.1f, Boss1Attack.Attack6)]
        [TestCase(Boss1DistanceZone.Middle, 0.5f, Boss1Attack.Attack1)]
        [TestCase(Boss1DistanceZone.Middle, 0.7f, Boss1Attack.Attack3)]
        [TestCase(Boss1DistanceZone.Middle, 0.95f, Boss1Attack.Attack4)]
        [TestCase(Boss1DistanceZone.Far, 0.1f, Boss1Attack.Attack3)]
        [TestCase(Boss1DistanceZone.Far, 0.5f, Boss1Attack.Attack4)]
        [TestCase(Boss1DistanceZone.Far, 0.9f, Boss1Attack.Attack5)]
        public void AttackSelector_PreservesSourceWeightBoundaries(
            Boss1DistanceZone zone,
            float sample,
            Boss1Attack expected)
        {
            Assert.That(new Boss1AttackSelector().Select(zone, sample), Is.EqualTo(expected));
        }

        [Test]
        public void FarZone_DoesNotSelectAttack5TwiceInARow()
        {
            var selector = new Boss1AttackSelector();

            Assert.That(selector.Select(Boss1DistanceZone.Far, 0.9f), Is.EqualTo(Boss1Attack.Attack5));
            Assert.That(selector.Select(Boss1DistanceZone.Far, 0.9f), Is.EqualTo(Boss1Attack.Attack3));
        }

        [Test]
        public void AttackCycle_BlocksOverlapAndRequiresCooldown()
        {
            var cycle = new Boss1AttackCycle(3f);

            Assert.That(cycle.Tick(2.9f), Is.False);
            Assert.That(cycle.Tick(0.1f), Is.True);
            Assert.That(cycle.BeginAttack(), Is.True);
            Assert.That(cycle.BeginAttack(), Is.False);
            Assert.That(cycle.Tick(10f), Is.False);
            cycle.CompleteAttack(2f);
            Assert.That(cycle.Tick(2f), Is.True);
        }
    }
}
