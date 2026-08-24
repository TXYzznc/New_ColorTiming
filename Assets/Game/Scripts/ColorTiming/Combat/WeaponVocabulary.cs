using System;

namespace ColorTiming.Combat
{
    public enum WeaponColor
    {
        Red = 0,
        Green = 1,
        Purple = 2,
        Orange = 3,
    }

    public enum WeaponType
    {
        Normal = 0,
        Scissors = 1,
        Hammer = 2,
        Bomb = 3,
        Knife = 4,
        Axe = 5,
        Airplane = 6,
    }

    public readonly struct WeaponIdentity : IEquatable<WeaponIdentity>
    {
        public WeaponIdentity(WeaponColor color, WeaponType type)
        {
            if (!Enum.IsDefined(typeof(WeaponColor), color))
            {
                throw new ArgumentOutOfRangeException(nameof(color));
            }
            if (!Enum.IsDefined(typeof(WeaponType), type))
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            Color = color;
            Type = type;
        }

        public WeaponColor Color { get; }
        public WeaponType Type { get; }
        public bool IsNormal => Type == WeaponType.Normal;

        public int ToLegacyAnimatorIndex()
        {
            return IsNormal ? 0 : (int)Type + (int)Color * 6;
        }

        public static WeaponIdentity FromLegacy(int colorIndex, int weaponTypeIndex)
        {
            return new WeaponIdentity((WeaponColor)colorIndex, (WeaponType)weaponTypeIndex);
        }

        public static WeaponIdentity FromLegacyAnimatorIndex(int animatorIndex)
        {
            if (animatorIndex == 0)
            {
                return new WeaponIdentity(WeaponColor.Red, WeaponType.Normal);
            }
            if (animatorIndex < 1 || animatorIndex > 24)
            {
                throw new ArgumentOutOfRangeException(nameof(animatorIndex));
            }

            return new WeaponIdentity(
                (WeaponColor)((animatorIndex - 1) / 6),
                (WeaponType)(((animatorIndex - 1) % 6) + 1));
        }

        public bool Equals(WeaponIdentity other) => Color == other.Color && Type == other.Type;
        public override bool Equals(object obj) => obj is WeaponIdentity other && Equals(other);
        public override int GetHashCode() => ((int)Color * 397) ^ (int)Type;
        public override string ToString() => $"{Color}/{Type}";
    }
}
