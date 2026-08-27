using System;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestTools;

namespace ColorTiming.Editor
{
    /// <summary>
    /// Persists Test Runner evidence outside the UnitySkills in-memory job store so
    /// PlayMode domain reloads cannot erase the authoritative result.
    /// </summary>
    [InitializeOnLoad]
    public static class ColorTimingTestEvidence
    {
        const string PlayModeAssembly = "ColorTiming.PlayMode.Tests";
        const string EditModeAssembly = "ColorTiming.EditMode.Tests";
        static readonly TestRunnerApi Api;

        static ColorTimingTestEvidence()
        {
            Api = ScriptableObject.CreateInstance<TestRunnerApi>();
            Api.RegisterCallbacks(new EvidenceCallbacks(), -100);
        }

        [MenuItem("Game Framework/GameTools/Run ColorTiming PlayMode Tests", false, 1004)]
        public static void RunPlayModeTests()
        {
            ColorTimingMigrationValidator.SyncFormalBuildScenes();
            Api.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.PlayMode,
                assemblyNames = new[] { PlayModeAssembly },
            }));
        }

        [MenuItem("Game Framework/GameTools/Run ColorTiming EditMode Tests", false, 1005)]
        public static void RunEditModeTests()
        {
            Api.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[] { EditModeAssembly },
            }));
        }

        sealed class EvidenceCallbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun)
            {
                var prefix = PrefixFor(testsToRun.TestMode);
                if (prefix == null)
                {
                    return;
                }

                WriteLog(prefix, $"status=running{Environment.NewLine}startedUtc={DateTime.UtcNow:O}{Environment.NewLine}");
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                var prefix = PrefixFor(result.Test.TestMode);
                if (prefix == null)
                {
                    return;
                }

                var xmlPath = OutputPath($"{prefix}-color-timing-latest.xml");
                Directory.CreateDirectory(Path.GetDirectoryName(xmlPath) ?? ProjectRoot());
                using (var writer = XmlWriter.Create(xmlPath, new XmlWriterSettings { Indent = true }))
                {
                    result.ToXml().WriteTo(writer);
                }

                var total = result.PassCount + result.FailCount + result.SkipCount + result.InconclusiveCount;
                var log = new StringBuilder()
                    .AppendLine("status=completed")
                    .AppendLine($"finishedUtc={DateTime.UtcNow:O}")
                    .AppendLine($"total={total}")
                    .AppendLine($"passed={result.PassCount}")
                    .AppendLine($"failed={result.FailCount}")
                    .AppendLine($"skipped={result.SkipCount}")
                    .AppendLine($"inconclusive={result.InconclusiveCount}")
                    .AppendLine($"durationSeconds={result.Duration:F3}")
                    .AppendLine($"result={result.ResultState}")
                    .ToString();
                WriteLog(prefix, log);
                Debug.Log($"ColorTiming {result.Test.TestMode} evidence saved: {result.PassCount}/{total} passed, {result.FailCount} failed.");
            }

            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }

            static string PrefixFor(TestMode mode)
            {
                if (mode == TestMode.PlayMode) return "playmode";
                if (mode == TestMode.EditMode) return "editmode";
                return null;
            }

            static void WriteLog(string prefix, string text)
            {
                var path = OutputPath($"{prefix}-color-timing-latest.log");
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ProjectRoot());
                File.WriteAllText(path, text, Encoding.UTF8);
            }

            static string OutputPath(string fileName)
            {
                return Path.Combine(ProjectRoot(), "openspec", "changes", "refactor-color-timing-runtime-presentation", "evidence", "TestResults", fileName);
            }

            static string ProjectRoot()
            {
                return Directory.GetParent(Application.dataPath)?.FullName
                    ?? throw new InvalidOperationException("Could not resolve the Unity project root.");
            }
        }
    }
}
