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
        private const string HeroHealthContainerPrefabPath = "Assets/Game/Prefabs/UI/ColorTiming/P_HPBox.prefab";
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
                MigrateReusableItemPrefabs();
                RenamePauseMenuPrefab();
                MigrateGameFormPrefabNames();
                UGF.EditorTools.GameDataGenerator.GenerateDataTables();
                DeleteDeprecatedHeroHealthContainer();
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

                Rename(root.transform, "PlayerInfoPanel", "Grp_PlayerInfo");
                Rename(root.transform, "BossInfoPanel", "Grp_BossInfo");
                Rename(root.transform, "CharacterPortrait", "Img_CharacterPortrait");
                Rename(root.transform, "WeaponHint", "Img_WeaponHint");
                Rename(root.transform, "ChargeWeaponHint", "Img_ChargeWeaponHint");
                RenamePath(root.transform, "Grp_BossInfo/Grp_HeroHP", "Slot_BossHP");
                RenamePath(root.transform, "Grp_BossInfo/Slot_HeroHP", "Slot_BossHP");
                Rename(root.transform, "HealthPips", "Slot_HeroHP");
                Rename(root.transform, "Grp_HeroHP", "Slot_HeroHP");
                Rename(root.transform, "HPBox", "Slot_BossHP");
                Rename(root.transform, "Grp_BossHP", "Slot_BossHP");
                Rename(root.transform, "BossNameBanner", "Img_BossNameBanner");

                var heroHealthContainer = Find(root.transform, "Slot_HeroHP");
                Assert(heroHealthContainer != null, "BattleHud hero health container is missing.");
                if (PrefabUtility.IsPartOfPrefabInstance(heroHealthContainer.gameObject))
                {
                    PrefabUtility.UnpackPrefabInstance(heroHealthContainer.gameObject,
                        PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }

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
                SeparateBossHealthSlots(ref boss1Health, ref boss2Health);
                RemoveAuthoredBossHealthItems(boss1Health.transform);
                RemoveAuthoredBossHealthItems(boss2Health.transform);
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
                Assert(Find(root.transform, "Grp_PlayerInfo") != null && Find(root.transform, "Grp_BossInfo") != null,
                    "BattleHud semantic panel names are incomplete.");
                Assert(Find(root.transform, "Img_CharacterPortrait") != null && Find(root.transform, "Img_WeaponHint") != null
                    && Find(root.transform, "Img_ChargeWeaponHint") != null && Find(root.transform, "Slot_HeroHP") != null
                    && Find(root.transform, "Slot_Boss1HP") != null && Find(root.transform, "Slot_Boss2HP") != null,
                    "BattleHud semantic content names are incomplete.");
                var serializedForm = new SerializedObject(form);
                Assert(serializedForm.FindProperty("heroInfo").objectReferenceValue != null
                    && serializedForm.FindProperty("heroHealth").objectReferenceValue != null
                    && serializedForm.FindProperty("boss1Health").objectReferenceValue != null
                    && serializedForm.FindProperty("boss2Health").objectReferenceValue != null,
                    "BattleHudForm references are incomplete.");
                var boss1Health = root.GetComponentInChildren<UI_BossHPController>(true);
                var boss2Health = root.GetComponentInChildren<UI_BossHPController2>(true);
                Assert(boss1Health != null && boss2Health != null
                    && boss1Health.transform != boss2Health.transform
                    && boss1Health.transform.name == "Slot_Boss1HP"
                    && boss2Health.transform.name == "Slot_Boss2HP"
                    && boss1Health.transform.childCount == 0 && boss2Health.transform.childCount == 0,
                    "BattleHud must not serialize Boss HP items; controllers create them at runtime.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void MigrateReusableItemPrefabs()
        {
            RenamePrefabNodes("Assets/Game/Prefabs/UI/ColorTiming/HeroHP_Item.prefab",
                new[] { ("Image", "Img_HealthPip") });
            RenamePrefabNodes("Assets/Game/Prefabs/UI/ColorTiming/BossHP_Item.prefab",
                new[] { ("sprite", "Img_HealthPip"), ("Tip1", "Img_BossTipPrimary"), ("Tip2", "Img_BossTipSecondary") });
        }

        private static void MigrateGameFormPrefabNames()
        {
            RenamePrefabNodes("Assets/Game/Prefabs/UI/ColorTiming/Game/BattleResult.prefab",
                new[] { ("Shibai_Copy", "Img_Defeat"), ("Shengli_Copy", "Img_Victory") });
            RenamePrefabPaths("Assets/Game/Prefabs/UI/ColorTiming/Game/BattleResult.prefab",
                new[]
                {
                    ("Img_Defeat/Text (Legacy)", "Txt_DefeatTitle"),
                    ("Img_Victory/Text (Legacy)", "Txt_VictoryTitle"),
                });
            RenamePrefabNodes("Assets/Game/Prefabs/UI/ColorTiming/Game/MainMenu.prefab",
                new[] { ("StartButtonBox", "Grp_StartActions"),
                    ("GoGameButtonBox", "Grp_StageSelection"), ("SettingButtonBox", "Grp_Settings") });
            RenamePrefabPaths("Assets/Game/Prefabs/UI/ColorTiming/Game/MainMenu.prefab",
                new[]
                {
                    ("Img_Background", "Grp_Background"),
                    ("Grp_Background/BackGround", "Img_Background"),
                    ("Grp_Background/Title", "Img_Title"),
                    ("Grp_StartActions/Button_Start", "Btn_OpenStageSelection"),
                    ("Grp_StartActions/Btn_OpenStageSelection/Text (Legacy)", "Txt_OpenStageSelection"),
                    ("Grp_StartActions/Button_Start (1)", "Btn_StartStage2"),
                    ("Grp_StartActions/Btn_StartStage2/Text (Legacy)", "Txt_StartStage2"),
                    ("Grp_StartActions/Button_SystemSetup", "Btn_OpenSettings"),
                    ("Grp_StartActions/Btn_OpenSettings/Text (Legacy)", "Txt_OpenSettings"),
                    ("Grp_StartActions/Button_Exit", "Btn_ExitGame"),
                    ("Grp_StartActions/Btn_ExitGame/Text (Legacy)", "Txt_ExitGame"),
                    ("Grp_StageSelection/Button_Start", "Btn_StartStage1"),
                    ("Grp_StageSelection/Btn_StartStage1/Text (Legacy)", "Txt_StartStage1"),
                    ("Grp_StageSelection/Button_Start_1", "Btn_StartStage2"),
                    ("Grp_StageSelection/Button_Back", "Btn_CloseStageSelection"),
                    ("Grp_StageSelection/Btn_CloseStageSelection/Text (Legacy)", "Txt_CloseStageSelection"),
                    ("GameObject", "Anchor_MenuContent"),
                    ("Grp_Settings/Btn_BGM", "Grp_BgmToggle"),
                    ("Grp_Settings/Grp_BgmToggle/Button_BGM_Open", "Btn_EnableBgm"),
                    ("Grp_Settings/Grp_BgmToggle/Btn_EnableBgm/Text (Legacy)", "Txt_EnableBgm"),
                    ("Grp_Settings/Grp_BgmToggle/Button_BGM_Off", "Btn_DisableBgm"),
                    ("Grp_Settings/Grp_BgmToggle/Btn_DisableBgm/Text (Legacy)", "Txt_DisableBgm"),
                    ("Grp_Settings/BtnSFX", "Grp_SfxToggle"),
                    ("Grp_Settings/Grp_SfxToggle/Button_SFX_Off", "Btn_DisableSfx"),
                    ("Grp_Settings/Grp_SfxToggle/Button_SFX_Open", "Btn_EnableSfx"),
                    ("Grp_Settings/OffTip", "Grp_AudioToggleHints"),
                    ("Grp_Settings/Grp_AudioToggleHints/OffTip", "Btn_HideAudioHints"),
                    ("Grp_Settings/Grp_AudioToggleHints/OpenTip", "Btn_ShowAudioHints"),
                    ("Grp_Settings/Button_Back", "Btn_CloseSettings"),
                    ("Grp_Settings/Btn_CloseSettings/Text (Legacy)", "Txt_CloseSettings"),
                });
            RenamePrefabPaths("Assets/Game/Prefabs/UI/ColorTiming/Game/PauseMenu.prefab",
                new[]
                {
                    ("Box", "Panel_PauseContent"),
                    ("Panel_PauseContent/Text (Legacy)", "Txt_PauseTitle"),
                    ("Panel_PauseContent/OffTip", "Grp_PauseHints"),
                    ("Panel_PauseContent/Grp_PauseHints/OffTip", "Btn_HidePauseHints"),
                    ("Panel_PauseContent/Grp_PauseHints/OpenTip", "Btn_ShowPauseHints"),
                    ("Panel_PauseContent/GoNext", "Btn_NextLevel"),
                    ("Panel_PauseContent/GoLast", "Btn_PreviousLevel"),
                    ("Panel_PauseContent/BackMenu", "Btn_ReturnToMenu"),
                });
        }

        private static void RenamePauseMenuPrefab()
        {
            const string sourcePath = "Assets/Game/Prefabs/UI/ColorTiming/Game/Esc.prefab";
            const string targetPath = "Assets/Game/Prefabs/UI/ColorTiming/Game/PauseMenu.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath) != null
                && AssetDatabase.LoadAssetAtPath<GameObject>(targetPath) == null)
            {
                Assert(string.IsNullOrEmpty(AssetDatabase.MoveAsset(sourcePath, targetPath)),
                    "Could not rename Esc prefab to PauseMenu.");
            }
        }

        private static void RenamePrefabNodes(string prefabPath, (string OldName, string NewName)[] names)
        {
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                foreach (var pair in names)
                {
                    Rename(root.transform, pair.OldName, pair.NewName);
                }
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void RenamePrefabPaths(string prefabPath, (string Path, string NewName)[] names)
        {
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                foreach (var pair in names)
                {
                    var target = root.transform.Find(pair.Path);
                    if (target != null)
                    {
                        target.name = pair.NewName;
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void DeleteDeprecatedHeroHealthContainer()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(HeroHealthContainerPrefabPath) != null)
            {
                Assert(AssetDatabase.DeleteAsset(HeroHealthContainerPrefabPath),
                    "Could not delete deprecated P_HPBox prefab.");
            }
        }

        private static void Rename(Transform root, string oldName, string newName)
        {
            var target = Find(root, oldName);
            if (target != null)
            {
                target.name = newName;
            }
        }

        private static void RemoveAuthoredBossHealthItems(Transform container)
        {
            for (var index = container.childCount - 1; index >= 0; index--)
            {
                UnityEngine.Object.DestroyImmediate(container.GetChild(index).gameObject);
            }
        }

        private static void SeparateBossHealthSlots(
            ref UI_BossHPController boss1Health,
            ref UI_BossHPController2 boss2Health)
        {
            if (boss1Health.transform != boss2Health.transform)
            {
                boss1Health.name = "Slot_Boss1HP";
                boss2Health.name = "Slot_Boss2HP";
                return;
            }

            var boss1Slot = (RectTransform)boss1Health.transform;
            boss1Slot.name = "Slot_Boss1HP";
            var boss2SlotObject = new GameObject("Slot_Boss2HP", typeof(RectTransform));
            boss2SlotObject.layer = boss1Slot.gameObject.layer;
            var boss2Slot = (RectTransform)boss2SlotObject.transform;
            boss2Slot.SetParent(boss1Slot.parent, false);
            CopyRectTransform(boss1Slot, boss2Slot);
            var replacement = boss2SlotObject.AddComponent<UI_BossHPController2>();
            replacement.HPItem = boss2Health.HPItem;
            UnityEngine.Object.DestroyImmediate(boss2Health);
            boss2Health = replacement;
        }

        private static void CopyRectTransform(RectTransform source, RectTransform destination)
        {
            destination.anchorMin = source.anchorMin;
            destination.anchorMax = source.anchorMax;
            destination.anchoredPosition = source.anchoredPosition;
            destination.sizeDelta = source.sizeDelta;
            destination.pivot = source.pivot;
            destination.localRotation = source.localRotation;
            destination.localScale = source.localScale;
        }

        private static void RenamePath(Transform root, string path, string newName)
        {
            var target = root.Find(path);
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
