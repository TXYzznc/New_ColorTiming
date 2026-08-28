using System;
using System.Collections.Generic;
using System.Linq;
using ColorTiming.Application.Battle;
using ColorTiming.Bootstrap;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Combat;
using ColorTiming.Input;
using ColorTiming.Presentation.Audio;
using ColorTiming.Presentation.Actors;
using ColorTiming.Presentation.Entities;
using ColorTiming.Presentation.UI;
using ColorTiming.Settings;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorTiming.Editor
{
    public static class ColorTimingBattleArchitectureMigration
    {
        static readonly string[] BattleScenes =
        {
            "Assets/Game/Scene/Boss1.unity",
            "Assets/Game/Scene/Boss2.unity",
        };

        [MenuItem("Game Framework/GameTools/ColorTiming/Migrate Battle Architecture Anchors", false, 1010)]
        public static void Migrate()
        {
            var original = SceneManager.GetActiveScene().path;
            try
            {
                foreach (var path in BattleScenes) MigrateScene(path);
                AssetDatabase.SaveAssets();
                Debug.Log("ColorTiming battle architecture anchors migrated for Boss1 and Boss2.");
            }
            finally
            {
                if (!string.IsNullOrEmpty(original) && AssetDatabase.LoadAssetAtPath<SceneAsset>(original) != null)
                    EditorSceneManager.OpenScene(original, OpenSceneMode.Single);
            }
        }

        static void MigrateScene(string path)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var behaviours = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true))
                .Where(value => value != null)
                .ToArray();
            var hero = Single<PlayerActorView>(behaviours, path);
            var boss1 = behaviours.OfType<Boss1ActorView>().SingleOrDefault();
            var boss2 = behaviours.OfType<Boss2ActorView>().SingleOrDefault();
            if ((boss1 != null) == (boss2 != null))
                throw new InvalidOperationException($"{path} must contain exactly one supported boss.");
            var camera = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Camera>(true))
                .FirstOrDefault(value => value.CompareTag("MainCamera"))
                ?? throw new InvalidOperationException($"{path} has no MainCamera.");

            var existing = scene.GetRootGameObjects()
                .Select(root => root.GetComponent<BattleSceneAnchors>())
                .Where(value => value != null)
                .ToArray();
            if (existing.Length > 1) throw new InvalidOperationException($"{path} has duplicate BattleSceneAnchors.");
            var anchors = existing.SingleOrDefault();
            if (anchors == null)
            {
                var root = new GameObject("Anchor_BattleScene");
                SceneManager.MoveGameObjectToScene(root, scene);
                anchors = root.AddComponent<BattleSceneAnchors>();
            }

            var explicitBindings = behaviours
                .Where(IsExplicitBinding)
                .Distinct()
                .OrderBy(value => HierarchyPath(value.transform), StringComparer.Ordinal)
                .ThenBy(value => value.GetType().FullName, StringComparer.Ordinal)
                .ToArray();
            var audioSources = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<AudioSource>(true))
                .Where(source => source != null && source.playOnAwake && source.clip != null)
                .OrderBy(source => HierarchyPath(source.transform), StringComparer.Ordinal)
                .ToArray();

            var serialized = new SerializedObject(anchors);
            serialized.FindProperty("hero").objectReferenceValue = hero;
            serialized.FindProperty("boss1").objectReferenceValue = boss1;
            serialized.FindProperty("boss2").objectReferenceValue = boss2;
            serialized.FindProperty("gameplayCamera").objectReferenceValue = camera;
            WriteObjects(serialized.FindProperty("explicitBindings"), explicitBindings);
            var cues = serialized.FindProperty("soundCues");
            cues.arraySize = audioSources.Length;
            for (var i = 0; i < audioSources.Length; i++)
            {
                var source = audioSources[i];
                var element = cues.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("source").objectReferenceValue = source;
                element.FindPropertyRelative("channel").enumValueIndex = (int)ClassifyOnce(source);
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            anchors.Validate(boss1 != null);
            EditorUtility.SetDirty(anchors);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static bool IsExplicitBinding(MonoBehaviour value)
        {
            return value is IBattleSessionConsumer
                || value is IGameInputConsumer
                || value is IGameTimeConsumer
                || value is ITransientEntityConsumer
                || value is IColorTimingSceneFlowConsumer
                || value is IColorTimingSettingsConsumer
                || value is IColorTimingSoundConsumer
                || value is IColorTimingUiConsumer
                || value is IGameplayPointerConsumer
                || value is IGameplayCameraConsumer
                || value is IPlayerTargetConsumer
                || value is IPlayerDamageSignalConsumer;
        }

        static ColorTimingSoundChannel ClassifyOnce(AudioSource source)
        {
            if (source.clip.name.StartsWith("amb_", StringComparison.OrdinalIgnoreCase))
                return ColorTimingSoundChannel.Environment;
            return source.gameObject.name.IndexOf("BGM", StringComparison.OrdinalIgnoreCase) >= 0
                ? ColorTimingSoundChannel.BGM
                : ColorTimingSoundChannel.Environment;
        }

        static T Single<T>(IEnumerable<MonoBehaviour> behaviours, string path) where T : MonoBehaviour
        {
            var values = behaviours.OfType<T>().ToArray();
            if (values.Length != 1) throw new InvalidOperationException($"{path} expected one {typeof(T).Name}, found {values.Length}.");
            return values[0];
        }

        static void WriteObjects(SerializedProperty property, UnityEngine.Object[] values)
        {
            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        static string HierarchyPath(Transform transform)
        {
            var names = new Stack<string>();
            for (var current = transform; current != null; current = current.parent) names.Push(current.name);
            return string.Join("/", names);
        }
    }
}
