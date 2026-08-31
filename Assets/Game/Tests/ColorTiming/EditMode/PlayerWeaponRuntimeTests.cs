using System.Collections.Generic;
using ColorTiming.Combat;
using ColorTiming.Player;
using NUnit.Framework;
using CombatWeaponType = ColorTiming.Combat.WeaponType;

namespace ColorTiming.Tests.EditMode
{
    public sealed class PlayerWeaponRuntimeTests
    {
        [Test]
        public void Inventory_AllowsOnlyOneWeapon_AndReturnsDroppedIdentity()
        {
            var inventory = new PlayerWeaponInventory();
            var scissors = new WeaponIdentity(WeaponColor.Purple, CombatWeaponType.Scissors);

            Assert.That(inventory.TryPickup(scissors), Is.True);
            Assert.That(inventory.TryPickup(new WeaponIdentity(WeaponColor.Red, CombatWeaponType.Bomb)), Is.False);
            Assert.That(inventory.TryDrop(out var dropped), Is.True);
            Assert.That(dropped, Is.EqualTo(scissors));
            Assert.That(inventory.IsEmpty, Is.True);
        }

        [Test]
        public void Inventory_EquipOrSwap_ReplacesHeldWeaponAtomically()
        {
            var inventory = new PlayerWeaponInventory();
            var held = new WeaponIdentity(WeaponColor.Green, CombatWeaponType.Hammer);
            var incoming = new WeaponIdentity(WeaponColor.Purple, CombatWeaponType.Scissors);

            Assert.That(inventory.TryPickup(held), Is.True);
            Assert.That(inventory.TryEquipOrSwap(incoming, out var replaced), Is.True);
            Assert.That(replaced, Is.EqualTo(held));
            Assert.That(inventory.Current, Is.EqualTo(incoming));
        }

        [Test]
        public void SpawnerRuntime_UsesClock_AndGuaranteesCurrentWeakness()
        {
            var runtime = new WeaponSpawnerRuntime(
                5f,
                new WeaponSpawnPolicy(new[]
                {
                    new WeaponIdentity(WeaponColor.Red, CombatWeaponType.Scissors),
                    new WeaponIdentity(WeaponColor.Green, CombatWeaponType.Hammer),
                    new WeaponIdentity(WeaponColor.Purple, CombatWeaponType.Bomb),
                }, activeLimit: 5),
                new SeededRandomSource(17));
            var active = new List<WeaponColor>
            {
                WeaponColor.Red,
                WeaponColor.Red,
                WeaponColor.Green,
            };

            var first = runtime.Tick(0f, active, WeaponColor.Purple);
            var beforeInterval = runtime.Tick(4.9f, active, WeaponColor.Purple);
            var afterInterval = runtime.Tick(0.2f, active, WeaponColor.Purple);

            Assert.That(first.ShouldSpawn, Is.True);
            Assert.That(first.Weapon.Color, Is.EqualTo(WeaponColor.Purple));
            Assert.That(beforeInterval.ShouldSpawn, Is.False);
            Assert.That(afterInterval.ShouldSpawn, Is.True);
            Assert.That(afterInterval.Weapon.Color, Is.EqualTo(WeaponColor.Purple));
        }

        [TestCase(WeaponColor.Red, CombatWeaponType.Scissors)]
        [TestCase(WeaponColor.Green, CombatWeaponType.Hammer)]
        [TestCase(WeaponColor.Purple, CombatWeaponType.Bomb)]
        [TestCase(WeaponColor.Orange, CombatWeaponType.Airplane)]
        public void AnimatorIndex_RoundTripsDomainIdentity(
            WeaponColor color,
            CombatWeaponType type)
        {
            var identity = new WeaponIdentity(color, type);
            Assert.That(
                WeaponIdentity.FromLegacyAnimatorIndex(identity.ToLegacyAnimatorIndex()),
                Is.EqualTo(identity));
        }
    }
}
