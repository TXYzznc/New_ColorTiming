using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ColorTiming.Presentation.Audio;
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

        static readonly MethodInfo EnterGrass = typeof(XiaoCao).GetMethod(
            "OnTriggerEnter2D",
            BindingFlags.Instance | BindingFlags.NonPublic);

        static readonly MethodInfo ExitGrass = typeof(XiaoCao).GetMethod(
            "OnTriggerExit2D",
            BindingFlags.Instance | BindingFlags.NonPublic);

        [UnityTest]
        [Timeout(90000)]
        public IEnumerator GrassEnterExit_AnimatesAndSwitchesFrameworkFootstepCueSet()
        {
            yield return BootToStartMenu();
            FindActive<UI_ButtonAction>().GoTest1();
            yield return WaitForScene("Boss1", TransitionTimeout);

            var hero = FindActive<HeroController>();
            var heroCollider = hero.GetComponentsInChildren<Collider2D>(true)
                .FirstOrDefault(candidate => candidate.CompareTag("Player"));
            var heroSound = hero.GetComponentInChildren<HeroSoundManager>(true);
            var grass = UnityEngine.Object.FindObjectsOfType<XiaoCao>(true)
                .First(candidate => candidate.gameObject.activeInHierarchy);
            var animator = grass.GetComponent<Animator>();
            var sound = new RecordingSoundService();

            Assert.That(heroCollider, Is.Not.Null);
            Assert.That(heroSound, Is.Not.Null);
            Assert.That(grass.audioClips, Is.Not.Empty.And.All.Not.Null,
                "Grass rustle clips must have no missing references.");
            Assert.That(heroSound.rMoveAudio, Is.Not.Empty.And.All.Not.Null);
            Assert.That(heroSound.rMove_Overwrite_Audio, Is.Not.Empty.And.All.Not.Null,
                "The repaired grass-footstep override list must have no missing references.");
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
            Assert.That(sound.Calls, Has.Count.EqualTo(1));
            Assert.That(sound.Calls[0].Channel, Is.EqualTo(ColorTimingSoundChannel.Player));
            Assert.That(heroSound.rMove_Overwrite_Audio, Does.Contain(sound.Calls[0].Clip));

            ExitGrass.Invoke(grass, new object[] { heroCollider });
            Assert.That(heroSound.moveCase, Does.Not.Contain(grass.gameObject.name));
            sound.Reset();
            heroSound.PlayAuido_Random("move");
            Assert.That(sound.Calls, Has.Count.EqualTo(1));
            Assert.That(sound.Calls[0].Channel, Is.EqualTo(ColorTimingSoundChannel.Player));
            Assert.That(heroSound.rMoveAudio, Does.Contain(sound.Calls[0].Clip));

            var hud = FindActive<UI_HeroInfo>();
            hud.TogglePause();
            yield return WaitUntil(() => FindActive<UI_ESC>() != null, 10f,
                "Grass contract cleanup could not open the pause form.");
            FindActive<UI_ESC>().BackMenu();
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
            yield return ColorTimingPlayModeBoot.EnsureFormalLaunchStartedInBatchMode();
            yield return WaitForScene("StartMenu", BootTimeout);
            yield return ColorTimingPlayModeBoot.WaitForProductSceneTransitions();
            yield return WaitUntil(() => FindActive<UI_ButtonAction>() != null, 10f,
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

            public int Play(AudioClip clip, ColorTimingSoundChannel channel, Vector3 position, bool loop = false)
            {
                Calls.Add(new Call(clip, channel));
                return Calls.Count;
            }

            public void ResetTrackedSounds()
            {
                Reset();
            }

            public void Reset()
            {
                Calls.Clear();
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
