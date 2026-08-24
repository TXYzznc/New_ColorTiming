using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ColorTiming.Presentation.Audio;
using ColorTiming.Settings;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.Video;

namespace ColorTiming.Tests.PlayMode
{
    public sealed class ColorTimingUiAndSoundLifecyclePlayModeTests
    {
        const float BootTimeout = 30f;
        const float TransitionTimeout = 20f;

        [UnityTest]
        [Timeout(90000)]
        public IEnumerator StartMenuVideo_RendersAndSwitchesFromIntroToLoop()
        {
            yield return BootToStartMenu();

            var sequence = Object.FindObjectOfType<StartVido>(true);
            Assert.That(sequence, Is.Not.Null);
            sequence.RestartSequence();
            yield return null;

            var intro = sequence.GetComponent<VideoPlayer>();
            var loop = sequence.loop2;
            var output = sequence.transform.parent.Find("VideoOutput")?.GetComponent<RawImage>();
            Assert.That(intro, Is.Not.Null);
            Assert.That(loop, Is.Not.Null);
            Assert.That(output, Is.Not.Null);
            Assert.That(intro.renderMode, Is.EqualTo(VideoRenderMode.RenderTexture));
            Assert.That(loop.renderMode, Is.EqualTo(VideoRenderMode.RenderTexture));
            Assert.That(intro.targetTexture, Is.Not.Null.And.SameAs(loop.targetTexture));
            Assert.That(output.texture, Is.SameAs(intro.targetTexture));
            Assert.That(intro.gameObject.activeInHierarchy, Is.True);
            Assert.That(loop.gameObject.activeSelf, Is.False);

            yield return WaitUntil(() => intro.isPrepared && intro.isPlaying, 10f,
                "The StartMenu intro video did not prepare and start.");
            yield return WaitUntil(
                () => loop.gameObject.activeInHierarchy && loop.isPrepared && loop.isPlaying
                      && !sequence.gameObject.activeSelf,
                10f,
                "The intro video did not switch to the looping video.");

            Assert.That(sequence.gameObject.activeSelf, Is.False);
            Assert.That(loop.isLooping, Is.True);
            Assert.That(loop.texture, Is.Not.Null);
        }

        [UnityTest]
        [Timeout(90000)]
        public IEnumerator PauseForm_ReopensAndSceneExitReleasesPauseLease()
        {
            yield return BootToStartMenu();

            var startMenu = FindActive<UI_ButtonAction>();
            Assert.That(startMenu, Is.Not.Null);
            startMenu.GoTest1();
            yield return WaitForScene("Boss1", TransitionTimeout);

            var hud = FindActive<UI_HeroInfo>();
            Assert.That(hud, Is.Not.Null, "Boss1 HUD did not bind through the ColorTiming composition root.");

            for (var cycle = 0; cycle < 2; cycle++)
            {
                hud.TogglePause();
                yield return WaitUntil(() => FindActive<UI_ESC>() != null, 10f,
                    $"Pause form did not open on cycle {cycle + 1}.");
                Assert.That(Time.timeScale, Is.EqualTo(0f));

                hud.TogglePause();
                yield return WaitUntil(() => FindActive<UI_ESC>() == null, 10f,
                    $"Pause form did not close on cycle {cycle + 1}.");
                Assert.That(Time.timeScale, Is.EqualTo(1f));
            }

            hud.TogglePause();
            yield return WaitUntil(() => FindActive<UI_ESC>() != null, 10f,
                "Pause form did not open before the scene-exit check.");
            FindActive<UI_ESC>().GoNextLevel(2);
            yield return WaitForScene("Boss2", TransitionTimeout);

            Assert.That(Time.timeScale, Is.EqualTo(1f),
                "Leaving paused Boss1 must release the UI-owned game-time lease.");
            Assert.That(FindActive<UI_ESC>(), Is.Null,
                "The Boss1 pause form must not survive the transition to Boss2.");

            var boss2Hud = FindActive<UI_HeroInfo>();
            Assert.That(boss2Hud, Is.Not.Null, "Boss2 HUD did not bind through the composition root.");
            boss2Hud.TogglePause();
            yield return WaitUntil(() => FindActive<UI_ESC>() != null, 10f,
                "Boss2 pause form did not open before returning to StartMenu.");
            FindActive<UI_ESC>().BackMenu();
            yield return WaitForScene("StartMenu", TransitionTimeout);

            Assert.That(Time.timeScale, Is.EqualTo(1f),
                "Leaving paused Boss2 must release the UI-owned game-time lease.");
            Assert.That(FindActive<UI_ESC>(), Is.Null,
                "The outgoing pause form must not survive the scene transition.");
            Assert.That(Object.FindObjectsOfType<UI_ButtonAction>(true).Length, Is.EqualTo(1),
                "GF.UI must reuse exactly one pooled StartMenu form after returning from gameplay.");
        }

