using System;
using ColorTiming.Combat;
using CombatWeaponType = ColorTiming.Combat.WeaponType;

namespace ColorTiming.Player
{
    /// <summary>
    /// Owns the player's single weapon slot without depending on scene objects.
    /// Normal represents the empty-hand fallback used by the legacy animator.
    /// </summary>
    public sealed class PlayerWeaponInventory
    {
        public PlayerWeaponInventory()
        {
            Current = new WeaponIdentity(WeaponColor.Red, CombatWeaponType.Normal);
        }

        public WeaponIdentity Current { get; private set; }
        public bool IsEmpty => Current.IsNormal;
        public event Action<WeaponIdentity> Changed;

        public bool TryPickup(WeaponIdentity weapon)
        {
            if (weapon.IsNormal || !IsEmpty)
            {
                return false;
            }

            Set(weapon);
            return true;
        }

        public bool TryDrop(out WeaponIdentity dropped)
        {
            dropped = Current;
            if (IsEmpty)
            {
                return false;
            }

            Set(new WeaponIdentity(WeaponColor.Red, CombatWeaponType.Normal));
            return true;
        }

        public bool ConsumeAttackWeapon(out WeaponIdentity consumed)
        {
            return TryDrop(out consumed);
        }

        private void Set(WeaponIdentity weapon)
        {
            Current = weapon;
            Changed?.Invoke(Current);
        }
    }
}
