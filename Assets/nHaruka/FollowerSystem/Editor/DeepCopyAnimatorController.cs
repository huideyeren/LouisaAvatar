using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using AnimatorControllerLayer = UnityEditor.Animations.AnimatorControllerLayer;
using BlendTree = UnityEditor.Animations.BlendTree;

namespace nharuka
{
    public class DeepCopyAnimatorController: Editor
    {
        public static AnimatorControllerLayer DeepCopyLayer(AnimatorControllerLayer srcLayer, string dstLayerName, bool isFirst = false)
        {
            var newLayer = new AnimatorControllerLayer()
            {
                avatarMask = srcLayer.avatarMask,
                blendingMode = srcLayer.blendingMode,
                defaultWeight = srcLayer.defaultWeight,
                iKPass = srcLayer.iKPass,
                name = dstLayerName,
                syncedLayerAffectsTiming = srcLayer.syncedLayerAffectsTiming,
                syncedLayerIndex = srcLayer.syncedLayerIndex,
                stateMachine = new AnimatorStateMachine()
            };

            if (isFirst) newLayer.defaultWeight = 1f;

            newLayer.stateMachine.name = newLayer.name;
            newLayer.stateMachine.hideFlags = HideFlags.HideInHierarchy;
            newLayer.stateMachine.defaultState = srcLayer.stateMachine.defaultState;

            CopyStates(srcLayer.stateMachine, newLayer.stateMachine);

            CopyTransitions(srcLayer.stateMachine, newLayer.stateMachine);

            return newLayer;
        }

        private static void CopyStates(AnimatorStateMachine srcStateMachine, AnimatorStateMachine dstStateMachine)
        {
            for (int i = 0; i < srcStateMachine.states.Length; i++)
            {
                var addState = dstStateMachine.AddState(srcStateMachine.states[i].state.name, srcStateMachine.states[i].position);
                addState.motion = srcStateMachine.states[i].state.motion;
                addState.cycleOffset = srcStateMachine.states[i].state.cycleOffset;
                addState.cycleOffsetParameter = srcStateMachine.states[i].state.cycleOffsetParameter;
                addState.cycleOffsetParameterActive = srcStateMachine.states[i].state.cycleOffsetParameterActive;
                addState.mirror = srcStateMachine.states[i].state.mirror;
                addState.mirrorParameter = srcStateMachine.states[i].state.mirrorParameter;
                addState.timeParameter = srcStateMachine.states[i].state.timeParameter;
                addState.timeParameterActive = srcStateMachine.states[i].state.timeParameterActive;
                addState.iKOnFeet = srcStateMachine.states[i].state.iKOnFeet;
                addState.speed = srcStateMachine.states[i].state.speed;
                addState.speedParameter = srcStateMachine.states[i].state.speedParameter;
                addState.speedParameterActive = srcStateMachine.states[i].state.speedParameterActive;
                addState.writeDefaultValues = srcStateMachine.states[i].state.writeDefaultValues;

                if (srcStateMachine.states[i].state.motion != null && srcStateMachine.states[i].state.motion.GetType() == typeof(UnityEditor.Animations.BlendTree))
                {
                    addState.motion = CopyBlendTree((BlendTree)addState.motion);
                }

                addState.timeParameter = srcStateMachine.states[i].state.timeParameter;
                addState.timeParameter = srcStateMachine.states[i].state.timeParameter;
            }
        }

        static BlendTree CopyBlendTree(BlendTree orig)
        {
            BlendTree newBlendTree = new BlendTree();
            newBlendTree.name = orig.name;
            newBlendTree.blendParameter = orig.blendParameter;
            newBlendTree.blendParameterY = orig.blendParameterY;
            newBlendTree.blendType = orig.blendType;
            newBlendTree.maxThreshold = orig.maxThreshold;
            newBlendTree.minThreshold = orig.minThreshold;
            newBlendTree.useAutomaticThresholds = orig.useAutomaticThresholds;

            var origChildren = orig.children;
            var newChildren = new ChildMotion[origChildren.Length];

            for (int n = 0; n < origChildren.Length; n++)
            {
                if (origChildren[n].motion != null && origChildren[n].motion.GetType() == typeof(BlendTree))
                {
                    newChildren[n].motion = CopyBlendTree((BlendTree)origChildren[n].motion);
                }
                else if (origChildren[n].motion != null)
                {
                    newChildren[n].motion = origChildren[n].motion;

                    if (orig.blendType == BlendTreeType.Simple1D || orig.blendType == BlendTreeType.Direct)
                    {
                        newChildren[n].threshold = origChildren[n].threshold;
                    }
                    else
                    {
                        newChildren[n].position = origChildren[n].position;
                    }

                    newChildren[n].mirror = origChildren[n].mirror;
                    newChildren[n].timeScale = origChildren[n].timeScale;
                    newChildren[n].cycleOffset = origChildren[n].cycleOffset;
                    newChildren[n].directBlendParameter = origChildren[n].directBlendParameter;
                }
            }
            newBlendTree.children = newChildren;

            return newBlendTree;
        }

