// 文件职责：定义 玩家武器Inventory，承担 玩家 模块中的对应职责。
// 所属模块：ColorTiming / Domain / Player。

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
        // 初始化玩家武器Inventory实例及其核心依赖。
        public PlayerWeaponInventory()
        {
            Current = new WeaponIdentity(WeaponColor.Red, CombatWeaponType.Normal);
        }

        public WeaponIdentity Current { get; private set; }
        public bool IsEmpty => Current.IsNormal;
        public event Action<WeaponIdentity> Changed;

        // 尝试拾取，并通过返回值报告是否成功。
        public bool TryPickup(WeaponIdentity weapon)
        {
            if (weapon.IsNormal || !IsEmpty)
            {
                return false;
            }

            Set(weapon);
            return true;
        }

        // 尝试丢弃，并通过返回值报告是否成功。
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

        // 执行Consume攻击武器对应的主要流程。
        public bool ConsumeAttackWeapon(out WeaponIdentity consumed)
        {
            return TryDrop(out consumed);
        }

        // 写入新的值并替换旧状态。
        private void Set(WeaponIdentity weapon)
        {
            Current = weapon;
            Changed?.Invoke(Current);
        }
    }
}
