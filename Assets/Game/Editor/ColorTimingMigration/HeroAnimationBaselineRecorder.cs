// 文件职责：记录并校验 Hero Animator Controller 的迁移前动画契约。
// 所属模块：ColorTiming / Editor / Migration。

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ColorTiming.Editor.Migration
{
    /// <summary>
    /// Provides a source-controlled migration guard for splitting Hero weapon animations.
    /// The recorder is read-only with respect to animation assets and writes only a JSON
    /// evidence file under the active OpenSpec change.
    /// </summary>
    internal static class HeroAnimationBaselineRecorder
    {
        private const string SourceControllerPath = "Assets/Game/Sprites/ColorTiming/Hero/Animations/Hero.controller";
        private const string EvidenceRelativePath =
            "openspec/changes/optimize-color-timing-runtime-performance/evidence/hero-animation-baseline.json";

        [MenuItem("Game Framework/GameTools/ColorTiming/Animation Migration/Record Hero Baseline", false, 1011)]
        private static void RecordHeroBaseline()
        {
            AnimatorController controller = LoadSourceController();
            if (controller == null)
            {
                return;
            }

            HeroAnimatorBaseline baseline = CreateBaseline(controller);
            string evidencePath = GetEvidencePath();
            Directory.CreateDirectory(Path.GetDirectoryName(evidencePath));
            File.WriteAllText(evidencePath, JsonUtility.ToJson(baseline, true));
            AssetDatabase.Refresh();
            Debug.Log($"Recorded Hero animation baseline: {evidencePath}", controller);
        }

        [MenuItem("Game Framework/GameTools/ColorTiming/Animation Migration/Validate Selected Controller", false, 1012)]
        private static void ValidateSelectedController()
        {
            AnimatorController candidate = Selection.activeObject as AnimatorController;
            if (candidate == null)
            {
                Debug.LogError("Select an AnimatorController before validating the Hero animation baseline.");
                return;
            }

            ValidateController(candidate);
        }

        private static AnimatorController LoadSourceController()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(SourceControllerPath);
            if (controller == null)
            {
                Debug.LogError($"Hero source controller was not found: {SourceControllerPath}");
            }

            return controller;
        }

        private static HeroAnimatorBaseline CreateBaseline(AnimatorController controller)
        {
            var baseline = new HeroAnimatorBaseline
            {
                controllerPath = AssetDatabase.GetAssetPath(controller),
                parameters = controller.parameters
                    .Select(parameter => new AnimatorParameterContract
                    {
                        name = parameter.name,
                        type = parameter.type.ToString(),
                        defaultFloat = parameter.defaultFloat,
                        defaultInt = parameter.defaultInt,
                        defaultBool = parameter.defaultBool,
                    })
                    .OrderBy(parameter => parameter.name, StringComparer.Ordinal)
                    .ToList(),
            };

            var animationClips = new HashSet<AnimationClip>();
            for (int layerIndex = 0; layerIndex < controller.layers.Length; layerIndex++)
            {
                CollectStateContracts(
                    controller.layers[layerIndex].stateMachine,
                    controller.layers[layerIndex].name,
                    baseline.states,
                    animationClips);
                CollectTransitionContracts(
                    controller.layers[layerIndex].stateMachine,
                    controller.layers[layerIndex].name,
                    baseline.transitions);
            }

            baseline.states = baseline.states
                .OrderBy(state => state.path, StringComparer.Ordinal)
                .ToList();
            baseline.transitions = baseline.transitions
                .OrderBy(transition => transition.sourcePath, StringComparer.Ordinal)
                .ThenBy(transition => transition.destinationPath, StringComparer.Ordinal)
                .ThenBy(transition => transition.conditions, StringComparer.Ordinal)
                .ToList();
            baseline.animationEvents = animationClips
                .SelectMany(CreateAnimationEventContracts)
                .OrderBy(contract => contract.clipPath, StringComparer.Ordinal)
                .ThenBy(contract => contract.signature, StringComparer.Ordinal)
                .ToList();
            baseline.dependencyPaths = AssetDatabase.GetDependencies(baseline.controllerPath, true)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList();
            return baseline;
        }

        private static void CollectStateContracts(
            AnimatorStateMachine stateMachine,
            string path,
            List<AnimatorStateContract> states,
            ISet<AnimationClip> animationClips)
        {
            foreach (ChildAnimatorState childState in stateMachine.states)
            {
                var state = childState.state;
                var stateContract = new AnimatorStateContract
                {
                    path = $"{path}/{state.name}",
                    motionType = state.motion == null ? "None" : state.motion.GetType().Name,
                };
                CollectMotionClips(state.motion, stateContract.clipPaths, animationClips);
                stateContract.clipPaths.Sort(StringComparer.Ordinal);
                states.Add(stateContract);
            }

            foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
            {
                CollectStateContracts(childStateMachine.stateMachine, $"{path}/{childStateMachine.stateMachine.name}", states,
                    animationClips);
            }
        }

        [MenuItem("Game Framework/GameTools/ColorTiming/Animation Migration/Validate Hero Source", false, 1013)]
        private static void ValidateHeroSource()
        {
            AnimatorController controller = LoadSourceController();
            if (controller == null)
            {
                return;
            }

            ValidateController(controller);
        }

        private static void ValidateController(AnimatorController candidate)
        {
            string evidencePath = GetEvidencePath();
            if (!File.Exists(evidencePath))
            {
                Debug.LogError($"Hero animation baseline is missing: {evidencePath}", candidate);
                return;
            }

            HeroAnimatorBaseline expected = JsonUtility.FromJson<HeroAnimatorBaseline>(File.ReadAllText(evidencePath));
            HeroAnimatorBaseline actual = CreateBaseline(candidate);
            List<string> differences = CompareContracts(expected, actual);
            if (differences.Count == 0)
            {
                Debug.Log($"Hero animation contract validation passed: {AssetDatabase.GetAssetPath(candidate)}", candidate);
                return;
            }

            Debug.LogError(
                $"Hero animation contract validation failed for {AssetDatabase.GetAssetPath(candidate)}:\n"
                + string.Join("\n", differences.Select(difference => $"- {difference}")),
                candidate);
        }

        private static void CollectMotionClips(Motion motion, List<string> clipPaths, ISet<AnimationClip> animationClips)
        {
            if (motion is AnimationClip clip)
            {
                animationClips.Add(clip);
                clipPaths.Add(AssetDatabase.GetAssetPath(clip));
                return;
            }

            if (!(motion is BlendTree blendTree))
            {
                return;
            }

            foreach (ChildMotion childMotion in blendTree.children)
            {
                CollectMotionClips(childMotion.motion, clipPaths, animationClips);
            }
        }

        private static void CollectTransitionContracts(
            AnimatorStateMachine stateMachine,
            string path,
            List<AnimatorTransitionContract> transitions)
        {
            foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions)
            {
                AddTransitionContract($"{path}/AnyState", transition, transitions);
            }

            foreach (ChildAnimatorState childState in stateMachine.states)
            {
                foreach (AnimatorStateTransition transition in childState.state.transitions)
                {
                    AddTransitionContract($"{path}/{childState.state.name}", transition, transitions);
                }
            }

            foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
            {
                CollectTransitionContracts(
                    childStateMachine.stateMachine,
                    $"{path}/{childStateMachine.stateMachine.name}",
                    transitions);
            }
        }

        private static void AddTransitionContract(
            string sourcePath,
            AnimatorStateTransition transition,
            List<AnimatorTransitionContract> transitions)
        {
            string destinationPath = transition.destinationStateMachine != null
                ? transition.destinationStateMachine.name
                : transition.destinationState == null ? "Exit" : transition.destinationState.name;
            string conditions = string.Join(",", transition.conditions
                .Select(condition => $"{condition.parameter}:{condition.mode}:{condition.threshold:F0}")
                .OrderBy(condition => condition, StringComparer.Ordinal));
            transitions.Add(new AnimatorTransitionContract
            {
                sourcePath = sourcePath,
                destinationPath = destinationPath,
                conditions = conditions,
            });
        }

        private static IEnumerable<AnimationEventContract> CreateAnimationEventContracts(AnimationClip clip)
        {
            string clipPath = AssetDatabase.GetAssetPath(clip);
            foreach (AnimationEvent animationEvent in AnimationUtility.GetAnimationEvents(clip))
            {
                yield return new AnimationEventContract
                {
                    clipPath = clipPath,
                    signature = string.Join("|", new[]
                    {
                        animationEvent.functionName ?? string.Empty,
                        animationEvent.time.ToString("F6"),
                        animationEvent.floatParameter.ToString("F6"),
                        animationEvent.intParameter.ToString(),
                        animationEvent.stringParameter ?? string.Empty,
                        animationEvent.objectReferenceParameter == null
                            ? string.Empty
                            : AssetDatabase.GetAssetPath(animationEvent.objectReferenceParameter),
                    }),
                };
            }
        }

        private static List<string> CompareContracts(HeroAnimatorBaseline expected, HeroAnimatorBaseline actual)
        {
            var differences = new List<string>();
            CompareSequence("parameters", expected.parameters.Select(FormatParameter), actual.parameters.Select(FormatParameter),
                differences);
            CompareSequence("states", expected.states.Select(FormatState), actual.states.Select(FormatState), differences);
            CompareSequence("transitions", expected.transitions.Select(FormatTransition),
                actual.transitions.Select(FormatTransition), differences);
            CompareSequence("animation events", expected.animationEvents.Select(FormatAnimationEvent),
                actual.animationEvents.Select(FormatAnimationEvent), differences);
            return differences;
        }

        private static void CompareSequence(
            string contractName,
            IEnumerable<string> expected,
            IEnumerable<string> actual,
            List<string> differences)
        {
            var expectedSet = new HashSet<string>(expected, StringComparer.Ordinal);
            var actualSet = new HashSet<string>(actual, StringComparer.Ordinal);
            foreach (string missing in expectedSet.Except(actualSet).OrderBy(value => value, StringComparer.Ordinal))
            {
                differences.Add($"Missing {contractName}: {missing}");
            }

            foreach (string unexpected in actualSet.Except(expectedSet).OrderBy(value => value, StringComparer.Ordinal))
            {
                differences.Add($"Unexpected {contractName}: {unexpected}");
            }
        }

        private static string FormatParameter(AnimatorParameterContract parameter)
        {
            return string.Join("|", new[]
            {
                parameter.name,
                parameter.type,
                parameter.defaultFloat.ToString("F6"),
                parameter.defaultInt.ToString(),
                parameter.defaultBool.ToString(),
            });
        }

        private static string FormatState(AnimatorStateContract state)
        {
            return $"{state.path}|{state.motionType}|{string.Join(",", state.clipPaths)}";
        }

        private static string FormatAnimationEvent(AnimationEventContract animationEvent)
        {
            return $"{animationEvent.clipPath}|{animationEvent.signature}";
        }

        private static string FormatTransition(AnimatorTransitionContract transition)
        {
            return $"{transition.sourcePath}|{transition.destinationPath}|{transition.conditions}";
        }

        private static string GetEvidencePath()
        {
            string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath).FullName;
            return Path.Combine(projectRoot, EvidenceRelativePath);
        }

        [Serializable]
        private sealed class HeroAnimatorBaseline
        {
            public string controllerPath;
            public List<AnimatorParameterContract> parameters = new List<AnimatorParameterContract>();
            public List<AnimatorStateContract> states = new List<AnimatorStateContract>();
            public List<AnimatorTransitionContract> transitions = new List<AnimatorTransitionContract>();
            public List<AnimationEventContract> animationEvents = new List<AnimationEventContract>();
            public List<string> dependencyPaths = new List<string>();
        }

        [Serializable]
        private sealed class AnimatorParameterContract
        {
            public string name;
            public string type;
            public float defaultFloat;
            public int defaultInt;
            public bool defaultBool;
        }

        [Serializable]
        private sealed class AnimatorStateContract
        {
            public string path;
            public string motionType;
            public List<string> clipPaths = new List<string>();
        }

        [Serializable]
        private sealed class AnimationEventContract
        {
            public string clipPath;
            public string signature;
        }

        [Serializable]
        private sealed class AnimatorTransitionContract
        {
            public string sourcePath;
            public string destinationPath;
            public string conditions;
        }
    }
}
