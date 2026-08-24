using System.Collections;
using GameFramework;
using GameFramework.Procedure;
using System;
using UnityEngine;

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

            // UGF's ProcedureComponent waits for WaitForEndOfFrame before starting
            // its entrance procedure. Unity does not advance that yield instruction
            // in batch mode, so headless tests complete the same formal start after
            // one ordinary frame. Editor and Player runtime paths remain unchanged.
            yield return null;
            var manager = GameFrameworkEntry.GetModule<IProcedureManager>();
            if (manager != null
                && manager.CurrentProcedure == null
                && manager.HasProcedure<LaunchProcedure>())
            {
                manager.StartProcedure<LaunchProcedure>();
                GFTrace.Info("Test", "BatchMode.LaunchProcedure.Started");
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
