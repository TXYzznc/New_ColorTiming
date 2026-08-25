using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 按资源组导出 Unity .unitypackage，使用 Unity 原生 ExportPackage API。
/// </summary>
[ToolHubItem("资源工具/资源导出工具", "配置资源组并分别或合并导出 Unity Package", 20)]
public sealed class ResourceExportPanel : IToolHubPanel
{
    private const string SettingsPath = "Assets/Game/ScriptsBuiltin/Editor/MigratedToolbox/ResourceExportSettings.asset";
    private const float RowHeight = 22f;

    private ResourceExportSettings _settings;
    private Vector2 _scrollPosition;
    private bool _includeDependencies = true;
    private bool _includeAllScripts;
    private bool _mergeSelectedGroups;

    public void OnEnable()
    {
        _settings = LoadOrCreateSettings();
    }

    public void OnDisable()
    {
        SaveSettings();
    }

    public void OnDestroy()
    {
        SaveSettings();
    }

    public string GetHelpText()
    {
        return "按资源组配置路径并导出 .unitypackage，支持依赖项、脚本和合并导出。";
    }

    public void OnGUI()
    {
        if (_settings == null)
        {
            EditorGUILayout.HelpBox("资源导出配置无法加载。", MessageType.Error);
            return;
        }

        DrawToolbar();
        EditorGUILayout.Space(4f);
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        DrawGroups();
        EditorGUILayout.Space(8f);
        DrawExportOptions();
        EditorGUILayout.EndScrollView();
        DrawFooter();
    }

    private void DrawToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            EditorGUILayout.LabelField("配置", GUILayout.Width(38f));
            var newSettings = EditorGUILayout.ObjectField(
                _settings,
                typeof(ResourceExportSettings),
                false,
                GUILayout.MinWidth(180f)
            ) as ResourceExportSettings;

            if (newSettings != null && newSettings != _settings)
            {
                _settings = newSettings;
                SaveSettings();
            }

            if (GUILayout.Button("新建配置", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                CreateSettingsAsset();

            if (GUILayout.Button("定位", EditorStyles.toolbarButton, GUILayout.Width(48f)))
            {
                Selection.activeObject = _settings;
                EditorGUIUtility.PingObject(_settings);
            }
        }
    }

    private void DrawGroups()
    {
        EditorGUILayout.LabelField("资源组", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("每个资源组可以添加文件或文件夹。选择文件夹时会按 Unity 原生导出规则递归包含其内容。", MessageType.Info);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("新增资源组", GUILayout.Width(100f)))
            {
                _settings.groups.Add(new ResourceExportSettings.ResourceGroup());
                SaveSettings();
            }

            using (new EditorGUI.DisabledScope(_settings.groups.Count == 0))
            {
                if (GUILayout.Button("全选", GUILayout.Width(60f)))
                    SetAllGroupsSelected(true);
                if (GUILayout.Button("全不选", GUILayout.Width(70f)))
                    SetAllGroupsSelected(false);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"共 {_settings.groups.Count} 组", EditorStyles.miniLabel, GUILayout.Width(70f));
        }

        if (_settings.groups.Count == 0)
        {
            EditorGUILayout.HelpBox("请先新增一个资源组。", MessageType.Warning);
            return;
        }

