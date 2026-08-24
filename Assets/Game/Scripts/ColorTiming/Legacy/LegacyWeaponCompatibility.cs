using ColorTiming.Combat;

/// <summary>
/// Compatibility vocabulary retained for serialized scenes and animation-event APIs.
/// New gameplay rules use <see cref="WeaponIdentity"/> through <see cref="Identity"/>.
/// </summary>
public sealed class Weapon
{
    public Weapon(ColorType color, WeaponType type)
        : this(WeaponIdentity.FromLegacy((int)color, (int)type))
    {
    }

    public Weapon(WeaponIdentity identity)
    {
        Identity = identity;
    }

    public WeaponIdentity Identity { get; }
    public ColorType colorType => (ColorType)Identity.Color;
    public WeaponType weaponType => (WeaponType)Identity.Type;

    public int GetIntType()
    {
        return Identity.ToLegacyAnimatorIndex();
    }
}

public enum WeaponType
{
    nor,
    jiandao,
    chuizhi,
    zhadan,
    meigongdao,
    futou,
    zhifeiji,
}

public enum ColorType
{
    hong,
    lv,
    zi,
    chen,
}
