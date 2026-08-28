// ColorTiming 项目美术资源命名迁移工具。
// 先生成完整预览并校验，再由独立菜单执行；所有移动均通过 AssetDatabase 保留 GUID。

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorTiming.EditorTools
{
    internal static class ColorTimingArtAssetNamingMigration
    {
        private const string Root = "Assets/Game/Sprites/ColorTiming";
        private const string EvidenceDirectory =
            "openspec/changes/normalize-color-timing-art-asset-naming/evidence";
        private const string PreviewPath = EvidenceDirectory + "/asset-migration-preview.csv";

        private static readonly string[] ApprovedDeletePaths =
        {
            Root + "/Boss/第二关BOSS拆分3.png",
            Root + "/Scene/B2/第二关BOSS拆分3.png",
            Root + "/Scene/B1/摆放图（注意前后）.png",
            Root + "/Scene/B2/标注.jpg",
            Root + "/System/tongguan.png",
            Root + "/ui/摆放 1.png",
            Root + "/ui/布局示意图.jpg",
            Root + "/ui/教程.png",
        };

        private static readonly Dictionary<string, string> DirectoryMap =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Boss/Skill5"] = "Boss1/Effects/Attack5",
                ["Boss/Spine"] = "Boss1/Core",
                ["Boss/Spine2"] = "Boss1/Attack5",
                ["Boss/tip"] = "Boss1/Telegraph",
                ["Boss2/di"] = "Boss2/Burrow",
                ["Boss2/Shenti"] = "Boss2/Core",
                ["Boss2/Skill"] = "Boss2/Effects",
                ["Boss2/Weiba"] = "Boss2/Tail",
                ["Hero/Anim"] = "Hero/Animations",
                ["Hero/Sequence/Chongci"] = "Hero/Sequences/Dash",
                ["Hero/Sequence/Chuizi/Daiji"] = "Hero/Sequences/Hammer/Idle",
                ["Hero/Sequence/Chuizi/Gongji/Hong"] = "Hero/Sequences/Hammer/Attack/Red",
                ["Hero/Sequence/Chuizi/Gongji/Lv"] = "Hero/Sequences/Hammer/Attack/Green",
                ["Hero/Sequence/Chuizi/Gongji/Zi"] = "Hero/Sequences/Hammer/Attack/Purple",
                ["Hero/Sequence/Chuizi/yidong"] = "Hero/Sequences/Hammer/Move",
                ["Hero/Sequence/Daiji"] = "Hero/Sequences/Idle",
                ["Hero/Sequence/Dao/daiji"] = "Hero/Sequences/RingBlade/Idle",
                ["Hero/Sequence/Dao/gongji"] = "Hero/Sequences/RingBlade/Attack",
                ["Hero/Sequence/Dao/yidong"] = "Hero/Sequences/RingBlade/Move",
                ["Hero/Sequence/feiji"] = "Hero/Sequences/Plane",
                ["Hero/Sequence/feiji/daiji"] = "Hero/Sequences/Plane/Idle",
                ["Hero/Sequence/feiji/gongji"] = "Hero/Sequences/Plane/Attack",
                ["Hero/Sequence/feiji/skill"] = "Hero/Sequences/Plane/Skill",
                ["Hero/Sequence/feiji/yidong"] = "Hero/Sequences/Plane/Move",
                ["Hero/Sequence/futou/daiji"] = "Hero/Sequences/Axe/Idle",
                ["Hero/Sequence/futou/gongji/chen"] = "Hero/Sequences/Axe/Attack/Orange",
                ["Hero/Sequence/futou/gongji/hong"] = "Hero/Sequences/Axe/Attack/Red",
                ["Hero/Sequence/futou/gongji/lv"] = "Hero/Sequences/Axe/Attack/Green",
                ["Hero/Sequence/futou/gongji/skill"] = "Hero/Sequences/Axe/Skill",
                ["Hero/Sequence/futou/gongji/zhi"] = "Hero/Sequences/Axe/Attack/Purple",
                ["Hero/Sequence/futou/yidong"] = "Hero/Sequences/Axe/Move",
                ["Hero/Sequence/Gongji"] = "Hero/Sequences/BasicAttack",
                ["Hero/Sequence/jiandao/daiji"] = "Hero/Sequences/Scissors/Idle",
                ["Hero/Sequence/jiandao/gongji"] = "Hero/Sequences/Scissors/Attack",
                ["Hero/Sequence/jiandao/gongji/Skill"] = "Hero/Sequences/Scissors/HitEffects",
                ["Hero/Sequence/jiandao/yidong"] = "Hero/Sequences/Scissors/Move",
                ["Hero/Sequence/Shouji"] = "Hero/Sequences/Hit",
                ["Hero/Sequence/Siwang"] = "Hero/Sequences/Death",
                ["Hero/Sequence/Siwang/sc"] = "Hero/Sequences/Death/Transition",
                ["Hero/Sequence/Yidong"] = "Hero/Sequences/Move/Front",
                ["Hero/Sequence/Yidong/Beimian"] = "Hero/Sequences/Move/Back",
                ["Hero/Sequence/Zhadan/daiji"] = "Hero/Sequences/Bomb/Idle",
                ["Hero/Sequence/Zhadan/gongji"] = "Hero/Sequences/Bomb/Attack",
                ["Hero/Sequence/Zhadan/skill"] = "Hero/Sequences/Bomb/Skill",
                ["Hero/Sequence/Zhadan/yidong"] = "Hero/Sequences/Bomb/Move",
                ["Loding"] = "UI/Loading",
                ["Mouse"] = "Cursors",
                ["Scene/B1"] = "Scenes/Boss1",
                ["Scene/B1/a1"] = "Scenes/Boss1/Grass01",
                ["Scene/B1/a2"] = "Scenes/Boss1/Grass02",
                ["Scene/B1/a2/c"] = "Scenes/Boss1/Grass02/Motion",
                ["Scene/B1/a2/dj"] = "Scenes/Boss1/Grass02/Idle",
                ["Scene/B1/a3"] = "Scenes/Boss1/Grass03",
                ["Scene/B1/a3/c"] = "Scenes/Boss1/Grass03/Motion",
                ["Scene/B1/a3/dj"] = "Scenes/Boss1/Grass03/Idle",
                ["Scene/B1/a4"] = "Scenes/Boss1/Grass04",
                ["Scene/B1/a4/c"] = "Scenes/Boss1/Grass04/Motion",
                ["Scene/B1/a4/dj"] = "Scenes/Boss1/Grass04/Idle",
                ["Scene/B1/a5"] = "Scenes/Boss1/Grass05",
                ["Scene/B1/a5/c"] = "Scenes/Boss1/Grass05/Motion",
                ["Scene/B1/a5/dj"] = "Scenes/Boss1/Grass05/Idle",
                ["Scene/B1/a6"] = "Scenes/Boss1/Grass06",
                ["Scene/B1/a6/c"] = "Scenes/Boss1/Grass06/Motion",
                ["Scene/B1/a6/dj"] = "Scenes/Boss1/Grass06/Idle",
                ["Scene/B1/jianci"] = "Scenes/Boss1/Thorns",
                ["Scene/B2"] = "Scenes/Boss2",
                ["Scene/B2/cao"] = "Scenes/Boss2/Grass",
                ["System"] = "UI/MainMenu",
                ["ui"] = "UI/Battle",
                ["UI_ESC"] = "UI/PauseMenu",
                ["UI_Set"] = "UI/Settings",
                ["UI_Start"] = "UI/MainMenu",
                ["Weapon"] = "Weapons/Icons",
                ["Weapon/Tip1"] = "Weapons/Tutorial/Boss1",
                ["Weapon/Tip2"] = "Weapons/Tutorial/Boss2",
                ["yin"] = "Shadows",
            };

        private static readonly Dictionary<string, string> SpineBaseNames =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Boss/Spine"] = "Boss1Core",
                ["Boss/Spine2"] = "Boss1Attack5",
                ["Boss/tip"] = "Boss1Telegraph",
                ["Boss2/di"] = "Boss2Burrow",
                ["Boss2/Shenti"] = "Boss2Core",
                ["Boss2/Weiba"] = "Boss2Tail",
            };

        private static readonly Dictionary<string, string> TechnicalExactNames =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Boss/Skill5/boos1_atk5.anim"] = "Boss1_Attack5_Effect.anim",
                ["Boss/Skill5/boos1_atk5.controller"] = "Boss1_Attack5_Effect.controller",
                // 多页 Spine atlas 的材质名由导入器按“Atlas基名_贴图页名”约定解析，必须预先使用该名称。
                ["Boss/Spine/BOSS拆分_BOSS拆分.mat"] = "Boss1Core_Boss1Core.mat",
                ["Boss/Spine/BOSS拆分_BOSS拆分2.mat"] = "Boss1Core_Boss1Core_Page02.mat",
                ["Boss/Spine/BOSS拆分_BOSS拆分3.mat"] = "Boss1Core_Boss1Core_Page03.mat",
                ["Boss/Spine2/BOSS拆分_BOSS拆分.mat"] = "Boss1Attack5_Boss1Attack5.mat",
                ["Boss/Spine2/BOSS拆分_BOSS拆分2.mat"] = "Boss1Attack5_Boss1Attack5_Page02.mat",
                ["Boss2/Skill/boss2_atk2.anim"] = "Boss2_Attack2_Effect.anim",
                ["Boss2/Skill/boss2_atk2.controller"] = "Boss2_Attack2_Effect.controller",
                ["Hero/Anim/Hero Animator Controller.controller"] = "Hero.controller",
                ["Hero/Anim/HeroTest.controller"] = "HeroAnimator_Test.controller",
                ["Hero/Anim/t.anim"] = "HeroAnimation_Test.anim",
                ["Hero/Anim/tttttt.controller"] = "HeroController_Test.controller",
                ["Hero/Sequence/Chongci/dash.anim"] = "Hero_Dash.anim",
                ["Hero/Sequence/Daiji/daiji.anim"] = "Hero_Idle.anim",
                ["Hero/Sequence/Gongji/Atk_nor.anim"] = "Hero_BasicAttack.anim",
                ["Hero/Sequence/Gongji/HitFx_Anim.controller"] = "Hero_BasicAttackHitEffect.controller",
                ["Hero/Sequence/Gongji/HitFX_Nor.anim"] = "Hero_BasicAttackHitEffect.anim",
                ["Hero/Sequence/Shouji/hit_sc.anim"] = "Hero_HitExposure.anim",
                ["Hero/Sequence/Shouji/hit_sccc.controller"] = "Hero_HitEffects.controller",
                ["Hero/Sequence/Shouji/shouji.anim"] = "Hero_Hit.anim",
                ["Hero/Sequence/Siwang/death.anim"] = "Hero_Death.anim",
                ["Hero/Sequence/Siwang/sc/death_sc.anim"] = "Hero_DeathTransition.anim",
                ["Hero/Sequence/Siwang/sc/death_sc.controller"] = "Hero_DeathTransition.controller",
                ["Hero/Sequence/feiji/qi_feiji.anim"] = "Hero_Plane_GetUp.anim",
                ["Hero/Sequence/feiji/skill/feiji.controller"] = "Hero_Plane_Skill.controller",
                ["Hero/Sequence/futou/gongji/skill/sk_futou.anim"] = "Hero_Axe_Skill.anim",
                ["Hero/Sequence/futou/gongji/skill/sk_futou.controller"] = "Hero_Axe_Skill.controller",
                ["Hero/Sequence/jiandao/gongji/Skill/HitFX_jd_ 2.controller"] = "Hero_Scissors_HitEffect02.controller",
                ["Hero/Sequence/jiandao/gongji/Skill/HitFX_jd_.controller"] = "Hero_Scissors_HitEffect01.controller",
                ["Hero/Sequence/jiandao/gongji/Skill/HitFX_Jiandao1.anim"] = "Hero_Scissors_HitEffect01.anim",
                ["Hero/Sequence/jiandao/gongji/Skill/HitFX_Jiandao2.anim"] = "Hero_Scissors_HitEffect02.anim",
                ["Hero/Sequence/Zhadan/skill/zhadan.controller"] = "Hero_Bomb_Skill.controller",
                ["Weapon/Tip1/HpTip.anim"] = "Boss1_HealthTip.anim",
                ["Weapon/Tip1/HpTip1.controller"] = "Boss1_HealthTip.controller",
                ["Weapon/Tip1/WeaponTip1.anim"] = "Boss1_WeaponTip.anim",
                ["Weapon/Tip1/WeaponTip1.controller"] = "Boss1_WeaponTip.controller",
                ["Weapon/Tip2/HPTip2.anim"] = "Boss2_HealthTip.anim",
                ["Weapon/Tip2/HPTip2.controller"] = "Boss2_HealthTip.controller",
                ["Weapon/Tip2/Tip2.controller"] = "Boss2_WeaponTip.controller",
                ["Weapon/Tip2/WeaponTip2.anim"] = "Boss2_WeaponTip.anim",
            };

        private static readonly Dictionary<string, string> ArtExactNames =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                // 两个源文件字节哈希一致且均无引用；保留错括号版本并显式标记，避免破坏式去重。
                ["Hero/Sequence/futou/gongji/zhi/紫色斧头蓄力(原地）_0004.png"] =
                    "英雄_斧头_蓄力_原地_紫色_重复副本_0004.png",
                ["System/游戏logo.png"] = "游戏标志.png",
            };

        private static readonly Dictionary<string, string> TokenTranslations =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["daiji"] = "Idle", ["gongji"] = "Attack", ["yidong"] = "Move",
                ["cuizi"] = "Hammer", ["dao"] = "RingBlade", ["feiji"] = "Plane",
                ["futou"] = "Axe", ["futoui"] = "Axe", ["jiandao"] = "Scissors",
                ["zhadan"] = "Bomb", ["hong"] = "Red", ["lv"] = "Green",
                ["zi"] = "Purple", ["zhi"] = "Purple", ["chen"] = "Orange",
                ["xuli"] = "Charge", ["xuliw"] = "ChargeComplete", ["do"] = "Moving",
                ["yu"] = "Stationary", ["move"] = "Move", ["Anim"] = "Animation",
                ["Hero"] = "Hero",
            };

        private sealed class Entry
        {
            public string Source;
            public string Guid;
            public string Target;
            public string Action;
            public string Note;
        }

        [MenuItem("Tools/ColorTiming/Art Naming/1. Preview Migration")]
        private static void PreviewMigration()
        {
            List<Entry> entries = BuildPlan();
            WritePreview(entries);
            ReportPlan(entries);
        }

        private static void ExecuteMigration()
        {
            List<Entry> entries = BuildPlan();
            ValidatePlanOrThrow(entries);

            if (!File.Exists(PreviewPath))
            {
                throw new InvalidOperationException("请先执行预览菜单并检查迁移清单。");
            }

            Dictionary<string, string> hashes = CaptureArtHashes(entries);
            AssetDatabase.StartAssetEditing();
            try
            {
                EnsureTargetDirectories(entries);

                foreach (Entry entry in entries.Where(item => item.Action == "Move"))
                {
                    string error = AssetDatabase.MoveAsset(entry.Source, entry.Target);
                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new InvalidOperationException(
                            $"资源移动失败：{entry.Source} -> {entry.Target}\n{error}");
                    }
                }


                UpdateMovedSpineAtlasPages(entries);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }

            ValidateGuids(entries);
            ValidateArtHashes(entries, hashes);
            WriteExecutionReport(entries);
            Debug.Log($"[ColorTimingArtNaming] 迁移完成：{entries.Count(item => item.Action == "Move")} 个资源，GUID 与图片字节校验通过。");
        }

        [MenuItem("Tools/ColorTiming/Art Naming/2. Execute Boss Batch")]
        private static void ExecuteBossBatch()
        {
            ExecuteBatch(entry => entry.Source.StartsWith(Root + "/Boss/", StringComparison.Ordinal) ||
                                  entry.Source.StartsWith(Root + "/Boss2/", StringComparison.Ordinal), "Boss");
        }

        private static void ExecuteHeroBatch()
        {
            ExecuteBatch(entry => entry.Source.StartsWith(Root + "/Hero/", StringComparison.Ordinal), "Hero");
        }

        [MenuItem("Tools/ColorTiming/Art Naming/3a. Execute Hero Core Batch")]
        private static void ExecuteHeroCoreBatch()
        {
            ExecuteBatch(entry => entry.Source.StartsWith(Root + "/Hero/", StringComparison.Ordinal) &&
                                  !entry.Source.Contains("/Sequence/Chuizi/", StringComparison.Ordinal) &&
                                  !entry.Source.Contains("/Sequence/Dao/", StringComparison.Ordinal) &&
                                  !entry.Source.Contains("/Sequence/feiji/", StringComparison.Ordinal) &&
                                  !entry.Source.Contains("/Sequence/futou/", StringComparison.Ordinal) &&
                                  !entry.Source.Contains("/Sequence/jiandao/", StringComparison.Ordinal) &&
                                  !entry.Source.Contains("/Sequence/Zhadan/", StringComparison.Ordinal), "HeroCore");
        }

        [MenuItem("Tools/ColorTiming/Art Naming/3b. Execute Hero Hammer RingBlade Batch")]
        private static void ExecuteHeroHammerRingBladeBatch()
        {
            ExecuteBatch(entry => entry.Source.Contains("/Hero/Sequence/Chuizi/", StringComparison.Ordinal) ||
                                  entry.Source.Contains("/Hero/Sequence/Dao/", StringComparison.Ordinal), "HeroHammerRingBlade");
        }

        [MenuItem("Tools/ColorTiming/Art Naming/3c. Execute Hero Plane Axe Batch")]
        private static void ExecuteHeroPlaneAxeBatch()
        {
            ExecuteBatch(entry => entry.Source.Contains("/Hero/Sequence/feiji/", StringComparison.Ordinal) ||
                                  entry.Source.Contains("/Hero/Sequence/futou/", StringComparison.Ordinal), "HeroPlaneAxe");
        }

        [MenuItem("Tools/ColorTiming/Art Naming/3d. Execute Hero Scissors Bomb Batch")]
        private static void ExecuteHeroScissorsBombBatch()
        {
            ExecuteBatch(entry => entry.Source.Contains("/Hero/Sequence/jiandao/", StringComparison.Ordinal) ||
                                  entry.Source.Contains("/Hero/Sequence/Zhadan/", StringComparison.Ordinal), "HeroScissorsBomb");
        }

        private static void ExecuteSceneBatch()
        {
            ExecuteBatch(entry => entry.Source.StartsWith(Root + "/Scene/", StringComparison.Ordinal), "Scenes");
        }

        [MenuItem("Tools/ColorTiming/Art Naming/4a. Execute Scene Boss1 Grass01-03 Batch")]
        private static void ExecuteSceneBoss1GrassEarlyBatch()
        {
            ExecuteBatch(entry => Regex.IsMatch(entry.Source, @"/Scene/B1/a[1-3](?:/|$)"), "SceneBoss1Grass01To03");
        }

        [MenuItem("Tools/ColorTiming/Art Naming/4b. Execute Scene Boss1 Grass04-06 Batch")]
        private static void ExecuteSceneBoss1GrassLateBatch()
        {
            ExecuteBatch(entry => Regex.IsMatch(entry.Source, @"/Scene/B1/a[4-6](?:/|$)"), "SceneBoss1Grass04To06");
        }

        [MenuItem("Tools/ColorTiming/Art Naming/4c. Execute Scene Remaining Batch")]
        private static void ExecuteSceneRemainingBatch()
        {
            ExecuteBatch(entry => entry.Source.StartsWith(Root + "/Scene/", StringComparison.Ordinal) &&
                                  !Regex.IsMatch(entry.Source, @"/Scene/B1/a[1-6](?:/|$)"), "SceneRemaining");
        }

        [MenuItem("Tools/ColorTiming/Art Naming/5. Execute Shared UI Batch")]
        private static void ExecuteSharedBatch()
        {
            ExecuteBatch(entry => !entry.Source.StartsWith(Root + "/Boss/", StringComparison.Ordinal) &&
                                  !entry.Source.StartsWith(Root + "/Boss2/", StringComparison.Ordinal) &&
                                  !entry.Source.StartsWith(Root + "/Hero/", StringComparison.Ordinal) &&
                                  !entry.Source.StartsWith(Root + "/Scene/", StringComparison.Ordinal), "SharedUI");
        }

        [MenuItem("Tools/ColorTiming/Art Naming/6. Cleanup Approved References")]
        private static void CleanupApprovedReferences()
        {
            const string boss2ScenePath = "Assets/Game/Scene/Boss2.unity";
            const string duplicateBossSpritePath = Root + "/Boss/第二关BOSS拆分3.png";
            RemoveBoss2DuplicateReferenceObject(boss2ScenePath, duplicateBossSpritePath);

            var deleted = new List<string>();
            foreach (string path in ApprovedDeletePaths)
            {
                if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path)))
                {
                    continue;
                }

                if (!AssetDatabase.DeleteAsset(path))
                {
                    throw new InvalidOperationException("删除已批准参考资源失败：" + path);
                }
                deleted.Add(path);
            }

            int emptyDirectoryCount = DeleteEmptyDirectories();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            string reportPath = EvidenceDirectory + "/approved-reference-cleanup.md";
            var report = new StringBuilder();
            report.AppendLine("# 已批准参考资源清理").AppendLine();
            report.AppendLine($"- 完成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine("- Boss2 场景：已移除默认关闭且只引用重复合成图的 `Square` 调试对象");
            report.AppendLine($"- 删除资源：{deleted.Count}");
            foreach (string path in deleted) report.AppendLine("  - `" + path + "`");
            report.AppendLine($"- 删除迁移遗留空目录：{emptyDirectoryCount}");
            File.WriteAllText(reportPath, report.ToString(), new UTF8Encoding(true));
            Debug.Log($"[ColorTimingArtNaming] 清理完成：删除 {deleted.Count} 个已批准资源和 {emptyDirectoryCount} 个空目录。");
        }

        [MenuItem("Tools/ColorTiming/Art Naming/7. Normalize UI Directory Case")]
        private static void NormalizeUiDirectoryCase()
        {
            string source = Root + "/ui";
            string temporary = Root + "/UiCaseMigrationTemp";
            string target = Root + "/UI";
            string originalGuid = AssetDatabase.AssetPathToGUID(source);
            if (string.IsNullOrEmpty(originalGuid))
            {
                throw new InvalidOperationException("找不到待修正大小写的 ui 目录。");
            }

            string firstError = AssetDatabase.MoveAsset(source, temporary);
            if (!string.IsNullOrEmpty(firstError))
            {
                throw new InvalidOperationException("ui 临时目录迁移失败：" + firstError);
            }

            string secondError = AssetDatabase.MoveAsset(temporary, target);
            if (!string.IsNullOrEmpty(secondError))
            {
                AssetDatabase.MoveAsset(temporary, source);
                throw new InvalidOperationException("UI 目录大小写修正失败：" + secondError);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            string actualGuid = AssetDatabase.AssetPathToGUID(target);
            if (actualGuid != originalGuid)
            {
                throw new InvalidOperationException($"UI 目录 GUID 发生变化：{originalGuid} -> {actualGuid}");
            }

            Debug.Log("[ColorTimingArtNaming] ui -> UI 大小写修正完成，目录 GUID 保持一致。");
        }

        private static void RemoveBoss2DuplicateReferenceObject(string scenePath, string spritePath)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool openedForCleanup = !scene.IsValid() || !scene.isLoaded;
            if (!openedForCleanup && scene.isDirty)
            {
                throw new InvalidOperationException("Boss2 场景已有未保存修改，停止清理以避免覆盖用户工作。");
            }

            if (openedForCleanup)
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }

            try
            {
                Sprite duplicateSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (duplicateSprite == null)
                {
                    throw new InvalidOperationException("无法加载待清理的 Boss2 重复合成图：" + spritePath);
                }

                SpriteRenderer[] matches = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<SpriteRenderer>(true))
                    .Where(renderer => renderer.sprite == duplicateSprite)
                    .ToArray();
                if (matches.Length != 1)
                {
                    throw new InvalidOperationException($"Boss2 重复合成图引用对象应为 1 个，实际为 {matches.Length} 个。");
                }

                GameObject target = matches[0].gameObject;
                Component[] components = target.GetComponents<Component>();
                bool expectedShape = target.name == "Square" && !target.activeSelf &&
                                     components.All(component => component is Transform || component is SpriteRenderer);
                if (!expectedShape)
                {
                    throw new InvalidOperationException(
                        $"Boss2 重复图引用对象结构与审计不一致，停止删除：{target.name}，activeSelf={target.activeSelf}。");
                }

                UnityEngine.Object.DestroyImmediate(target);
                if (!EditorSceneManager.SaveScene(scene))
                {
                    throw new InvalidOperationException("保存 Boss2 场景失败。");
                }
            }
            finally
            {
                if (openedForCleanup && scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static int DeleteEmptyDirectories()
        {
            int deleted = 0;
            bool changed;
            do
            {
                changed = false;
                string[] directories = Directory.GetDirectories(Root, "*", SearchOption.AllDirectories)
                    .Select(path => path.Replace('\\', '/'))
                    .OrderByDescending(path => path.Count(character => character == '/'))
                    .ToArray();
                foreach (string directory in directories)
                {
                    if (Directory.EnumerateFileSystemEntries(directory).Any()) continue;
                    if (!AssetDatabase.DeleteAsset(directory)) continue;
                    deleted++;
                    changed = true;
                }
            } while (changed);

            return deleted;
        }

        private static void ExecuteBatch(Func<Entry, bool> predicate, string label)
        {
            List<Entry> allEntries = ReadPreview();
            ValidatePlanOrThrow(allEntries);
            List<Entry> entries = allEntries
                .Where(entry => entry.Action == "Move" && predicate(entry))
                .Where(entry => AssetDatabase.AssetPathToGUID(entry.Source) == entry.Guid)
                .ToList();

            if (entries.Count == 0)
            {
                Debug.Log($"[ColorTimingArtNaming] {label} 批次无需执行，资源已迁移或批次为空。");
                return;
            }

            Dictionary<string, string> hashes = CaptureArtHashes(entries);
            AssetDatabase.StartAssetEditing();
            try
            {
                EnsureTargetDirectories(entries);
                foreach (Entry entry in entries)
                {
                    string error = AssetDatabase.MoveAsset(entry.Source, entry.Target);
                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new InvalidOperationException(
                            $"[{label}] 资源移动失败：{entry.Source} -> {entry.Target}\n{error}");
                    }
                }

                if (label == "Boss")
                {
                    UpdateMovedSpineAtlasPages(allEntries);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }

            ValidateGuids(entries);
            ValidateArtHashes(entries, hashes);
            AppendBatchReport(label, entries.Count);
            Debug.Log($"[ColorTimingArtNaming] {label} 批次完成：{entries.Count} 个资源，GUID 与图片字节校验通过。");
        }

        private static List<Entry> ReadPreview()
        {
            if (!File.Exists(PreviewPath))
            {
                throw new InvalidOperationException("缺少迁移预览，请先执行 1. Preview Migration。");
            }

            var entries = new List<Entry>();
            foreach (string line in File.ReadLines(PreviewPath).Skip(1))
            {
                MatchCollection fields = Regex.Matches(line, "\"((?:\"\"|[^\"])*)\"");
                if (fields.Count != 5)
                {
                    throw new InvalidOperationException("无法解析迁移预览行：" + line);
                }

                string Value(int index) => fields[index].Groups[1].Value.Replace("\"\"", "\"");
                entries.Add(new Entry
                {
                    Action = Value(0), Guid = Value(1), Source = Value(2), Target = Value(3), Note = Value(4),
                });
            }

            return entries;
        }

        private static List<Entry> BuildPlan()
        {
            string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { Root });
            var entries = new List<Entry>(guids.Length);

            foreach (string guid in guids)
            {
                string source = AssetDatabase.GUIDToAssetPath(guid).Replace('\\', '/');
                if (AssetDatabase.IsValidFolder(source) || source.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (ApprovedDeletePaths.Contains(source, StringComparer.Ordinal))
                {
                    entries.Add(new Entry { Source = source, Guid = guid, Action = "Delete", Note = "用户确认的未使用制作参考资源" });
                    continue;
                }

                string relative = source.Substring(Root.Length + 1);
                string directory = Path.GetDirectoryName(relative)?.Replace('\\', '/') ?? string.Empty;
                string targetDirectory = GetTargetDirectory(relative, directory);
                string targetName = GetTargetName(relative, directory, source);
                string target = Root + "/" + targetDirectory + "/" + targetName;

                entries.Add(new Entry
                {
                    Source = source,
                    Guid = guid,
                    Target = target,
                    Action = source == target ? "Keep" : "Move",
                    Note = source == Root + "/Hero/白色.png" ? "用户确认保持原名" : string.Empty,
                });
            }

            entries.Sort((left, right) => string.CompareOrdinal(left.Source, right.Source));
            return entries;
        }

        private static string GetTargetDirectory(string relative, string directory)
        {
            if (relative == "Hero/Anim/HeroTest.controller" ||
                relative == "Hero/Anim/t.anim" ||
                relative == "Hero/Anim/tttttt.controller")
            {
                return "Hero/Tests";
            }

            if (relative == "System/游戏logo.png")
            {
                return "UI/MainMenu";
            }

            if (directory == "Hero")
            {
                return "Hero";
            }

            if (!DirectoryMap.TryGetValue(directory, out string target))
            {
                throw new InvalidOperationException($"未分类目录：{directory}（资源 {relative}）");
            }

            return target;
        }

        private static string GetTargetName(string relative, string directory, string source)
        {
            string fileName = Path.GetFileName(relative);
            string extension = GetCompoundExtension(fileName);
            string stem = fileName.Substring(0, fileName.Length - extension.Length);

            if (source == Root + "/Hero/白色.png")
            {
                return fileName;
            }

            if (TechnicalExactNames.TryGetValue(relative, out string exactName))
            {
                return exactName;
            }

            if (ArtExactNames.TryGetValue(relative, out exactName))
            {
                return exactName;
            }

            if (SpineBaseNames.TryGetValue(directory, out string spineBase) && IsSpineAsset(stem, extension))
            {
                return BuildSpineName(spineBase, stem, extension);
            }

            if (extension == ".anim" || extension == ".controller")
            {
                return BuildTechnicalAnimationName(directory, stem, extension);
            }

            if (extension == ".asset" || extension == ".mat" || extension == ".json" || extension == ".atlas.txt")
            {
                throw new InvalidOperationException($"未分类技术资源：{relative}");
            }

            if (extension != ".png" && extension != ".jpg")
            {
                throw new InvalidOperationException($"未分类扩展名：{relative}");
            }

            return BuildArtName(directory, stem, extension);
        }

        private static string BuildSpineName(string baseName, string stem, string extension)
        {
            if (extension == ".atlas.txt") return baseName + extension;
            if (extension == ".json") return baseName + extension;
            if (extension == ".png")
            {
                Match page = Regex.Match(stem, @"(\d*)$");
                return page.Groups[1].Length == 0
                    ? baseName + extension
                    : baseName + "_Page" + int.Parse(page.Groups[1].Value, CultureInfo.InvariantCulture).ToString("00") + extension;
            }

            if (stem.EndsWith("_Atlas", StringComparison.OrdinalIgnoreCase)) return baseName + "_Atlas" + extension;
            if (stem.EndsWith("_SkeletonData", StringComparison.OrdinalIgnoreCase)) return baseName + "_SkeletonData" + extension;
            if (stem.IndexOf("InsideMask", StringComparison.OrdinalIgnoreCase) >= 0) return baseName + "_Material_InsideMask" + extension;
            if (stem.IndexOf("OutsideMask", StringComparison.OrdinalIgnoreCase) >= 0) return baseName + "_Material_OutsideMask" + extension;

            Match material = Regex.Match(stem, @"(?:Material|BOSS拆分)(\d*)$", RegexOptions.IgnoreCase);
            if (extension == ".mat" && material.Success)
            {
                return material.Groups[1].Length == 0
                    ? baseName + "_Material" + extension
                    : baseName + "_Material" + int.Parse(material.Groups[1].Value, CultureInfo.InvariantCulture).ToString("00") + extension;
            }

            throw new InvalidOperationException($"无法确定 Spine 资源后缀：{stem}{extension}");
        }

        private static bool IsSpineAsset(string stem, string extension)
        {
            if (extension == ".asset" || extension == ".mat" || extension == ".json" || extension == ".atlas.txt")
            {
                return true;
            }

            return extension == ".png" &&
                   (stem.StartsWith("BOSS拆分", StringComparison.Ordinal) ||
                    stem.StartsWith("第二章boss", StringComparison.OrdinalIgnoreCase) ||
                    stem.StartsWith("skeleton", StringComparison.OrdinalIgnoreCase));
        }

        private static string BuildTechnicalAnimationName(string directory, string stem, string extension)
        {
            if (directory.StartsWith("Scene/B1/a", StringComparison.Ordinal))
            {
                Match grass = Regex.Match(directory, @"Scene/B1/a(?<index>[1-6])");
                string index = int.Parse(grass.Groups["index"].Value, CultureInfo.InvariantCulture).ToString("00");
                string state = directory == "Scene/B1/a1" && extension == ".anim" ? "Idle" :
                    stem.EndsWith("_c", StringComparison.OrdinalIgnoreCase) ? "Motion" :
                    stem.EndsWith("_dj", StringComparison.OrdinalIgnoreCase) ? "Idle" : "Controller";
                if (extension == ".controller") state = "Controller";
                return "Boss1_Grass" + index + "_" + state + extension;
            }

            Match planeSkill = Regex.Match(stem, @"^feiji_(chen|hong|lv|zi)( 1)?$", RegexOptions.IgnoreCase);
            if (directory == "Hero/Sequence/feiji/skill" && planeSkill.Success)
            {
                string color = TokenTranslations[planeSkill.Groups[1].Value];
                string state = planeSkill.Groups[2].Success ? "Explosion" : "Airborne";
                return $"Hero_Plane_{state}_{color}{extension}";
            }

            Match bombSkill = Regex.Match(stem, @"^zhadan_(hong|lv|zi)( 1)?$", RegexOptions.IgnoreCase);
            if (directory == "Hero/Sequence/Zhadan/skill" && bombSkill.Success)
            {
                string color = TokenTranslations[bombSkill.Groups[1].Value];
                string state = bombSkill.Groups[2].Success ? "Explosion" : "Airborne";
                return $"Hero_Bomb_{state}_{color}{extension}";
            }

            if (directory == "Hero/Sequence/Yidong" || directory == "Hero/Sequence/Yidong/Beimian")
            {
                string color = stem.IndexOf("Hong", StringComparison.OrdinalIgnoreCase) >= 0 ? "Red" :
                    stem.IndexOf("Lv", StringComparison.OrdinalIgnoreCase) >= 0 ? "Green" :
                    stem.IndexOf("Zi", StringComparison.OrdinalIgnoreCase) >= 0 ? "Purple" : "Orange";
                string facing = directory.EndsWith("Beimian", StringComparison.Ordinal) ? "Back" : "Front";
                return $"Hero_Move_{facing}_{color}{extension}";
            }

            string weapon = directory.Contains("/Chuizi/", StringComparison.Ordinal) ? "Hammer" :
                directory.Contains("/Dao/", StringComparison.Ordinal) ? "RingBlade" :
                directory.Contains("/feiji/", StringComparison.Ordinal) ? "Plane" :
                directory.Contains("/futou/", StringComparison.Ordinal) ? "Axe" :
                directory.Contains("/jiandao/", StringComparison.Ordinal) ? "Scissors" :
                directory.Contains("/Zhadan/", StringComparison.Ordinal) ? "Bomb" : null;
            if (!string.IsNullOrEmpty(weapon))
            {
                string color = stem.IndexOf("hong", StringComparison.OrdinalIgnoreCase) >= 0 ? "Red" :
                    stem.IndexOf("lv", StringComparison.OrdinalIgnoreCase) >= 0 ? "Green" :
                    stem.IndexOf("zi", StringComparison.OrdinalIgnoreCase) >= 0 || stem.IndexOf("zhi", StringComparison.OrdinalIgnoreCase) >= 0 ? "Purple" :
                    stem.IndexOf("chen", StringComparison.OrdinalIgnoreCase) >= 0 ? "Orange" : null;
                string action = directory.EndsWith("Daiji", StringComparison.OrdinalIgnoreCase) || directory.EndsWith("daiji", StringComparison.OrdinalIgnoreCase) ? "Idle" :
                    directory.EndsWith("yidong", StringComparison.OrdinalIgnoreCase) ? "Move" : "Attack";
                if (stem.StartsWith("xuli", StringComparison.OrdinalIgnoreCase))
                {
                    action = stem.StartsWith("xuliw", StringComparison.OrdinalIgnoreCase) ? "ChargeComplete" : "Charge";
                    action += stem.IndexOf("_do", StringComparison.OrdinalIgnoreCase) >= 0 ? "_Moving" : "_Stationary";
                }

                return $"Hero_{weapon}_{action}{(string.IsNullOrEmpty(color) ? string.Empty : "_" + color)}{extension}";
            }

            string normalized = stem;
            normalized = Regex.Replace(normalized, @"[^A-Za-z0-9_]+", "_");
            string[] tokens = normalized.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            var translated = new List<string>(tokens.Length + 1) { "Hero" };
            foreach (string token in tokens)
            {
                translated.Add(TokenTranslations.TryGetValue(token, out string value) ? value : token);
            }

            return string.Join("_", translated.DistinctConsecutive()) + extension;
        }

        private static string BuildArtName(string directory, string stem, string extension)
        {
            if (directory == "ui" && Regex.IsMatch(stem, "^[rgpo][123]$", RegexOptions.IgnoreCase))
            {
                return NormalizeBattleUi(stem) + extension.ToLowerInvariant();
            }

            if (directory == "Weapon")
            {
                Match icon = Regex.Match(stem, @"^(锤|刀|飞机|斧|剪|炸)([1-4])(亮|带白边)?$");
                if (icon.Success)
                {
                    string weapon = icon.Groups[1].Value switch
                    {
                        "锤" => "锤子", "刀" => "戒刀", "飞机" => "飞机", "斧" => "斧头",
                        "剪" => "剪刀", "炸" => "炸弹", _ => throw new InvalidOperationException("未知武器图标：" + stem),
                    };
                    string state = icon.Groups[3].Success ? "选中" : "默认";
                    return $"武器图标_{weapon}_{int.Parse(icon.Groups[2].Value):00}_{state}{extension.ToLowerInvariant()}";
                }
            }

            Match sequence = Regex.Match(stem, @"^(?<prefix>.*?)(?:[_ -]?)(?<frame>\d+)$");
            string prefix = sequence.Success ? sequence.Groups["prefix"].Value : stem;
            string frame = sequence.Success
                ? "_" + int.Parse(sequence.Groups["frame"].Value, CultureInfo.InvariantCulture).ToString("0000")
                : string.Empty;

            prefix = GetContextualPrefix(directory, prefix);
            return prefix + frame + extension.ToLowerInvariant();
        }

        private static string GetContextualPrefix(string directory, string prefix)
        {
            if (directory == "Boss/Skill5") return "首领1_第五招_尖刺特效";
            if (directory == "Boss/tip") return "首领1_" + Regex.Replace(NormalizeChinese(prefix), @"攻击_(\d+)范围", "攻击$1_范围");
            if (directory.StartsWith("Boss2/", StringComparison.Ordinal)) return "首领2_" + NormalizeChinese(prefix);
            if (directory.StartsWith("Hero/Sequence", StringComparison.Ordinal)) return "英雄_" + NormalizeHeroPrefix(directory, prefix);
            if (directory == "Loding") return "加载_" + NormalizeChinese(prefix.Replace("加载时", string.Empty));
            if (directory == "Mouse") return "光标_" + NormalizeChinese(prefix);
            if (directory.StartsWith("Scene/B1/a", StringComparison.Ordinal))
            {
                Match grass = Regex.Match(directory, @"Scene/B1/a(?<index>[1-6])");
                string state = directory.EndsWith("/c", StringComparison.Ordinal) ? "摆动" : "待机";
                return $"首领1场景_草丛{int.Parse(grass.Groups["index"].Value):00}_{state}";
            }
            if (directory == "Scene/B1/jianci") return prefix.StartsWith("b", StringComparison.OrdinalIgnoreCase)
                ? "首领1场景_尖刺" : "首领1场景_尖刺_" + NormalizeChinese(prefix);
            if (directory == "Scene/B1") return "首领1场景_" + NormalizeChinese(prefix);
            if (directory == "Scene/B2/cao") return "首领2场景_草丛_组" + NormalizeChinese(prefix);
            if (directory == "Scene/B2") return "首领2场景_" + NormalizeChinese(prefix);
            if (directory == "System") return "游戏_" + NormalizeChinese(prefix);
            if (directory == "ui") return NormalizeBattleUi(prefix);
            if (directory == "UI_ESC" || directory == "UI_Set" || directory == "UI_Start") return NormalizeMenuUi(prefix);
            if (directory == "Weapon") return "武器图标_" + NormalizeChinese(prefix);
            if (directory == "Weapon/Tip1") return "首领1_" + NormalizeChinese(prefix);
            if (directory == "Weapon/Tip2") return "首领2_" + NormalizeChinese(prefix);
            if (directory == "yin")
            {
                Match grassShadow = Regex.Match(prefix, @"^a([2-6])草投影$");
                return grassShadow.Success
                    ? $"投影_草丛{int.Parse(grassShadow.Groups[1].Value):00}"
                    : "投影_" + NormalizeChinese(prefix.Replace("投影", string.Empty));
            }

            throw new InvalidOperationException($"未分类美术名称：{directory}/{prefix}");
        }

        private static string NormalizeHeroPrefix(string directory, string prefix)
        {
            if (directory.Contains("/Dao/", StringComparison.Ordinal)) prefix = prefix.Replace("刀", "戒刀");
            string normalized = NormalizeChinese(prefix).Replace("攻击动作", "攻击");
            string weapon = directory.Contains("/Chuizi/", StringComparison.Ordinal) ? "锤子" :
                directory.Contains("/Dao/", StringComparison.Ordinal) ? "戒刀" :
                directory.Contains("/feiji", StringComparison.Ordinal) ? "飞机" :
                directory.Contains("/futou/", StringComparison.Ordinal) ? "斧头" :
                directory.Contains("/jiandao/", StringComparison.Ordinal) ? "剪刀" :
                directory.Contains("/Zhadan/", StringComparison.Ordinal) ? "炸弹" : null;
            if (!string.IsNullOrEmpty(weapon) && !normalized.Split('_').Contains(weapon))
            {
                normalized = weapon + "_" + normalized;
            }

            return normalized;
        }

        private static string NormalizeBattleUi(string prefix)
        {
            if (Regex.IsMatch(prefix, "^[rgpo][123]$", RegexOptions.IgnoreCase))
            {
                string color = prefix[0] switch
                {
                    'r' => "红色", 'g' => "绿色", 'p' => "紫色", 'o' => "橙色", _ => "未知",
                };
                string position = prefix[1] switch
                {
                    '1' => "中段", '2' => "起始", '3' => "末端", _ => "未知",
                };
                return $"首领弱点格_{color}_{position}";
            }

            if (prefix == "摆放") return "首领名称横幅";
            return "战斗界面_" + NormalizeChinese(prefix);
        }

        private static string NormalizeMenuUi(string prefix)
        {
            prefix = prefix.Replace("开始提示弹窗", "开启提示弹窗");
            string normalized = NormalizeChinese(prefix);
            if (normalized.Contains("正常") || normalized.Contains("悬停") || normalized.Contains("按下"))
            {
                return "按钮_" + normalized;
            }

            return normalized;
        }

        private static string NormalizeChinese(string value)
        {
            string normalized = value
                .Replace("（红）", "_红色").Replace("（绿）", "_绿色")
                .Replace("（紫）", "_紫色").Replace("（橙）", "_橙色")
                .Replace("(红)", "_红色").Replace("(绿)", "_绿色")
                .Replace("(紫)", "_紫色").Replace("(橙)", "_橙色")
                .Replace("（背面）", "_背面").Replace("（移动）", "_移动")
                .Replace("（原地）", "_原地").Replace("(原地）", "_原地")
                .Replace("（空中）", "_空中").Replace("（注意前后）", "_制作参考_前后层级")
                .Replace("（", "_").Replace("）", "_").Replace("(", "_").Replace(")", "_")
                .Replace("-", "_").Replace(" ", "_").Replace("、", "_")
                .Replace("BOSS", "首领").Replace("boss", "首领");

            string[] semanticTokens =
            {
                "红色", "绿色", "紫色", "橙色", "锤子", "戒刀", "飞机", "斧头", "剪刀", "炸弹",
                "待机", "移动", "攻击", "蓄力", "完成", "原地", "投掷", "空中", "爆炸", "特效", "受击",
                "震动", "曝光", "背面", "黑屏", "转场", "背景", "操作提示", "正常", "悬停", "按下",
            };
            foreach (string token in semanticTokens.OrderByDescending(token => token.Length))
            {
                normalized = normalized.Replace(token, "_" + token + "_");
            }

            normalized = Regex.Replace(normalized, "_+", "_").Trim('_');

            string[] colors = { "红色", "绿色", "紫色", "橙色" };
            string color = colors.FirstOrDefault(item => normalized.Split('_').Contains(item));
            if (!string.IsNullOrEmpty(color))
            {
                normalized = string.Join("_", normalized.Split('_').Where(item => item != color));
                normalized += "_" + color;
            }

            return normalized;
        }

        private static string GetCompoundExtension(string fileName)
        {
            return fileName.EndsWith(".atlas.txt", StringComparison.OrdinalIgnoreCase)
                ? ".atlas.txt"
                : Path.GetExtension(fileName);
        }

        private static void ValidatePlanOrThrow(IReadOnlyCollection<Entry> entries)
        {
            var errors = new List<string>();
            foreach (IGrouping<string, Entry> duplicate in entries
                         .Where(item => item.Action == "Move")
                         .GroupBy(item => item.Target, StringComparer.OrdinalIgnoreCase)
                         .Where(group => group.Count() > 1))
            {
                errors.Add("目标冲突：" + duplicate.Key + " <= " + string.Join(", ", duplicate.Select(item => item.Source)));
            }

            foreach (Entry entry in entries.Where(item => item.Action == "Move"))
            {
                string existingGuid = AssetDatabase.AssetPathToGUID(entry.Target);
                if (!string.IsNullOrEmpty(existingGuid) && existingGuid != entry.Guid)
                {
                    errors.Add($"目标已存在：{entry.Target}（{existingGuid}）");
                }
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException("迁移计划校验失败：\n" + string.Join("\n", errors));
            }
        }

        private static void WritePreview(List<Entry> entries)
        {
            ValidatePlanOrThrow(entries);
            Directory.CreateDirectory(EvidenceDirectory);
            var builder = new StringBuilder("Action,GUID,Source,Target,Note\n");
            foreach (Entry entry in entries)
            {
                builder.Append(Csv(entry.Action)).Append(',').Append(Csv(entry.Guid)).Append(',')
                    .Append(Csv(entry.Source)).Append(',').Append(Csv(entry.Target)).Append(',')
                    .Append(Csv(entry.Note)).Append('\n');
            }
            File.WriteAllText(PreviewPath, builder.ToString(), new UTF8Encoding(true));
        }

        private static void ReportPlan(IReadOnlyCollection<Entry> entries)
        {
            Debug.Log($"[ColorTimingArtNaming] 预览通过：Move={entries.Count(item => item.Action == "Move")}, " +
                      $"Keep={entries.Count(item => item.Action == "Keep")}, " +
                      $"Delete={entries.Count(item => item.Action == "Delete")}。清单：{PreviewPath}");
        }

        private static void EnsureTargetDirectories(IEnumerable<Entry> entries)
        {
            foreach (string targetDirectory in entries.Where(item => item.Action == "Move")
                         .Select(item => Path.GetDirectoryName(item.Target)?.Replace('\\', '/'))
                         .Where(item => !string.IsNullOrEmpty(item))
                         .Distinct(StringComparer.Ordinal)
                         .OrderBy(item => item.Count(character => character == '/')))
            {
                EnsureDirectory(targetDirectory);
            }
        }

        private static void UpdateMovedSpineAtlasPages(IReadOnlyCollection<Entry> entries)
        {
            foreach (Entry atlas in entries.Where(item => item.Action == "Move" &&
                                                           item.Source.EndsWith(".atlas.txt", StringComparison.OrdinalIgnoreCase)))
            {
                string sourceDirectory = Path.GetDirectoryName(atlas.Source)?.Replace('\\', '/');
                string text = File.ReadAllText(atlas.Target, Encoding.UTF8);
                foreach (Entry page in entries.Where(item => item.Action == "Move" &&
                                                              Path.GetDirectoryName(item.Source)?.Replace('\\', '/') == sourceDirectory &&
                                                              item.Source.EndsWith(".png", StringComparison.OrdinalIgnoreCase)))
                {
                    text = text.Replace(Path.GetFileName(page.Source), Path.GetFileName(page.Target));
                }

                File.WriteAllText(atlas.Target, text, new UTF8Encoding(false));
            }
        }

        private static void EnsureDirectory(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;
            string parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(parent)) throw new InvalidOperationException("无效目标目录：" + assetPath);
            EnsureDirectory(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(assetPath));
        }

        private static Dictionary<string, string> CaptureArtHashes(IEnumerable<Entry> entries)
        {
            return entries.Where(item => item.Action == "Move" &&
                                         (item.Source.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                          item.Source.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)))
                .ToDictionary(item => item.Guid, item => ComputeSha256(item.Source), StringComparer.Ordinal);
        }

        private static string ComputeSha256(string assetPath)
        {
            using var stream = File.OpenRead(assetPath);
            using var sha = System.Security.Cryptography.SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }

        private static void ValidateGuids(IEnumerable<Entry> entries)
        {
            foreach (Entry entry in entries.Where(item => item.Action == "Move"))
            {
                string actual = AssetDatabase.AssetPathToGUID(entry.Target);
                if (actual != entry.Guid)
                {
                    throw new InvalidOperationException($"GUID 校验失败：{entry.Target}，期望 {entry.Guid}，实际 {actual}");
                }
            }
        }

        private static void ValidateArtHashes(IEnumerable<Entry> entries, IReadOnlyDictionary<string, string> hashes)
        {
            foreach (Entry entry in entries.Where(item => item.Action == "Move" && hashes.ContainsKey(item.Guid)))
            {
                string actual = ComputeSha256(entry.Target);
                if (actual != hashes[entry.Guid])
                {
                    throw new InvalidOperationException($"图片字节发生变化：{entry.Target}");
                }
            }
        }

        private static void WriteExecutionReport(IEnumerable<Entry> entries)
        {
            string reportPath = EvidenceDirectory + "/asset-migration-result.md";
            var builder = new StringBuilder();
            builder.AppendLine("# 美术资源命名迁移结果");
            builder.AppendLine();
            builder.AppendLine($"- 完成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            builder.AppendLine($"- 已移动：{entries.Count(item => item.Action == "Move")}");
            builder.AppendLine($"- 保持不变：{entries.Count(item => item.Action == "Keep")}");
            builder.AppendLine("- GUID：全部保持一致");
            builder.AppendLine("- PNG/JPG 字节哈希：全部保持一致");
            builder.AppendLine("- 待清理参考资源：由独立清理步骤处理，未在本迁移菜单删除");
            File.WriteAllText(reportPath, builder.ToString(), new UTF8Encoding(true));
        }

        private static void AppendBatchReport(string label, int count)
        {
            string reportPath = EvidenceDirectory + "/asset-migration-batches.md";
            if (!File.Exists(reportPath))
            {
                File.WriteAllText(reportPath,
                    "# 美术资源命名迁移批次记录\n\n",
                    new UTF8Encoding(true));
            }

            File.AppendAllText(reportPath,
                $"- {DateTime.Now:yyyy-MM-dd HH:mm:ss}：`{label}`，移动 {count} 项，GUID 与 PNG/JPG SHA-256 校验通过。\n",
                new UTF8Encoding(false));
        }

        private static string Csv(string value)
        {
            value ??= string.Empty;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private static IEnumerable<string> DistinctConsecutive(this IEnumerable<string> values)
        {
            string previous = null;
            foreach (string value in values)
            {
                if (value == previous) continue;
                previous = value;
                yield return value;
            }
        }
    }
}