        for (int i = 0; i < _settings.groups.Count; i++)
            DrawGroup(i, _settings.groups[i]);
    }

    private void DrawGroup(int index, ResourceExportSettings.ResourceGroup group)
    {
        if (group == null)
            return;

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                group.selected = EditorGUILayout.Toggle(group.selected, GUILayout.Width(18f));
                group.name = EditorGUILayout.TextField(group.name, GUILayout.MinWidth(150f));
                EditorGUILayout.LabelField($"{group.assetPaths.Count} 个路径", EditorStyles.miniLabel, GUILayout.Width(70f));

                if (GUILayout.Button("添加路径", GUILayout.Width(70f)))
                    group.assetPaths.Add(string.Empty);

                GUI.color = new Color(1f, 0.65f, 0.65f);
                if (GUILayout.Button("删除组", GUILayout.Width(60f)))
                {
                    if (EditorUtility.DisplayDialog("删除资源组", $"确定删除资源组“{group.name}”吗？", "删除", "取消"))
                    {
                        _settings.groups.RemoveAt(index);
                        GUI.color = Color.white;
                        SaveSettings();
                        GUIUtility.ExitGUI();
                    }
                }
                GUI.color = Color.white;
            }

            if (string.IsNullOrWhiteSpace(group.name))
                EditorGUILayout.HelpBox("资源组名称不能为空。", MessageType.Warning);

            for (int pathIndex = 0; pathIndex < group.assetPaths.Count; pathIndex++)
                DrawPathRow(group, pathIndex);

            if (group.assetPaths.Count == 0)
                EditorGUILayout.HelpBox("请添加至少一个 Assets 下的文件或文件夹。", MessageType.Warning);
        }
    }

    private void DrawPathRow(ResourceExportSettings.ResourceGroup group, int pathIndex)
    {
        string path = group.assetPaths[pathIndex];
        UnityEngine.Object asset = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField($"路径 {pathIndex + 1}", GUILayout.Width(48f));
            var newAsset = EditorGUILayout.ObjectField(asset, typeof(UnityEngine.Object), false, GUILayout.MinWidth(180f));
            if (newAsset != asset)
            {
                string newPath = newAsset == null ? string.Empty : AssetDatabase.GetAssetPath(newAsset);
                if (newAsset != null && !newPath.StartsWith("Assets/", StringComparison.Ordinal))
                {
                    EditorUtility.DisplayDialog("路径无效", "只能选择 Assets 目录下的资源。", "确定");
                }
                else
                {
                    group.assetPaths[pathIndex] = newPath;
                    SaveSettings();
                }
            }

            EditorGUILayout.LabelField(string.IsNullOrEmpty(group.assetPaths[pathIndex]) ? "未设置" : group.assetPaths[pathIndex], EditorStyles.miniLabel, GUILayout.MinWidth(180f));
            if (GUILayout.Button("移除", GUILayout.Width(48f)))
            {
                group.assetPaths.RemoveAt(pathIndex);
                SaveSettings();
                GUIUtility.ExitGUI();
            }
        }
    }

    private void DrawExportOptions()
    {
        EditorGUILayout.LabelField("导出选项", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            _includeDependencies = EditorGUILayout.ToggleLeft("包括依赖项", _includeDependencies);
            _includeAllScripts = EditorGUILayout.ToggleLeft("Include all scripts", _includeAllScripts);
            _mergeSelectedGroups = EditorGUILayout.ToggleLeft("合并选中的资源组为一个包", _mergeSelectedGroups);
            EditorGUILayout.HelpBox(
                _mergeSelectedGroups
                    ? "将所有选中的资源组合并导出为一个 .unitypackage。"
                    : "每个选中的资源组分别导出为一个 .unitypackage。",
                MessageType.None
            );
        }
    }

    private void DrawFooter()
    {
        int selectedCount = _settings.groups.Count(group => group != null && group.selected);
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            EditorGUILayout.LabelField($"已选择 {selectedCount}/{_settings.groups.Count} 组", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            using (new EditorGUI.DisabledScope(selectedCount == 0))
            {
                if (GUILayout.Button(_mergeSelectedGroups ? "导出合并包" : "导出选中资源组", GUILayout.Height(RowHeight)))
                    ExportSelectedGroups();
            }
        }
    }

    private void ExportSelectedGroups()
    {
        var selectedGroups = _settings.groups
            .Where(group => group != null && group.selected)
            .ToList();

        var validGroups = selectedGroups
            .Where(group => !string.IsNullOrWhiteSpace(group.name) && GetValidPaths(group).Count > 0)
            .ToList();

        if (validGroups.Count == 0)
        {
            EditorUtility.DisplayDialog("无法导出", "选中的资源组没有有效名称或路径。", "确定");
            return;
        }

        if (validGroups.Count != selectedGroups.Count)
        {
            if (!EditorUtility.DisplayDialog("部分资源组无效", "部分选中的资源组没有有效内容，是否跳过它们继续导出？", "继续", "取消"))
                return;
        }

        if (_mergeSelectedGroups)
            ExportMerged(validGroups);
        else
            ExportSeparately(validGroups);
    }

    private void ExportMerged(List<ResourceExportSettings.ResourceGroup> groups)
    {
        string defaultName = SanitizeFileName(string.Join("_", groups.Select(group => group.name)));
        string outputPath = EditorUtility.SaveFilePanel("导出合并资源包", GetProjectRoot(), defaultName + ".unitypackage", "unitypackage");
        if (string.IsNullOrEmpty(outputPath))
            return;

        var paths = groups.SelectMany(GetValidPaths).Distinct(StringComparer.Ordinal).ToArray();
        ExportPackage(paths, outputPath);
    }

    private void ExportSeparately(List<ResourceExportSettings.ResourceGroup> groups)
    {
        string outputFolder = EditorUtility.OpenFolderPanel("选择资源包输出目录", GetProjectRoot(), "");
        if (string.IsNullOrEmpty(outputFolder))
            return;

        int exported = 0;
        foreach (var group in groups)
        {
            string fileName = SanitizeFileName(group.name) + ".unitypackage";
            string outputPath = Path.Combine(outputFolder, fileName);
            if (File.Exists(outputPath) && !EditorUtility.DisplayDialog("覆盖文件", $"文件已存在：\n{outputPath}\n是否覆盖？", "覆盖", "取消"))
                continue;

            if (ExportPackage(GetValidPaths(group).ToArray(), outputPath))
                exported++;
        }

        EditorUtility.DisplayDialog("导出完成", $"成功导出 {exported}/{groups.Count} 个资源包。\n{outputFolder}", "确定");
        EditorUtility.RevealInFinder(outputFolder);
    }

    private bool ExportPackage(string[] paths, string outputPath)
    {
        try
        {
            ExportPackageOptions options = ExportPackageOptions.Recurse;
            if (_includeDependencies)
                options |= ExportPackageOptions.IncludeDependencies;

            if (_includeAllScripts)
                paths = paths.Concat(FindAllScriptPaths()).Distinct(StringComparer.Ordinal).ToArray();

            AssetDatabase.ExportPackage(paths, outputPath, options);
            Debug.Log($"[ResourceExport] Package 导出成功: {outputPath}");
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"[ResourceExport] Package 导出失败: {outputPath}\n{exception}");
            EditorUtility.DisplayDialog("导出失败", exception.Message, "确定");
            return false;
        }
    }

    private static List<string> GetValidPaths(ResourceExportSettings.ResourceGroup group)
    {
        if (group == null || group.assetPaths == null)
            return new List<string>();

        return group.assetPaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Where(path => path.StartsWith("Assets/", StringComparison.Ordinal))
            .Where(path => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private void SetAllGroupsSelected(bool selected)
    {
        foreach (var group in _settings.groups)
        {
            if (group != null)
                group.selected = selected;
        }
        SaveSettings();
    }

    private ResourceExportSettings LoadOrCreateSettings()
    {
        var asset = AssetDatabase.LoadAssetAtPath<ResourceExportSettings>(SettingsPath);
        if (asset != null)
            return asset;

        EnsureFolderExists(Path.GetDirectoryName(SettingsPath).Replace("\\", "/"));
        asset = ScriptableObject.CreateInstance<ResourceExportSettings>();
        AssetDatabase.CreateAsset(asset, SettingsPath);
        AssetDatabase.SaveAssets();
        return asset;
    }

    private void CreateSettingsAsset()
    {
        string path = EditorUtility.SaveFilePanelInProject("新建资源导出配置", "ResourceExportSettings", "asset", "选择配置保存位置");
        if (string.IsNullOrEmpty(path))
            return;

        var asset = ScriptableObject.CreateInstance<ResourceExportSettings>();
        AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(path));
        AssetDatabase.SaveAssets();
        _settings = asset;
    }

    private void SaveSettings()
    {
        if (_settings == null)
            return;
        EditorUtility.SetDirty(_settings);
        AssetDatabase.SaveAssetIfDirty(_settings);
    }

    private static void EnsureFolderExists(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        string parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
        string name = Path.GetFileName(folderPath);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolderExists(parent);
        AssetDatabase.CreateFolder(string.IsNullOrEmpty(parent) ? "Assets" : parent, name);
    }

    private static string[] FindAllScriptPaths()
    {
        return AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => !string.IsNullOrEmpty(path))
            .ToArray();
    }

    private static string GetProjectRoot()
    {
        return Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
    }

    private static string SanitizeFileName(string value)
    {
        string result = string.IsNullOrWhiteSpace(value) ? "ResourcePackage" : value.Trim();
        foreach (char invalid in Path.GetInvalidFileNameChars())
            result = result.Replace(invalid.ToString(), "_");
        return result;
    }
}
