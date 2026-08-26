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

            var root = new GameObject("Loading", typeof(RectTransform));
            try
            {
                ConfigureRoot(root);
                var fadeImage = CreateImage("Img_Fade", root.transform, new Color(0f, 0f, 0f, 1f));
                Stretch((RectTransform)fadeImage.transform);
                fadeImage.raycastTarget = true;

                var progressGroup = new GameObject("Grp_Progress", typeof(RectTransform));
                progressGroup.transform.SetParent(root.transform, false);
                var progressGroupRect = (RectTransform)progressGroup.transform;
                progressGroupRect.anchorMin = new Vector2(0.5f, 0.2f);
                progressGroupRect.anchorMax = new Vector2(0.5f, 0.2f);
                progressGroupRect.pivot = new Vector2(0.5f, 0.5f);
                progressGroupRect.sizeDelta = new Vector2(480f, 32f);

                var progressSlider = CreateProgressSlider(progressGroup.transform);
                var form = root.AddComponent<ColorTimingLoadingForm>();
                var serializedForm = new SerializedObject(form);
                serializedForm.FindProperty("progressSlider").objectReferenceValue = progressSlider;
                serializedForm.FindProperty("fadeImage").objectReferenceValue = fadeImage;
                serializedForm.FindProperty("canvasGroup").objectReferenceValue = root.GetComponent<CanvasGroup>();
                serializedForm.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, LoadingPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void ConfigureRoot(GameObject root)
        {
            root.layer = LayerMask.NameToLayer("UI");
            Stretch((RectTransform)root.transform);
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();
            var group = root.AddComponent<CanvasGroup>();
            group.alpha = 1f;
            group.interactable = false;
            group.blocksRaycasts = true;
        }

        private static Slider CreateProgressSlider(Transform parent)
        {
            var root = new GameObject("Sld_Progress", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Slider));
            root.transform.SetParent(parent, false);
            Stretch((RectTransform)root.transform);
            var background = root.GetComponent<Image>();
            background.sprite = GetDefaultSprite();
            background.color = new Color(1f, 1f, 1f, 0.2f);
            background.raycastTarget = false;

            var fillGroup = new GameObject("Grp_Fill", typeof(RectTransform));
            fillGroup.transform.SetParent(root.transform, false);
            Stretch((RectTransform)fillGroup.transform);
            var fill = CreateImage("Img_Fill", fillGroup.transform, new Color(0.2f, 0.75f, 1f, 1f));
            Stretch((RectTransform)fill.transform);
            fill.raycastTarget = false;

            var slider = root.GetComponent<Slider>();
            slider.transition = Selectable.Transition.None;
            slider.interactable = false;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.SetValueWithoutNotify(0f);
            slider.targetGraphic = background;
            slider.fillRect = (RectTransform)fill.transform;
            slider.handleRect = null;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            var image = imageObject.GetComponent<Image>();
            image.sprite = GetDefaultSprite();
            image.color = color;
            return image;
        }

        private static Sprite GetDefaultSprite()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
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

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }
    }
}
