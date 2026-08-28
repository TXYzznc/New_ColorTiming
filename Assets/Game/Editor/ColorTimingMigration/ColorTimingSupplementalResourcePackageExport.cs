using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ColorTiming.Editor
{
    /// <summary>Exports the supplementary non-art resource group configured in ResourceExportSettings.</summary>
    public static class ColorTimingSupplementalResourcePackageExport
    {
        private const string SettingsPath = "Assets/Game/ScriptsBuiltin/Editor/MigratedToolbox/ResourceExportSettings.asset";
        private const string GroupName = "项目补充资源_非已导出美术";
        private const string PackageRelativePath = "Exports/ColorTiming_项目补充资源_非已导出美术.unitypackage";

        [MenuItem("Game Framework/GameTools/Export ColorTiming Supplemental Resource Package", false, 1010)]
        public static void Export()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ResourceExportSettings>(SettingsPath);
            if (settings == null)
            {
                throw new InvalidOperationException($"Resource export settings are missing: {SettingsPath}");
            }

            var group = settings.groups.SingleOrDefault(item => item != null && item.name == GroupName);
            if (group == null)
            {
                throw new InvalidOperationException($"Resource export group is missing: {GroupName}");
            }

            var paths = group.assetPaths
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Where(path => path.StartsWith("Assets/", StringComparison.Ordinal))
                .Where(path => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (paths.Length == 0)
            {
                throw new InvalidOperationException($"Resource export group has no valid paths: {GroupName}");
            }

            var projectRoot = Directory.GetParent(global::UnityEngine.Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Could not resolve the project root.");
            var packagePath = Path.Combine(projectRoot, PackageRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(packagePath) ?? projectRoot);
            AssetDatabase.ExportPackage(paths, packagePath,
                ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
            Debug.Log($"[ResourceExport] Exported group '{GroupName}' with {paths.Length} paths: {packagePath}");
        }
    }
}
