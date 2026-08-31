// 文件职责：在 Package Player 构建前同步 GF 资源包到 StreamingAssets。
// 所属模块：ColorTiming / Editor / Migration。

using System;
using System.IO;
using System.Linq;
using UGF.EditorTools;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ColorTiming.Editor.Migration
{
    internal sealed class ColorTimingPackageStreamingAssetsBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            string projectRoot = Directory.GetParent(global::UnityEngine.Application.dataPath)?.FullName
                ?? throw new BuildFailedException("Could not resolve project root.");
            string packageRoot = Path.Combine(projectRoot, AppBuildSettings.Instance.ResourceBuildDir, "Package");
            string versionDirectory = Directory.Exists(packageRoot)
                ? Directory.GetDirectories(packageRoot).OrderByDescending(Directory.GetLastWriteTimeUtc).FirstOrDefault()
                : null;
            string platformDirectory = versionDirectory == null
                ? null
                : Path.Combine(versionDirectory, GetPlatformDirectoryName(report.summary.platform));
            if (string.IsNullOrEmpty(platformDirectory) || !Directory.Exists(platformDirectory))
                throw new BuildFailedException($"GF package output is unavailable for '{report.summary.platform}': {platformDirectory}");

            int copiedCount = 0;
            foreach (string sourcePath in Directory.GetFiles(platformDirectory, "*", SearchOption.AllDirectories))
            {
                string relativePath = sourcePath.Substring(platformDirectory.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string destinationPath = Path.Combine(global::UnityEngine.Application.streamingAssetsPath, relativePath);
                string destinationDirectory = Path.GetDirectoryName(destinationPath)
                    ?? throw new BuildFailedException($"Could not resolve destination directory for '{destinationPath}'.");
                Directory.CreateDirectory(destinationDirectory);
                File.Copy(sourcePath, destinationPath, true);
                copiedCount++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[ColorTiming] GF Package resources synchronized to StreamingAssets: {copiedCount} files from '{platformDirectory}'.");
        }

        private static string GetPlatformDirectoryName(BuildTarget target)
        {
            return target switch
            {
                BuildTarget.StandaloneWindows => "Windows",
                BuildTarget.StandaloneWindows64 => "Windows64",
                BuildTarget.StandaloneOSX => "MacOS",
                BuildTarget.StandaloneLinux64 => "Linux",
                BuildTarget.Android => "Android",
                BuildTarget.iOS => "IOS",
                BuildTarget.WebGL => "WebGL",
                _ => throw new BuildFailedException($"Unsupported Package build target '{target}'."),
            };
        }
    }
}
