using System.Collections.Generic;
using ColorTiming.Combat;
using NUnit.Framework;

namespace ColorTiming.Tests.EditMode
{
    public sealed class WeaknessSlotLedgerTests
    {
        [Test]
        public void OrangeConsumption_UsesOrangeSlotWithoutMutatingPurple()
        {
            var ledger = new WeaknessSlotLedger(new Dictionary<WeaponColor, IEnumerable<int>>
            {
                [WeaponColor.Purple] = new[] { 6, 2, 4 },
                [WeaponColor.Orange] = new[] { 5, 1, 3 },
            });

            var consumed = ledger.Consume(WeaponColor.Orange);

            Assert.That(consumed, Is.EqualTo(5));
            Assert.That(ledger.Remaining(WeaponColor.Orange), Is.EqualTo(new[] { 1, 3 }));
            Assert.That(ledger.Remaining(WeaponColor.Purple), Is.EqualTo(new[] { 6, 2, 4 }));
        }
    }
}
