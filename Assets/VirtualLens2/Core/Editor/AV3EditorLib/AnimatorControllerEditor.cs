using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using Object = UnityEngine.Object;

namespace VirtualLens2.AV3EditorLib
{
    /// <summary>
    /// Utility functions for manipulating animator controllers.
    /// </summary>
    public static class AnimatorControllerEditor
    {
        /// <summary>
        /// Returns deep copy of an animator controller.
        /// </summary>
        /// <param name="controller">The animator controller to be cloned</param>
        /// <returns>The deep copy of <c>controller</c>.</returns>
        public static AnimatorController Clone(AnimatorController controller)
        {
            Motion DuplicateMotion(Motion src)
            {
                if (src is BlendTree tree)
                {
                    var result = new BlendTree()
                    {
                        hideFlags = tree.hideFlags,
                        name = tree.name,
                        blendParameter = tree.blendParameter,
                        blendParameterY = tree.blendParameterY,
                        blendType = tree.blendType,
                        maxThreshold = tree.maxThreshold,
                        minThreshold = tree.minThreshold,
                        useAutomaticThresholds = tree.useAutomaticThresholds
                    };
                    result.children = tree.children
                        .Select(c => new ChildMotion()
                        {
                            mirror = c.mirror,
                            motion = DuplicateMotion(c.motion),
                            cycleOffset = c.cycleOffset,
                            directBlendParameter = c.directBlendParameter,
                            position = c.position,
                            threshold = c.threshold,
                            timeScale = c.timeScale
                        })
                        .ToArray();
                    return result;
                }
                return src;
            }

            StateMachineBehaviour DuplicateBehaviour(StateMachineBehaviour src)
            {
                var result = Object.Instantiate(src);
                result.name = src.name;
                result.hideFlags = src.hideFlags;
                return result;
            }

            AnimatorState DuplicateState(AnimatorState src)
            {
                var result = new AnimatorState()
                {
                    name = src.name,
                    hideFlags = src.hideFlags,
                    behaviours = src.behaviours.Select(DuplicateBehaviour).ToArray(),
                    cycleOffset = src.cycleOffset,
                    cycleOffsetParameter = src.cycleOffsetParameter,
                    cycleOffsetParameterActive = src.cycleOffsetParameterActive,
                    iKOnFeet = src.iKOnFeet,
                    mirror = src.mirror,
                    mirrorParameter = src.mirrorParameter,
                    mirrorParameterActive = src.mirrorParameterActive,
                    motion = DuplicateMotion(src.motion),
                    speed = src.speed,
                    speedParameter = src.speedParameter,
                    speedParameterActive = src.speedParameterActive,
                    tag = src.tag,
                    timeParameter = src.timeParameter,
                    timeParameterActive = src.timeParameterActive,
                    transitions = new AnimatorStateTransition[] { },
                    writeDefaultValues = src.writeDefaultValues
                };
                return result;
            }

            AnimatorStateTransition DuplicateStateTransition(
                AnimatorStateTransition src,
                IDictionary<AnimatorState, AnimatorState> stateMap,
                IDictionary<AnimatorStateMachine, AnimatorStateMachine> stateMachineMap)
            {
                var state = src.destinationState ? stateMap[src.destinationState] : null;
                var stateMachine = src.destinationStateMachine ? stateMachineMap[src.destinationStateMachine] : null;
                var result = new AnimatorStateTransition()
                {
                    conditions = (AnimatorCondition[]) src.conditions.Clone(),
                    destinationState = state,
                    destinationStateMachine = stateMachine,
                    isExit = src.isExit,
                    mute = src.mute,
                    solo = src.solo,
                    hideFlags = src.hideFlags,
                    name = src.name,
                    canTransitionToSelf = src.canTransitionToSelf,
                    duration = src.duration,
                    exitTime = src.exitTime,
                    hasExitTime = src.hasExitTime,
                    hasFixedDuration = src.hasFixedDuration,
                    interruptionSource = src.interruptionSource,
                    offset = src.offset,
                    orderedInterruption = src.orderedInterruption
                };
                return result;
            }

            AnimatorTransition DuplicateTransition(
                AnimatorTransition src,
                IDictionary<AnimatorState, AnimatorState> stateMap,
                IDictionary<AnimatorStateMachine, AnimatorStateMachine> stateMachineMap)
            {
                var state = src.destinationState ? stateMap[src.destinationState] : null;
                var stateMachine = src.destinationStateMachine ? stateMachineMap[src.destinationStateMachine] : null;
                var result = new AnimatorTransition()
                {
                    conditions = (AnimatorCondition[]) src.conditions.Clone(),
                    destinationState = state,
                    destinationStateMachine = stateMachine,
                    isExit = src.isExit,
                    mute = src.mute,
                    solo = src.solo,
                    hideFlags = src.hideFlags,
                    name = src.name,
                };
                return result;
            }

            AnimatorStateMachine DuplicateStateMachine(AnimatorStateMachine src)
            {
                var stateMap = new Dictionary<AnimatorState, AnimatorState>(
                    new ReferenceEqualityComparer<AnimatorState>());
                var states = new List<ChildAnimatorState>();
                foreach (var s in src.states)
                {
                    var state = DuplicateState(s.state);
                    states.Add(new ChildAnimatorState() {position = s.position, state = state});
                    stateMap.Add(s.state, state);
                }
                var stateMachineMap = new Dictionary<AnimatorStateMachine, AnimatorStateMachine>(
                    new ReferenceEqualityComparer<AnimatorStateMachine>());
                var stateMachines = new List<ChildAnimatorStateMachine>();
                foreach (var s in src.stateMachines)
                {
                    var stateMachine = DuplicateStateMachine(s.stateMachine);
                    stateMachines.Add(new ChildAnimatorStateMachine()
                    {
                        position = s.position,
                        stateMachine = stateMachine
                    });
                    stateMachineMap.Add(s.stateMachine, stateMachine);
                }
                foreach (var e in stateMap)
                {
                    e.Value.transitions = e.Key.transitions
                        .Select(t => DuplicateStateTransition(t, stateMap, stateMachineMap))
                        .ToArray();
                }
                var dst = new AnimatorStateMachine()
                {
                    name = src.name,
                    hideFlags = src.hideFlags,
                    anyStatePosition = src.anyStatePosition,
                    anyStateTransitions = src.anyStateTransitions
                        .Select(t => DuplicateStateTransition(t, stateMap, stateMachineMap))
                        .ToArray(),
                    behaviours = src.behaviours.Select(DuplicateBehaviour).ToArray(),
                    defaultState = src.defaultState ? stateMap[src.defaultState] : null,
                    entryPosition = src.entryPosition,
                    entryTransitions = src.entryTransitions
                        .Select(t => DuplicateTransition(t, stateMap, stateMachineMap))
                        .ToArray(),
                    exitPosition = src.exitPosition,
                    parentStateMachinePosition = src.parentStateMachinePosition,
                    stateMachines = stateMachines.ToArray(),
                    states = states.ToArray()
                };
                return dst;
            }

            AnimatorController clone = new AnimatorController();
            foreach (var parameter in controller.parameters)
            {
                clone.AddParameter(new AnimatorControllerParameter
                {
                    defaultBool = parameter.defaultBool,
                    defaultFloat = parameter.defaultFloat,
                    defaultInt = parameter.defaultInt,
                    name = parameter.name,
                    type = parameter.type
                });
            }
            foreach (var src in controller.layers)
            {
                var dst = new AnimatorControllerLayer()
                {
                    name = src.name,
                    avatarMask = src.avatarMask,
                    blendingMode = src.blendingMode,
                    defaultWeight = src.defaultWeight,
                    iKPass = src.iKPass,
                    stateMachine = DuplicateStateMachine(src.stateMachine),
                    syncedLayerAffectsTiming = src.syncedLayerAffectsTiming,
                    syncedLayerIndex = src.syncedLayerIndex
                };
                clone.AddLayer(dst);
            }
            return clone;
        }

        
        /// <summary>
        /// Removes all layers satisfying a condition from an animator controller.
        /// </summary>
        /// <param name="controller">The animator controller to be modified.</param>
        /// <param name="pred">The function to test each layer for a condition.</param>
        /// <param name="descriptor">The avatar descriptor that uses <c>controller</c>.</param>
        /// <exception cref="ArgumentNullException"><c>controller</c> or <c>pred</c> is <c>null</c>.</exception>
        public static void RemoveLayers(
            AnimatorController controller, Predicate<AnimatorControllerLayer> pred, VRCAvatarDescriptor descriptor)
        {
            if (pred == null) { throw new ArgumentNullException(nameof(pred)); }

            var removed = new HashSet<int>();
            var usage = new HashSet<VRCAvatarDescriptor.AnimLayerType>();

            VRCAvatarDescriptor.AnimLayerType TranslateLayerType(VRC_AnimatorLayerControl.BlendableLayer orig)
            {
                switch (orig)
                {
                    case VRC_AnimatorLayerControl.BlendableLayer.Action:
                        return VRCAvatarDescriptor.AnimLayerType.Action;
                    case VRC_AnimatorLayerControl.BlendableLayer.Gesture:
                        return VRCAvatarDescriptor.AnimLayerType.Gesture;
                    case VRC_AnimatorLayerControl.BlendableLayer.Additive:
                        return VRCAvatarDescriptor.AnimLayerType.Additive;
                    case VRC_AnimatorLayerControl.BlendableLayer.FX:
                        return VRCAvatarDescriptor.AnimLayerType.FX;
                    default:
                        return 0;
                }
            }

            void UpdateAnimatorLayerControl(AnimatorStateMachine stateMachine)
            {
                StateMachineBehaviour[] UpdateBehaviours(IEnumerable<StateMachineBehaviour> behaviours)
                {
                    var result = new List<StateMachineBehaviour>();
                    foreach (var behaviour in behaviours)
                    {
                        if (!(behaviour is VRC_AnimatorLayerControl))
                        {
                            result.Add(behaviour);
                            continue;
                        }
                        var control = behaviour as VRC_AnimatorLayerControl;
                        if (!usage.Contains(TranslateLayerType(control.playable)))
                        {
                            result.Add(behaviour);
                            continue;
                        }
                        if (removed.Contains(control.layer)) { continue; }
                        control.layer -= removed.Count(i => i < control.layer);
                        result.Add(control);
                    }
                    return result.ToArray();
                }

                stateMachine.behaviours = UpdateBehaviours(stateMachine.behaviours);
                foreach (var child in stateMachine.states)
                {
                    child.state.behaviours = UpdateBehaviours(child.state.behaviours);
                }
                foreach (var child in stateMachine.stateMachines)
                {
                    UpdateAnimatorLayerControl(child.stateMachine);
                }
            }

            for (int i = controller.layers.Length - 1; i >= 0; --i)
            {
                if (pred(controller.layers[i]))
                {
                    controller.RemoveLayer(i);
                    removed.Add(i);
                }
            }
            if (removed.Count == 0) { return; }
            if (descriptor == null || !descriptor.customizeAnimationLayers) { return; }
            foreach (var playableLayer in descriptor.baseAnimationLayers)
            {
                if (playableLayer.animatorController == controller) { usage.Add(playableLayer.type); }
            }
            foreach (var playableLayer in descriptor.baseAnimationLayers)
            {
                var c = playableLayer.animatorController as AnimatorController;
                if (c is null) { continue; }
                foreach (var layer in c.layers)
                {
                    UpdateAnimatorLayerControl(layer.stateMachine);
                }
            }
        }

