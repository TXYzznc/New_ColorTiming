using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ColorTiming.Presentation.Audio;
using ColorTiming.Presentation.UI.Components;
using ColorTiming.Presentation.UI.Forms;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ColorTiming.Tests.PlayMode
{
    public sealed class GrassWorldInteractionPlayModeTests
    {
        const float BootTimeout = 30f;
        const float TransitionTimeout = 20f;

        static readonly MethodInfo EnterGrass = typeof(GrassInteractionView).GetMethod(
            "OnTriggerEnter2D",
            BindingFlags.Instance | BindingFlags.NonPublic);

        static readonly MethodInfo ExitGrass = typeof(GrassInteractionView).GetMethod(
            "OnTriggerExit2D",
            BindingFlags.Instance | BindingFlags.NonPublic);

        [UnityTest]
        [Timeout(90000)]
        public IEnumerator GrassEnterExit_AnimatesAndSwitchesFrameworkFootstepCueSet()
        {
            yield return BootToStartMenu();
            FindActive<MainMenuForm>().GoTest1();
            yield return WaitForScene("Boss1", TransitionTimeout);
            yield return ColorTimingPlayModeBoot.WaitForBattleReady("Boss1", TransitionTimeout);

            var hero = FindActive<PlayerActorView>();
            var heroCollider = hero.GetComponentsInChildren<Collider2D>(true)
                .FirstOrDefault(candidate => candidate.CompareTag("Player"));
            var heroSound = hero.GetComponentInChildren<PlayerSoundView>(true);
            var grass = UnityEngine.Object.FindObjectsOfType<GrassInteractionView>(true)
                .FirstOrDefault(candidate => candidate.gameObject.activeInHierarchy
                                             && candidate.audioClips != null
                                             && candidate.audioClips.Count > 0
                                             && candidate.audioClips.All(clip => clip != null));
            var sound = new RecordingSoundService();

            Assert.That(heroCollider, Is.Not.Null);
            Assert.That(heroSound, Is.Not.Null);
            Assert.That(grass, Is.Not.Null,
                "Boss1 must contain an active GrassInteractionView with non-empty grass rustle clips.");
            var animator = grass.GetComponent<Animator>();
            Assert.That(grass.audioClips, Is.Not.Empty.And.All.Not.Null,
                "Grass rustle clips must have no missing references.");
            Assert.That(heroSound.MoveCueCount, Is.GreaterThan(0));
            Assert.That(heroSound.MoveOverrideCueCount, Is.GreaterThan(0),
                "Boss1 must configure grass-footstep override cues.");
            Assert.That(animator, Is.Not.Null);
            Assert.That(animator.parameters.Any(parameter =>
                parameter.name == "Trigger" && parameter.type == AnimatorControllerParameterType.Trigger), Is.True);
            Assert.That(EnterGrass, Is.Not.Null);
            Assert.That(ExitGrass, Is.Not.Null);

            grass.BindSoundService(sound);
            heroSound.BindSoundService(sound);
            while (heroSound.moveCase.Contains(grass.gameObject.name))
            {
                heroSound.moveCase.Remove(grass.gameObject.name);
            }

            EnterGrass.Invoke(grass, new object[] { heroCollider });
            Assert.That(heroSound.moveCase, Does.Contain(grass.gameObject.name));
            Assert.That(sound.Calls, Has.Count.EqualTo(1));
            Assert.That(sound.Calls[0].Channel, Is.EqualTo(ColorTimingSoundChannel.Environment));
            Assert.That(grass.audioClips, Does.Contain(sound.Calls[0].Clip));

            sound.Reset();
            heroSound.PlayAuido_Random("move");
            Assert.That(sound.Cues, Has.Count.EqualTo(1));
            Assert.That(sound.Cues[0], Does.StartWith("player.boss1.move-override."));

            ExitGrass.Invoke(grass, new object[] { heroCollider });
            Assert.That(heroSound.moveCase, Does.Not.Contain(grass.gameObject.name));
            sound.Reset();
            heroSound.PlayAuido_Random("move");
            Assert.That(sound.Cues, Has.Count.EqualTo(1));
            Assert.That(sound.Cues[0], Does.StartWith("player.boss1.move."));

            var hud = FindActive<BattlePlayerInfoView>();
            hud.TogglePause();
            yield return WaitUntil(() => FindActive<PauseMenuForm>() != null, 10f,
                "Grass contract cleanup could not open the pause form.");
            FindActive<PauseMenuForm>().BackMenu();
            yield return WaitForScene("StartMenu", TransitionTimeout);
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

        sealed class RecordingSoundService : IColorTimingSoundService
        {
            public readonly List<Call> Calls = new List<Call>();
            public readonly List<string> Cues = new List<string>();

            public int Play(AudioClip clip, ColorTimingSoundChannel channel, Vector3 position, bool loop = false)
            {
                Calls.Add(new Call(clip, channel));
                return Calls.Count;
            }

            public int PlayCue(string cueId, Vector3 position)
            {
                Cues.Add(cueId);
                return Cues.Count;
            }

            public void ResetTrackedSounds()
            {
                Reset();
            }

            public void Stop(int serialId)
            {
            }

            public void Reset()
            {
                Calls.Clear();
                Cues.Clear();
            }
        }

        readonly struct Call
        {
            public Call(AudioClip clip, ColorTimingSoundChannel channel)
            {
                Clip = clip;
                Channel = channel;
            }

            public AudioClip Clip { get; }
            public ColorTimingSoundChannel Channel { get; }
        }
    }
}