        [UnityTest]
        [Timeout(90000)]
        public IEnumerator SoundGroups_PersistMutePolicyAndResetTrackedSceneSounds()
        {
            yield return BootToStartMenu();

            Assert.That(GF.Sound, Is.Not.Null);
            Assert.That(GF.Setting, Is.Not.Null);
            Assert.That(GF.Sound.GetSoundGroup("BGM").SoundAgentCount, Is.EqualTo(1),
                "BGM is the singleton channel.");
            Assert.That(GF.Sound.GetSoundGroup("UI").SoundAgentCount, Is.EqualTo(4));
            Assert.That(GF.Sound.GetSoundGroup("Player").SoundAgentCount, Is.EqualTo(8));
            Assert.That(GF.Sound.GetSoundGroup("Boss").SoundAgentCount, Is.EqualTo(8));
            Assert.That(GF.Sound.GetSoundGroup("Environment").SoundAgentCount, Is.EqualTo(4));

            var settings = new GfColorTimingSettings();
            var originalBgm = settings.BgmEnabled;
            var originalSfx = settings.SfxEnabled;
            try
            {
                settings.BgmEnabled = false;
                settings.SfxEnabled = false;

                var reloaded = new GfColorTimingSettings();
                Assert.That(reloaded.BgmEnabled, Is.False);
                Assert.That(reloaded.SfxEnabled, Is.False);
                Assert.That(GF.Sound.GetSoundGroup("BGM").Mute, Is.True);
                Assert.That(GF.Sound.GetSoundGroup("UI").Mute, Is.True);
                Assert.That(GF.Sound.GetSoundGroup("Player").Mute, Is.True);
                Assert.That(GF.Sound.GetSoundGroup("Boss").Mute, Is.True);
                Assert.That(GF.Sound.GetSoundGroup("Environment").Mute, Is.True);
            }
            finally
            {
                settings.BgmEnabled = originalBgm;
                settings.SfxEnabled = originalSfx;
            }

            var service = Object.FindObjectOfType<GfColorTimingSoundService>(true);
            Assert.That(service, Is.Not.Null);
            service.ResetTrackedSounds();
            yield return null;

            var sceneSounds = (HashSet<int>)typeof(GfColorTimingSoundService)
                .GetField("sceneSounds", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(service);
            var gameplaySounds = (HashSet<int>)typeof(GfColorTimingSoundService)
                .GetField("gameplaySounds", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(service);
            Assert.That(sceneSounds, Is.Not.Null.And.Empty,
                "Scene sound tracking must be empty after explicit scene-exit cleanup.");
            Assert.That(gameplaySounds, Is.Not.Null.And.Empty,
                "Gameplay pause tracking must be empty after explicit scene-exit cleanup.");
            Assert.That(GF.Sound.GetAllLoadingSoundSerialIds(), Is.Empty,
                "Scene-exit cleanup must also cancel in-flight ColorTiming sound loads.");
        }

        static IEnumerator BootToStartMenu()
        {
            Time.timeScale = 1f;
            // Launch is the framework's persistent scene and must boot only once per
            // PlayMode session. Reloading it with Single destroys GameEntry while its
            // static component registry is still live, which is not a supported flow.
            if (!SceneManager.GetSceneByName("Launch").isLoaded)
            {
                SceneManager.LoadScene("Launch", LoadSceneMode.Single);
            }
            yield return WaitForScene("StartMenu", BootTimeout);
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