        /// <summary>
        /// Removes all layers satisfying a condition from an animator controller.
        /// </summary>
        /// <param name="controller">The animator controller to be modified.</param>
        /// <param name="re">The regex to test name of each layer for a condition.</param>
        /// <param name="descriptor">The avatar descriptor that uses <c>controller</c>.</param>
        /// <exception cref="ArgumentNullException"><c>controller</c> or <c>re</c> is <c>null</c>.</exception>
        public static void RemoveLayers(AnimatorController controller, Regex re, VRCAvatarDescriptor descriptor)
        {
            if (controller == null) { throw new ArgumentNullException(nameof(controller)); }
            if (re == null) { throw new ArgumentNullException(nameof(re)); }
            RemoveLayers(controller, layer => re.IsMatch(layer.name), descriptor);
        }


        /// <summary>
        /// Removes all parameters satisfying a condition from an animator controller.
        /// </summary>
        /// <param name="controller">The animator controller to be modified.</param>
        /// <param name="pred">The function to test each parameter for a condition.</param>
        /// <exception cref="ArgumentNullException"><c>controller</c> or <c>pred</c> is <c>null</c>.</exception>
        public static void RemoveParameters(AnimatorController controller, Predicate<AnimatorControllerParameter> pred)
        {
            if (controller == null) { throw new ArgumentNullException(nameof(controller)); }
            if (pred == null) { throw new ArgumentNullException(nameof(pred)); }
            for (int i = controller.parameters.Length - 1; i >= 0; --i)
            {
                if (pred(controller.parameters[i])) { controller.RemoveParameter(i); }
            }
        }

