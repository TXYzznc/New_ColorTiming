using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using ColorTiming.Combat;
using ColorTiming.Infrastructure.GF.Settings;
using ColorTiming.Infrastructure.Unity.Input;
using ColorTiming.Input;
using ColorTiming.Player;
using ColorTiming.Presentation.UI.Components;
using ColorTiming.Presentation.UI.Forms;
using ColorTiming.Settings;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ColorTiming.Tests.PlayMode
{
    public sealed class PlayerRuntimeExecutionPlayModeTests
    {
        const float BootTimeout = 30f;
        const float TransitionTimeout = 20f;

        static readonly FieldInfo PlayerState = typeof(PlayerActorView).GetField(
            "playerState",
            BindingFlags.Instance | BindingFlags.NonPublic);

        [UnityTest]
        [Timeout(180000)]
        public IEnumerator SemanticInput_PickupHitDashDeathAndAnimationRestartExecuteInBoss1()
        {
            yield return BootToStartMenu();
            FindActive<MainMenuForm>().GoTest1();
            yield return WaitForScene("Boss1", TransitionTimeout);

            var hero = FindActive<PlayerActorView>();
            var input = new MutableGameInput();
            var settings = new GfColorTimingSettings();
            var originalKeyTips = settings.KeyTipsDisabled;
            Assert.That(hero, Is.Not.Null);
            Assert.That(PlayerState, Is.Not.Null);

            settings.KeyTipsDisabled = true;
            try
            {
                hero.BindGameInput(input);
                hero.BindGameplayPointer(new FixedPointerWorld(hero.transform.position + Vector3.right));
                var state = (PlayerActionStateMachine)PlayerState.GetValue(hero);
                Assert.That(state, Is.Not.Null);

                yield return WaitUntil(
                    () => hero.animator.GetCurrentAnimatorStateInfo(0).IsName("Daiji")
                          || hero.animator.GetCurrentAnimatorStateInfo(0).IsName("Move"),
                    5f,
                    "Hero Animator did not enter an authored locomotion state.");

                var start = hero.transform.position;
                input.Move = Vector2.right;
                for (var i = 0; i < 12; i++)
                {
                    yield return new WaitForFixedUpdate();
                }
                Assert.That(hero.transform.position.x, Is.GreaterThan(start.x + 0.05f));
                Assert.That(hero.characterSprite.transform.localScale.x, Is.GreaterThan(0f));

                input.Move = Vector2.left;
                for (var i = 0; i < 6; i++)
                {
                    yield return new WaitForFixedUpdate();
                }
                Assert.That(hero.characterSprite.transform.localScale.x, Is.LessThan(0f));
                input.Move = Vector2.zero;

                Assert.That(hero.PickUPWeapon(
                    new WeaponIdentity(WeaponColor.Red, ColorTiming.Combat.WeaponType.Scissors)), Is.True);
                Assert.That(hero.nowweapon.Type, Is.EqualTo(ColorTiming.Combat.WeaponType.Scissors));
                Assert.That(hero.nowweapon.Color, Is.EqualTo(WeaponColor.Red));

                hero.ReceiveDamage(BattleDamageTestFactory.ToPlayer(
                    (Vector2)hero.transform.position + Vector2.left, "player-contract-hit"));
                Assert.That(hero.heroHP, Is.EqualTo(4));
                Assert.That(hero.nowweapon.Type, Is.EqualTo(ColorTiming.Combat.WeaponType.Normal),
                    "A damaging hit must force the held weapon to drop.");
                yield return WaitForHitRecovery(state);

                yield return WaitUntil(
                    () => hero.animator.GetCurrentAnimatorStateInfo(0).IsName("Daiji")
                          || hero.animator.GetCurrentAnimatorStateInfo(0).IsName("Move"),
                    5f,
                    "Hero did not return to locomotion before Dash.");
                input.Move = Vector2.right;
                input.DashPressed = true;
                yield return null;
                input.DashPressed = false;
                var dashDeadline = Time.realtimeSinceStartup + 3f;
                while (!state.CanEvadeDamage && Time.realtimeSinceStartup < dashDeadline)
                {
                    yield return null;
                }
                Assert.That(state.CanEvadeDamage, Is.True,
                    $"Semantic DashPressed did not enter the authored dash-invulnerability window. " +
                    $"DomainState={state.State}, DashSignal={state.HasDashInvulnerability}, " +
                    $"AnimatorState={hero.animator.GetCurrentAnimatorStateInfo(0).fullPathHash}.");

                var dashStart = hero.transform.position;
                hero.ReceiveDamage(BattleDamageTestFactory.ToPlayer(
                    (Vector2)hero.transform.position + Vector2.left, "successful-dash-contract"));
                Assert.That(hero.heroHP, Is.EqualTo(5),
                    "A successful Dash must heal the previously missing heart.");
                Assert.That(Time.timeScale, Is.EqualTo(0.45f).Within(0.01f));
                yield return WaitUntil(() => !state.IsDashing, 5f,
                    "Dash animation events did not close the Dash state.");
                Assert.That(Vector2.Distance(dashStart, hero.transform.position), Is.GreaterThan(0.05f));
                yield return WaitUntil(() => Mathf.Approximately(Time.timeScale, 1f), 3f,
                    "Successful-Dash slow motion did not restore normal game time.");
                input.Move = Vector2.zero;

                for (var remaining = 4; remaining >= 1; remaining--)
                {
                    hero.ReceiveDamage(BattleDamageTestFactory.ToPlayer(
                        (Vector2)hero.transform.position + Vector2.left, "player-contract-hit"));
                    Assert.That(hero.heroHP, Is.EqualTo(remaining));
                    yield return WaitForHitRecovery(state);
                }

                var oldHeroId = hero.GetInstanceID();
                hero.ReceiveDamage(BattleDamageTestFactory.ToPlayer(
                    (Vector2)hero.transform.position + Vector2.left, "player-contract-death"));
                Assert.That(hero.heroHP, Is.Zero);
                Assert.That(state.IsAlive, Is.False);
                Assert.That(hero.deathShow.activeSelf, Is.True);
                Assert.That(hero.GetComponent<PlayerCameraLifecycleView>().enabled, Is.False);

                yield return WaitUntil(
                    () => UnityEngine.Object.FindObjectsOfType<PlayerActorView>(true).Any(
                        candidate => candidate.gameObject.activeInHierarchy
                                     && candidate.GetInstanceID() != oldHeroId
                                     && candidate.heroHP == candidate.heroMaxHP),
                    20f,
                    "Death animation event did not restart Boss1 with a fresh full-health hero.");
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Boss1"));

                var restartedHud = FindActive<BattlePlayerInfoView>();
                Assert.That(restartedHud, Is.Not.Null);
                restartedHud.TogglePause();
                yield return WaitUntil(() => FindActive<PauseMenuForm>() != null, 10f,
                    "Player contract cleanup could not open the pause form.");
                FindActive<PauseMenuForm>().BackMenu();
                yield return WaitForScene("StartMenu", TransitionTimeout);
            }
            finally
            {
                settings.KeyTipsDisabled = originalKeyTips;
                Time.timeScale = 1f;
            }
        }

        static IEnumerator WaitForHitRecovery(PlayerActionStateMachine state)
        {
            yield return WaitUntil(() => state.RejectsDamage, 3f,
                "Hit animation event did not open the damage-rejection window.");
            yield return WaitUntil(() => !state.RejectsDamage, 5f,
                "Hit damage-rejection window did not expire.");
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
            Assert.That(condition(), Is.True, failure);
        }

        static T FindActive<T>() where T : Component
        {
            return UnityEngine.Object.FindObjectsOfType<T>(true)
                .FirstOrDefault(candidate => candidate.gameObject.activeInHierarchy);
        }

        sealed class FixedPointerWorld : IGameplayPointerWorld
        {
            readonly Vector2 world;

            public FixedPointerWorld(Vector2 world)
            {
                this.world = world;
            }

            public Vector2 Resolve(Vector2 screenPosition)
            {
                return world;
            }
        }

        sealed class MutableGameInput : IGameInput
        {
            public Vector2 Move { get; set; }
            public bool DashPressed { get; set; }
            public bool AttackPressed { get; set; }
            public bool AttackHeld { get; set; }
            public bool DropPressed { get; set; }
            public bool PausePressed { get; set; }
            public Vector2 PointerScreenPosition { get; set; }
            public bool AnyPressed { get; set; }
            public bool ConfirmPressed { get; set; }

            public bool ConsumeAnyPressForOverlay()
            {
                if (!AnyPressed && !ConfirmPressed)
                {
                    return false;
                }
                AnyPressed = false;
                ConfirmPressed = false;
                return true;
            }
        }
    }
}
