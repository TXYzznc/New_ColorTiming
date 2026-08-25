using System.Collections;
using GameFramework;
using GameFramework.Procedure;
using System;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace ColorTiming.Tests.PlayMode
{
    internal static class ColorTimingPlayModeBoot
    {
        const string TestRunnerObjectName = "Code-based tests runner";

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
            if (!Application.isBatchMode)
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
