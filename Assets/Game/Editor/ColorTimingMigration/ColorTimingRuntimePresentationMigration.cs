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
                RebuildLoadingPrefab();
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

        [MenuItem("Game Framework/GameTools/Restore ColorTiming Loading Visual Hierarchy", false, 1006)]
        public static void RestoreLoadingVisualHierarchy()
        {
            try
            {
                RebuildLoadingPrefab();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                ValidateLoadingVisualHierarchy();
                Debug.Log("[ColorTiming] Loading visual hierarchy restored and validated.");
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
                RestoreLoadingVisualHierarchy();
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

        private static void RebuildLoadingPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(LoadingPrefabPath);
            if (existing == null)
            {
                throw new InvalidOperationException($"Loading prefab is missing: {LoadingPrefabPath}");
            }

            var root = new GameObject("Loading");
            try
            {
                var canvas = CreateLegacyCanvas(root.transform);
                var progressRoot = CreateRect("Grp_Progress", canvas.transform, true);
                ApplyRect((RectTransform)progressRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2029f, 1300f), new Vector2(0.5f, 0.5f));
                CreateImage("Img_Background", progressRoot.transform, LoadSprite(LoadingBackgroundGuid), Color.white, true,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2029f, 1300f), new Vector2(0.5f, 0.5f));
                var progressBar = CreateImage("Img_ProgressBar", progressRoot.transform, LoadSprite(LoadingBarGuid), Color.white, true,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2029f, 1300f), new Vector2(0.5f, 0.5f));
                var sliderContainer = CreateRect("Grp_Slider", progressBar.transform, true);
                ApplyRect((RectTransform)sliderContainer.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(328.80347f, -481f), new Vector2(1005.6069f, 60f), new Vector2(0.5f, 0.5f));
                var progressSlider = CreateLegacySlider(sliderContainer.transform);
                var fadeImage = CreateImage("Overlay_Fade", canvas.transform, null, new Color(0f, 0f, 0f, 1f), true,
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
                var form = root.AddComponent<ColorTimingLoadingForm>();
                var serializedForm = new SerializedObject(form);
                serializedForm.FindProperty("loadingCanvas").objectReferenceValue = canvas;
                serializedForm.FindProperty("progressRoot").objectReferenceValue = progressRoot;
                serializedForm.FindProperty("progressSlider").objectReferenceValue = progressSlider;
                serializedForm.FindProperty("fadeImage").objectReferenceValue = fadeImage;
                serializedForm.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, LoadingPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateLegacyCanvas(Transform parent)
        {
            var root = CreateRect("Canvas_Loading", parent, false);
            root.layer = LayerMask.NameToLayer("UI");
            var rect = (RectTransform)root.transform;
            ApplyRect(rect, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            rect.localScale = Vector3.zero;
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;
            scaler.referencePixelsPerUnit = 100f;
            root.AddComponent<GraphicRaycaster>();
            return root;
        }

        private static Slider CreateLegacySlider(Transform parent)
        {
            var root = CreateRect("Sld_Progress", parent, true);
            ApplyRect((RectTransform)root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            var slider = root.AddComponent<Slider>();
            slider.transition = Selectable.Transition.ColorTint;
            slider.interactable = true;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.SetValueWithoutNotify(0f);
            var fillArea = CreateRect("Area_Fill", root.transform, false);
            ApplyRect((RectTransform)fillArea.transform, new Vector2(0f, 0.25f), new Vector2(1f, 0.75f), new Vector2(-5f, 0f), new Vector2(-20f, 0f), new Vector2(0.5f, 0.5f));
            var fill = CreateImage("Img_Fill", fillArea.transform, LoadSprite(LoadingHandleGuid), Color.white, true,
                new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(10f, 0f), new Vector2(0.5f, 0.5f));
            CreateImage("Img_SliderBackground", root.transform, GetDefaultSprite(), Color.white, true,
                new Vector2(0f, 0.25f), new Vector2(1f, 0.75f), Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f)).gameObject.SetActive(false);
            var handleArea = CreateRect("Area_HandleSlide", root.transform, true);
            ApplyRect((RectTransform)handleArea.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-20f, 0f), new Vector2(0.5f, 0.5f));
            var handle = CreateImage("Img_Handle", handleArea.transform, LoadSprite(LoadingHandleGuid), Color.white, true,
                new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(60f, 0f), new Vector2(0.5f, 0.5f));
            slider.targetGraphic = handle;
            slider.fillRect = (RectTransform)fill.transform;
            slider.handleRect = (RectTransform)handle.transform;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private static GameObject CreateRect(string name, Transform parent, bool active)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            gameObject.SetActive(active);
            return gameObject;
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color, bool raycastTarget,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, Vector2 pivot)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            ApplyRect((RectTransform)imageObject.transform, anchorMin, anchorMax, anchoredPosition, sizeDelta, pivot);
            var image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
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

        private static void ApplyRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPosition, Vector2 sizeDelta, Vector2 pivot)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            rect.pivot = pivot;
        }

        private static Sprite GetDefaultSprite()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        private static void ValidateLoadingVisualHierarchy()
        {
            var root = PrefabUtility.LoadPrefabContents(LoadingPrefabPath);
            try
            {
                Assert(root.name == "Loading", "Loading root name changed unexpectedly.");
                Assert(root.GetComponent<Canvas>() == null, "Loading root must not own a Canvas.");

                var canvas = FindRequired(root.transform, "Canvas_Loading");
                Assert(!canvas.gameObject.activeSelf, "Canvas_Loading must preserve the source inactive state.");
                Assert(canvas.localScale == Vector3.zero, "Canvas_Loading scale must preserve the source value.");
                var canvasComponent = canvas.GetComponent<Canvas>();
                var scaler = canvas.GetComponent<CanvasScaler>();
                Assert(canvasComponent != null && canvasComponent.renderMode == RenderMode.ScreenSpaceOverlay && canvasComponent.sortingOrder == 100,
                    "Canvas_Loading canvas settings differ from the source.");
                Assert(scaler != null && scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize,
                    "Canvas_Loading scaler settings differ from the source.");

                var progressRoot = FindRequired(canvas, "Grp_Progress");
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
                Assert(!FindRequired(sliderRoot, "Area_Fill").gameObject.activeSelf, "Area_Fill must preserve the source inactive state.");
                Assert(!FindRequired(sliderRoot, "Img_SliderBackground").gameObject.activeSelf, "Img_SliderBackground must preserve the source inactive state.");
                var handle = FindRequired(sliderRoot, "Area_HandleSlide/Img_Handle").GetComponent<Image>();
                AssertImageSprite(handle, LoadingHandleGuid, "Img_Handle");
                Assert(slider.targetGraphic == handle && slider.handleRect == handle.rectTransform,
                    "Sld_Progress handle references differ from the source.");

                var fade = FindRequired(canvas, "Overlay_Fade");
                Assert(fade.GetSiblingIndex() == 1, "Overlay_Fade must remain after Grp_Progress in draw order.");
                AssertRect((RectTransform)fade, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var fadeImage = fade.GetComponent<Image>();
                Assert(fadeImage != null && fadeImage.sprite == null && fadeImage.color == new Color(0f, 0f, 0f, 1f),
                    "Overlay_Fade settings differ from the source.");

                var form = root.GetComponent<ColorTimingLoadingForm>();
                Assert(form != null, "Loading prefab must retain ColorTimingLoadingForm.");
                var serializedForm = new SerializedObject(form);
                Assert(serializedForm.FindProperty("loadingCanvas").objectReferenceValue == canvas.gameObject,
                    "ColorTimingLoadingForm.loadingCanvas is not bound to Canvas_Loading.");
                Assert(serializedForm.FindProperty("progressRoot").objectReferenceValue == progressRoot.gameObject,
                    "ColorTimingLoadingForm.progressRoot is not bound to Grp_Progress.");
                Assert(serializedForm.FindProperty("progressSlider").objectReferenceValue == slider,
                    "ColorTimingLoadingForm.progressSlider is not bound to Sld_Progress.");
                Assert(serializedForm.FindProperty("fadeImage").objectReferenceValue == fadeImage,
                    "ColorTimingLoadingForm.fadeImage is not bound to Overlay_Fade.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
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
