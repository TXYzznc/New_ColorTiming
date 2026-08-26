using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;

namespace ColorTiming.Editor
{
    public static class ColorTimingBattleHudPrefabMigration
    {
        const string Boss1Scene = "Assets/Game/Scene/Boss1.unity";
        const string Boss2Scene = "Assets/Game/Scene/Boss2.unity";
        const string HeroItem = "Assets/Game/Prefabs/UI/ColorTiming/HeroHP_Item.prefab";
        const string HeroBox = "Assets/Game/Prefabs/UI/ColorTiming/P_HPBox.prefab";
        const string BossItem = "Assets/Game/Prefabs/UI/ColorTiming/BossHP_Item.prefab";
        const string PrefabFolder = "Assets/Game/Prefabs/UI/ColorTiming/Game";
        const string BattleHud = "BattleHud.prefab";

        [MenuItem("Game Framework/GameTools/Migrate ColorTiming Battle HUD", false, 1004)]
        public static void Migrate()
        {
            try
            {
                AssetDatabase.Refresh();
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Game/Prefabs/UI/ColorTiming/Game"));
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Game/Prefabs/UI/ColorTiming"));
                CreateSharedBattleHud();
                MigrateScene(Boss1Scene);
                MigrateScene(Boss2Scene);
                NormalizeHeroBoxPrefabs();
                AssetDatabase.DeleteAsset($"{PrefabFolder}/BattleHud_Boss1.prefab");
                AssetDatabase.DeleteAsset($"{PrefabFolder}/BattleHud_Boss2.prefab");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[ColorTiming HUD] migration completed with shared BattleHud.prefab.");
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                throw;
            }
        }

