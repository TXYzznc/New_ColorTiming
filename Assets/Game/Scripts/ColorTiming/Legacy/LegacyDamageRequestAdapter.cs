using ColorTiming.Combat;
using UnityEngine;

public static class LegacyDamageRequestAdapter
{
    public static void Deliver(I_Damage receiver, DamageRequest request)
    {
        if (receiver == null || request == null)
        {
            return;
        }

        receiver.OnDamage(
            request.Attacker as GameObject,
            new Weapon(request.Weapon),
            new Vector2(request.ContactPoint.X, request.ContactPoint.Y),
            request.Parameter);
    }
}
