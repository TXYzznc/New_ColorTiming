// 文件职责：生成 Hero 武器动画资源映射并写入 Boss 场景中的玩家组件。
// 所属模块：ColorTiming / Editor / Migration。

using System;
using System.IO;
using ColorTiming.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorTiming.Editor.Migration
{
    internal static class HeroWeaponAnimationCatalogMigration
    {
        private const string CandidateDirectory = "Assets/Game/Sprites/ColorTiming/Hero/Animations/RuntimeCandidates";
        private const string CatalogDirectory = "Assets/Game/ScriptableAssets/ColorTiming/Player";
        private const string CatalogPath = CatalogDirectory + "/HeroWeaponAnimationCatalog.asset";
        private static readonly string[] ScenePaths = { "Assets/Game/Scene/Boss1.unity", "Assets/Game/Scene/Boss2.unity" };

        [MenuItem("Game Framework/GameTools/ColorTiming/Animation Migration/Migrate Hero Weapon Animation Catalog", false, 1016)]
        private static void Migrate()
        {
            EnsureDirectory(CatalogDirectory);
            HeroWeaponAnimationCatalogAsset catalog = AssetDatabase.LoadAssetAtPath<HeroWeaponAnimationCatalogAsset>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<HeroWeaponAnimationCatalogAsset>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            Populate(catalog);
            string originalScene = SceneManager.GetActiveScene().path;
            try
            {
                foreach (string scenePath in ScenePaths) AssignCatalog(scenePath, catalog);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                if (!string.IsNullOrEmpty(originalScene) && AssetDatabase.LoadAssetAtPath<SceneAsset>(originalScene) != null)
                    EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
            }
        }

        private static void Populate(HeroWeaponAnimationCatalogAsset catalog)
        {
            var serialized = new SerializedObject(catalog);
            serialized.FindProperty("baseControllerAssetName").stringValue = CandidateDirectory + "/Hero_Base.controller";
            SerializedProperty entries = serialized.FindProperty("entries");
            int supportedCount = 0;
            for (int color = 0; color < 4; color++)
            {
                for (int type = 1; type <= 6; type++)
                {
                    WeaponIdentity weapon = new WeaponIdentity((WeaponColor)color, (WeaponType)type);
                    if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(GetCandidatePath(weapon)) != null)
                        supportedCount++;
                }
            }
            entries.arraySize = supportedCount;
            int index = 0;
            for (int color = 0; color < 4; color++)
            {
                for (int type = 1; type <= 6; type++)
                {
                    WeaponIdentity weapon = new WeaponIdentity((WeaponColor)color, (WeaponType)type);
                    string candidatePath = GetCandidatePath(weapon);
                    if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(candidatePath) == null) continue;
                    SerializedProperty entry = entries.GetArrayElementAtIndex(index++);
                    entry.FindPropertyRelative("color").enumValueIndex = color;
                    entry.FindPropertyRelative("type").enumValueIndex = type;
                    entry.FindPropertyRelative("controllerAssetName").stringValue = candidatePath;
                }
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
        }

        private static string GetCandidatePath(WeaponIdentity weapon)
        {
            return CandidateDirectory + $"/Hero_{weapon.Color}_{weapon.Type}.controller";
        }

        private static void AssignCatalog(string scenePath, HeroWeaponAnimationCatalogAsset catalog)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            PlayerActorView[] players = UnityEngine.Object.FindObjectsOfType<PlayerActorView>(true);
            if (players.Length != 1) throw new InvalidOperationException($"{scenePath} expected one PlayerActorView, found {players.Length}.");
            RuntimeAnimatorController baseController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                CandidateDirectory + "/Hero_Base.controller");
            if (baseController == null) throw new InvalidOperationException("Hero_Base.controller is missing.");
            var serialized = new SerializedObject(players[0]);
            serialized.FindProperty("weaponAnimationCatalog").objectReferenceValue = catalog;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            if (players[0].animator == null) throw new InvalidOperationException($"{scenePath} PlayerActorView has no Animator.");
            players[0].animator.runtimeAnimatorController = baseController;
            EditorUtility.SetDirty(players[0].animator);
            EditorUtility.SetDirty(players[0]);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureDirectory(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(parent)) throw new InvalidOperationException($"Invalid asset directory {path}.");
            EnsureDirectory(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
