using System.Collections;
using GameFramework;
using GameFramework.Procedure;
using System;
using System.Reflection;
using ColorTiming.Bootstrap;
using ColorTiming.Bootstrap.Flow;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityGameFramework.Runtime;

namespace ColorTiming.Tests.PlayMode
{
    internal static class ColorTimingPlayModeBoot
    {
        const string TestRunnerObjectName = "Code-based tests runner";
        static readonly FieldInfo CompositionRootField = typeof(ColorTimingStartupProcedure).GetField(
            "compositionRoot",
            BindingFlags.Instance | BindingFlags.NonPublic);

        public static void PreserveTestRunnerAcrossFrameworkScenes()
        {
            var controller = GameObject.Find(TestRunnerObjectName);
            if (controller != null && controller.scene.name != "DontDestroyOnLoad")
            {
                UnityEngine.Object.DontDestroyOnLoad(controller);
            }
        }

        public static string DescribeFrameworkState(string reason)
        {
            return JsonUtility.ToJson(GFDiagnosticSnapshot.Capture(reason, 40), true);
        }

        public static IEnumerator EnsureFormalLaunchStartedInBatchMode()
        {
            if (!global::UnityEngine.Application.isBatchMode)
            {
                yield break;
            }

            yield return null;
            var manager = GameFrameworkEntry.GetModule<IProcedureManager>();
            var initDeadline = Time.realtimeSinceStartup + 10f;
            while (Time.realtimeSinceStartup < initDeadline)
            {
                manager = GameFrameworkEntry.GetModule<IProcedureManager>();
                if (TryHasProcedure<LaunchProcedure>(manager))
                {
                    break;
                }

                yield return null;
            }

            if (manager != null
                && TryHasProcedure<LaunchProcedure>(manager))
            {
                var shouldStart = true;
                if (TryGetCurrentProcedure(manager, out var current))
                {
                    shouldStart = current == null || current.GetType() != typeof(LaunchProcedure);
                }

                if (shouldStart)
                {
                    manager.StartProcedure<LaunchProcedure>();
                    GFTrace.Info("Test", "BatchMode.LaunchProcedure.Started");
                }
            }
        }

        /// <summary>
        /// Gives every framework PlayMode test a deterministic StartMenu boundary, including
        /// recovery after a preceding test fails while a Boss scene is still loaded.
        /// </summary>
        public static IEnumerator EnsureStartMenu(float timeout = 30f)
        {
            PreserveTestRunnerAcrossFrameworkScenes();
            yield return EnsureFormalLaunchStartedInBatchMode();
            yield return WaitForProductSceneTransitions();
            if (SceneManager.GetSceneByName("StartMenu").isLoaded)
            {
                yield break;
            }

            var manager = GameFrameworkEntry.GetModule<IProcedureManager>();
            ColorTimingStartupProcedure startup = null;
            var procedureDeadline = Time.realtimeSinceStartup + 10f;
            while (Time.realtimeSinceStartup < procedureDeadline)
            {
                manager = GameFrameworkEntry.GetModule<IProcedureManager>();
                if (TryGetCurrentProcedure(manager, out var current)
                    && current is ColorTimingStartupProcedure ready)
                {
                    startup = ready;
                    break;
                }
                if (SceneManager.GetSceneByName("StartMenu").isLoaded)
                {
                    yield break;
                }
                yield return null;
            }

            if (startup == null)
            {
                throw new InvalidOperationException(
                    "ColorTiming startup procedure is unavailable while recovering the PlayMode test boundary.");
            }

            var compositionRoot = CompositionRootField?.GetValue(startup) as ColorTimingCompositionRoot;
            if (compositionRoot == null)
            {
                throw new InvalidOperationException(
                    "ColorTiming composition root is unavailable while recovering the PlayMode test boundary.");
            }

            // False is also valid when the startup procedure has already requested StartMenu
            // and GF.Scene has not yet exposed the loading operation to the polling helper.
            compositionRoot.SceneFlow.TryLoad(ColorTimingSceneId.StartMenu);

            var deadline = Time.realtimeSinceStartup + timeout;
            while (!SceneManager.GetSceneByName("StartMenu").isLoaded
                   && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            if (!SceneManager.GetSceneByName("StartMenu").isLoaded)
            {
                throw new TimeoutException("ColorTiming did not recover StartMenu before the PlayMode test timeout.");
            }
            yield return WaitForProductSceneTransitions();
        }

        static bool TryGetCurrentProcedure(IProcedureManager manager, out ProcedureBase current)
        {
            current = null;
            if (manager == null)
            {
                return false;
            }

            try
            {
                current = manager.CurrentProcedure;
                return true;
            }
            catch (GameFrameworkException)
            {
                return false;
            }
        }

        static bool TryHasProcedure<T>(IProcedureManager manager) where T : ProcedureBase
        {
            if (manager == null)
            {
                return false;
            }

            try
            {
                return manager.HasProcedure<T>();
            }
            catch (GameFrameworkException)
            {
                return false;
            }
        }

        public static IEnumerator WaitForProductSceneTransitions(float timeout = 20f)
        {
            if (GF.Scene == null)
            {
                yield break;
            }

            var productScenes = new[]
            {
                "Assets/Game/Scene/StartMenu.unity",
                "Assets/Game/Scene/Boss1.unity",
                "Assets/Game/Scene/Boss2.unity"
            };
            var deadline = Time.realtimeSinceStartup + timeout;
            while (HasPendingSceneTransition(productScenes)
                   && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            if (HasPendingSceneTransition(productScenes))
            {
                throw new TimeoutException(
                    "ColorTiming product scenes did not finish loading/unloading between PlayMode tests.");
            }
        }

        static bool HasPendingSceneTransition(string[] sceneAssets)
        {
            foreach (var sceneAsset in sceneAssets)
            {
                if (GF.Scene.SceneIsLoading(sceneAsset) || GF.Scene.SceneIsUnloading(sceneAsset))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
