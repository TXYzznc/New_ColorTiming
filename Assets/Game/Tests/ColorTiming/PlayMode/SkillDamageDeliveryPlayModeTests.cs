using System.Reflection;
using ColorTiming.Combat;
using ColorTiming.Presentation.Combat;
using NUnit.Framework;
using UnityEngine;

namespace ColorTiming.Tests.PlayMode
{
    public sealed class SkillDamageDeliveryPlayModeTests
    {
        static readonly MethodInfo OnHit = typeof(Skill_base).GetMethod(
            "OnHit",
            BindingFlags.Instance | BindingFlags.NonPublic);

        [Test]
        public void BossSkill_ForwardsPayloadToReceiverOnTaggedParent()
        {
            var targetRoot = new GameObject("PlayerDamageReceiver") { tag = "Player" };
            var targetChild = new GameObject("PlayerHitbox");
            var skillObject = new GameObject("BossSkill");
            try
            {
                targetChild.transform.SetParent(targetRoot.transform);
                var receiver = targetRoot.AddComponent<DamageProbe>();
                var collider = targetChild.AddComponent<BoxCollider2D>();
                var skill = skillObject.AddComponent<Skill_base>();
                skill.cTag = "Player";
                skill.SetSkillData(
                    ActorId.BossHead,
                    new WeaponIdentity(WeaponColor.Red, WeaponType.Normal),
                    1,
                    string.Empty);

                Assert.That(OnHit, Is.Not.Null);
                OnHit.Invoke(skill, new object[] { collider, skillObject });

                Assert.That(receiver.ReceivedCount, Is.EqualTo(1));
                Assert.That(receiver.LastDamage.Attacker, Is.EqualTo(ActorId.BossHead));
                Assert.That(receiver.LastDamage.Target, Is.EqualTo(ActorId.Player));
                Assert.That(receiver.LastDamage.Weapon.Color, Is.EqualTo(WeaponColor.Red));
            }
            finally
            {
                Object.DestroyImmediate(skillObject);
                Object.DestroyImmediate(targetRoot);
            }
        }

        private sealed class DamageProbe : MonoBehaviour, IBattleDamageReceiver
        {
            public int ReceivedCount { get; private set; }
            public BattleDamage LastDamage { get; private set; }
            public ActorId DamageActorId => ActorId.Player;

            public void ReceiveDamage(BattleDamage damage)
            {
                ReceivedCount++;
                LastDamage = damage;
            }
        }
    }
}
