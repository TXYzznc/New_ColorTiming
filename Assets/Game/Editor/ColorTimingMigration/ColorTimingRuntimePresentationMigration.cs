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
    /// <summary>Applies the serialized-asset side of the runtime-presentation OpenSpec change.</summary>
    public static class ColorTimingRuntimePresentationMigration
    {
        private const string StartMenuScenePath = "Assets/Game/Scene/StartMenu.unity";
        private const string LoadingPrefabPath = "Assets/Game/Prefabs/UI/ColorTiming/Game/Loading.prefab";
        private const string MainMenuPrefabPath = "Assets/Game/Prefabs/UI/ColorTiming/Game/MainMenu.prefab";
        private const string LoadingBackgroundGuid = "f8a2a4a8b5afa774b9c90cab2a62c7a3";
        private const string LoadingBarGuid = "0302e3eca1486a041af96c28b4c2bf08";
        private const string LoadingHandleGuid = "581414a5691b14244a714af973cbd51f";

        [MenuItem("Game Framework/GameTools/Migrate ColorTiming Runtime Presentation", false, 1005)]
        public static void Apply()
        {
            try
            {
                var bgmClip = RemoveLegacyStartMenuPresentation();
                NormalizeLoadingNodeNames();
                ValidateLoadingVisualHierarchy();
                AssignStartMenuBgm(bgmClip);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[ColorTiming] Runtime presentation migration completed.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                throw;
            }
        }

        [MenuItem("Game Framework/GameTools/Validate ColorTiming Loading UI Structure", false, 1006)]
        public static void ValidateLoadingUiStructure()
        {
            try
            {
                ValidateLoadingVisualHierarchy();
                Debug.Log("[ColorTiming] Loading UI structure validated.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                throw;
            }
        }

        [MenuItem("Game Framework/GameTools/Normalize ColorTiming Loading UI Names", false, 1007)]
        public static void NormalizeLoadingUiNames()
        {
            try
            {
                NormalizeLoadingNodeNames();
                ValidateLoadingVisualHierarchy();
                AssetDatabase.SaveAssets();
                Debug.Log("[ColorTiming] Loading UI node names normalized.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                throw;
            }
        }

        /// <summary>CI-friendly entry point: serialized migration then authoritative table generation.</summary>
        public static void RunBatch()
        {
            try
            {
                Apply();
                UGF.EditorTools.GameDataGenerator.GenerateDataTables();
                UGF.EditorTools.ResourceTools.ResourceRuleEditorUtility.RefreshResourceCollection();
                AssetDatabase.SaveAssets();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        /// <summary>CI-friendly entry point limited to the Loading visual hierarchy.</summary>
        public static void RunLoadingVisualHierarchyBatch()
        {
            try
            {
                ValidateLoadingUiStructure();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static AudioClip RemoveLegacyStartMenuPresentation()
        {
            var scene = EditorSceneManager.OpenScene(StartMenuScenePath, OpenSceneMode.Single);
            var audioSources = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<AudioSource>(true))
                .Where(source => source.gameObject.name.IndexOf("BGM", StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();
            var bgmClip = audioSources.Select(source => source.clip).FirstOrDefault(clip => clip != null);
            foreach (var source in audioSources)
            {
                UnityEngine.Object.DestroyImmediate(source.gameObject);
            }

            var loaders = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<LoadScenes>(true))
                .ToArray();
            foreach (var loader in loaders)
            {
                UnityEngine.Object.DestroyImmediate(loader.gameObject);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            return bgmClip;
        }

        private static Sprite LoadSprite(string guid)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException($"Loading sprite GUID is unavailable: {guid}");
            }

            var mainSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (mainSprite != null)
            {
                return mainSprite;
            }

            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            if (sprites.Length != 1)
            {
                throw new InvalidOperationException($"Expected one loading sprite at '{path}', got {sprites.Length}.");
            }

            return sprites[0];
        }

        private static void ValidateLoadingVisualHierarchy()
        {
            var root = PrefabUtility.LoadPrefabContents(LoadingPrefabPath);
            try
            {
                Assert(root.name == "Loading", "Loading root name changed unexpectedly.");
                var canvasComponent = root.GetComponent<Canvas>();
                var scaler = root.GetComponent<CanvasScaler>();
                Assert(canvasComponent != null && canvasComponent.renderMode == RenderMode.ScreenSpaceOverlay && canvasComponent.sortingOrder == 100,
                    "Loading must use the GF.UI root Canvas settings.");
                Assert(scaler != null && scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize,
                    "Loading root CanvasScaler settings differ from the source.");
                Assert(root.GetComponent<GraphicRaycaster>() != null, "Loading root must own a GraphicRaycaster.");

                var progressRoot = FindRequired(root.transform, "Grp_Progress");
                AssertRect((RectTransform)progressRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2029f, 1300f));
                AssertImageSprite(FindRequired(progressRoot, "Img_Background").GetComponent<Image>(), LoadingBackgroundGuid, "Img_Background");
                var progressBar = FindRequired(progressRoot, "Img_ProgressBar");
                AssertImageSprite(progressBar.GetComponent<Image>(), LoadingBarGuid, "Img_ProgressBar");

                var sliderContainer = FindRequired(progressBar, "Grp_Slider");
                AssertRect((RectTransform)sliderContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(328.80347f, -481f), new Vector2(1005.6069f, 60f));
                var sliderRoot = FindRequired(sliderContainer, "Sld_Progress");
                var slider = sliderRoot.GetComponent<Slider>();
                Assert(slider != null && Mathf.Approximately(slider.minValue, 0f) && Mathf.Approximately(slider.maxValue, 1f),
                    "Sld_Progress settings differ from the source.");
                Assert(!FindRequired(sliderRoot, "Grp_Fill").gameObject.activeSelf, "Grp_Fill must preserve the source inactive state.");
                Assert(!FindRequired(sliderRoot, "Img_SliderBackground").gameObject.activeSelf, "Img_SliderBackground must preserve the source inactive state.");
                var handle = FindRequired(sliderRoot, "Grp_Handle/Img_Handle").GetComponent<Image>();
                AssertImageSprite(handle, LoadingHandleGuid, "Img_Handle");
                Assert(slider.targetGraphic == handle && slider.handleRect == handle.rectTransform,
                    "Sld_Progress handle references differ from the source.");

                var fade = FindRequired(root.transform, "Img_FadeOverlay");
                Assert(fade.GetSiblingIndex() == 1, "Img_FadeOverlay must remain after Grp_Progress in draw order.");
                AssertRect((RectTransform)fade, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var fadeImage = fade.GetComponent<Image>();
                Assert(fadeImage != null && fadeImage.sprite == null && fadeImage.color == new Color(0f, 0f, 0f, 1f),
                    "Img_FadeOverlay settings differ from the source.");

                var form = root.GetComponent<ColorTimingLoadingForm>();
                Assert(form != null, "Loading prefab must retain ColorTimingLoadingForm.");
                var serializedForm = new SerializedObject(form);
                Assert(serializedForm.FindProperty("progressRoot").objectReferenceValue == progressRoot.gameObject,
                    "ColorTimingLoadingForm.progressRoot is not bound to Grp_Progress.");
                Assert(serializedForm.FindProperty("progressSlider").objectReferenceValue == slider,
                    "ColorTimingLoadingForm.progressSlider is not bound to Sld_Progress.");
                Assert(serializedForm.FindProperty("fadeImage").objectReferenceValue == fadeImage,
                    "ColorTimingLoadingForm.fadeImage is not bound to Img_FadeOverlay.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void NormalizeLoadingNodeNames()
        {
            var root = PrefabUtility.LoadPrefabContents(LoadingPrefabPath);
            try
            {
                Rename(root.transform, "Area_Fill", "Grp_Fill");
                Rename(root.transform, "Area_HandleSlide", "Grp_Handle");
                Rename(root.transform, "Overlay_Fade", "Img_FadeOverlay");
                PrefabUtility.SaveAsPrefabAsset(root, LoadingPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void Rename(Transform root, string oldName, string newName)
        {
            var target = FindRequiredOrNull(root, oldName);
            if (target != null)
            {
                target.name = newName;
            }
        }

        private static Transform FindRequiredOrNull(Transform root, string name)
        {
            if (root.name == name)
            {
                return root;
            }

            for (var index = 0; index < root.childCount; index++)
            {
                var result = FindRequiredOrNull(root.GetChild(index), name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static Transform FindRequired(Transform root, string path)
        {
            var result = root.Find(path);
            if (result == null)
            {
                throw new InvalidOperationException($"Loading prefab is missing required node '{path}'.");
            }

            return result;
        }

        private static void AssertImageSprite(Image image, string expectedGuid, string nodeName)
        {
            Assert(image != null && image.sprite == LoadSprite(expectedGuid), $"{nodeName} sprite reference differs from the source.");
        }

        private static void AssertRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            Assert(rect.anchorMin == anchorMin && rect.anchorMax == anchorMax && rect.anchoredPosition == anchoredPosition && rect.sizeDelta == sizeDelta,
                $"{rect.name} RectTransform differs from the source.");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void AssignStartMenuBgm(AudioClip migratedClip)
        {
            var root = PrefabUtility.LoadPrefabContents(MainMenuPrefabPath);
            try
            {
                var form = root.GetComponentInChildren<UI_ButtonAction>(true);
                if (form == null)
                {
                    throw new InvalidOperationException("MainMenu prefab must contain UI_ButtonAction.");
                }

                var serializedForm = new SerializedObject(form);
                var bgmProperty = serializedForm.FindProperty("bgm");
                if (migratedClip != null)
                {
                    bgmProperty.objectReferenceValue = migratedClip;
                    serializedForm.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.SaveAsPrefabAsset(root, MainMenuPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

    }
}
