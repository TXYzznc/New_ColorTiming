using System;

namespace ColorTiming.Combat
{
    public readonly struct CombatPoint
    {
        public CombatPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; }
        public float Y { get; }
    }

    public sealed class DamageRequest
    {
        public DamageRequest(object attacker, WeaponIdentity weapon, CombatPoint contactPoint, string parameter = "")
        {
            Attacker = attacker;
            Weapon = weapon;
            ContactPoint = contactPoint;
            Parameter = parameter ?? string.Empty;
        }

        public object Attacker { get; }
        public WeaponIdentity Weapon { get; }
        public CombatPoint ContactPoint { get; }
        public string Parameter { get; }
        public bool IsInstantKill => Parameter.IndexOf("miaosha", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