        /// <summary>
        /// Removes all parameters satisfying a condition from an animator controller.
        /// </summary>
        /// <param name="controller">The animator controller to be modified.</param>
        /// <param name="re">The regex to test name of each parameter for a condition.</param>
        /// <exception cref="ArgumentNullException"><c>controller</c> or <c>re</c> is <c>null</c>.</exception>
        public static void RemoveParameters(AnimatorController controller, Regex re)
        {
            if (controller == null) { throw new ArgumentNullException(nameof(controller)); }
            if (re == null) { throw new ArgumentNullException(nameof(re)); }
            RemoveParameters(controller, p => re.IsMatch(p.name));
        }


        /// <summary>
        /// Determines whether tow animator controllers are mergeable.
        /// </summary>
        /// <param name="destination">The animator controller to be modified.</param>
        /// <param name="source">The animator controller to be merged with the <c>destination</c>.</param>
        /// <returns><c>true</c> if <c>source</c> can be merged into <c>destination</c>, <c>false</c> if otherwise.</returns>
        public static bool IsMergeable(AnimatorController destination, AnimatorController source)
        {
            if (destination == null) { return false; }
            if (source == null) { return false; }

            var srcParameters = new Dictionary<string, AnimatorControllerParameter>();
            foreach (var src in source.parameters) { srcParameters.Add(src.name, src); }
            foreach (var dst in destination.parameters)
            {
                if (!srcParameters.ContainsKey(dst.name)) { continue; }
                var src = srcParameters[dst.name];
                if (src.type != dst.type) { return false; }
                switch (dst.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        if (src.defaultBool != dst.defaultBool) { return false; }
                        break;
                    case AnimatorControllerParameterType.Float:
                        if (!src.defaultFloat.Equals(dst.defaultFloat)) { return false; }
                        break;
                    case AnimatorControllerParameterType.Int:
                        if (src.defaultInt != dst.defaultInt) { return false; }
                        break;
                }
            }
            return true;
        }

