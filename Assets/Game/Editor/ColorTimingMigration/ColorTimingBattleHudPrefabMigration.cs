using System;
using System.Linq;
using ColorTiming.Presentation.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ColorTiming.Editor
{
    /// <summary>Applies the serialized GF.UI conversion for ColorTiming's battle HUD.</summary>
    public static class ColorTimingBattleHudPrefabMigration
    {
        private const string BattleHudPrefabPath = "Assets/Game/Prefabs/UI/ColorTiming/Game/BattleHud.prefab";
        private static readonly string[] BattleScenePaths =
        {
            "Assets/Game/Scene/Boss1.unity",
            "Assets/Game/Scene/Boss2.unity",
        };

        [MenuItem("Game Framework/GameTools/Migrate ColorTiming Battle HUD To GF.UI", false, 1004)]
        public static void Migrate()
        {
            try
            {
                AssetDatabase.Refresh();
                MigrateBattleHudPrefab();
                foreach (var scenePath in BattleScenePaths)
                {
                    MigrateBattleScene(scenePath);
                }

                UGF.EditorTools.GameDataGenerator.GenerateDataTables();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Validate();
                Debug.Log("[ColorTiming HUD] GF.UI migration completed.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                throw;
            }
        }

        [MenuItem("Game Framework/GameTools/Validate ColorTiming Battle HUD GF.UI", false, 1005)]
        public static void Validate()
        {
            ValidateBattleHudPrefab();
            foreach (var scenePath in BattleScenePaths)
            {
                ValidateBattleScene(scenePath);
            }

            Debug.Log("[ColorTiming HUD] GF.UI structure validated.");
        }

        private static void MigrateBattleHudPrefab()
        {
            var root = PrefabUtility.LoadPrefabContents(BattleHudPrefabPath);
            try
            {
                root.name = "BattleHud";
                root.layer = LayerMask.NameToLayer("UI");
                var canvas = root.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = root.AddComponent<Canvas>();
                }
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.worldCamera = null;
                var scaler = root.GetComponent<CanvasScaler>();
                if (scaler == null)
                {
                    scaler = root.AddComponent<CanvasScaler>();
                }
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                if (root.GetComponent<GraphicRaycaster>() == null)
                {
                    root.AddComponent<GraphicRaycaster>();
                }

                Rename(root.transform, "PlayerInfo", "PlayerInfoPanel");
                Rename(root.transform, "BossInfo", "BossInfoPanel");
                Rename(root.transform, "juese", "CharacterPortrait");
                Rename(root.transform, "tishi", "WeaponHint");
                Rename(root.transform, "tishix", "ChargeWeaponHint");
                Rename(root.transform, "P_HPBox", "HealthPips");
                Rename(root.transform, "HPBox", "HealthPips");
                Rename(root.transform, "Image", "BossNameBanner");

                var form = root.GetComponent<BattleHudForm>() ?? root.AddComponent<BattleHudForm>();
                var rootRect = (RectTransform)root.transform;
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.anchoredPosition = Vector2.zero;
                rootRect.sizeDelta = Vector2.zero;
                rootRect.pivot = new Vector2(0.5f, 0.5f);
                rootRect.localScale = Vector3.one;
                var heroInfo = root.GetComponentInChildren<UI_HeroInfo>(true);
                var heroHealth = root.GetComponentInChildren<UI_HeroHPBox>(true);
                var boss1Health = root.GetComponentInChildren<UI_BossHPController>(true);
                var boss2Health = root.GetComponentInChildren<UI_BossHPController2>(true);
                Assert(heroInfo != null && heroHealth != null && boss1Health != null && boss2Health != null,
                    "Battle HUD must contain each serialized presentation component.");
                var serializedForm = new SerializedObject(form);
                serializedForm.FindProperty("heroInfo").objectReferenceValue = heroInfo;
                serializedForm.FindProperty("heroHealth").objectReferenceValue = heroHealth;
                serializedForm.FindProperty("boss1Health").objectReferenceValue = boss1Health;
                serializedForm.FindProperty("boss2Health").objectReferenceValue = boss2Health;
                serializedForm.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(root);
                PrefabUtility.SaveAsPrefabAsset(root, BattleHudPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void MigrateBattleScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var context = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<ColorTimingBattleHudBootstrap>(true))
                .SingleOrDefault();
            Assert(context != null, $"{scenePath} requires exactly one BattleHudContext.");
            context.gameObject.name = "BattleHudContext";

            var serializedContext = new SerializedObject(context);
            var hero = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<HeroController>(true)).SingleOrDefault();
            var boss1 = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Boss1_Controller>(true)).SingleOrDefault();
            var boss2 = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Boss2_Controller>(true)).SingleOrDefault();
            Assert(hero != null && ((boss1 == null) != (boss2 == null)),
                $"{scenePath} must have one hero and exactly one supported boss.");
            serializedContext.FindProperty("hero").objectReferenceValue = hero;
            serializedContext.FindProperty("boss1").objectReferenceValue = boss1;
            serializedContext.FindProperty("boss2").objectReferenceValue = boss2;
            serializedContext.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(context);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ValidateBattleHudPrefab()
        {
            var root = PrefabUtility.LoadPrefabContents(BattleHudPrefabPath);
            try
            {
                Assert(root.GetComponent<Canvas>() != null, "BattleHud root requires a Canvas.");
                Assert(root.GetComponent<CanvasScaler>() != null, "BattleHud root requires a CanvasScaler.");
                Assert(root.GetComponent<GraphicRaycaster>() != null, "BattleHud root requires a GraphicRaycaster.");
                var rootRect = (RectTransform)root.transform;
                Assert(rootRect.anchorMin == Vector2.zero && rootRect.anchorMax == Vector2.one
                    && rootRect.sizeDelta == Vector2.zero && rootRect.localScale == Vector3.one,
                    "BattleHud root RectTransform is not normalized for GF.UI.");
                var form = root.GetComponent<BattleHudForm>();
                Assert(form != null, "BattleHud root requires BattleHudForm.");
                Assert(Find(root.transform, "PlayerInfoPanel") != null && Find(root.transform, "BossInfoPanel") != null,
                    "BattleHud semantic panel names are incomplete.");
                Assert(Find(root.transform, "CharacterPortrait") != null && Find(root.transform, "WeaponHint") != null
                    && Find(root.transform, "ChargeWeaponHint") != null && Find(root.transform, "HealthPips") != null,
                    "BattleHud semantic content names are incomplete.");
                var serializedForm = new SerializedObject(form);
                Assert(serializedForm.FindProperty("heroInfo").objectReferenceValue != null
                    && serializedForm.FindProperty("heroHealth").objectReferenceValue != null
                    && serializedForm.FindProperty("boss1Health").objectReferenceValue != null
                    && serializedForm.FindProperty("boss2Health").objectReferenceValue != null,
                    "BattleHudForm references are incomplete.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ValidateBattleScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var contexts = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<ColorTimingBattleHudBootstrap>(true))
                .ToArray();
            Assert(contexts.Length == 1 && contexts[0].gameObject.name == "BattleHudContext",
                $"{scenePath} requires one explicitly named BattleHudContext.");
            var serializedContext = new SerializedObject(contexts[0]);
            Assert(serializedContext.FindProperty("hero").objectReferenceValue != null,
                $"{scenePath} BattleHudContext hero reference is missing.");
            var hasBoss1 = serializedContext.FindProperty("boss1").objectReferenceValue != null;
            var hasBoss2 = serializedContext.FindProperty("boss2").objectReferenceValue != null;
            Assert(hasBoss1 != hasBoss2, $"{scenePath} BattleHudContext requires exactly one boss reference.");
        }

        private static void Rename(Transform root, string oldName, string newName)
        {
            var target = Find(root, oldName);
            if (target != null)
            {
                target.name = newName;
            }
        }

        private static Transform Find(Transform root, string name)
        {
            if (root.name == name) return root;
            for (var index = 0; index < root.childCount; index++)
            {
                var result = Find(root.GetChild(index), name);
                if (result != null) return result;
            }

            return null;
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
