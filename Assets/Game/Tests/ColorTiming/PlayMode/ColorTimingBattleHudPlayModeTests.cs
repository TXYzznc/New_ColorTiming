using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ColorTiming.Tests.PlayMode
{
    public sealed class ColorTimingBattleHudPlayModeTests
    {
        const float BootTimeout = 30f;
        const float TransitionTimeout = 20f;

        [UnityTest]
        [Timeout(120000)]
        public IEnumerator BattleHud_IsRuntimeOwned_Unique_AndPlayerItemsUseExpectedLayout()
        {
            yield return BootToStartMenu();

            var menu = FindActive<UI_ButtonAction>();
            Assert.That(menu, Is.Not.Null);
            menu.GoTest1();
            yield return WaitForScene("Boss1", TransitionTimeout);
            yield return WaitForHud("BattleHud_Boss1", TransitionTimeout);

            var boss1Hud = Object.FindObjectOfType<ColorTimingBattleHudBootstrap>(true);
            Assert.That(boss1Hud, Is.Not.Null);
            AssertHudRoot(boss1Hud, "BattleHud_Boss1");
            AssertBoss1Hud(boss1Hud);

            var boss1 = FindActive<Boss1_Controller>();
            Assert.That(boss1, Is.Not.Null);
            while (boss1.Boss1HP.Count > 0)
            {
                var color = boss1.Boss1HP[0];
                boss1.OnDamage(null, new Weapon(color, WeaponType.nor), Vector2.zero, "hud-runtime-test");
                yield return null;
            }

            yield return WaitForScene("Boss2", TransitionTimeout);
            yield return WaitForHud("BattleHud_Boss2", TransitionTimeout);
            var boss2Hud = Object.FindObjectOfType<ColorTimingBattleHudBootstrap>(true);
            Assert.That(boss2Hud, Is.Not.Null);
            AssertHudRoot(boss2Hud, "BattleHud_Boss2");
            AssertBoss2Hud(boss2Hud);

            var heroHud = FindActive<UI_HeroInfo>();
            Assert.That(heroHud, Is.Not.Null);
            heroHud.TogglePause();
            yield return WaitUntil(() => FindActive<UI_ESC>() != null, 10f,
                "HUD test cleanup could not open the pause form.");
            FindActive<UI_ESC>().BackMenu();
            yield return WaitForScene("StartMenu", TransitionTimeout);

            Debug.Log("[ColorTiming HUD] test-result unique-runtime-hud-and-player-layout=PASS");
        }

        static void AssertHudRoot(ColorTimingBattleHudBootstrap bootstrap, string expectedName)
        {
            var hud = bootstrap.transform.Find(expectedName);
            Assert.That(hud, Is.Not.Null, "The battle HUD must be instantiated under the bootstrap.");
            Assert.That(hud.GetComponent<RectTransform>(), Is.Not.Null,
                "The runtime HUD root must be a RectTransform when parented under Canvas UI.");
            Assert.That(hud.GetComponent<RectTransform>().sizeDelta, Is.EqualTo(new Vector2(1920f, 1080f)));
        }

        static void AssertBoss1Hud(ColorTimingBattleHudBootstrap bootstrap)
        {
            var hud = bootstrap.transform.Find("BattleHud_Boss1");
            var heroBoxes = Object.FindObjectsOfType<UI_HeroHPBox>(true);
            var bossControllers = Object.FindObjectsOfType<UI_BossHPController>(true);
            var bossControllers2 = Object.FindObjectsOfType<UI_BossHPController2>(true);
            Assert.That(heroBoxes, Has.Length.EqualTo(1));
            Assert.That(bossControllers, Has.Length.EqualTo(1));
            Assert.That(bossControllers2, Has.Length.EqualTo(0));
            Assert.That(heroBoxes[0].transform.IsChildOf(hud), Is.True);
            Assert.That(bossControllers[0].transform.IsChildOf(hud), Is.True);

            AssertHeroLayout(heroBoxes[0]);
            Assert.That(bossControllers[0].transform.childCount, Is.EqualTo(7));
            Debug.Log("[ColorTiming HUD] test scene=Boss1 controllers=hero:1 boss1:1 boss2:0 staticOutsideHud:0");
        }

        static void AssertBoss2Hud(ColorTimingBattleHudBootstrap bootstrap)
        {
            var hud = bootstrap.transform.Find("BattleHud_Boss2");
            var heroBoxes = Object.FindObjectsOfType<UI_HeroHPBox>(true);
            var bossControllers = Object.FindObjectsOfType<UI_BossHPController>(true);
            var bossControllers2 = Object.FindObjectsOfType<UI_BossHPController2>(true);
            Assert.That(heroBoxes, Has.Length.EqualTo(1));
            Assert.That(bossControllers, Has.Length.EqualTo(0));
            Assert.That(bossControllers2, Has.Length.EqualTo(1));
            Assert.That(heroBoxes[0].transform.IsChildOf(hud), Is.True);
            Assert.That(bossControllers2[0].transform.IsChildOf(hud), Is.True);

            AssertHeroLayout(heroBoxes[0]);
            Assert.That(bossControllers2[0].transform.childCount, Is.EqualTo(7));
            Debug.Log("[ColorTiming HUD] test scene=Boss2 controllers=hero:1 boss1:0 boss2:1 staticOutsideHud:0");
        }

        static void AssertHeroLayout(UI_HeroHPBox heroBox)
        {
            Assert.That(heroBox.controller, Is.Not.Null);
            Assert.That(heroBox.transform.name, Is.EqualTo("P_HPBox"));
            Assert.That(heroBox.transform.childCount, Is.EqualTo(heroBox.controller.heroMaxHP));
            var items = heroBox.GetComponentsInChildren<UI_HeroHPItem>(true);
            Assert.That(items, Has.Length.EqualTo(heroBox.controller.heroMaxHP));
            for (var i = 0; i < items.Length; i++)
            {
                var expected = new Vector2(i * 35f, i % 2 == 0 ? 0f : -33f);
                var actual = ((RectTransform)items[i].transform).anchoredPosition;
                Assert.That(actual, Is.EqualTo(expected).Within(0.01f),
                    $"Player HP item {i} has an unexpected runtime position.");
                Debug.Log($"[ColorTiming HUD] test hero-item index={i} anchored={actual}", items[i]);
            }
        }

        static IEnumerator BootToStartMenu()
        {
            Time.timeScale = 1f;
            ColorTimingPlayModeBoot.PreserveTestRunnerAcrossFrameworkScenes();
            if (!SceneManager.GetSceneByName("Launch").isLoaded)
                SceneManager.LoadScene("Launch", LoadSceneMode.Single);
            yield return ColorTimingPlayModeBoot.EnsureFormalLaunchStartedInBatchMode();
            yield return WaitForScene("StartMenu", BootTimeout);
            yield return ColorTimingPlayModeBoot.WaitForProductSceneTransitions();
            yield return WaitUntil(() => FindActive<UI_ButtonAction>() != null, 10f,
                "StartMenu GF.UI form did not become active.");
        }

        static IEnumerator WaitForHud(string hudName, float timeout)
        {
            yield return WaitUntil(() =>
            {
                var bootstrap = Object.FindObjectOfType<ColorTimingBattleHudBootstrap>(true);
                return bootstrap != null && bootstrap.transform.Find(hudName) != null
                    && bootstrap.transform.Find(hudName).GetComponentInChildren<UI_HeroHPBox>(true)?.controller != null;
            }, timeout, $"Runtime HUD '{hudName}' did not finish binding.");
        }

        static IEnumerator WaitForScene(string sceneName, float timeout)
        {
            yield return WaitUntil(() => SceneManager.GetSceneByName(sceneName).isLoaded
                && SceneManager.GetActiveScene().name == sceneName, timeout,
                $"Scene '{sceneName}' did not become the active product scene.");
        }

        static IEnumerator WaitUntil(System.Func<bool> condition, float timeout, string failure)
        {
            var deadline = Time.realtimeSinceStartup + timeout;
            while (!condition() && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(condition(), Is.True, failure);
        }

        static T FindActive<T>() where T : Component
        {
            foreach (var candidate in Object.FindObjectsOfType<T>(true))
                if (candidate.gameObject.activeInHierarchy) return candidate;
            return null;
        }
    }
}
