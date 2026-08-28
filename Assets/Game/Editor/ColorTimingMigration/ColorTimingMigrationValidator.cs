using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace ColorTiming.Editor
{
    public static class ColorTimingMigrationValidator
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/Game/Scene/StartMenu.unity",
            "Assets/Game/Scene/Boss1.unity",
            "Assets/Game/Scene/Boss2.unity",
        };

        private static readonly string[] FormalBuildScenePaths =
        {
            "Assets/Game/Scene/Launch.unity",
            "Assets/Game/Scene/StartMenu.unity",
            "Assets/Game/Scene/Boss1.unity",
            "Assets/Game/Scene/Boss2.unity",
        };

        private static readonly HashSet<string> SerializedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".unity", ".prefab", ".controller", ".anim", ".asset", ".mat",
        };

        private static readonly Regex GuidReference = new Regex(
            @"guid:\s*([0-9a-fA-F]{32})",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly HashSet<string> SourceBaselineUnresolvedGuids = new HashSet<string>(StringComparer.Ordinal)
        {
            // One missing Animator StateMachineBehaviour and four missing AudioClip assets already absent in source.
            "2b1e9356cb2d5ee4e8406c3fd664c52b",
            "03c9da642a0b3bd439aacc38e58cd06d",
            "8bd8297382c57c641a66af972eca98f4",
            "bdacb22ff60676743887395ad9d4a6f2",
            "fdf644124e717f24d87a043e47365b93",
        };

        [Serializable]
        private sealed class ValidationReport
        {
            public string unityVersion;
            public string generatedAtUtc;
            public List<SceneReport> scenes = new List<SceneReport>();
            public int scannedSerializedAssets;
            public int scannedGuidReferences;
            public List<string> missingScriptObjects = new List<string>();
            public List<string> unresolvedGuidReferences = new List<string>();
            public List<string> sourceBaselineUnresolvedGuidReferences = new List<string>();
            public List<string> loadFailures = new List<string>();
            public bool passed;
        }

        [Serializable]
        private sealed class SceneReport
        {
            public string path;
            public int gameObjects;
            public int components;
            public int missingScripts;
        }

        public static void Run()
        {
            var report = new ValidationReport
            {
                unityVersion = global::UnityEngine.Application.unityVersion,
                generatedAtUtc = DateTime.UtcNow.ToString("O"),
            };

            try
            {
                ValidateScenes(report);
                ValidateGuidReferences(report);
            }
            catch (Exception exception)
            {
                report.loadFailures.Add(exception.ToString());
            }

            report.passed = report.missingScriptObjects.Count == 0
                && report.unresolvedGuidReferences.Count == 0
                && report.loadFailures.Count == 0;

            var projectRoot = Directory.GetParent(global::UnityEngine.Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Could not resolve project root.");
            var outputPath = Path.Combine(projectRoot, "Documentation", "Refactor", "asset-validation.json");
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? projectRoot);
            File.WriteAllText(outputPath, JsonUtility.ToJson(report, true));

            Debug.Log($"ColorTiming migration asset validation: {(report.passed ? "PASS" : "FAIL")} ({outputPath})");
            EditorApplication.Exit(report.passed ? 0 : 1);
        }

        public static void RepairLegacyEventSystems()
        {
            var repaired = 0;
            foreach (var scenePath in ScenePaths.Where(path => path.EndsWith("Boss1.unity", StringComparison.Ordinal)
                                                               || path.EndsWith("Boss2.unity", StringComparison.Ordinal)))
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var eventSystem = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<EventSystem>(true))
                    .SingleOrDefault();
                if (eventSystem == null)
                {
                    throw new InvalidOperationException($"Expected one EventSystem in {scenePath}.");
                }

                var gameObject = eventSystem.gameObject;
                var missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                if (missing > 0)
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                }

                if (gameObject.GetComponent<BaseInputModule>() == null)
                {
                    gameObject.AddComponent<StandaloneInputModule>();
                }

                EditorUtility.SetDirty(gameObject);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                repaired++;
            }

            Debug.Log($"ColorTiming legacy EventSystem repair completed: {repaired} scenes.");
            EditorApplication.Exit(repaired == 2 ? 0 : 1);
        }

        [MenuItem("Game Framework/GameTools/Refresh ColorTiming Resource Collection", false, 1002)]
        public static void RefreshFrameworkResourceCollection()
        {
            UGF.EditorTools.ResourceTools.ResourceRuleEditorUtility.RefreshResourceCollection();
            AssetDatabase.SaveAssets();
            Debug.Log("ColorTiming framework resource collection refreshed.");
        }

        [MenuItem("Game Framework/GameTools/Sync ColorTiming Build Scenes", false, 1003)]
        public static void SyncFormalBuildScenes()
        {
            var scenes = FormalBuildScenePaths.Select(path =>
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
                {
                    throw new InvalidOperationException($"Formal ColorTiming scene is missing: {path}");
                }

                return new EditorBuildSettingsScene(path, true);
            }).ToArray();

            EditorBuildSettings.scenes = scenes;
            AssetDatabase.SaveAssets();
            Debug.Log($"ColorTiming formal Build Settings synchronized: {scenes.Length} enabled scenes.");
        }

        private static void ValidateScenes(ValidationReport report)
        {
            foreach (var scenePath in ScenePaths)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
                {
                    report.loadFailures.Add($"Scene asset could not be loaded: {scenePath}");
                    continue;
                }

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var sceneReport = new SceneReport { path = scenePath };
                foreach (var root in scene.GetRootGameObjects())
                {
                    Visit(root.transform, scenePath, sceneReport, report);
                }

                report.scenes.Add(sceneReport);
            }
        }

        private static void Visit(
            Transform transform,
            string scenePath,
            SceneReport sceneReport,
            ValidationReport report)
        {
            var gameObject = transform.gameObject;
            sceneReport.gameObjects++;
            sceneReport.components += gameObject.GetComponents<Component>().Length;

            var missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
            sceneReport.missingScripts += missingCount;
            if (missingCount > 0)
            {
                report.missingScriptObjects.Add($"{scenePath}:{GetHierarchyPath(transform)} ({missingCount})");
            }

            for (var index = 0; index < transform.childCount; index++)
            {
                Visit(transform.GetChild(index), scenePath, sceneReport, report);
            }
        }

        private static string GetHierarchyPath(Transform transform)
        {
            var names = new Stack<string>();
            for (var current = transform; current != null; current = current.parent)
            {
                names.Push(current.name);
            }

            return string.Join("/", names);
        }

        private static void ValidateGuidReferences(ValidationReport report)
        {
            var productRoots = new[]
            {
                Path.Combine(global::UnityEngine.Application.dataPath, "Game", "ColorTiming"),
                Path.Combine(global::UnityEngine.Application.dataPath, "Game", "Prefabs", "Entity", "ColorTiming"),
                Path.Combine(global::UnityEngine.Application.dataPath, "Game", "Prefabs", "UI", "ColorTiming"),
                Path.Combine(global::UnityEngine.Application.dataPath, "Game", "Prefabs", "World", "ColorTiming"),
            };

            var projectRoot = Directory.GetParent(global::UnityEngine.Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Could not resolve project root.");
            var serializedFiles = productRoots
                         .Where(Directory.Exists)
                         .SelectMany(path => Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                         .Where(path => SerializedExtensions.Contains(Path.GetExtension(path)))
                         .Concat(ScenePaths.Select(path => Path.Combine(projectRoot, path.Replace('/', Path.DirectorySeparatorChar))))
                         .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var filePath in serializedFiles)
            {
                report.scannedSerializedAssets++;
                var projectPath = "Assets" + filePath.Substring(global::UnityEngine.Application.dataPath.Length).Replace('\\', '/');
                foreach (Match match in GuidReference.Matches(File.ReadAllText(filePath)))
                {
                    report.scannedGuidReferences++;
                    var guid = match.Groups[1].Value.ToLowerInvariant();
                    if (guid.StartsWith("0000000000000000", StringComparison.Ordinal)
                        || !string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid)))
                    {
                        continue;
                    }

                    var finding = $"{projectPath}:{guid}";
                    if (SourceBaselineUnresolvedGuids.Contains(guid))
                    {
                        report.sourceBaselineUnresolvedGuidReferences.Add(finding);
                    }
                    else
                    {
                        report.unresolvedGuidReferences.Add(finding);
                    }
                }
            }

            report.unresolvedGuidReferences = report.unresolvedGuidReferences
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
            report.sourceBaselineUnresolvedGuidReferences = report.sourceBaselineUnresolvedGuidReferences
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
        }
    }
}
