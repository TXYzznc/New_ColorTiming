// 文件职责：以幂等方式把 Boss 共享 HUD、武器生成器和场景绑定迁移到通用角色。
// 所属模块：ColorTiming / Editor / Migration。

using System;
using System.Collections.Generic;
using System.Linq;
using ColorTiming.Application.Battle;
using ColorTiming.Bootstrap;
using ColorTiming.Presentation.UI.Components;
using ColorTiming.Presentation.UI.Forms;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ColorTiming.Editor.Migration
{
    public static class ColorTimingSharedBattleRolesMigration
    {
        const string BattleHudPath = "Assets/Game/Prefabs/UI/ColorTiming/Game/BattleHud.prefab";
        const string BossHpItemPath = "Assets/Game/Prefabs/UI/ColorTiming/BossHP_Item.prefab";
        static readonly string[] BattleScenePaths =
        {
            "Assets/Game/Scene/Boss1.unity",
            "Assets/Game/Scene/Boss2.unity",
        };

        [MenuItem("Tools/ColorTiming/Migration/Consolidate Shared Battle Roles")]
        public static void MigrateAll()
        {
            EnsureLoadedScenesAreSaved();
            MigrateBattleHud();

            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                for (var i = 0; i < BattleScenePaths.Length; i++)
                {
                    MigrateBattleScene(BattleScenePaths[i], i + 1);
                }
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(setup);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ColorTiming.Migration] action=ConsolidateSharedBattleRoles result=success");
        }

        static void EnsureLoadedScenesAreSaved()
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty)
                {
                    throw new InvalidOperationException(
                        $"Scene '{scene.path}' has unsaved changes. Save it before running the shared-role migration.");
                }
            }
        }

        static void MigrateBattleHud()
        {
            var root = PrefabUtility.LoadPrefabContents(BattleHudPath);
            try
            {
                var slot = Find(root.transform, "Slot_BossHP") ?? Find(root.transform, "Slot_Boss1HP");
                if (slot == null)
                {
                    throw new InvalidOperationException("BattleHud has no reusable Boss HP slot.");
                }

                var health = slot.GetComponent<BossHealthView>() ?? slot.gameObject.AddComponent<BossHealthView>();
                var hpItem = AssetDatabase.LoadAssetAtPath<GameObject>(BossHpItemPath);
                if (hpItem == null)
                {
                    throw new InvalidOperationException("Boss HP item prefab is missing.");
                }
                SetObjectReference(health, "hpItem", hpItem);

                slot.name = "Slot_BossHP";
                var duplicate = Find(root.transform, "Slot_Boss2HP");
                if (duplicate != null && duplicate != slot)
                {
                    Object.DestroyImmediate(duplicate.gameObject);
                }

                var form = root.GetComponent<BattleHudForm>();
                if (form == null)
                {
                    throw new InvalidOperationException("BattleHud is missing BattleHudForm.");
                }

                SetObjectReference(form, "bossHealth", health);
                PrefabUtility.SaveAsPrefabAsset(root, BattleHudPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static void MigrateBattleScene(string scenePath, int tutorialTipId)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var behaviours = GetSceneBehaviours(scene);
            var player = behaviours.OfType<PlayerActorView>().SingleOrDefault()
                         ?? throw new InvalidOperationException($"{scenePath} requires one PlayerActorView.");
            var anchor = behaviours.OfType<BattleSceneAnchors>().SingleOrDefault()
                         ?? throw new InvalidOperationException($"{scenePath} requires one BattleSceneAnchors.");
            var boss = behaviours.OfType<IBossBattleSessionConsumer>().Cast<MonoBehaviour>().SingleOrDefault()
                       ?? throw new InvalidOperationException($"{scenePath} requires one boss session consumer.");

            var spawner = behaviours.OfType<WeaponSpawnerView>()
                .SingleOrDefault(component => component.GetType() == typeof(WeaponSpawnerView));

            if (spawner == null)
            {
                throw new InvalidOperationException($"{scenePath} requires one WeaponSpawnerView.");
            }

            SetInteger(spawner, "tutorialTipId", tutorialTipId);
            SetObjectReference(player, "weaponSpawner", spawner);
            EnsureExplicitBinding(anchor, spawner);
            EnsureExplicitBinding(anchor, boss);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException($"Failed to save migrated scene '{scenePath}'.");
            }
        }

        static List<MonoBehaviour> GetSceneBehaviours(Scene scene)
        {
            var result = new List<MonoBehaviour>();
            foreach (var root in scene.GetRootGameObjects())
            {
                result.AddRange(root.GetComponentsInChildren<MonoBehaviour>(true));
            }
            return result;
        }

        static void EnsureExplicitBinding(BattleSceneAnchors anchor, MonoBehaviour binding)
        {
            var serialized = new SerializedObject(anchor);
            var bindings = serialized.FindProperty("explicitBindings")
                           ?? throw new InvalidOperationException("BattleSceneAnchors.explicitBindings is missing.");
            for (var i = 0; i < bindings.arraySize; i++)
            {
                if (bindings.GetArrayElementAtIndex(i).objectReferenceValue == binding)
                {
                    return;
                }
            }

            bindings.InsertArrayElementAtIndex(bindings.arraySize);
            bindings.GetArrayElementAtIndex(bindings.arraySize - 1).objectReferenceValue = binding;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetObjectReference(Object target, string propertyName, Object value)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName)
                           ?? throw new InvalidOperationException($"{target.GetType().Name}.{propertyName} is missing.");
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetInteger(Object target, string propertyName, int value)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName)
                           ?? throw new InvalidOperationException($"{target.GetType().Name}.{propertyName} is missing.");
            property.intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static Transform Find(Transform root, string objectName)
        {
            if (root.name == objectName)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var match = Find(root.GetChild(i), objectName);
                if (match != null)
                {
                    return match;
                }
            }
            return null;
        }
    }
}
