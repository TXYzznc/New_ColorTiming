// 文件职责：创建武器生成规则资产并将现有场景迁移到配置驱动的生成器。
// 所属模块：ColorTiming / Editor / Migration。

using System;
using System.Collections.Generic;
using System.IO;
using ColorTiming.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorTiming.Editor.Migration
{
    internal static class ColorTimingWeaponSpawnRuleMigration
    {
        private const string RuleDirectory = "Assets/Game/ScriptableAssets/ColorTiming/Combat/WeaponSpawnRules";
        private const string Boss1RulePath = RuleDirectory + "/Boss1WeaponSpawnRule.asset";
        private const string Boss2RulePath = RuleDirectory + "/Boss2WeaponSpawnRule.asset";

        [MenuItem("Game Framework/GameTools/ColorTiming/Migrate Weapon Spawn Rules", false, 1014)]
        private static void Migrate()
        {
            EnsureDirectory(RuleDirectory);
            WeaponSpawnRuleAsset boss1Rule = CreateOrLoadRule(Boss1RulePath, 5f, 5, 3, BuildBoss1Entries());
            WeaponSpawnRuleAsset boss2Rule = CreateOrLoadRule(Boss2RulePath, 5f, 10, 3, BuildBoss2Entries());

            string originalScene = SceneManager.GetActiveScene().path;
            try
            {
                AssignRule("Assets/Game/Scene/Boss1.unity", typeof(Boss1WeaponSpawnerView), boss1Rule);
                AssignRule("Assets/Game/Scene/Boss2.unity", typeof(Boss2WeaponSpawnerView), boss2Rule);
                AssetDatabase.SaveAssets();
                Debug.Log("ColorTiming weapon spawn rules migrated to configuration assets.");
            }
            finally
            {
                if (!string.IsNullOrEmpty(originalScene) && AssetDatabase.LoadAssetAtPath<SceneAsset>(originalScene) != null)
                {
                    EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
                }
            }
        }

        private static void AssignRule(string scenePath, Type spawnerType, WeaponSpawnRuleAsset rule)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var spawners = new List<WeaponSpawnerView>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (WeaponSpawnerView spawner in root.GetComponentsInChildren<WeaponSpawnerView>(true))
                {
                    if (spawner.GetType() == spawnerType)
                    {
                        spawners.Add(spawner);
                    }
                }
            }

            if (spawners.Count != 1)
            {
                throw new InvalidOperationException($"{scenePath} expected one {spawnerType.Name}, found {spawners.Count}.");
            }

            var serialized = new SerializedObject(spawners[0]);
            serialized.FindProperty("spawnRule").objectReferenceValue = rule;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(spawners[0]);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static WeaponSpawnRuleAsset CreateOrLoadRule(
            string assetPath,
            float spawnInterval,
            int activeLimit,
            int guaranteeThreshold,
            IReadOnlyList<WeaponIdentity> entries)
        {
            WeaponSpawnRuleAsset rule = AssetDatabase.LoadAssetAtPath<WeaponSpawnRuleAsset>(assetPath);
            if (rule == null)
            {
                rule = ScriptableObject.CreateInstance<WeaponSpawnRuleAsset>();
                AssetDatabase.CreateAsset(rule, assetPath);
            }

            var serialized = new SerializedObject(rule);
            serialized.FindProperty("spawnInterval").floatValue = spawnInterval;
            serialized.FindProperty("activeLimit").intValue = activeLimit;
            serialized.FindProperty("guaranteeThreshold").intValue = guaranteeThreshold;
            SerializedProperty elements = serialized.FindProperty("entries");
            elements.arraySize = entries.Count;
            for (int index = 0; index < entries.Count; index++)
            {
                SerializedProperty element = elements.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("color").enumValueIndex = (int)entries[index].Color;
                element.FindPropertyRelative("type").enumValueIndex = (int)entries[index].Type;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(rule);
            return rule;
        }

        private static WeaponIdentity[] BuildBoss1Entries()
        {
            return BuildEntries(
                new[] { WeaponColor.Red, WeaponColor.Green, WeaponColor.Purple },
                new[] { WeaponType.Scissors, WeaponType.Hammer, WeaponType.Bomb });
        }

        private static WeaponIdentity[] BuildBoss2Entries()
        {
            return BuildEntries(
                new[] { WeaponColor.Red, WeaponColor.Green, WeaponColor.Purple, WeaponColor.Orange },
                new[] { WeaponType.Knife, WeaponType.Axe, WeaponType.Airplane });
        }

        private static WeaponIdentity[] BuildEntries(IReadOnlyList<WeaponColor> colors, IReadOnlyList<WeaponType> types)
        {
            var entries = new List<WeaponIdentity>(colors.Count * types.Count);
            foreach (WeaponColor color in colors)
            {
                foreach (WeaponType type in types)
                {
                    entries.Add(new WeaponIdentity(color, type));
                }
            }

            return entries.ToArray();
        }

        private static void EnsureDirectory(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(parent))
            {
                throw new InvalidOperationException($"Invalid asset directory: {assetPath}");
            }

            EnsureDirectory(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(assetPath));
        }
    }
}
