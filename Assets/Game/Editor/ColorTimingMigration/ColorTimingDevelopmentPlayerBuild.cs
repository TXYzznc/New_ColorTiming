// 文件职责：使用现有 Jenkins 构建链路生成 Windows Development Player，供性能验收使用。
// 所属模块：ColorTiming / Editor / Migration。

using System;
using System.IO;
using UGF.EditorTools;
using UnityEditor;
using UnityEngine;

namespace ColorTiming.Editor.Migration
{
    internal static class ColorTimingDevelopmentPlayerBuild
    {
        private const string BuildAppConfigRelativePath = "Tools/Jenkins/BuildAppConfig.json";

        [MenuItem("Game Framework/GameTools/ColorTiming/Performance/Build Windows Development Player", false, 1017)]
        private static void BuildWindowsDevelopmentPlayer()
        {
            string projectRoot = Directory.GetParent(global::UnityEngine.Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Could not resolve project root.");
            string configPath = Path.Combine(projectRoot, BuildAppConfigRelativePath);
            string originalConfig = File.ReadAllText(configPath);

            try
            {
                const string developmentBuildDisabled = "\"DevelopmentBuild\": false";
                const string developmentBuildEnabled = "\"DevelopmentBuild\": true";
                if (!originalConfig.Contains(developmentBuildDisabled))
                    throw new InvalidOperationException("BuildAppConfig.json must explicitly disable DevelopmentBuild before this performance build.");

                File.WriteAllText(configPath, originalConfig.Replace(developmentBuildDisabled, developmentBuildEnabled));
                JenkinsBuilder.BuildApp();
            }
            finally
            {
                File.WriteAllText(configPath, originalConfig);
            }
        }
    }
}
