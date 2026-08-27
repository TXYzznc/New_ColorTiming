using System;
using System.Linq;
using ColorTiming.Presentation.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ColorTiming.Editor
{
    /// <summary>Migrates authored battle UI into dynamically-owned GF.UI presentation forms.</summary>
    public static class ColorTimingBattleUiLifecycleMigration
    {
        private const string LaunchScenePath = "Assets/Game/Scene/Launch.unity";
        private const string BattleTutorialPrefabPath = "Assets/Game/Prefabs/UI/ColorTiming/Game/BattleTutorial.prefab";
        private static readonly string[] BattleScenePaths =
        {
            "Assets/Game/Scene/Boss1.unity",
            "Assets/Game/Scene/Boss2.unity",
        };

        [MenuItem("Game Framework/GameTools/Migrate ColorTiming Battle UI Lifecycle", false, 1008)]
        public static void Migrate()
        {
            try
            {
                AssetDatabase.Refresh();
                CreateBattleTutorialPrefab();
                NormalizeBattleTutorialRoot();
                CreateWorldUiRoot();
                foreach (var scenePath in BattleScenePaths)
                {
                    RemoveLegacyBattleUi(scenePath);
                }

                UGF.EditorTools.GameDataGenerator.GenerateDataTables();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Validate();
                Debug.Log("[ColorTiming] Battle UI lifecycle migration completed.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                throw;
            }
        }

        [MenuItem("Game Framework/GameTools/Validate ColorTiming Battle UI Lifecycle", false, 1009)]
        public static void Validate()
        {
            ValidateBattleTutorialPrefab();
            ValidateWorldUiRoot();
            foreach (var scenePath in BattleScenePaths)
            {
                ValidateBattleScene(scenePath);
            }

            Debug.Log("[ColorTiming] Battle UI lifecycle structure validated.");
        }

        private static void CreateBattleTutorialPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(BattleTutorialPrefabPath) != null)
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(BattleScenePaths[0], OpenSceneMode.Single);
            var source = FindComponents<UI_WeaponTip>(scene).SingleOrDefault();
            Assert(source != null, "Boss1 must provide one legacy UI_WeaponTip source for migration.");
            Assert(source.showTip != null && source.weaponTipImage != null && source.weaponTips != null,
                "Legacy UI_WeaponTip references are incomplete.");

            var root = new GameObject("BattleTutorial", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster), typeof(BattleTutorialForm));
            try
            {
                root.layer = LayerMask.NameToLayer("UI");
                var rootRect = (RectTransform)root.transform;
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.anchoredPosition = Vector2.zero;
                rootRect.sizeDelta = Vector2.zero;
                rootRect.localScale = Vector3.one;
                root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                root.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

                var visual = UnityEngine.Object.Instantiate(source.gameObject, root.transform, false);
                visual.name = "Panel_Tutorial";
                var clonedSource = visual.GetComponent<UI_WeaponTip>();
                Assert(clonedSource != null, "Weapon-tip visual clone lost its source component.");
                var tipContent = clonedSource.showTip;
                var weaponTipImage = clonedSource.weaponTipImage;
                tipContent.name = "Panel_TipContent";
                weaponTipImage.gameObject.name = "Img_WeaponTip";
                tipContent.SetActive(false);
                UnityEngine.Object.DestroyImmediate(clonedSource);

                var form = root.GetComponent<BattleTutorialForm>();
                var serializedForm = new SerializedObject(form);
                serializedForm.FindProperty("tipContent").objectReferenceValue = tipContent;
                serializedForm.FindProperty("weaponTipImage").objectReferenceValue = weaponTipImage;
                var sprites = serializedForm.FindProperty("weaponTips");
                sprites.arraySize = source.weaponTips.Length;
                for (var index = 0; index < source.weaponTips.Length; index++)
                {
                    sprites.GetArrayElementAtIndex(index).objectReferenceValue = source.weaponTips[index];
                }

                serializedForm.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, BattleTutorialPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void CreateWorldUiRoot()
        {
            var scene = EditorSceneManager.OpenScene(LaunchScenePath, OpenSceneMode.Single);
            var roots = scene.GetRootGameObjects().Where(root => root.name == "WorldUIRoot").ToArray();
            Assert(roots.Length <= 1, "Launch must not contain more than one WorldUIRoot.");
            if (roots.Length == 0)
            {
                var root = new GameObject("WorldUIRoot", typeof(RectTransform));
                root.layer = LayerMask.NameToLayer("UI");
                EditorSceneManager.MoveGameObjectToScene(root, scene);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        private static void NormalizeBattleTutorialRoot()
        {
            var root = PrefabUtility.LoadPrefabContents(BattleTutorialPrefabPath);
            try
            {
                var rect = (RectTransform)root.transform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.localScale = Vector3.one;
                PrefabUtility.SaveAsPrefabAsset(root, BattleTutorialPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void RemoveLegacyBattleUi(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var legacyUiRoots = scene.GetRootGameObjects()
                .Where(root => root.GetComponent<Canvas>() != null || root.GetComponentInChildren<UI_Game>(true) != null)
                .ToArray();
            Assert(legacyUiRoots.Length <= 1, $"{scenePath} must not contain more than one legacy battle UI root.");
            if (legacyUiRoots.Length == 1)
            {
                UnityEngine.Object.DestroyImmediate(legacyUiRoots[0]);
            }

            foreach (var eventSystem in FindComponents<EventSystem>(scene))
            {
                UnityEngine.Object.DestroyImmediate(eventSystem.gameObject);
            }

            Assert(FindComponents<ColorTimingBattleHudBootstrap>(scene).Length == 0,
                $"{scenePath} still contains an authored BattleHudContext outside the legacy UI root.");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ValidateBattleTutorialPrefab()
        {
            var root = PrefabUtility.LoadPrefabContents(BattleTutorialPrefabPath);
            try
            {
                Assert(root.GetComponent<Canvas>() != null && root.GetComponent<CanvasScaler>() != null
                    && root.GetComponent<GraphicRaycaster>() != null, "BattleTutorial requires GF.UI root components.");
                var rootRect = (RectTransform)root.transform;
                Assert(rootRect.anchorMin == Vector2.zero && rootRect.anchorMax == Vector2.one
                    && rootRect.sizeDelta == Vector2.zero && rootRect.localScale == Vector3.one,
                    "BattleTutorial root RectTransform must be normalized.");
                var form = root.GetComponent<BattleTutorialForm>();
                Assert(form != null, "BattleTutorial requires BattleTutorialForm.");
                var serializedForm = new SerializedObject(form);
                Assert(serializedForm.FindProperty("tipContent").objectReferenceValue != null
                    && serializedForm.FindProperty("weaponTipImage").objectReferenceValue != null
                    && serializedForm.FindProperty("weaponTips").arraySize > 0,
                    "BattleTutorial runtime bindings are incomplete.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ValidateWorldUiRoot()
        {
            var scene = EditorSceneManager.OpenScene(LaunchScenePath, OpenSceneMode.Single);
            var roots = scene.GetRootGameObjects().Where(root => root.name == "WorldUIRoot").ToArray();
            Assert(roots.Length == 1 && roots[0].GetComponents<Component>().Length == 1 && roots[0].transform.childCount == 0,
                "Launch requires one empty WorldUIRoot Transform root.");
        }

        private static void ValidateBattleScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Assert(FindComponents<Canvas>(scene).Length == 0 && FindComponents<EventSystem>(scene).Length == 0,
                $"{scenePath} must not retain authored product UI roots or EventSystem.");
            Assert(FindComponents<UI_Game>(scene).Length == 0
                && FindComponents<UI_WeaponTip>(scene).Length == 0
                && FindComponents<UI_SoundManager>(scene).Length == 0
                && FindComponents<ColorTimingBattleHudBootstrap>(scene).Length == 0,
                $"{scenePath} retains a legacy battle UI behaviour.");
        }

        private static T[] FindComponents<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
