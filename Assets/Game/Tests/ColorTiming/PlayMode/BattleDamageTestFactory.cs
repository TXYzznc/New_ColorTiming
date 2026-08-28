using ColorTiming.Combat;
using UnityEngine;

namespace ColorTiming.Tests.PlayMode
{
    internal static class BattleDamageTestFactory
    {
        public static BattleDamage ToPlayer(Vector2 point, string parameter)
        {
            return new BattleDamage(
                ActorId.BossHead,
                ActorId.Player,
                new WeaponIdentity(WeaponColor.Red, ColorTiming.Combat.WeaponType.Normal),
                new CombatPoint(point.x, point.y),
                parameter);
        }

        public static BattleDamage ToBoss(WeaponColor color, string parameter, ActorId? target = null)
        {
            return new BattleDamage(
                ActorId.Player,
                target ?? ActorId.BossHead,
                new WeaponIdentity(color, ColorTiming.Combat.WeaponType.Normal),
                new CombatPoint(0f, 0f),
                parameter);
        }
    }
}
