using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ColorTiming.Bosses.Boss2;
using ColorTiming.Application.Battle;
using ColorTiming.Bootstrap;
using ColorTiming.Combat;
using ColorTiming.Configuration;
using ColorTiming.Infrastructure.GF.Entity;
using ColorTiming.Presentation.Entities;
using ColorTiming.Presentation.UI.Components;
using ColorTiming.Presentation.UI.Forms;
using NUnit.Framework;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ColorTiming.Tests.PlayMode
{
    public sealed class BossAttackExecutionPlayModeTests
    {
        const float BootTimeout = 30f;
        const float TransitionTimeout = 20f;
        const float AttackTimeout = 15f;

        static readonly MethodInfo PlayBoss1Animation = typeof(Boss1ActorView).GetMethod(
            "AnimPlay",
            BindingFlags.Instance | BindingFlags.NonPublic);

        static readonly MethodInfo PlayBoss2HeadAnimation = typeof(Boss2ActorView).GetMethod(
            "AnimPlay",
            BindingFlags.Instance | BindingFlags.NonPublic);

        static readonly MethodInfo PlayBoss2TailAnimation = typeof(Boss2TailActorView).GetMethod(
            "AnimPlay",
            BindingFlags.Instance | BindingFlags.NonPublic);

        static readonly FieldInfo Boss2HeadCooldown = typeof(Boss2ActorView).GetField(
            "atkCD",
            BindingFlags.Instance | BindingFlags.NonPublic);

        static readonly FieldInfo Boss2HeadBurrow = typeof(Boss2ActorView).GetField(
            "burrowFlow",
            BindingFlags.Instance | BindingFlags.NonPublic);

        static readonly FieldInfo Boss2TailCooldown = typeof(Boss2TailActorView).GetField(
            "atkCD",
            BindingFlags.Instance | BindingFlags.NonPublic);

        static readonly FieldInfo Boss2TailBurrow = typeof(Boss2TailActorView).GetField(
            "burrowFlow",
            BindingFlags.Instance | BindingFlags.NonPublic);

        static readonly FieldInfo SkillHasDamagePayload = typeof(Skill_base).GetField(
            "hasDamagePayload",
            BindingFlags.Instance | BindingFlags.NonPublic);

        [UnityTest]
        [Timeout(180000)]
        public IEnumerator Boss1_AllSixAttacksPlayAndDispatchTheirAuthoredSpineEvents()
        {
            yield return BootToStartMenu();
            FindActive<MainMenuForm>().GoTest1();
            yield return WaitForScene("Boss1", TransitionTimeout);
            yield return ColorTimingPlayModeBoot.WaitForBattleReady("Boss1", TransitionTimeout);

            AssertAuthoredBossSkillLifetimes();

            var boss = FindActive<Boss1ActorView>();
            var presentation = boss.GetComponent<Boss1AnimationEventRelay>();
            Assert.That(boss, Is.Not.Null);
            Assert.That(presentation, Is.Not.Null);
            Assert.That(PlayBoss1Animation, Is.Not.Null);
            Assert.That(SkillHasDamagePayload, Is.Not.Null);
            var session = FindActive<BattleRuntimeContext>()?.Session;
            Assert.That(session, Is.Not.Null);
            // Keep the authored Spine tracks and callbacks live while preventing the
            // controller Update loop from starting a random attack over the forced contract run.
            boss.enabled = false;

            yield return VerifyAttack(
                boss,
                boss.skeletonAnimation1,
                "attack_1_test1_60fps",
                session,
                false,
                presentation.sk1.name);
            yield return VerifyAttack(
                boss,
                boss.skeletonAnimation1,
                "attack_2_test1_60fps",
                session,
                false,
                presentation.sk2.name);
            yield return VerifyAttack(
                boss,
                boss.skeletonAnimation1,
                "attack_3_test2_60fps",
                session,
                false,
                presentation.sk3.name,
                presentation.sk3_1.name);
            yield return VerifyAttack(
                boss,
                boss.skeletonAnimation1,
                "attack_4_test1_60fps",
                session,
                false,
                presentation.sk1.name,
                presentation.sk3.name,
                presentation.sk3_1.name);
            yield return VerifyAttack(
                boss,
                boss.skeletonAnimation2,
                "attack_5_test1_60fps2",
                session,
                true,
                presentation.sk5.name);
            yield return VerifyAttack(
                boss,
                boss.skeletonAnimation1,
                "attack_6_60fps",
                session,
                false,
                presentation.sk6.name);

            var hud = FindActive<BattlePlayerInfoView>();
            Assert.That(hud, Is.Not.Null);
            hud.TogglePause();
            yield return WaitUntil(() => FindActive<PauseMenuForm>() != null, 10f,
                "Boss1 attack contract cleanup could not open the pause form.");
            FindActive<PauseMenuForm>().BackMenu();
            yield return WaitForScene("StartMenu", TransitionTimeout);
        }

        [UnityTest]
        [Timeout(240000)]
        public IEnumerator Boss2_HeadTailBurrowAndAttackEventsExecuteThroughFrameworkEntities()
        {
            yield return BootToStartMenu();
            FindActive<MainMenuForm>().GoTest2();
            yield return WaitForScene("Boss2", TransitionTimeout);
            yield return ColorTimingPlayModeBoot.WaitForBattleReady("Boss2", TransitionTimeout);

            var boss = FindActive<Boss2ActorView>();
            var presentation = boss.GetComponent<Boss2AnimationEventRelay>();
            var tail = UnityEngine.Object.FindObjectsOfType<Boss2TailActorView>(true).Single();
            Assert.That(boss, Is.Not.Null);
            Assert.That(presentation, Is.Not.Null);
            Assert.That(tail, Is.Not.Null);
            Assert.That(PlayBoss2HeadAnimation, Is.Not.Null);
            Assert.That(PlayBoss2TailAnimation, Is.Not.Null);
            Assert.That(Boss2HeadCooldown, Is.Not.Null);
            Assert.That(Boss2HeadBurrow, Is.Not.Null);
            Assert.That(Boss2TailCooldown, Is.Not.Null);
            Assert.That(Boss2TailBurrow, Is.Not.Null);

            // 本测试只验证 Boss 动画事件与实体生成合同。隔离玩家受击，避免长流程中
            // 实际技能随机击杀玩家并触发场景重载，使后续 Spine 引用失效。
            var player = FindActive<PlayerActorView>();
            Assert.That(player, Is.Not.Null);
            foreach (var playerCollider in player.GetComponentsInChildren<Collider2D>())
            {
                playerCollider.enabled = false;
            }

            // FixedUpdate must remain enabled for burrow movement; an infinite cooldown
            // suppresses only the random attack chooser while the forced contracts run.
            Boss2HeadCooldown.SetValue(boss, float.PositiveInfinity);
            yield return VerifyAnimationEvents(
                boss,
                PlayBoss2HeadAnimation,
                boss.skeletonAnimation1,
                "attack_1",
                presentation.sk1.name);
            Boss2HeadCooldown.SetValue(boss, float.PositiveInfinity);
            yield return VerifyAnimationEvents(
                boss,
                PlayBoss2HeadAnimation,
                boss.skeletonAnimation1,
                "attack_2",
                presentation.sk2.GetComponent<Skill_Bo2_atk2_s>().b.name);

            Boss2HeadCooldown.SetValue(boss, float.PositiveInfinity);
            yield return VerifyHeadBurrow(boss, presentation);

            // Stop only the head's Update/FixedUpdate loop before consuming the four
            // threshold segments. OnDamage and the authored Spine callbacks remain valid.
            boss.enabled = false;
            while (boss.Boss1HP.Count > 11)
            {
                var color = boss.Boss1HP[0];
                boss.ReceiveDamage(BattleDamageTestFactory.ToBoss(
                    (ColorTiming.Combat.WeaponColor)color, "tail-threshold-contract"));
            }
            Assert.That(tail.gameObject.activeSelf, Is.True,
                "The tail did not activate on the 12-to-11 transition.");

            yield return null;
            Boss2TailCooldown.SetValue(tail, float.PositiveInfinity);
            yield return VerifyInitialTailBurrow(tail);

            Boss2TailCooldown.SetValue(tail, float.PositiveInfinity);
            yield return VerifyAnimationEvents(
                tail,
                PlayBoss2TailAnimation,
                tail.skeletonAnimation,
                "attack_1",
                tail.sk1.name);
            Boss2TailCooldown.SetValue(tail, float.PositiveInfinity);
            yield return VerifyAnimationEvents(
                tail,
                PlayBoss2TailAnimation,
                tail.skeletonAnimation,
                "attack_2",
                tail.sk2.name);

            var hud = FindActive<BattlePlayerInfoView>();
            Assert.That(hud, Is.Not.Null);
            hud.TogglePause();
            yield return WaitUntil(() => FindActive<PauseMenuForm>() != null, 10f,
                "Boss2 attack contract cleanup could not open the pause form.");
            FindActive<PauseMenuForm>().BackMenu();
            yield return WaitForScene("StartMenu", TransitionTimeout);
        }

        static IEnumerator VerifyAttack(
            Boss1ActorView boss,
            SkeletonAnimation view,
            string animationName,
            BattleSession session,
            bool expectInvulnerability,
            params string[] expectedEntityNames)
        {
            yield return HideActiveTransientEntities(expectedEntityNames);
            Assert.That(session.Snapshot.BossDamageable, Is.True, $"{animationName} must start damageable.");

            var observedEvents = new List<string>();
            void RecordEvent(TrackEntry _, Spine.Event spineEvent)
            {
                observedEvents.Add($"{spineEvent}:{spineEvent.String}");
            }
            view.AnimationState.Event += RecordEvent;
            PlayBoss1Animation.Invoke(boss, new object[] { animationName, false });
            yield return null;

            Assert.That(view.gameObject.activeInHierarchy, Is.True,
                $"{animationName} selected the wrong authored Spine view.");
            var entry = view.AnimationState.GetCurrent(0);
            Assert.That(entry, Is.Not.Null);
            Assert.That(entry.Animation.Name, Is.EqualTo(animationName));

            var completed = false;
            var sawInvulnerability = false;
            var seenEntities = new HashSet<string>();
            entry.Complete += _ => completed = true;

            var deadline = Time.realtimeSinceStartup + AttackTimeout;
            while (Time.realtimeSinceStartup < deadline)
            {
                if (!session.Snapshot.BossDamageable)
                {
                    sawInvulnerability = true;
                }

                foreach (var entity in ActiveTransientEntities())
                {
                    foreach (var expected in expectedEntityNames)
                    {
                        if (entity.gameObject.name.StartsWith(expected, StringComparison.Ordinal))
                        {
                            seenEntities.Add(expected);
                            var skill = entity.GetComponent<Skill_base>();
                            Assert.That(skill, Is.Not.Null,
                                $"{animationName} spawned {expected} without a Skill_base damage adapter.");
                            Assert.That((bool)SkillHasDamagePayload.GetValue(skill), Is.True,
                                $"{animationName} spawned {expected} without Boss1 damage payload.");
                        }
                    }
                }

                if (completed
                    && expectedEntityNames.All(seenEntities.Contains)
                    && (!expectInvulnerability || sawInvulnerability))
                {
                    break;
                }
                yield return null;
            }

            Assert.That(completed, Is.True, $"{animationName} did not complete.");
            Assert.That(seenEntities, Is.SupersetOf(expectedEntityNames),
                $"{animationName} did not dispatch every authored attack entity event. " +
                $"Observed Spine events: [{string.Join(", ", observedEvents)}].");
            Assert.That(sawInvulnerability, Is.EqualTo(expectInvulnerability),
                $"{animationName} produced an unexpected invulnerability contract.");
            Assert.That(session.Snapshot.BossDamageable, Is.True,
                $"{animationName} did not restore boss damageability on completion.");
            view.AnimationState.Event -= RecordEvent;
        }

        static IEnumerator HideActiveTransientEntities(params string[] namesThatMustBeAbsent)
        {
            foreach (var entity in ActiveTransientEntities())
            {
                GFBuiltin.Entity.HideEntitySafe(entity);
            }
            yield return null;
            yield return null;
            Assert.That(
                ActiveTransientEntities().Any(entity => namesThatMustBeAbsent.Any(
                    expected => entity.gameObject.name.StartsWith(expected, StringComparison.Ordinal))),
                Is.False,
                "The previous attack retained a matching GF.Entity effect.");
        }

        static IEnumerator VerifyAnimationEvents(
            object owner,
            MethodInfo playMethod,
            SkeletonAnimation view,
            string animationName,
            params string[] expectedEntityNames)
        {
            yield return HideActiveTransientEntities(expectedEntityNames);
            var observedEvents = new List<string>();
            var observedEntities = new HashSet<string>();
            void RecordEvent(TrackEntry _, Spine.Event spineEvent)
            {
                observedEvents.Add($"{spineEvent}:{spineEvent.String}");
            }
            view.AnimationState.Event += RecordEvent;
            playMethod.Invoke(owner, new object[] { animationName, false });
            yield return null;

            var entry = view.AnimationState.GetCurrent(0);
            Assert.That(entry, Is.Not.Null);
            Assert.That(entry.Animation.Name, Is.EqualTo(animationName));
            var completed = false;
            var seenEntities = new HashSet<string>();
            entry.Complete += _ => completed = true;

            var deadline = Time.realtimeSinceStartup + AttackTimeout;
            while (Time.realtimeSinceStartup < deadline)
            {
                foreach (var entity in ActiveTransientEntities())
                {
                    observedEntities.Add(entity.gameObject.name);
                }
                RecordExpectedEntities(expectedEntityNames, seenEntities);
                if (completed && expectedEntityNames.All(seenEntities.Contains))
                {
                    break;
                }
                yield return null;
            }

            Assert.That(completed, Is.True, $"{animationName} did not complete.");
            Assert.That(seenEntities, Is.SupersetOf(expectedEntityNames),
                $"{animationName} did not dispatch every authored attack entity event. " +
                $"Observed Spine events: [{string.Join(", ", observedEvents)}]. " +
                $"Observed entities: [{string.Join(", ", observedEntities)}].");
            view.AnimationState.Event -= RecordEvent;
        }

        static IEnumerator VerifyHeadBurrow(Boss2ActorView boss, Boss2AnimationEventRelay presentation)
        {
            var expectedEntityNames = new[] { boss.dundiObj.name, presentation.sk0.name };
            yield return HideActiveTransientEntities(expectedEntityNames);
            var flow = (Boss2BurrowFlow)Boss2HeadBurrow.GetValue(boss);
            var collider = boss.GetComponent<PolygonCollider2D>();
            var origin = boss.transform.position;
            Assert.That(flow, Is.Not.Null);
            Assert.That(flow.BeginEntering(), Is.True);

            PlayBoss2HeadAnimation.Invoke(boss, new object[] { "RuTu", false });
            var seenStates = new HashSet<Boss2BurrowState>();
            var seenEntities = new HashSet<string>();
            var sawColliderDisabled = false;
            var deadline = Time.realtimeSinceStartup + 30f;
            while (Time.realtimeSinceStartup < deadline)
            {
                seenStates.Add(flow.State);
                if (collider != null)
                    sawColliderDisabled |= !collider.enabled;
                RecordExpectedEntities(expectedEntityNames, seenEntities);
                if (seenStates.Contains(Boss2BurrowState.HiddenMoving)
                    && seenStates.Contains(Boss2BurrowState.Emerging)
                    && flow.State == Boss2BurrowState.AboveGround
                    && boss.skeletonAnimation1.AnimationState.GetCurrent(0)?.Animation?.Name == "idel")
                {
                    break;
                }
                yield return null;
            }

            Assert.That(seenStates, Does.Contain(Boss2BurrowState.HiddenMoving));
            Assert.That(seenStates, Does.Contain(Boss2BurrowState.Emerging));
            Assert.That(flow.State, Is.EqualTo(Boss2BurrowState.AboveGround));
            Assert.That(sawColliderDisabled, Is.True);
            if (collider != null)
                Assert.That(collider.enabled, Is.True);
            Assert.That(Vector2.Distance(origin, boss.transform.position), Is.GreaterThan(4f),
                "Boss2 head did not relocate to a different authored anchor.");
            Assert.That(seenEntities, Is.SupersetOf(expectedEntityNames),
                "Boss2 burrow did not emit both trail and resurfacing entities.");
        }

        static IEnumerator VerifyInitialTailBurrow(Boss2TailActorView tail)
        {
            var flow = (Boss2BurrowFlow)Boss2TailBurrow.GetValue(tail);
            var collider = tail.GetComponent<PolygonCollider2D>();
            var seenStates = new HashSet<Boss2BurrowState>();
            var sawColliderDisabled = false;
            var deadline = Time.realtimeSinceStartup + 30f;
            while (Time.realtimeSinceStartup < deadline)
            {
                seenStates.Add(flow.State);
                sawColliderDisabled |= !collider.enabled;
                if (seenStates.Contains(Boss2BurrowState.HiddenMoving)
                    && seenStates.Contains(Boss2BurrowState.Emerging)
                    && flow.State == Boss2BurrowState.AboveGround
                    && tail.skeletonAnimation.AnimationState.GetCurrent(0)?.Animation?.Name == "idel")
                {
                    break;
                }
                yield return null;
            }

            Assert.That(seenStates, Does.Contain(Boss2BurrowState.HiddenMoving));
            Assert.That(seenStates, Does.Contain(Boss2BurrowState.Emerging));
            Assert.That(flow.State, Is.EqualTo(Boss2BurrowState.AboveGround));
            Assert.That(sawColliderDisabled, Is.True);
            Assert.That(collider.enabled, Is.True);
            Assert.That(tail.skY.activeSelf, Is.True);
        }

        static void RecordExpectedEntities(IEnumerable<string> expectedNames, ISet<string> seen)
        {
            foreach (var entity in ActiveTransientEntities())
            {
                foreach (var expected in expectedNames)
                {
                    if (entity.gameObject.name.StartsWith(expected, StringComparison.Ordinal))
                    {
                        seen.Add(expected);
                    }
                }
            }
        }

        static ColorTimingTransientEntity[] ActiveTransientEntities()
        {
            return UnityEngine.Object.FindObjectsOfType<ColorTimingTransientEntity>(true)
                .Where(entity => entity.gameObject.activeInHierarchy)
                .ToArray();
        }

        static void AssertAuthoredBossSkillLifetimes()
        {
            var configuration = new GfColorTimingConfiguration();
            var expected = new Dictionary<string, float>
            {
                ["sk_Boss1_atk1"] = 0f,
                ["sk_Boss1_atk2"] = 0.5f,
                ["sk_Boss1_atk3"] = 0f,
                ["sk_Boss1_atk3 1"] = 0f,
                ["sk_Boss1_atk5"] = 0f,
                ["sk_Boss1_atk5_b"] = 0f,
                ["sk_boos_atk5_item"] = 0f,
                ["sk_Boss1_atk6"] = 1f,
                ["sk_Boss2_Atk1"] = 0.5f,
                ["sk_Boss2_atk2"] = 0f,
                ["sk_Boss2_atk2_s"] = 0f,
                ["sk_Bo2w_atk1-t"] = 0.5f,
                ["sk_Bo2_atk0"] = 0.8f,
            };

            foreach (var pair in expected)
            {
                Assert.That(configuration.TryGetSkillByEntity(pair.Key, out var skill), Is.True,
                    $"Missing skill configuration for {pair.Key}.");
                Assert.That(skill.Lifetime, Is.EqualTo(pair.Value).Within(0.001f),
                    $"{pair.Key} lifetime no longer matches the authored prefab contract.");
            }
        }

        static IEnumerator BootToStartMenu()
        {
            Time.timeScale = 1f;
            ColorTimingPlayModeBoot.PreserveTestRunnerAcrossFrameworkScenes();
            if (!SceneManager.GetSceneByName("Launch").isLoaded)
            {
                SceneManager.LoadScene("Launch", LoadSceneMode.Single);
            }
            yield return ColorTimingPlayModeBoot.EnsureStartMenu(BootTimeout);
            yield return WaitUntil(() => FindActive<MainMenuForm>() != null, 10f,
                "StartMenu GF.UI form did not become active.");
        }

        static IEnumerator WaitForScene(string sceneName, float timeout)
        {
            yield return WaitUntil(
                () => SceneManager.GetSceneByName(sceneName).isLoaded
                      && SceneManager.GetActiveScene().name == sceneName,
                timeout,
                $"Scene '{sceneName}' did not become the active product scene.");
        }

        static IEnumerator WaitUntil(Func<bool> condition, float timeout, string failure)
        {
            var deadline = Time.realtimeSinceStartup + timeout;
            while (!condition() && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }
            Assert.That(condition(), Is.True,
                $"{failure}\n{ColorTimingPlayModeBoot.DescribeFrameworkState(failure)}");
        }

        static T FindActive<T>() where T : Component
        {
            return UnityEngine.Object.FindObjectsOfType<T>(true)
                .FirstOrDefault(candidate => candidate.gameObject.activeInHierarchy);
        }
    }
}
