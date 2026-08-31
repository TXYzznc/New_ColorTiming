// 文件职责：从原 Hero Controller 重建可审计的基础动作加单武器候选 Controller。
// 所属模块：ColorTiming / Editor / Migration。

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ColorTiming.Combat;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ColorTiming.Editor.Migration
{
    /// <summary>
    /// Creates independent controllers instead of copying and pruning Controller sub-assets.
    /// Copying a Controller then removing nested state machines leaves internal PPtrs behind;
    /// rebuilding the retained graph avoids dangling references and keeps only the selected
    /// weapon's animation-clip dependencies.
    /// </summary>
    internal static class HeroWeaponControllerCandidateMigration
    {
        private const string SourcePath = "Assets/Game/Sprites/ColorTiming/Hero/Animations/Hero.controller";
        private const string OutputDirectory = "Assets/Game/Sprites/ColorTiming/Hero/Animations/RuntimeCandidates";
        private static readonly string[] RulePaths =
        {
            "Assets/Game/ScriptableAssets/ColorTiming/Combat/WeaponSpawnRules/Boss1WeaponSpawnRule.asset",
            "Assets/Game/ScriptableAssets/ColorTiming/Combat/WeaponSpawnRules/Boss2WeaponSpawnRule.asset",
        };

        [MenuItem("Game Framework/GameTools/ColorTiming/Animation Migration/Generate Weapon Controller Candidates", false, 1015)]
        private static void Generate()
        {
            AnimatorController source = AssetDatabase.LoadAssetAtPath<AnimatorController>(SourcePath);
            if (source == null) throw new InvalidOperationException("Hero source controller is missing.");
            EnsureDirectory(OutputDirectory);
            var weapons = RulePaths.Select(AssetDatabase.LoadAssetAtPath<WeaponSpawnRuleAsset>)
                .Where(rule => rule != null).SelectMany(rule => rule.GetSupportedWeapons()).Distinct()
                .OrderBy(weapon => weapon.ToLegacyAnimatorIndex()).ToArray();
            if (weapons.Length == 0) throw new InvalidOperationException("No weapon rule entries were found.");

            var weaponMachines = FindWeaponMachines(source.layers[0].stateMachine);
            CreateCandidate(source, null, "Base");
            foreach (WeaponIdentity weapon in weapons)
            {
                if (!weaponMachines.TryGetValue(weapon.ToLegacyAnimatorIndex(), out AnimatorStateMachine target))
                    throw new InvalidOperationException($"Hero Controller has no switch transition for {weapon}.");
                CreateCandidate(source, weapon, $"{weapon.Color}_{weapon.Type}");
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Generated {weapons.Length} Hero weapon controller candidates in {OutputDirectory}.");
        }

        private static Dictionary<int, AnimatorStateMachine> FindWeaponMachines(AnimatorStateMachine root)
        {
            AnimatorState switchState = root.states.Select(child => child.state)
                .SingleOrDefault(state => state.name == "switchWeapon");
            if (switchState == null) throw new InvalidOperationException("Hero Controller has no root switchWeapon state.");
            var result = new Dictionary<int, AnimatorStateMachine>();
            foreach (AnimatorStateTransition transition in switchState.transitions)
            {
                if (transition.destinationStateMachine == null) continue;
                foreach (AnimatorCondition condition in transition.conditions)
                {
                    if (condition.parameter == "weaponType" && condition.mode == AnimatorConditionMode.Equals)
                        result[(int)condition.threshold] = transition.destinationStateMachine;
                }
            }
            return result;
        }

        private static void CreateCandidate(AnimatorController source, WeaponIdentity? weapon, string name)
        {
            string assetPath = $"{OutputDirectory}/Hero_{name}.controller";
            AnimatorController candidate = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath)
                                         ?? AnimatorController.CreateAnimatorControllerAtPath(assetPath);
            CopyParameters(source, candidate);
            CopyLayers(source, candidate, weapon);
            EditorUtility.SetDirty(candidate);
            ValidateCandidate(source, candidate, weapon, name);
        }

        private static void CopyParameters(AnimatorController source, AnimatorController destination)
        {
            foreach (AnimatorControllerParameter parameter in destination.parameters) destination.RemoveParameter(parameter);
            foreach (AnimatorControllerParameter parameter in source.parameters) destination.AddParameter(parameter);
        }

        private static void CopyLayers(AnimatorController source, AnimatorController destination, WeaponIdentity? weapon)
        {
            while (destination.layers.Length > 0) destination.RemoveLayer(destination.layers.Length - 1);
            for (int index = 0; index < source.layers.Length; index++)
            {
                AnimatorControllerLayer sourceLayer = source.layers[index];
                destination.AddLayer(sourceLayer.name);
                AnimatorControllerLayer destinationLayer = destination.layers[index];
                destinationLayer.name = sourceLayer.name;
                destinationLayer.avatarMask = sourceLayer.avatarMask;
                destinationLayer.blendingMode = sourceLayer.blendingMode;
                destinationLayer.defaultWeight = sourceLayer.defaultWeight;
                destinationLayer.iKPass = sourceLayer.iKPass;
                destinationLayer.syncedLayerAffectsTiming = sourceLayer.syncedLayerAffectsTiming;
                destinationLayer.syncedLayerIndex = sourceLayer.syncedLayerIndex;
                if (sourceLayer.stateMachine == null) throw new InvalidOperationException($"Layer {sourceLayer.name} has no state machine.");
                destinationLayer.stateMachine.name = sourceLayer.stateMachine.name;
                AnimatorStateMachine targetWeapon = FindWeaponMachine(sourceLayer.stateMachine, weapon);
                var clone = new StateMachineClone(sourceLayer.stateMachine, destinationLayer.stateMachine, targetWeapon);
                clone.Copy();
                CopyEffectiveMotions(source, destination, index, clone);
                destination.layers[index] = destinationLayer;
            }
        }

        // Layer-level motion overrides are not stored on AnimatorState.motion. The original
        // Hero Controller uses them for the xuli layer, including hammer/axe movement. If
        // omitted, the cloned state graph enters correctly but renders the fallback Normal
        // frame. Rebuild the override table against each cloned state.
        private static void CopyEffectiveMotions(
            AnimatorController source,
            AnimatorController destination,
            int layerIndex,
            StateMachineClone clone)
        {
            foreach (var pair in clone.StatePairs)
            {
                Motion motion = source.GetStateEffectiveMotion(pair.Key, layerIndex);
                destination.SetStateEffectiveMotion(pair.Value, motion, layerIndex);
            }
        }

        private static AnimatorStateMachine FindWeaponMachine(AnimatorStateMachine root, WeaponIdentity? weapon)
        {
            if (!weapon.HasValue) return null;
            return FindWeaponMachines(root).TryGetValue(weapon.Value.ToLegacyAnimatorIndex(), out AnimatorStateMachine machine)
                ? machine
                : null;
        }

        private static void ValidateCandidate(AnimatorController source, AnimatorController candidate,
            WeaponIdentity? weapon, string name)
        {
            string[] sourceDependencies = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(source), true);
            string[] candidateDependencies = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(candidate), true);
            if (candidateDependencies.Length >= sourceDependencies.Length)
                throw new InvalidOperationException($"{candidate.name} did not reduce dependency count.");
            if (candidate.parameters.Length != source.parameters.Length)
                throw new InvalidOperationException($"{candidate.name} does not preserve Animator parameters.");
            AnimatorStateMachine root = candidate.layers[0].stateMachine;
            AnimatorStateMachine targetWeapon = FindWeaponMachine(source.layers[0].stateMachine, weapon);
            if (targetWeapon != null && !root.stateMachines.Any(child => child.stateMachine.name == targetWeapon.name))
                throw new InvalidOperationException($"{candidate.name} is missing {targetWeapon.name}.");
            if (root.stateMachines.Any(child => child.stateMachine.name != "wu"
                                                && (targetWeapon == null || child.stateMachine.name != targetWeapon.name)))
                throw new InvalidOperationException($"{candidate.name} retains an unrelated root weapon state machine.");
            Debug.Log($"Hero candidate validated: {name}; dependencies {sourceDependencies.Length} -> {candidateDependencies.Length}.", candidate);
        }

        private static void EnsureDirectory(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(parent)) throw new InvalidOperationException($"Invalid asset directory {path}.");
            EnsureDirectory(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private sealed class StateMachineClone
        {
            private readonly AnimatorStateMachine _source;
            private readonly AnimatorStateMachine _destination;
            private readonly AnimatorStateMachine _retainedWeapon;
            private readonly Dictionary<AnimatorState, AnimatorState> _states = new Dictionary<AnimatorState, AnimatorState>();
            private readonly Dictionary<AnimatorStateMachine, AnimatorStateMachine> _machines =
                new Dictionary<AnimatorStateMachine, AnimatorStateMachine>();

            public IEnumerable<KeyValuePair<AnimatorState, AnimatorState>> StatePairs => _states;

            public StateMachineClone(AnimatorStateMachine source, AnimatorStateMachine destination, AnimatorStateMachine retainedWeapon)
            {
                _source = source;
                _destination = destination;
                _retainedWeapon = retainedWeapon;
            }

            public void Copy()
            {
                Clear(_destination);
                CloneStructure(_source, _destination, true);
                CopyTransitions(_source);
            }

            private void CloneStructure(AnimatorStateMachine source, AnimatorStateMachine destination, bool isRoot)
            {
                _machines.Add(source, destination);
                CopyBehaviours(source.behaviours, destination.AddStateMachineBehaviour);
                foreach (ChildAnimatorState child in source.states)
                {
                    AnimatorState copy = destination.AddState(child.state.name, child.position);
                    CopyState(child.state, copy);
                    _states.Add(child.state, copy);
                }
                foreach (ChildAnimatorStateMachine child in source.stateMachines)
                {
                    if (isRoot && child.stateMachine != _retainedWeapon && child.stateMachine.name != "wu") continue;
                    AnimatorStateMachine copy = destination.AddStateMachine(child.stateMachine.name, child.position);
                    CloneStructure(child.stateMachine, copy, false);
                }
                if (source.defaultState != null && _states.TryGetValue(source.defaultState, out AnimatorState defaultState))
                    destination.defaultState = defaultState;
            }

            private void CopyTransitions(AnimatorStateMachine source)
            {
                AnimatorStateMachine destination = _machines[source];
                foreach (AnimatorStateTransition transition in source.anyStateTransitions)
                    CopyTransition(CreateAnyStateTransition(destination, transition), transition);
                foreach (ChildAnimatorState child in source.states)
                {
                    AnimatorState state = _states[child.state];
                    foreach (AnimatorStateTransition transition in child.state.transitions)
                        CopyTransition(CreateStateTransition(state, transition), transition);
                }
                foreach (ChildAnimatorStateMachine child in source.stateMachines)
                {
                    if (_machines.ContainsKey(child.stateMachine)) CopyTransitions(child.stateMachine);
                }
            }

            private AnimatorStateTransition CreateAnyStateTransition(AnimatorStateMachine source, AnimatorStateTransition transition)
            {
                if (transition.destinationState != null && _states.TryGetValue(transition.destinationState, out AnimatorState state))
                    return source.AddAnyStateTransition(state);
                return transition.destinationStateMachine != null && _machines.TryGetValue(transition.destinationStateMachine, out AnimatorStateMachine machine)
                    ? source.AddAnyStateTransition(machine) : null;
            }

            private AnimatorStateTransition CreateStateTransition(AnimatorState source, AnimatorStateTransition transition)
            {
                if (transition.isExit) return source.AddExitTransition();
                if (transition.destinationState != null && _states.TryGetValue(transition.destinationState, out AnimatorState state))
                    return source.AddTransition(state);
                return transition.destinationStateMachine != null && _machines.TryGetValue(transition.destinationStateMachine, out AnimatorStateMachine machine)
                    ? source.AddTransition(machine) : null;
            }

            private static void CopyTransition(AnimatorStateTransition destination, AnimatorStateTransition source)
            {
                if (destination == null) return;
                destination.canTransitionToSelf = source.canTransitionToSelf;
                destination.duration = source.duration;
                destination.exitTime = source.exitTime;
                destination.hasExitTime = source.hasExitTime;
                destination.hasFixedDuration = source.hasFixedDuration;
                destination.interruptionSource = source.interruptionSource;
                destination.offset = source.offset;
                destination.orderedInterruption = source.orderedInterruption;
                foreach (AnimatorCondition condition in source.conditions)
                    destination.AddCondition(condition.mode, condition.threshold, condition.parameter);
            }

            private static void CopyState(AnimatorState source, AnimatorState destination)
            {
                destination.cycleOffset = source.cycleOffset;
                destination.cycleOffsetParameter = source.cycleOffsetParameter;
                destination.cycleOffsetParameterActive = source.cycleOffsetParameterActive;
                destination.iKOnFeet = source.iKOnFeet;
                destination.mirror = source.mirror;
                destination.mirrorParameter = source.mirrorParameter;
                destination.mirrorParameterActive = source.mirrorParameterActive;
                destination.motion = source.motion;
                destination.speed = source.speed;
                destination.speedParameter = source.speedParameter;
                destination.speedParameterActive = source.speedParameterActive;
                destination.tag = source.tag;
                destination.timeParameter = source.timeParameter;
                destination.timeParameterActive = source.timeParameterActive;
                destination.writeDefaultValues = source.writeDefaultValues;
                CopyBehaviours(source.behaviours, destination.AddStateMachineBehaviour);
            }

            private static void CopyBehaviours(IEnumerable<StateMachineBehaviour> source,
                Func<Type, StateMachineBehaviour> addBehaviour)
            {
                foreach (StateMachineBehaviour sourceBehaviour in source)
                {
                    StateMachineBehaviour destinationBehaviour = addBehaviour(sourceBehaviour.GetType());
                    EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(sourceBehaviour), destinationBehaviour);
                }
            }

            private static void Clear(AnimatorStateMachine stateMachine)
            {
                foreach (ChildAnimatorState state in stateMachine.states.ToArray()) stateMachine.RemoveState(state.state);
                foreach (ChildAnimatorStateMachine child in stateMachine.stateMachines.ToArray())
                    stateMachine.RemoveStateMachine(child.stateMachine);
            }
        }
    }
}
