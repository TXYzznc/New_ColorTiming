using System.Collections;
using NUnit.Framework;
using System.Linq;
using ColorTiming.Bootstrap;
using ColorTiming.Combat;
using ColorTiming.Configuration;
using ColorTiming.Presentation.UI.Components;
using ColorTiming.Presentation.UI.Forms;
using ColorTiming.Presentation.UI.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

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

            var menu = FindActive<MainMenuForm>();
            Assert.That(menu, Is.Not.Null);
            menu.GoTest1();
            yield return WaitForScene("Boss1", TransitionTimeout);
            yield return WaitForHud(TransitionTimeout);

            var boss1Hud = Object.FindObjectOfType<BattleHudForm>(true);
            Assert.That(boss1Hud, Is.Not.Null);
            AssertHudRoot(boss1Hud);
            AssertBoss1Hud(boss1Hud);
            AssertDynamicBattlePresentation("Boss1");

            var boss1 = FindActive<Boss1ActorView>();
            Assert.That(boss1, Is.Not.Null);
            yield return AssertHudPresentationResetsAfterPoolReuse(boss1);
            while (boss1.Boss1HP.Count > 0)
            {
                var color = boss1.Boss1HP[0];
                boss1.ReceiveDamage(BossDamage((WeaponColor)color, "hud-runtime-test"));
                yield return null;
            }

            yield return WaitForScene("Boss2", TransitionTimeout);
            yield return WaitForHud(TransitionTimeout);
            var boss2Hud = Object.FindObjectOfType<BattleHudForm>(true);
            Assert.That(boss2Hud, Is.Not.Null);
            AssertHudRoot(boss2Hud);
            AssertBoss2Hud(boss2Hud);
            AssertDynamicBattlePresentation("Boss2");

            var heroHud = FindActive<BattlePlayerInfoView>();
            Assert.That(heroHud, Is.Not.Null);
            heroHud.TogglePause();
            yield return WaitUntil(() => FindActive<PauseMenuForm>() != null, 10f,
                "HUD test cleanup could not open the pause form.");
            FindActive<PauseMenuForm>().BackMenu();
            yield return WaitForScene("StartMenu", TransitionTimeout);

            Debug.Log("[ColorTiming HUD] test-result unique-runtime-hud-and-player-layout=PASS");
        }

        static IEnumerator AssertHudPresentationResetsAfterPoolReuse(Boss1ActorView boss1)
        {
            var heroInfo = FindActive<BattlePlayerInfoView>();
            Assert.That(heroInfo, Is.Not.Null);

            var session = heroInfo.Session;
            Assert.That(session, Is.Not.Null);
            // 测试只关心 HUD 快照消费；直接固定库存，不受开场教程暂停状态影响。
            session.TryDrop(out _);
            var testWeapon = new WeaponIdentity(WeaponColor.Red, ColorTiming.Combat.WeaponType.Hammer);
            var configuration = new GfColorTimingConfiguration();
            var testPresentation = WeaponPresentationState.From(configuration.GetWeapon(testWeapon));
            Assert.That(session.Inventory.TryPickup(testWeapon), Is.True);
            Assert.That(heroInfo.heroWeapon.sprite, Is.SameAs(heroInfo.weapons[testPresentation.IconIndex]));
            Assert.That(heroInfo.weaponTip.gameObject.activeSelf, Is.False);
            Assert.That(heroInfo.weaponTipx.gameObject.activeSelf, Is.True);

            heroInfo.BindSession(null);
            var normalPresentation = WeaponPresentationState.From(configuration.GetWeapon(
                new WeaponIdentity(WeaponColor.Red, ColorTiming.Combat.WeaponType.Normal)));
            Assert.That(heroInfo.heroWeapon.sprite,
                Is.SameAs(heroInfo.weapons[normalPresentation.IconIndex]));
            Assert.That(heroInfo.weaponTip.gameObject.activeSelf, Is.True);
            Assert.That(heroInfo.weaponTipx.gameObject.activeSelf, Is.False);
            heroInfo.BindSession(session);

            var bossHud = FindActive<BossHealthView>();
            Assert.That(bossHud, Is.Not.Null);
            for (var i = 0; i < 3; i++)
            {
                var color = boss1.Boss1HP[0];
                boss1.ReceiveDamage(BossDamage((WeaponColor)color, "hud-pool-reset-test"));
                yield return null;
            }

            var firstItem = bossHud.GetComponentsInChildren<BossWeaknessPipView>(true)
                .First(item => item.transform.GetSiblingIndex() == 0);
            Assert.That(firstItem.tip1.activeSelf, Is.False,
                "Boss1 weakness tutorial should stop after the initial three presentations.");

            bossHud.Bind(null);
            bossHud.Bind(session);
            Assert.That(firstItem.tip1.activeSelf, Is.True,
                "Boss1 weakness tutorial count must reset when the pooled HUD binds a new battle.");
        }

        static void AssertHudRoot(BattleHudForm hud)
        {
            Assert.That(hud.GetComponent<Canvas>(), Is.Not.Null);
            Assert.That(hud.GetComponent<CanvasScaler>(), Is.Not.Null);
            Assert.That(hud.GetComponent<GraphicRaycaster>(), Is.Not.Null);
            Assert.That(hud.GetComponent<RectTransform>().localScale, Is.EqualTo(Vector3.one));
        }

        static void AssertBoss1Hud(BattleHudForm hud)
        {
            AssertSharedBossHud(hud, "Boss1");
        }

        static void AssertBoss2Hud(BattleHudForm hud)
        {
            AssertSharedBossHud(hud, "Boss2");
        }

        static void AssertSharedBossHud(BattleHudForm hud, string sceneName)
        {
            var heroBoxes = Object.FindObjectsOfType<PlayerHealthPipsView>(true);
            var bossControllers = Object.FindObjectsOfType<BossHealthView>(true);
            Assert.That(heroBoxes, Has.Length.EqualTo(1));
            Assert.That(bossControllers, Has.Length.EqualTo(1));
            Assert.That(heroBoxes[0].transform.IsChildOf(hud.transform), Is.True);
            Assert.That(bossControllers[0].transform.IsChildOf(hud.transform), Is.True);
            Assert.That(bossControllers[0].transform.name, Is.EqualTo("Slot_BossHP"));
            Assert.That(bossControllers[0].GetComponent<HorizontalLayoutGroup>(), Is.Not.Null);

            AssertHeroLayout(heroBoxes[0]);
            Assert.That(bossControllers[0].transform.childCount, Is.EqualTo(7));
            Debug.Log($"[ColorTiming HUD] test scene={sceneName} controllers=hero:1 boss:1 shared-slot:PASS staticOutsideHud:0");
        }

        static void AssertDynamicBattlePresentation(string sceneName)
        {
            Assert.That(FindActive<BattleRuntimeContext>(), Is.Not.Null,
                $"{sceneName} must create its runtime battle context.");
            Assert.That(Object.FindObjectOfType<BattleTutorialForm>(true), Is.Not.Null,
                $"{sceneName} must open the runtime battle tutorial form.");
            var scene = SceneManager.GetSceneByName(sceneName);
            var spawners = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<WeaponSpawnerView>(true))
                .ToArray();
            Assert.That(spawners, Has.Length.EqualTo(1),
                $"{sceneName} must contain exactly one shared WeaponSpawnerView.");
            Assert.That(spawners[0].GetType(), Is.EqualTo(typeof(WeaponSpawnerView)),
                $"{sceneName} must not restore a Boss-specific weapon spawner subclass.");
            Assert.That(spawners[0].GetSupportedWeapons().Count,
                Is.EqualTo(sceneName == "Boss1" ? 9 : 12),
                $"{sceneName} must retain its authored weapon rule asset.");
            Assert.That(scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Canvas>(true)), Is.Empty,
                $"{sceneName} must not retain an authored Canvas UI root.");
            Assert.That(scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<EventSystem>(true)), Is.Empty,
                $"{sceneName} must not retain an authored EventSystem.");
        }

        static void AssertHeroLayout(PlayerHealthPipsView heroBox)
        {
            Assert.That(heroBox.Session, Is.Not.Null);
            Assert.That(heroBox.transform.name, Is.EqualTo("Slot_HeroHP"));
            Assert.That(heroBox.transform.childCount, Is.EqualTo(heroBox.Session.Snapshot.PlayerMaximumHealth));
            var items = heroBox.GetComponentsInChildren<PlayerHealthPipView>(true);
            Assert.That(items, Has.Length.EqualTo(heroBox.Session.Snapshot.PlayerMaximumHealth));
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
            yield return ColorTimingPlayModeBoot.EnsureStartMenu(BootTimeout);
            yield return WaitUntil(() => FindActive<MainMenuForm>() != null, 10f,
                "StartMenu GF.UI form did not become active.");
        }

        static IEnumerator WaitForHud(float timeout)
        {
            yield return WaitUntil(() =>
            {
                var hud = Object.FindObjectOfType<BattleHudForm>(true);
                return hud != null && hud.GetComponentInChildren<PlayerHealthPipsView>(true)?.Session != null;
            }, timeout, "Shared runtime BattleHud did not finish binding.");
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

        static BattleDamage BossDamage(WeaponColor color, string parameter)
        {
            return new BattleDamage(
                ActorId.Player,
                ActorId.BossHead,
                new WeaponIdentity(color, ColorTiming.Combat.WeaponType.Normal),
                new CombatPoint(0f, 0f),
                parameter);
        }
    }
}
