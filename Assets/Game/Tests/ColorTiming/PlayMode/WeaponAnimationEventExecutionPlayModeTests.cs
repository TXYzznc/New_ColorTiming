using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ColorTiming.Combat;
using ColorTiming.Infrastructure.GF.Entity;
using ColorTiming.Infrastructure.GF.Settings;
using ColorTiming.Infrastructure.Unity.Input;
using ColorTiming.Input;
using ColorTiming.Presentation.Entities;
using ColorTiming.Presentation.UI.Components;
using ColorTiming.Presentation.UI.Forms;
using ColorTiming.Settings;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using CombatWeaponType = ColorTiming.Combat.WeaponType;

namespace ColorTiming.Tests.PlayMode
{
    public sealed class WeaponAnimationEventExecutionPlayModeTests
    {
        const float BootTimeout = 30f;
        const float TransitionTimeout = 20f;

        static readonly MethodInfo DropWeapon = typeof(PlayerActorView).GetMethod(
            "DisWeapon",
            BindingFlags.Instance | BindingFlags.NonPublic);

        static readonly FieldInfo SkillWeapon = typeof(Skill_base).GetField(
            "atkWeapon",
            BindingFlags.Instance | BindingFlags.NonPublic);

        static readonly FieldInfo SkillAttacker = typeof(Skill_base).GetField(
            "attackerId",
            BindingFlags.Instance | BindingFlags.NonPublic);

        static readonly FieldInfo SkillParameter = typeof(Skill_base).GetField(
            "parm",
            BindingFlags.Instance | BindingFlags.NonPublic);

        [UnityTest]
        [Timeout(300000)]
        public IEnumerator EveryAuthoredWeaponColorExecutesAnimationEventThroughGfEntity()
        {
            yield return BootToStartMenu();
            FindActive<MainMenuForm>().GoTest1();
            yield return WaitForScene("Boss1", TransitionTimeout);

            var settings = new GfColorTimingSettings();
            var originalKeyTips = settings.KeyTipsDisabled;
            settings.KeyTipsDisabled = true;
            try
            {
                var boss1Hero = FindActive<PlayerActorView>();
                yield return VerifySceneWeaponSet(
                    boss1Hero,
                    new[] { WeaponColor.Red, WeaponColor.Green, WeaponColor.Purple },
                    new[] { CombatWeaponType.Scissors, CombatWeaponType.Hammer, CombatWeaponType.Bomb });

                var hud = FindActive<BattlePlayerInfoView>();
                hud.TogglePause();
                yield return WaitUntil(() => FindActive<PauseMenuForm>() != null, 10f,
                    "Boss1 weapon contract could not open the pause form.");
                FindActive<PauseMenuForm>().GoNextLevel(2);
                yield return WaitForScene("Boss2", TransitionTimeout);

                var boss2Hero = FindActive<PlayerActorView>();
                yield return VerifySceneWeaponSet(
                    boss2Hero,
                    new[] { WeaponColor.Red, WeaponColor.Green, WeaponColor.Purple, WeaponColor.Orange },
                    new[] { CombatWeaponType.Knife, CombatWeaponType.Axe, CombatWeaponType.Airplane });

                hud = FindActive<BattlePlayerInfoView>();
                hud.TogglePause();
                yield return WaitUntil(() => FindActive<PauseMenuForm>() != null, 10f,
                    "Boss2 weapon contract could not open the pause form.");
                FindActive<PauseMenuForm>().BackMenu();
                yield return WaitForScene("StartMenu", TransitionTimeout);
            }
            finally
            {
                settings.KeyTipsDisabled = originalKeyTips;
                Time.timeScale = 1f;
            }
        }

        static IEnumerator VerifySceneWeaponSet(
            PlayerActorView hero,
            IReadOnlyList<WeaponColor> colors,
            IReadOnlyList<CombatWeaponType> weaponTypes)
        {
            Assert.That(hero, Is.Not.Null);
            Assert.That(DropWeapon, Is.Not.Null);
            Assert.That(SkillWeapon, Is.Not.Null);
            Assert.That(SkillParameter, Is.Not.Null);

            var eventReceiver = hero.characterSprite.GetComponent<PlayerAnimationEventRelay>();
            var fire = hero.GetComponent<PlayerSkillEmitter>();
            var input = new PointerOnlyInput { PointerScreenPosition = new Vector2(320f, 180f) };
            fire.BindGameInput(input);
            fire.BindGameplayPointer(new FixedPointerWorld(new Vector2(12f, 34f)));

            yield return ExecuteAndAssert(
                hero,
                eventReceiver,
                fire.sk_nor.name,
                hero.nowweapon,
                "normal-event");

            foreach (var weaponType in weaponTypes)
            {
                foreach (var color in colors)
                {
                    var weapon = new WeaponIdentity(color, weaponType);
                    Assert.That(hero.PickUPWeapon(weapon), Is.True,
                        $"Could not pick up {color}/{weaponType}.");
                    var expectedPrefab = ExpectedPrefab(fire, weaponType);
                    yield return ExecuteAndAssert(
                        hero,
                        eventReceiver,
                        expectedPrefab.name,
                        weapon,
                        "animation-event");

                    if (weaponType == CombatWeaponType.Scissors && color == colors[0])
                    {
                        yield return ExecuteAndAssert(
                            hero,
                            eventReceiver,
                            fire.sk_jiandao2.name,
                            weapon,
                            "2");
                    }

                    DropWeapon.Invoke(hero, new object[] { false });
                    Assert.That(hero.nowweapon.Type, Is.EqualTo(CombatWeaponType.Normal));
                    yield return null;
                }
            }
        }