        static void CopyTransitions(AnimatorStateMachine src, AnimatorStateMachine dst)
        {
            for (int i = 0; i < src.anyStateTransitions.Length; i++)
            {
                if (src.anyStateTransitions[i].destinationState != null)
                {
                    for (int n = 0; n < dst.states.Length; n++)
                    {
                        if (dst.states[n].state.name == src.anyStateTransitions[i].destinationState.name)
                        {
                            var dstAny = dst.AddAnyStateTransition(dst.states[n].state);
                            dstAny.conditions = src.anyStateTransitions[i].conditions;
                            dstAny.canTransitionToSelf = src.anyStateTransitions[i].canTransitionToSelf;
                            dstAny.hasExitTime = src.anyStateTransitions[i].hasExitTime;
                            dstAny.exitTime = src.anyStateTransitions[i].exitTime;
                            dstAny.duration = src.anyStateTransitions[i].duration;
                            dstAny.interruptionSource = src.anyStateTransitions[i].interruptionSource;
                            dstAny.hasFixedDuration = src.anyStateTransitions[i].hasFixedDuration;
                            dstAny.orderedInterruption = src.anyStateTransitions[i].orderedInterruption;
                        }
                    }
                }
                else
                {
                    for (int n = 0; n < dst.stateMachines.Length; n++)
                    {
                        if (dst.stateMachines[n].stateMachine.name == src.anyStateTransitions[i].destinationStateMachine.name)
                        {
                            var dstAny = dst.AddAnyStateTransition(dst.stateMachines[n].stateMachine);
                            dstAny.conditions = src.anyStateTransitions[i].conditions;
                            dstAny.canTransitionToSelf = src.anyStateTransitions[i].canTransitionToSelf;
                            dstAny.hasExitTime = src.anyStateTransitions[i].hasExitTime;
                            dstAny.exitTime = src.anyStateTransitions[i].exitTime;
                            dstAny.duration = src.anyStateTransitions[i].duration;
                            dstAny.interruptionSource = src.anyStateTransitions[i].interruptionSource;
                            dstAny.hasFixedDuration = src.anyStateTransitions[i].hasFixedDuration;
                            dstAny.orderedInterruption = src.anyStateTransitions[i].orderedInterruption;
                        }
                    }

                }
            }

            for (int i = 0; i < src.states.Length; i++)
            {
                for (int k = 0; k < dst.states.Length; k++)
                {
                    if (src.states[i].state.name == dst.states[k].state.name)
                    {
                        for (int j = 0; j < src.states[i].state.transitions.Length; j++)
                        {
                            if (src.states[i].state.transitions[j].destinationState != null)
                            {
                                for (int n = 0; n < dst.states.Length; n++)
                                {
                                    if (dst.states[n].state.name == src.states[i].state.transitions[j].destinationState.name)
                                    {
                                        var transition = dst.states[k].state.AddTransition(dst.states[n].state);
                                        transition.offset = src.states[i].state.transitions[j].offset;
                                        transition.hasExitTime = src.states[i].state.transitions[j].hasExitTime;
                                        transition.exitTime = src.states[i].state.transitions[j].exitTime;
                                        transition.isExit = src.states[i].state.transitions[j].isExit;
                                        transition.conditions = src.states[i].state.transitions[j].conditions;
                                        transition.interruptionSource = src.states[i].state.transitions[j].interruptionSource;
                                        transition.canTransitionToSelf = src.states[i].state.transitions[j].canTransitionToSelf;
                                        transition.duration = src.states[i].state.transitions[j].duration;
                                        transition.hasFixedDuration = src.states[i].state.transitions[j].hasFixedDuration;
                                        transition.orderedInterruption = src.states[i].state.transitions[j].orderedInterruption;
                                    }
                                }
                            }
                            else
                            {
                                for (int n = 0; n < dst.stateMachines.Length; n++)
                                {
                                    if (dst.stateMachines[n].stateMachine.name == src.states[i].state.transitions[j].destinationStateMachine.name)
                                    {
                                        var transition = dst.states[k].state.AddTransition(dst.stateMachines[n].stateMachine);
                                        transition.offset = src.states[i].state.transitions[j].offset;
                                        transition.hasExitTime = src.states[i].state.transitions[j].hasExitTime;
                                        transition.exitTime = src.states[i].state.transitions[j].exitTime;
                                        transition.isExit = src.states[i].state.transitions[j].isExit;
                                        transition.conditions = src.states[i].state.transitions[j].conditions;
                                        transition.interruptionSource = src.states[i].state.transitions[j].interruptionSource;
                                        transition.canTransitionToSelf = src.states[i].state.transitions[j].canTransitionToSelf;
                                        transition.duration = src.states[i].state.transitions[j].duration;
                                        transition.hasFixedDuration = src.states[i].state.transitions[j].hasFixedDuration;
                                        transition.orderedInterruption = src.states[i].state.transitions[j].orderedInterruption;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}