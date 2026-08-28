using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ColorTiming.Combat;
using ColorTiming.Presentation.UI.Components;
using ColorTiming.Presentation.UI.Forms;
using ColorTiming.Presentation.UI.Models;
using NUnit.Framework;
using Spine.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ColorTiming.Tests.PlayMode
{
    public sealed class BossRuntimeProgressionPlayModeTests
    {
        const float BootTimeout = 30f;
        const float TransitionTimeout = 20f;

        [UnityTest]
        [Timeout(120000)]
        public IEnumerator FormalFlow_ConsumesEveryBossColor_ActivatesTailAndShowsFinalResult()
        {
            yield return BootToStartMenu();

            var menu = FindActive<MainMenuForm>();
            Assert.That(menu, Is.Not.Null);
            menu.GoTest1();
            yield return WaitForScene("Boss1", TransitionTimeout);

            var boss1 = FindActive<Boss1ActorView>();
            Assert.That(boss1, Is.Not.Null);
            Assert.That(boss1.Boss1HP, Has.Count.EqualTo(11));
            AssertAnimations(boss1.skeletonAnimation1,
                "idel_60fps", "hit1_60fps", "hit2_60fps",
                "attack_1_test1_60fps", "attack_2_test1_60fps", "attack_3_test2_60fps",
                "attack_4_test1_60fps", "attack_6_60fps");
            AssertAnimations(boss1.skeletonAnimation2, "attack_5_test1_60fps2");

            var boss1Colors = new HashSet<WeaponColor>();
            AssertWrongColorDoesNotDamage(boss1);
            while (boss1.Boss1HP.Count > 0)
            {
                var before = boss1.Boss1HP.Count;
                var color = boss1.Boss1HP[0];
                boss1Colors.Add(color);
                boss1.ReceiveDamage(BattleDamageTestFactory.ToBoss(
                    color, "playmode-regression"));
                Assert.That(boss1.Boss1HP, Has.Count.EqualTo(before - 1));
                yield return null;
            }

            Assert.That(boss1Colors.SetEquals(new[] { WeaponColor.Red, WeaponColor.Green, WeaponColor.Purple }), Is.True,
                "Boss1 runtime queue must exercise all three authored colors.");
            yield return WaitForScene("Boss2", TransitionTimeout);

            var boss2 = FindActive<Boss2ActorView>();
            var tail = Object.FindObjectsOfType<Boss2TailActorView>(true).Single();
            Assert.That(boss2, Is.Not.Null);
            Assert.That(boss2.Boss1HP, Has.Count.EqualTo(15));
            AssertAnimations(boss2.skeletonAnimation1,
                "idel", "ShouJi", "RuTu", "ChuTu", "attack_1", "attack_2");
            AssertAnimations(tail.skeletonAnimation,
                "idel", "RuTu", "ChuTu", "attack_1", "attack_2");
            Assert.That(tail.gameObject.activeSelf, Is.False,
                "Boss2 tail must start inactive before the 12-to-11 threshold.");

            var boss2Colors = new HashSet<WeaponColor>();
            AssertWrongColorDoesNotDamage(boss2);
            while (boss2.Boss1HP.Count > 0)
            {
                var before = boss2.Boss1HP.Count;
                var color = boss2.Boss1HP[0];
                boss2Colors.Add(color);
                boss2.ReceiveDamage(BattleDamageTestFactory.ToBoss(
                    color, "playmode-regression"));
                Assert.That(boss2.Boss1HP, Has.Count.EqualTo(before - 1));
                if (boss2.Boss1HP.Count == 11)
                {
                    Assert.That(tail.gameObject.activeSelf, Is.True,
                        "Boss2 tail must activate exactly on the 12-to-11 transition.");
                }
                yield return null;
            }

            Assert.That(boss2Colors.SetEquals(
                new[] { WeaponColor.Red, WeaponColor.Green, WeaponColor.Purple, WeaponColor.Orange }), Is.True,
                "Boss2 runtime queue must exercise all four authored colors.");
            Assert.That(boss2.death, Is.True);
            Assert.That(tail.IsStoppedForBattleEnd, Is.True,
                "Final victory must synchronously stop the tail before result UI pauses game time.");
            Assert.That(tail.GetComponent<Collider2D>().enabled, Is.False);

            yield return WaitUntil(() => FindActive<BattleResultForm>() != null, 10f,
                "Final victory did not open the GF.UI battle-result form.");
            Assert.That(Time.timeScale, Is.EqualTo(0f));

            var hud = FindActive<BattlePlayerInfoView>();
            Assert.That(hud, Is.Not.Null);
            hud.TogglePause();
            yield return WaitUntil(() => FindActive<PauseMenuForm>() != null, 10f,
                "Cleanup pause form did not open after the final-result assertion.");
            FindActive<PauseMenuForm>().BackMenu();
            yield return WaitForScene("StartMenu", TransitionTimeout);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        static void AssertWrongColorDoesNotDamage(Boss1ActorView boss)
        {
            var current = boss.Boss1HP[0];
            var wrong = new[] { WeaponColor.Red, WeaponColor.Green, WeaponColor.Purple }.First(color => color != current);
            var before = boss.Boss1HP.Count;
            boss.ReceiveDamage(BattleDamageTestFactory.ToBoss(
                wrong, "wrong-color"));
            Assert.That(boss.Boss1HP, Has.Count.EqualTo(before));
        }

        static void AssertWrongColorDoesNotDamage(Boss2ActorView boss)
        {
            var current = boss.Boss1HP[0];
            var wrong = new[] { WeaponColor.Red, WeaponColor.Green, WeaponColor.Purple, WeaponColor.Orange }
                .First(color => color != current);
            var before = boss.Boss1HP.Count;
            boss.ReceiveDamage(BattleDamageTestFactory.ToBoss(
                wrong, "wrong-color"));
            Assert.That(boss.Boss1HP, Has.Count.EqualTo(before));
        }

        static void AssertAnimations(SkeletonAnimation view, params string[] names)
        {
            Assert.That(view, Is.Not.Null);
            Assert.That(view.SkeletonDataAsset, Is.Not.Null);
            var data = view.SkeletonDataAsset.GetSkeletonData(true);
            Assert.That(data, Is.Not.Null);
            foreach (var animationName in names)
            {
                Assert.That(data.FindAnimation(animationName), Is.Not.Null,
                    $"Skeleton '{view.name}' is missing authored animation '{animationName}'.");
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

        static IEnumerator WaitUntil(System.Func<bool> condition, float timeout, string failure)
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
            foreach (var candidate in Object.FindObjectsOfType<T>(true))
            {
                if (candidate.gameObject.activeInHierarchy)
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