        static IEnumerator ExecuteAndAssert(
            PlayerActorView hero,
            PlayerAnimationEventRelay eventReceiver,
            string expectedEntityName,
            WeaponIdentity expectedWeapon,
            string eventParameter)
        {
            yield return HideMatchingEntities(expectedEntityName);
            eventReceiver.Attack(eventParameter);

            ColorTimingTransientEntity entity = null;
            Skill_base skill = null;
            var deadline = Time.realtimeSinceStartup + 10f;
            while (Time.realtimeSinceStartup < deadline)
            {
                entity = ActiveTransientEntities().FirstOrDefault(candidate =>
                    candidate.gameObject.name.StartsWith(expectedEntityName, StringComparison.Ordinal));
                skill = entity?.GetComponentInChildren<Skill_base>(true);
                if (skill != null && SkillWeapon.GetValue(skill) != null)
                {
                    break;
                }
                yield return null;
            }

            Assert.That(entity, Is.Not.Null,
                $"Animation Event did not show GF.Entity '{expectedEntityName}'.");
            Assert.That(skill, Is.Not.Null,
                $"GF.Entity '{expectedEntityName}' has no Skill_base participant.");
            var actualWeapon = (WeaponIdentity)SkillWeapon.GetValue(skill);
            Assert.That(actualWeapon, Is.EqualTo(expectedWeapon));
            Assert.That((ActorId)SkillAttacker.GetValue(skill), Is.EqualTo(ActorId.Player));

            var actualParameter = (string)SkillParameter.GetValue(skill);
            if (expectedWeapon.Type == CombatWeaponType.Bomb
                || expectedWeapon.Type == CombatWeaponType.Airplane)
            {
                Assert.That(actualParameter, Does.StartWith(((int)expectedWeapon.Color + 1) + "="));
                Assert.That(actualParameter, Does.Contain("12.00"));
                Assert.That(actualParameter, Does.Contain("34.00"));
            }
            else
            {
                Assert.That(actualParameter, Is.EqualTo(eventParameter));
            }
        }

        static GameObject ExpectedPrefab(PlayerSkillEmitter fire, CombatWeaponType type)
        {
            switch (type)
            {
                case CombatWeaponType.Scissors: return fire.sk_jiandao;
                case CombatWeaponType.Hammer: return fire.sk_chuizi;
                case CombatWeaponType.Bomb: return fire.sk_zhadan;
                case CombatWeaponType.Knife: return fire.sk_dao;
                case CombatWeaponType.Axe: return fire.sk_futou;
                case CombatWeaponType.Airplane: return fire.sk_feiji;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        static IEnumerator HideMatchingEntities(string expectedEntityName)
        {
            foreach (var entity in ActiveTransientEntities().Where(candidate =>
                         candidate.gameObject.name.StartsWith(expectedEntityName, StringComparison.Ordinal)))
            {
                GFBuiltin.Entity.HideEntitySafe(entity);
            }
            yield return null;
            yield return null;
            Assert.That(ActiveTransientEntities().Any(candidate =>
                candidate.gameObject.name.StartsWith(expectedEntityName, StringComparison.Ordinal)), Is.False);
        }

        static ColorTimingTransientEntity[] ActiveTransientEntities()
        {
            return UnityEngine.Object.FindObjectsOfType<ColorTimingTransientEntity>(true)
                .Where(entity => entity.gameObject.activeInHierarchy)
                .ToArray();
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

        sealed class PointerOnlyInput : IGameInput
        {
            public Vector2 Move => Vector2.zero;
            public bool DashPressed => false;
            public bool AttackPressed => false;
            public bool AttackHeld => false;
            public bool DropPressed => false;
            public bool PausePressed => false;
            public Vector2 PointerScreenPosition { get; set; }
            public bool AnyPressed => false;
            public bool ConfirmPressed => false;
            public bool ConsumeAnyPressForOverlay() => false;
        }
    }
}
