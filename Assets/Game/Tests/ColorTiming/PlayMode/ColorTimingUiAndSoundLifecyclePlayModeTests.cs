using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ColorTiming.Infrastructure.GF.Audio;
using ColorTiming.Infrastructure.GF.Settings;
using ColorTiming.Presentation.Audio;
using ColorTiming.Presentation.UI.Components;
using ColorTiming.Presentation.UI.Forms;
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

            var sequence = Object.FindObjectOfType<MainMenuIntroSequence>(true);
            Assert.That(sequence, Is.Not.Null);
            sequence.RestartSequence();
            yield return null;

            var intro = sequence.GetComponent<VideoPlayer>();
            var loop = sequence.loop2;
            var output = sequence.VideoDisplay;
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
            Assert.That(output.gameObject.layer, Is.EqualTo(sequence.gameObject.layer),
                "The runtime RawImage must stay on the same UI layer as the authored video objects.");

            var menu = FindActive<MainMenuForm>();
            Assert.That(menu, Is.Not.Null);
            menu.SettingBtnDwon();
            yield return new WaitForSecondsRealtime(0.25f);

            Assert.That(menu.SettingButtonBox.activeSelf, Is.True);
            Assert.That(loop.gameObject.activeInHierarchy, Is.True,
                "Opening settings must not disable the looping background video.");
            Assert.That(loop.isPlaying, Is.True,
                "The looping background video must continue while settings are open.");
            Assert.That(output.texture, Is.SameAs(loop.targetTexture));
        }

        [UnityTest]
        [Timeout(90000)]
        public IEnumerator StartMenuVideo_StopReleasesAndRestartRecreatesOutput()
        {
            yield return BootToStartMenu();

            var sequence = Object.FindObjectOfType<MainMenuIntroSequence>(true);
            Assert.That(sequence, Is.Not.Null);
            sequence.RestartSequence();
            yield return null;

            var intro = sequence.GetComponent<VideoPlayer>();
            var loop = sequence.loop2;
            var display = sequence.VideoDisplay;
            var firstOutput = intro.targetTexture;
            Assert.That(firstOutput, Is.Not.Null);
            Assert.That(display.texture, Is.SameAs(firstOutput));

            sequence.StopSequence();
            yield return null;

            Assert.That(intro.targetTexture, Is.Null);
            Assert.That(loop.targetTexture, Is.Null);
            Assert.That(display.texture, Is.Null);
            Assert.That(firstOutput == null, Is.True,
                "Stopping the pooled MainMenu form must destroy its runtime RenderTexture.");

            sequence.RestartSequence();
            yield return null;

            var recreatedOutput = intro.targetTexture;
            Assert.That(recreatedOutput, Is.Not.Null);
            Assert.That(ReferenceEquals(firstOutput, recreatedOutput), Is.False);
            Assert.That(loop.targetTexture, Is.SameAs(recreatedOutput));
            Assert.That(display.texture, Is.SameAs(recreatedOutput));
            Assert.That(intro.gameObject.activeSelf, Is.True,
                "Returning to MainMenu must replay the intro before switching to the loop video.");
        }

        [UnityTest]
        [Timeout(90000)]
        public IEnumerator PauseForm_ReopensAndSceneExitReleasesPauseLease()
        {
            yield return BootToStartMenu();

            var startMenu = FindActive<MainMenuForm>();
            Assert.That(startMenu, Is.Not.Null);
            startMenu.GoTest1();
            yield return WaitForScene("Boss1", TransitionTimeout);

            yield return WaitUntil(() => FindActive<BattlePlayerInfoView>() != null, 10f,
                "Boss1 HUD did not bind through the ColorTiming composition root.");
            var hud = FindActive<BattlePlayerInfoView>();

            for (var cycle = 0; cycle < 2; cycle++)
            {
                hud.TogglePause();
                yield return WaitUntil(() => FindActive<PauseMenuForm>() != null, 10f,
                    $"Pause form did not open on cycle {cycle + 1}.");
                Assert.That(Time.timeScale, Is.EqualTo(0f));

                hud.TogglePause();
                yield return WaitUntil(() => FindActive<PauseMenuForm>() == null, 10f,
                    $"Pause form did not close on cycle {cycle + 1}.");
                Assert.That(Time.timeScale, Is.EqualTo(1f));
            }

            hud.TogglePause();
            yield return WaitUntil(() => FindActive<PauseMenuForm>() != null, 10f,
                "Pause form did not open before the scene-exit check.");
            FindActive<PauseMenuForm>().GoNextLevel(2);
            yield return WaitForScene("Boss2", TransitionTimeout);

            Assert.That(Time.timeScale, Is.EqualTo(1f),
                "Leaving paused Boss1 must release the UI-owned game-time lease.");
            Assert.That(FindActive<PauseMenuForm>(), Is.Null,
                "The Boss1 pause form must not survive the transition to Boss2.");

            yield return WaitUntil(() => FindActive<BattlePlayerInfoView>() != null, 10f,
                "Boss2 HUD did not bind through the composition root.");
            var boss2Hud = FindActive<BattlePlayerInfoView>();
            boss2Hud.TogglePause();
            yield return WaitUntil(() => FindActive<PauseMenuForm>() != null, 10f,
                "Boss2 pause form did not open before returning to StartMenu.");
            FindActive<PauseMenuForm>().BackMenu();
            yield return WaitForScene("StartMenu", TransitionTimeout);

            Assert.That(Time.timeScale, Is.EqualTo(1f),
                "Leaving paused Boss2 must release the UI-owned game-time lease.");
            Assert.That(FindActive<PauseMenuForm>(), Is.Null,
                "The outgoing pause form must not survive the scene transition.");
            Assert.That(Object.FindObjectsOfType<MainMenuForm>(true).Length, Is.EqualTo(1),
                "GF.UI must reuse exactly one pooled StartMenu form after returning from gameplay.");
        }

        [UnityTest]
        [Timeout(90000)]
        public IEnumerator StartMenuNavigation_AndAllSettingsPersistThroughGfSetting()
        {
            yield return BootToStartMenu();

            var menu = FindActive<MainMenuForm>();
            Assert.That(menu, Is.Not.Null);
            Assert.That(menu.StartBtnBox.activeSelf, Is.True);
            Assert.That(menu.GoButtonBox.activeSelf, Is.False);
            Assert.That(menu.SettingButtonBox.activeSelf, Is.False);

            menu.StartGameBtnDown();
            Assert.That(menu.StartBtnBox.activeSelf, Is.False);
            Assert.That(menu.GoButtonBox.activeSelf, Is.True);
            menu.BackStartBtnDown();
            Assert.That(menu.StartBtnBox.activeSelf, Is.True);
            Assert.That(menu.GoButtonBox.activeSelf, Is.False);

            menu.SettingBtnDwon();
            Assert.That(menu.StartBtnBox.activeSelf, Is.False);
            Assert.That(menu.SettingButtonBox.activeSelf, Is.True);
            menu.BackSettingBtnDwon();
            Assert.That(menu.StartBtnBox.activeSelf, Is.True);
            Assert.That(menu.SettingButtonBox.activeSelf, Is.False);

            var settings = new GfColorTimingSettings();
            var originalBgm = settings.BgmEnabled;
            var originalSfx = settings.SfxEnabled;
            var originalKeyTips = settings.KeyTipsDisabled;
            try
            {
                menu.SetBGM(false);
                menu.SetSFX(false);
                menu.OffKeyTip();

                var disabled = new GfColorTimingSettings();
                Assert.That(disabled.BgmEnabled, Is.False);
                Assert.That(disabled.SfxEnabled, Is.False);
                Assert.That(disabled.KeyTipsDisabled, Is.True);
                Assert.That(menu.BGMBtn_Open.activeSelf, Is.True);
                Assert.That(menu.BGMBtn_Off.activeSelf, Is.False);
                Assert.That(menu.SFXBtn_Open.activeSelf, Is.True);
                Assert.That(menu.SFXBtn_Off.activeSelf, Is.False);
                Assert.That(menu.offTipButton.activeSelf, Is.False);
                Assert.That(menu.openTipButton.activeSelf, Is.True);

                menu.SetBGM(true);
                menu.SetSFX(true);
                menu.OpenKeyTip();

                var enabled = new GfColorTimingSettings();
                Assert.That(enabled.BgmEnabled, Is.True);
                Assert.That(enabled.SfxEnabled, Is.True);
                Assert.That(enabled.KeyTipsDisabled, Is.False);
                Assert.That(menu.BGMBtn_Open.activeSelf, Is.False);
                Assert.That(menu.BGMBtn_Off.activeSelf, Is.True);
                Assert.That(menu.SFXBtn_Open.activeSelf, Is.False);
                Assert.That(menu.SFXBtn_Off.activeSelf, Is.True);
                Assert.That(menu.offTipButton.activeSelf, Is.True);
                Assert.That(menu.openTipButton.activeSelf, Is.False);
            }
            finally
            {
                settings.BgmEnabled = originalBgm;
                settings.SfxEnabled = originalSfx;
                settings.KeyTipsDisabled = originalKeyTips;
                menu.BindSettings(settings);
            }
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

        [UnityTest]
        [Timeout(90000)]
        public IEnumerator BattleSceneAudio_UsesGfSoundWithoutAuthoredAudioSources()
        {
            yield return BootToStartMenu();

            FindActive<MainMenuForm>().GoTest1();
            yield return WaitForScene("Boss1", TransitionTimeout);
            yield return WaitUntil(() => FindActive<BattlePlayerInfoView>() != null, 10f,
                "Boss1 HUD did not bind before the audio lifecycle check.");

            var service = Object.FindObjectOfType<GfColorTimingSoundService>(true);
            Assert.That(service, Is.Not.Null);
            yield return WaitUntil(() => GetLoadedSceneSoundCount(service) >= 2, 10f,
                "Boss1 BGM and ambience did not finish loading and start through GF.Sound.");
            Assert.That(CountAuthoredSceneAudioSources(SceneManager.GetSceneByName("Boss1")), Is.Zero,
                "Boss1 must not retain authored AudioSource configuration objects.");

            var hud = FindActive<BattlePlayerInfoView>();
            hud.TogglePause();
            yield return WaitUntil(() => FindActive<PauseMenuForm>() != null, 10f,
                "Pause form did not open before the Boss2 audio lifecycle check.");
            FindActive<PauseMenuForm>().GoNextLevel(2);
            yield return WaitForScene("Boss2", TransitionTimeout);

            yield return WaitUntil(() => GetLoadedSceneSoundCount(service) >= 1, 10f,
                "Boss2 BGM did not finish loading and start through GF.Sound.");
            Assert.That(CountAuthoredSceneAudioSources(SceneManager.GetSceneByName("Boss2")), Is.Zero,
                "Boss2 must not retain authored AudioSource configuration objects.");
        }

        static IEnumerator BootToStartMenu()
        {
            Time.timeScale = 1f;
            ColorTimingPlayModeBoot.PreserveTestRunnerAcrossFrameworkScenes();
            // Launch is the framework's persistent scene and must boot only once per
            // PlayMode session. Reloading it with Single destroys GameEntry while its
            // static component registry is still live, which is not a supported flow.
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

        static int GetLoadedSceneSoundCount(GfColorTimingSoundService service)
        {
            var sounds = (HashSet<int>)typeof(GfColorTimingSoundService)
                .GetField("loadedSounds", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(service);
            return sounds?.Count ?? 0;
        }

        static int CountAuthoredSceneAudioSources(Scene scene)
        {
            var count = 0;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var source in root.GetComponentsInChildren<AudioSource>(true))
                if (source.clip != null) count++;
            return count;
        }
    }
}