        /// <summary>
        /// Merges two animator controllers.
        /// </summary>
        /// <param name="destination">The animator controller to be modified.</param>
        /// <param name="source">The animator controller to be merged with the <c>destination</c>.</param>
        /// <param name="writeDefaultsOverrideMode">The override option for write defaults flags.</param>
        /// <exception cref="ArgumentNullException"><c>destination</c> or <c>source</c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><c>source</c> cannot be merged into <c>destination</c>.</exception>
        public static void Merge(
            AnimatorController destination, AnimatorController source,
            WriteDefaultsOverrideMode writeDefaultsOverrideMode = WriteDefaultsOverrideMode.None)
        {
            if (destination == null) { throw new ArgumentNullException(nameof(destination)); }
            if (source == null) { throw new ArgumentNullException(nameof(source)); }
            if (!IsMergeable(destination, source))
            {
                throw new ArgumentException($"{nameof(source)} cannot be merged with {nameof(destination)}.");
            }

            Undo.RegisterCompleteObjectUndo(destination, "Merge animator controller");

            var destinationControllerPath = AssetDatabase.GetAssetPath(destination);
            void AddObjectToAsset<T>(T obj) where T : Object
            {
                if (!string.IsNullOrEmpty(destinationControllerPath))
                {
                    AssetDatabase.AddObjectToAsset(obj, destinationControllerPath);
                }
            }

            bool TransformWriteDefaults(string name, bool src)
            {
                if (name.EndsWith("(WD On)")) { return true; }
                if (name.EndsWith("(WD Off)")) { return false; }
                switch (writeDefaultsOverrideMode)
                {
                    case WriteDefaultsOverrideMode.ForceDisable: return false;
                    case WriteDefaultsOverrideMode.ForceEnable: return true;
                    default: return src;
                }
            }

            Motion DuplicateMotion(Motion src)
            {
                if (src is BlendTree tree)
                {
                    var result = new BlendTree()
                    {
                        hideFlags = tree.hideFlags,
                        name = tree.name,
                        blendParameter = tree.blendParameter,
                        blendParameterY = tree.blendParameterY,
                        blendType = tree.blendType,
                        maxThreshold = tree.maxThreshold,
                        minThreshold = tree.minThreshold,
                        useAutomaticThresholds = tree.useAutomaticThresholds
                    };
                    result.children = tree.children
                        .Select(c => new ChildMotion()
                        {
                            mirror = c.mirror,
                            motion = DuplicateMotion(c.motion),
                            cycleOffset = c.cycleOffset,
                            directBlendParameter = c.directBlendParameter,
                            position = c.position,
                            threshold = c.threshold,
                            timeScale = c.timeScale
                        })
                        .ToArray();
                    Undo.RegisterCreatedObjectUndo(result, "Create blend tree");
                    AddObjectToAsset(result);
                    return result;
                }
                return src;
            }

            StateMachineBehaviour DuplicateBehaviour(StateMachineBehaviour src)
            {
                var result = Object.Instantiate(src);
                result.name = src.name;
                result.hideFlags = src.hideFlags;
                Undo.RegisterCreatedObjectUndo(result, "Create state machine behaviour");
                AddObjectToAsset(result);
                return result;
            }

            AnimatorState DuplicateState(AnimatorState src)
            {
                var result = new AnimatorState()
                {
                    name = src.name,
                    hideFlags = src.hideFlags,
                    behaviours = src.behaviours.Select(DuplicateBehaviour).ToArray(),
                    cycleOffset = src.cycleOffset,
                    cycleOffsetParameter = src.cycleOffsetParameter,
                    cycleOffsetParameterActive = src.cycleOffsetParameterActive,
                    iKOnFeet = src.iKOnFeet,
                    mirror = src.mirror,
                    mirrorParameter = src.mirrorParameter,
                    mirrorParameterActive = src.mirrorParameterActive,
                    motion = DuplicateMotion(src.motion),
                    speed = src.speed,
                    speedParameter = src.speedParameter,
                    speedParameterActive = src.speedParameterActive,
                    tag = src.tag,
                    timeParameter = src.timeParameter,
                    timeParameterActive = src.timeParameterActive,
                    transitions = new AnimatorStateTransition[] { },
                    writeDefaultValues = TransformWriteDefaults(src.name, src.writeDefaultValues)
                };
                Undo.RegisterCreatedObjectUndo(result, "Create animator state");
                AddObjectToAsset(result);
                return result;
            }

            AnimatorStateTransition DuplicateStateTransition(
                AnimatorStateTransition src,
                IDictionary<AnimatorState, AnimatorState> stateMap,
                IDictionary<AnimatorStateMachine, AnimatorStateMachine> stateMachineMap)
            {
                var state = src.destinationState ? stateMap[src.destinationState] : null;
                var stateMachine = src.destinationStateMachine ? stateMachineMap[src.destinationStateMachine] : null;
                var result = new AnimatorStateTransition()
                {
                    conditions = (AnimatorCondition[]) src.conditions.Clone(),
                    destinationState = state,
                    destinationStateMachine = stateMachine,
                    isExit = src.isExit,
                    mute = src.mute,
                    solo = src.solo,
                    hideFlags = src.hideFlags,
                    name = src.name,
                    canTransitionToSelf = src.canTransitionToSelf,
                    duration = src.duration,
                    exitTime = src.exitTime,
                    hasExitTime = src.hasExitTime,
                    hasFixedDuration = src.hasFixedDuration,
                    interruptionSource = src.interruptionSource,
                    offset = src.offset,
                    orderedInterruption = src.orderedInterruption
                };
                Undo.RegisterCreatedObjectUndo(result, "Create animator state transition");
                AddObjectToAsset(result);
                return result;
            }

            AnimatorTransition DuplicateTransition(
                AnimatorTransition src,
                IDictionary<AnimatorState, AnimatorState> stateMap,
                IDictionary<AnimatorStateMachine, AnimatorStateMachine> stateMachineMap)
            {
                var state = src.destinationState ? stateMap[src.destinationState] : null;
                var stateMachine = src.destinationStateMachine ? stateMachineMap[src.destinationStateMachine] : null;
                var result = new AnimatorTransition()
                {
                    conditions = (AnimatorCondition[]) src.conditions.Clone(),
                    destinationState = state,
                    destinationStateMachine = stateMachine,
                    isExit = src.isExit,
                    mute = src.mute,
                    solo = src.solo,
                    hideFlags = src.hideFlags,
                    name = src.name,
                };
                Undo.RegisterCreatedObjectUndo(result, "Create animator transition");
                AddObjectToAsset(result);
                return result;
            }

            AnimatorStateMachine DuplicateStateMachine(AnimatorStateMachine src)
            {
                var stateMap = new Dictionary<AnimatorState, AnimatorState>(
                    new ReferenceEqualityComparer<AnimatorState>());
                var states = new List<ChildAnimatorState>();
                foreach (var s in src.states)
                {
                    var state = DuplicateState(s.state);
                    states.Add(new ChildAnimatorState() {position = s.position, state = state});
                    stateMap.Add(s.state, state);
                }
                var stateMachineMap = new Dictionary<AnimatorStateMachine, AnimatorStateMachine>(
                    new ReferenceEqualityComparer<AnimatorStateMachine>());
                var stateMachines = new List<ChildAnimatorStateMachine>();
                foreach (var s in src.stateMachines)
                {
                    var stateMachine = DuplicateStateMachine(s.stateMachine);
                    stateMachines.Add(new ChildAnimatorStateMachine()
                    {
                        position = s.position,
                        stateMachine = stateMachine
                    });
                    stateMachineMap.Add(s.stateMachine, stateMachine);
                }
                foreach (var e in stateMap)
                {
                    e.Value.transitions = e.Key.transitions
                        .Select(t => DuplicateStateTransition(t, stateMap, stateMachineMap))
                        .ToArray();
                }
                var dst = new AnimatorStateMachine()
                {
                    name = src.name,
                    hideFlags = src.hideFlags,
                    anyStatePosition = src.anyStatePosition,
                    anyStateTransitions = src.anyStateTransitions
                        .Select(t => DuplicateStateTransition(t, stateMap, stateMachineMap))
                        .ToArray(),
                    behaviours = src.behaviours.Select(DuplicateBehaviour).ToArray(),
                    defaultState = src.defaultState ? stateMap[src.defaultState] : null,
                    entryPosition = src.entryPosition,
                    entryTransitions = src.entryTransitions
                        .Select(t => DuplicateTransition(t, stateMap, stateMachineMap))
                        .ToArray(),
                    exitPosition = src.exitPosition,
                    parentStateMachinePosition = src.parentStateMachinePosition,
                    stateMachines = stateMachines.ToArray(),
                    states = states.ToArray()
                };
                Undo.RegisterCreatedObjectUndo(dst, "Create animator state machine");
                AddObjectToAsset(dst);
                return dst;
            }

            foreach (var src in source.parameters)
            {
                if (destination.parameters.Any(p => p.name.Equals(src.name))) { continue; }
                destination.AddParameter(new AnimatorControllerParameter()
                {
                    name = src.name,
                    type = src.type,
                    defaultBool = src.defaultBool,
                    defaultFloat = src.defaultFloat,
                    defaultInt = src.defaultInt
                });
            }

            var layerIndexOffset = destination.layers.Length - 1;
            // Skip base layers
            foreach (var src in source.layers.Skip(1))
            {
                var dst = new AnimatorControllerLayer()
                {
                    name = src.name,
                    avatarMask = src.avatarMask,
                    blendingMode = src.blendingMode,
                    defaultWeight = src.defaultWeight,
                    iKPass = src.iKPass,
                    stateMachine = DuplicateStateMachine(src.stateMachine),
                    syncedLayerAffectsTiming = src.syncedLayerAffectsTiming,
                    syncedLayerIndex = src.syncedLayerIndex < 0 ? -1 : src.syncedLayerIndex + layerIndexOffset
                };
                destination.AddLayer(dst);
            }
        }

        
        /// <summary>
        /// Replaces placeholder motions in an animator controller to actual motions.
        /// </summary>
        /// <param name="controller">The animator controller to be modified.</param>
        /// <param name="parameters">Mappings from a placeholder motion to an actual motion.</param>
        /// <exception cref="ArgumentException"><c>controller</c> or <c>parameters</c> is <c>null</c>.</exception>
        public static void ProcessMotionTemplate(AnimatorController controller, MotionTemplateParameters parameters)
        {
            if (controller == null) { throw new ArgumentNullException(nameof(controller)); }
            if (parameters == null) { throw new ArgumentNullException(nameof(parameters)); }
            
            Undo.RegisterCompleteObjectUndo(controller, "Process motion template");

            var controllerPath = AssetDatabase.GetAssetPath(controller);
            void AddObjectToAsset<T>(T obj) where T : Object
            {
                if (!string.IsNullOrEmpty(controllerPath))
                {
                    AssetDatabase.AddObjectToAsset(obj, controllerPath);
                }
            }

            Motion ProcessMotion(Motion motion)
            {
                if (motion == null) { return null; }
                if (parameters.ContainsKey(motion)) { return parameters[motion]; }
                if (motion is BlendTree tree)
                {
                    var modified = false;
                    var children = new List<ChildMotion>();
                    foreach (var c in tree.children)
                    {
                        var childMotion = ProcessMotion(c.motion);
                        if (childMotion != c.motion) { modified = true; }
                        children.Add(new ChildMotion()
                        {
                            mirror = c.mirror,
                            motion = childMotion,
                            cycleOffset = c.cycleOffset,
                            directBlendParameter = c.directBlendParameter,
                            position = c.position,
                            threshold = c.threshold,
                            timeScale = c.timeScale
                        });
                    }
                    if (!modified) { return tree; }
                    var newTree = new BlendTree()
                    {
                        hideFlags = tree.hideFlags,
                        name = tree.name,
                        blendParameter = tree.blendParameter,
                        blendParameterY = tree.blendParameterY,
                        blendType = tree.blendType,
                        maxThreshold = tree.maxThreshold,
                        minThreshold = tree.minThreshold,
                        useAutomaticThresholds = tree.useAutomaticThresholds
                    };
                    newTree.children = children.ToArray();
                    Undo.RegisterCreatedObjectUndo(newTree, "Create blend tree");
                    AddObjectToAsset(newTree);
                    return newTree;
                }
                return motion;
            }

            void ProcessStateMachine(AnimatorStateMachine stateMachine)
            {
                foreach (var child in stateMachine.states)
                {
                    var state = child.state;
                    var motion = ProcessMotion(state.motion);
                    if (!ReferenceEquals(motion, state.motion))
                    {
                        Undo.RegisterCompleteObjectUndo(state, "Process motion template");
                        state.motion = ProcessMotion(state.motion);
                    }
                }
                foreach (var child in stateMachine.stateMachines) { ProcessStateMachine(child.stateMachine); }
            }

            foreach (var layer in controller.layers)
            {
                ProcessStateMachine(layer.stateMachine);
            }
        }

        
        /// <summary>
        /// Renders template strings in an animator controller.
        /// </summary>
        /// <seealso cref="StringTemplateEngine"/>
        /// <param name="controller">The animator controller to be modified.</param>
        /// <param name="parameters">Key-value pairs used for rendering templates.</param>
        /// <exception cref="ArgumentNullException"><c>controller</c> or <c>parameters</c> is <c>null</c>.</exception>
        public static void ProcessStringTemplates(AnimatorController controller, IDictionary<string, string> parameters)
        {
            if (controller == null) { throw new ArgumentNullException(nameof(controller)); }
            if (parameters == null) { throw new ArgumentNullException(nameof(parameters)); }
            var engine = new StringTemplateEngine(parameters);

            void ProcessMotion(Motion motion)
            {
                if (motion is BlendTree tree)
                {
                    tree.name = engine.Render(tree.name);
                    tree.blendParameter = engine.Render(tree.blendParameter);
                    foreach (var child in tree.children) { ProcessMotion(child.motion); }
                }
            }

            void ProcessBehaviour(StateMachineBehaviour behaviour)
            {
                if (behaviour is VRCAnimatorLayerControl layerControl)
                {
                    layerControl.debugString = engine.Render(layerControl.debugString);
                }
                else if (behaviour is VRCAnimatorLocomotionControl locomotionControl)
                {
                    locomotionControl.debugString = engine.Render(locomotionControl.debugString);
                }
                else if (behaviour is VRCAnimatorTemporaryPoseSpace temporaryPoseSpace)
                {
                    temporaryPoseSpace.debugString = engine.Render(temporaryPoseSpace.debugString);
                }
                else if (behaviour is VRCAnimatorTrackingControl trackingControl)
                {
                    trackingControl.debugString = engine.Render(trackingControl.debugString);
                }
                else if (behaviour is VRCAvatarParameterDriver driver)
                {
                    foreach (var p in driver.parameters) { p.name = engine.Render(p.name); }
                    driver.debugString = engine.Render(driver.debugString);
                }
                else if (behaviour is VRCPlayableLayerControl playableLayerControl)
                {
                    playableLayerControl.debugString = engine.Render(playableLayerControl.debugString);
                }
            }

            void ProcessState(AnimatorState state)
            {
                state.name = engine.Render(state.name);
                state.mirrorParameter = engine.Render(state.mirrorParameter);
                state.speedParameter = engine.Render(state.speedParameter);
                state.timeParameter = engine.Render(state.timeParameter);
                state.cycleOffsetParameter = engine.Render(state.cycleOffsetParameter);
                ProcessMotion(state.motion);
                foreach (var b in state.behaviours) { ProcessBehaviour(b); }
            }

            void ProcessStateMachine(AnimatorStateMachine stateMachine)
            {
                stateMachine.name = engine.Render(stateMachine.name);
                foreach (var child in stateMachine.states) { ProcessState(child.state); }
                foreach (var child in stateMachine.stateMachines) { ProcessStateMachine(child.stateMachine); }
                foreach (var b in stateMachine.behaviours) { ProcessBehaviour(b); }
            }

            var layers = controller.layers;
            foreach (var layer in layers)
            {
                layer.name = engine.Render(layer.name);
                ProcessStateMachine(layer.stateMachine);
            }
            controller.layers = layers;

            var animatorControllerParameters = controller.parameters;
            foreach (var parameter in animatorControllerParameters)
            {
                parameter.name = engine.Render(parameter.name);
            }
            controller.parameters = animatorControllerParameters;
        }

        
        /// <summary>
        /// Replaces template animations referred from an animator controller to rendered them.
        /// </summary>
        /// <seealso cref="AnimationClipEditor.ProcessTemplate"/>
        /// <param name="controller">The animator controller to be modified.</param>
        /// <param name="root">The root object to determine paths in the hierarchy.</param>
        /// <param name="parameters">The set of name-object pairs representing new bindings.</param>
        /// <param name="folder">The folder to save modified animation clips.</param>
        /// <exception cref="ArgumentNullException"><c>root</c>, <c>parameters</c> or <c>folder</c> is <c>null</c>.</exception>
        public static void ProcessAnimationTemplates(
            AnimatorController controller, GameObject root, AnimationTemplateParameters parameters,
            ArtifactsFolder folder)
        {
            if (controller == null) { throw new ArgumentNullException(nameof(controller)); }
            if (root == null) { throw new ArgumentNullException(nameof(root)); }
            if (parameters == null) { throw new ArgumentNullException(nameof(parameters)); }
            if (folder == null) { throw new ArgumentNullException(nameof(folder)); }

            var motionMapping = new MotionTemplateParameters();
            
            void ProcessMotion(Motion motion)
            {
                if (motion is AnimationClip clip)
                {
                    if (!AnimationClipEditor.IsTemplate(clip)) { return; }
                    if (motionMapping.ContainsKey(clip)) { return; }
                    var newClip = AnimationClipEditor.Clone(clip);
                    folder.CreateAsset(newClip);
                    AnimationClipEditor.ProcessTemplate(newClip, root, parameters);
                    motionMapping.Add(clip, newClip);
                }
                else if (motion is BlendTree tree)
                {
                    foreach (var c in tree.children) { ProcessMotion(c.motion); }
                }
            }

            void ProcessStateMachine(AnimatorStateMachine stateMachine)
            {
                foreach (var child in stateMachine.states) { ProcessMotion(child.state.motion); }
                foreach (var child in stateMachine.stateMachines) { ProcessStateMachine(child.stateMachine); }
            }

            foreach (var layer in controller.layers)
            {
                ProcessStateMachine(layer.stateMachine);
            }
            ProcessMotionTemplate(controller, motionMapping);
        }
    }
}