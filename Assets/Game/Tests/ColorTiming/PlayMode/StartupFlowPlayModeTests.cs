using System.Collections;
using ColorTiming.Infrastructure.Unity.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ColorTiming.Tests.PlayMode
{
    public sealed class StartupFlowPlayModeTests
    {
        [UnityTest]
        [Timeout(60000)]
        public IEnumerator LaunchBootsFrameworkAndLoadsStartMenuOnce()
        {
            Time.timeScale = 1f;
            ColorTimingPlayModeBoot.PreserveTestRunnerAcrossFrameworkScenes();
            if (!SceneManager.GetSceneByName("Launch").isLoaded)
            {
                // The controller is moved to DontDestroyOnLoad first, so Launch can
                // reproduce the formal single-scene boot without ending the test run.
                SceneManager.LoadScene("Launch", LoadSceneMode.Single);
            }

            yield return ColorTimingPlayModeBoot.EnsureStartMenu(30f);
            var launchScene = SceneManager.GetSceneByName("Launch");
            Assert.That(launchScene.isLoaded, Is.True,
                "The framework Launch scene must remain loaded as the persistent bootstrap scene.");
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("StartMenu"),
                "StartMenu must become active while the persistent Launch scene stays inactive.");

            var adapters = Object.FindObjectsOfType<LegacyGameInputAdapter>(true);
            Assert.That(adapters.Length, Is.EqualTo(1),
                "The composition root must own exactly one persistent input adapter.");
        }
    }
}
