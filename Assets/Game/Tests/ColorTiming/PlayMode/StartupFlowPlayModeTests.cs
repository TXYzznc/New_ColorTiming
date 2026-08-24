using System.Collections;
using ColorTiming.Input.Adapters;
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
            if (!SceneManager.GetSceneByName("Launch").isLoaded)
            {
                SceneManager.LoadScene("Launch", LoadSceneMode.Single);
            }

            var deadline = Time.realtimeSinceStartup + 30f;
            while (!SceneManager.GetSceneByName("StartMenu").isLoaded && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(SceneManager.GetSceneByName("StartMenu").isLoaded, Is.True,
                "Framework startup did not reach StartMenu within 30 real-time seconds.");
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
