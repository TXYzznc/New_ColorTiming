using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using ColorTiming.Presentation.UI.Components;
using ColorTiming.Presentation.UI.Forms;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ColorTiming.Tests.PlayMode
{
    public sealed class CameraRuntimeExecutionPlayModeTests
    {
        const float BootTimeout = 30f;
        const float TransitionTimeout = 20f;

        static readonly MethodInfo UpdateParallax = typeof(CameraParallaxView).GetMethod(
            "Update",
            BindingFlags.Instance | BindingFlags.NonPublic);

        static readonly MethodInfo UpdateHeroCamera = typeof(PlayerCameraLifecycleView).GetMethod(
            "Update",
            BindingFlags.Instance | BindingFlags.NonPublic);

        static readonly FieldInfo VirtualCamera = typeof(PlayerCameraLifecycleView).GetField(
            "virtualCamera",
            BindingFlags.Instance | BindingFlags.Public);

        [UnityTest]
        [Timeout(90000)]
        public IEnumerator Boss1_ParallaxDistanceZoomAndCinemachineRuntimeWiringExecute()
        {
            yield return BootToStartMenu();
            FindActive<MainMenuForm>().GoTest1();
            yield return WaitForScene("Boss1", TransitionTimeout);
            yield return ColorTimingPlayModeBoot.WaitForBattleReady("Boss1", TransitionTimeout);

            var mainCamera = Camera.main;
            var parallax = UnityEngine.Object.FindObjectsOfType<CameraParallaxView>(true)
                .First(candidate => candidate.gameObject.activeInHierarchy && candidate.caseLevel > 0f);
            var heroCamera = FindActive<PlayerCameraLifecycleView>();

            Assert.That(mainCamera, Is.Not.Null);
            Assert.That(UpdateParallax, Is.Not.Null);
            Assert.That(UpdateHeroCamera, Is.Not.Null);
            Assert.That(VirtualCamera, Is.Not.Null);
            Assert.That(heroCamera.bossT, Is.Not.Null);

            var parallaxStart = parallax.transform.position;
            var cameraStart = mainCamera.transform.position;
            var cameraDelta = new Vector3(6f, -4f, 0f);
            try
            {
                parallax.enabled = false;
                parallax.BindGameplayCamera(mainCamera);
                mainCamera.transform.position = parallaxStart + cameraDelta;
                UpdateParallax.Invoke(parallax, null);

                var expected = parallaxStart + cameraDelta * parallax.caseLevel;
                expected.z = parallaxStart.z;
                Assert.That(parallax.transform.position.x, Is.EqualTo(expected.x).Within(0.001f));
                Assert.That(parallax.transform.position.y, Is.EqualTo(expected.y).Within(0.001f));
                Assert.That(parallax.transform.position.z, Is.EqualTo(expected.z).Within(0.001f));

                heroCamera.enabled = false;
                var direction = Vector3.right;
                heroCamera.transform.position = heroCamera.bossT.position + direction * 5f;
                UpdateHeroCamera.Invoke(heroCamera, null);
                Assert.That(ReadOrthographicSize(heroCamera),
                    Is.EqualTo(heroCamera.minSize).Within(0.001f));

                heroCamera.transform.position = heroCamera.bossT.position
                                                + direction * (5f + heroCamera.disRi);
                UpdateHeroCamera.Invoke(heroCamera, null);
                Assert.That(ReadOrthographicSize(heroCamera),
                    Is.EqualTo(heroCamera.maxSize).Within(0.001f));

                heroCamera.transform.position = heroCamera.bossT.position
                                                + direction * (5f + heroCamera.disRi * 2f);
                UpdateHeroCamera.Invoke(heroCamera, null);
                Assert.That(ReadOrthographicSize(heroCamera),
                    Is.EqualTo(heroCamera.maxSize).Within(0.001f),
                    "Mathf.Lerp must clamp the authored distance zoom at maxSize.");

                AssertRuntimeComponent("CinemachineBrain");
                AssertRuntimeComponent("CinemachineVirtualCamera");
                AssertRuntimeComponent("CinemachineConfiner2D");
                AssertRuntimeComponent("CinemachineImpulseSource");
                AssertRuntimeComponent("CinemachineImpulseListener");
            }
            finally
            {
                mainCamera.transform.position = cameraStart;
                parallax.transform.position = parallaxStart;
                parallax.BindGameplayCamera(mainCamera);
            }

            var hud = FindActive<BattlePlayerInfoView>();
            hud.TogglePause();
            yield return WaitUntil(() => FindActive<PauseMenuForm>() != null, 10f,
                "Camera contract cleanup could not open the pause form.");
            FindActive<PauseMenuForm>().BackMenu();
            yield return WaitForScene("StartMenu", TransitionTimeout);
        }

        static float ReadOrthographicSize(PlayerCameraLifecycleView heroCamera)
        {
            var virtualCamera = VirtualCamera.GetValue(heroCamera);
            Assert.That(virtualCamera, Is.Not.Null);

            var lensField = virtualCamera.GetType().GetField(
                "m_Lens",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(lensField, Is.Not.Null);
            var lens = lensField.GetValue(virtualCamera);
            var sizeField = lens.GetType().GetField(
                "OrthographicSize",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(sizeField, Is.Not.Null);
            return (float)sizeField.GetValue(lens);
        }

        static void AssertRuntimeComponent(string typeName)
        {
            var found = UnityEngine.Object.FindObjectsOfType<Component>(true)
                .Any(component => component.gameObject.activeInHierarchy
                                  && component.GetType().Name == typeName);
            Assert.That(found, Is.True, $"Boss1 lacks an active {typeName} runtime component.");
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
    }
}