        static void NormalizeHeroBoxPrefabs()
        {
            var heroBoxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HeroBox);
            foreach (var hudName in new[] { BattleHud })
            {
                var hudPath = $"{PrefabFolder}/{hudName}";
                var hudRoot = PrefabUtility.LoadPrefabContents(hudPath);
                try
                {
                    var box = FindDirectChild(hudRoot.transform, "P_HPBox");
                    if (box == null) throw new InvalidOperationException($"P_HPBox not found in {hudPath}.");
                    ConfigureHeroBox(box);
                    if (heroBoxPrefab == null)
                    {
                        var detached = UnityEngine.Object.Instantiate(box.gameObject);
                        detached.transform.SetParent(null, false);
                        heroBoxPrefab = PrefabUtility.SaveAsPrefabAsset(detached, HeroBox);
                        UnityEngine.Object.DestroyImmediate(detached);
                        if (heroBoxPrefab == null) throw new InvalidOperationException($"Could not create {HeroBox}.");
                    }

                    var parent = box.parent;
                    var siblingIndex = box.GetSiblingIndex();
                    var rect = box as RectTransform;
                    var anchoredPosition = rect == null ? Vector2.zero : rect.anchoredPosition;
                    var sizeDelta = rect == null ? Vector2.zero : rect.sizeDelta;
                    var anchorMin = rect == null ? Vector2.zero : rect.anchorMin;
                    var anchorMax = rect == null ? Vector2.one : rect.anchorMax;
                    var pivot = rect == null ? new Vector2(0.5f, 0.5f) : rect.pivot;
                    UnityEngine.Object.DestroyImmediate(box.gameObject);
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(heroBoxPrefab, parent);
                    instance.name = "P_HPBox";
                    instance.transform.SetSiblingIndex(siblingIndex);
                    var instanceRect = instance.transform as RectTransform;
                    if (instanceRect != null)
                    {
                        instanceRect.anchorMin = anchorMin;
                        instanceRect.anchorMax = anchorMax;
                        instanceRect.anchoredPosition = anchoredPosition;
                        instanceRect.sizeDelta = sizeDelta;
                        instanceRect.pivot = pivot;
                    }
                    EditorUtility.SetDirty(instance);
                    PrefabUtility.SaveAsPrefabAsset(hudRoot, hudPath);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(hudRoot);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void CreateSharedBattleHud()
        {
            var sharedPath = $"{PrefabFolder}/{BattleHud}";
            var sourcePath = $"{PrefabFolder}/BattleHud_Boss1.prefab";
            var sharedHud = AssetDatabase.LoadAssetAtPath<GameObject>(sharedPath);
            if (sharedHud == null)
            {
                var sourceHud = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
                if (sourceHud == null) throw new InvalidOperationException($"Battle HUD source not found: {sourcePath}.");
                sharedHud = PrefabUtility.SaveAsPrefabAsset(sourceHud, sharedPath);
                if (sharedHud == null) throw new InvalidOperationException($"Could not create {sharedPath}.");
            }

            var root = PrefabUtility.LoadPrefabContents(sharedPath);
            try
            {
                root.name = "BattleHud";
                var bossBox = FindDirectChild(root.transform, "HPBox");
                ConfigureBossBox(bossBox, false);
                var boss2 = FindComponent(bossBox, "UI_BossHPController2");
                if (boss2 == null) boss2 = AddComponentByName(bossBox.gameObject, "UI_BossHPController2");
                SetComponentReferences(bossBox, "UI_BossHPController2", "boss1_Controller", "HPItem", BossItem);
                ((Behaviour)boss2).enabled = false;
                PrefabUtility.SaveAsPrefabAsset(root, sharedPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static void MigrateScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var uiRoot = FindByName(scene, "UI_BasePanel (1)") ?? FindByName(scene, "UI_BasePanel");
            if (uiRoot == null) throw new InvalidOperationException($"UI base panel not found in {scenePath}.");

            var prefabPath = $"{PrefabFolder}/{BattleHud}";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var playerInfo = FindDirectChild(uiRoot.transform, "PlayerInfo");
            var bossInfo = FindDirectChild(uiRoot.transform, "BossInfo");
            if (prefab == null) throw new InvalidOperationException($"Shared battle HUD not found: {prefabPath}.");

            if (playerInfo != null) UnityEngine.Object.DestroyImmediate(playerInfo.gameObject);
            if (bossInfo != null) UnityEngine.Object.DestroyImmediate(bossInfo.gameObject);

            var existingBootstrap = FindDirectChild(uiRoot.transform, "BattleHudBootstrap");
            if (existingBootstrap != null) UnityEngine.Object.DestroyImmediate(existingBootstrap.gameObject);
            var bootstrapObject = new GameObject("BattleHudBootstrap");
            bootstrapObject.transform.SetParent(uiRoot.transform, false);
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Game/Scripts/ColorTiming/Legacy/UI/ColorTimingBattleHudBootstrap.cs");
            if (script == null) throw new InvalidOperationException("ColorTimingBattleHudBootstrap script is missing.");
            var bootstrapType = ResolveType(script);
            if (bootstrapType == null) throw new InvalidOperationException("Could not resolve ColorTimingBattleHudBootstrap runtime type.");
            var bootstrap = (MonoBehaviour)bootstrapObject.AddComponent(bootstrapType);
            var bootstrapSerialized = new SerializedObject(bootstrap);
            bootstrapSerialized.FindProperty("hudPrefab").objectReferenceValue = prefab;
            bootstrapSerialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bootstrapObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[ColorTiming HUD] migrated {scenePath} -> shared {prefabPath}.");
        }

        static System.Type ResolveType(MonoScript script)
        {
            var type = script.GetClass();
            if (type != null) return type;

            var scriptName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(script));
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(scriptName, false);
                if (type != null && typeof(MonoBehaviour).IsAssignableFrom(type)) return type;
            }
            return null;
        }

        static void ConfigureHeroBox(Transform box)
        {
            if (box == null) throw new InvalidOperationException("P_HPBox not found in copied HUD.");
            ClearChildren(box);
            SetComponentReferences(box, "UI_HeroHPBox", "controller", "hpItem", HeroItem);
        }

        static void ConfigureBossBox(Transform box, bool boss2)
        {
            if (box == null) throw new InvalidOperationException("HPBox not found in copied HUD.");
            ClearChildren(box);
            if (boss2)
            {
                SetComponentReferences(box, "UI_BossHPController2", "boss1_Controller", "HPItem", BossItem);
            }
            else
            {
                SetComponentReferences(box, "UI_BossHPController", "boss1_Controller", "HPItem", BossItem);
            }
        }

        static void SetComponentReferences(Transform box, string typeName, string sceneReference, string prefabReference, string prefabPath)
        {
            Component target = null;
            foreach (var component in box.GetComponents<Component>())
            {
                if (component != null && component.GetType().Name == typeName)
                {
                    target = component;
                    break;
                }
            }

            if (target == null) throw new InvalidOperationException($"{box.name} requires {typeName}.");
            var serialized = new SerializedObject(target);
            serialized.FindProperty(sceneReference).objectReferenceValue = null;
            serialized.FindProperty(prefabReference).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        static Component FindComponent(Transform target, string typeName)
        {
            foreach (var component in target.GetComponents<Component>())
            {
                if (component != null && component.GetType().Name == typeName) return component;
            }
            return null;
        }

        static Component AddComponentByName(GameObject target, string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(typeName, false);
                if (type == null || !typeof(MonoBehaviour).IsAssignableFrom(type)) continue;
                return target.AddComponent(type);
            }
            throw new InvalidOperationException($"Could not resolve component type {typeName}.");
        }

        static void ClearChildren(Transform parent)
        {
            while (parent.childCount > 0)
                UnityEngine.Object.DestroyImmediate(parent.GetChild(0).gameObject);
        }

        static GameObject FindByName(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == name) return root;
                var result = FindDirectChild(root.transform, name);
                if (result != null) return result.gameObject;
            }
            return null;
        }

        static Transform FindDirectChild(Transform parent, string name)
        {
            if (parent == null) return null;
            for (var i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).name == name) return parent.GetChild(i);
                var result = FindDirectChild(parent.GetChild(i), name);
                if (result != null) return result;
            }
            return null;
        }
    }
}
