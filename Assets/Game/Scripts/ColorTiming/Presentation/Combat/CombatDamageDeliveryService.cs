// 文件职责：集中执行伤害阶段与逻辑目标的唯一结算，隔离 Unity Collider 细节。
// 所属模块：ColorTiming / Presentation / Combat。

using System;
using System.Runtime.CompilerServices;
using ColorTiming.Combat;
using UnityEngine;

namespace ColorTiming.Presentation.Combat
{
    public enum DamageDeliveryResult
    {
        Delivered,
        SuppressedDuplicateTarget,
    }

    public interface ICombatDamageDeliveryService
    {
        DamageDeliveryResult TryDeliver(
            DamagePhaseContext phase,
            IBattleDamageReceiver receiver,
            BattleDamage damage,
            out int targetHandle);
    }

    public interface ICombatDamageDeliveryConsumer
    {
        void BindCombatDamageDelivery(ICombatDamageDeliveryService service);
    }

    public sealed class CombatDamageDeliveryService : ICombatDamageDeliveryService
    {
        public DamageDeliveryResult TryDeliver(
            DamagePhaseContext phase,
            IBattleDamageReceiver receiver,
            BattleDamage damage,
            out int targetHandle)
        {
            if (phase == null) throw new ArgumentNullException(nameof(phase));
            if (receiver == null) throw new ArgumentNullException(nameof(receiver));

            targetHandle = ResolveTargetHandle(receiver);
            if (!phase.TryRegisterTarget(targetHandle))
            {
                return DamageDeliveryResult.SuppressedDuplicateTarget;
            }

            receiver.ReceiveDamage(damage);
            return DamageDeliveryResult.Delivered;
        }

        private static int ResolveTargetHandle(IBattleDamageReceiver receiver)
        {
            // A receiver's Component is the stable logical target root for all of its child
            // colliders. The fallback keeps pure test doubles supported without Unity objects.
            return receiver is Component component
                ? component.GetInstanceID()
                : RuntimeHelpers.GetHashCode(receiver);
        }
    }
}
