using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using XWear.IO.XResource.AvatarMeta;

namespace XWear.IO.XResource.Animation
{
    [Serializable]
    public class AnimatorControllerResource : VrcAssetResource
    {
        public List<AnimatorControllerLayer> layers = new List<AnimatorControllerLayer>();

        public override VrcAssetResource Copy()
        {
            return new AnimatorControllerResource()
            {
                Guid = Guid,
                layers = layers.Select(x => x.Copy()).ToList(),
                Name = Name,
                type = type
            };
        }
    }

    [Serializable]
    public class AnimatorControllerLayer
    {
        public int layerIndex;
        public string avatarMaskGuid;
        public AnimatorControllerStateMachine animatorControllerStateMachine;

        public AnimatorControllerLayer Copy()
        {
            return new AnimatorControllerLayer()
            {
                layerIndex = layerIndex,
                avatarMaskGuid = avatarMaskGuid,
                animatorControllerStateMachine = animatorControllerStateMachine.Copy()
            };
        }
    }

    [Serializable]
    public class AnimatorControllerStateMachine
    {
        public List<AnimatorControllerState> states = new List<AnimatorControllerState>();

        public AnimatorControllerStateMachine Copy()
        {
            return new AnimatorControllerStateMachine()
            {
                states = states.Select(x => x.Copy()).ToList()
            };
        }
    }

    [Serializable]
    public class AnimatorControllerState
    {
        public int stateIndex;

        [SerializeReference]
        public AnimatorControllerMotion Motion;

        public AnimatorControllerState Copy()
        {
            return new AnimatorControllerState()
            {
                stateIndex = stateIndex,
                Motion = Motion == null ? new NullMotion() : Motion.Copy()
            };
        }
    }

    [Serializable]
    public abstract class AnimatorControllerMotion
    {
        public abstract AnimatorControllerMotion Copy();
    }

    [Serializable]
    public class BlendTreeMotion : AnimatorControllerMotion
    {
        public string name;
        public int blendType;
        public string blendParameter;
        public string blendParameterY;
        public float maxThreshold;
        public float minThreshold;
        public bool useAutomaticThresholds;
        public List<BlendTreeChild> children = new List<BlendTreeChild>();

        public override AnimatorControllerMotion Copy()
        {
            return new BlendTreeMotion
            {
                name = name,
                blendType = blendType,
                blendParameter = blendParameter,
                blendParameterY = blendParameterY,
                maxThreshold = maxThreshold,
                minThreshold = minThreshold,
                useAutomaticThresholds = useAutomaticThresholds,
                children = children.Select(x => x.Copy()).ToList(),
            };
        }
    }

    [Serializable]
    public class BlendTreeChild
    {
        [SerializeReference]
        public AnimatorControllerMotion Motion;
        public Vector2 position;
        public float threshold;
        public float timeScale;
        public float cycleOffset;
        public string directBlendParameter;
        public bool mirror;

        public BlendTreeChild Copy()
        {
            return new BlendTreeChild
            {
                Motion = Motion?.Copy(),
                position = position,
                threshold = threshold,
                timeScale = timeScale,
                cycleOffset = cycleOffset,
                directBlendParameter = directBlendParameter,
                mirror = mirror
            };
        }
    }

    [Serializable]
    public class AnimationClipMotion : AnimatorControllerMotion
    {
        public string animationClipGuid;

        public override AnimatorControllerMotion Copy()
        {
            return new AnimationClipMotion() { animationClipGuid = animationClipGuid };
        }
    }

    [Serializable]
    public class NullMotion : AnimatorControllerMotion
    {
        public override AnimatorControllerMotion Copy()
        {
            return new NullMotion();
        }
    }

    public static class UnityAnimatorControllerUtil
    {
#if UNITY_EDITOR

        public static AnimatorControllerResource ToAssetResource(
            RuntimeAnimatorController animatorController,
            List<(VrcAssetResource assetResource, string tmpPath)> vrcAssetResourceResult,
            Dictionary<AnimationClip, XResourceAnimationClip> clipInstanceToXResourceMap
        )
        {
            var currentLayerIndex = 0;
            try
            {
                var editorAnimatorController =
                    animatorController as UnityEditor.Animations.AnimatorController;
                if (editorAnimatorController == null)
                {
                    throw new Exception($"{animatorController} is invalid AnimatorController");
                }

                var result = new AnimatorControllerResource()
                {
                    Guid = Guid.NewGuid().ToString(),
                    Name = animatorController.name,
                    type = VrcAssetResource.AssetType.AnimatorController
                };
                var resultLayers = new List<AnimatorControllerLayer>();

                for (
                    int layerIndex = 0;
                    layerIndex < editorAnimatorController.layers.Length;
                    layerIndex++
                )
                {
                    currentLayerIndex = layerIndex;
                    resultLayers.Add(
                        CollectLater(
                            editorAnimatorController.layers[layerIndex],
                            layerIndex,
                            vrcAssetResourceResult: vrcAssetResourceResult,
                            clipInstanceToXResourceMap: clipInstanceToXResourceMap
                        )
                    );
                }

                result.layers = resultLayers;

                return result;
            }
            catch (Exception)
            {
                Debug.LogError(
                    $"AnimatorController ToAssetResource Error at {animatorController.name}_{currentLayerIndex}"
                );
                throw;
            }
        }

        private static AnimatorControllerLayer CollectLater(
            UnityEditor.Animations.AnimatorControllerLayer layer,
            int layerIndex,
            List<(VrcAssetResource assetResource, string tmpPath)> vrcAssetResourceResult,
            Dictionary<AnimationClip, XResourceAnimationClip> clipInstanceToXResourceMap
        )
        {
            var avatarMaskGuid = "";
#if UNITY_EDITOR
            if (layer.avatarMask != null)
            {
                avatarMaskGuid = CreateAvatarMaskAsset(
                    vrcAssetResourceResult,
                    layer.avatarMask
                ).Guid;
            }
#endif
            var stateMachine = CollectStateMachine(layer.stateMachine, clipInstanceToXResourceMap);
            return new AnimatorControllerLayer()
            {
                layerIndex = layerIndex,
                avatarMaskGuid = avatarMaskGuid,
                animatorControllerStateMachine = stateMachine
            };
        }

        private static SimpleVrcAssetResource CreateAvatarMaskAsset(
            List<(VrcAssetResource assetResource, string tmpPath)> vrcAssetResourceResult,
            UnityEngine.AvatarMask avatarMask
        )
        {
            var copiedPath = Editor.EditorAssetUtil.CopyAssetToTempDir(avatarMask);
            var copiedAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AvatarMask>(
                copiedPath
            );
            copiedAsset.name = avatarMask.name;

            var simpleAssetResource = new SimpleVrcAssetResource()
            {
                Guid = Guid.NewGuid().ToString(),
                Name = avatarMask.name,
                type = VrcAssetResource.AssetType.AvatarMask
            };

            vrcAssetResourceResult.Add((simpleAssetResource, copiedPath));

            return simpleAssetResource;
        }

        private static AnimatorControllerStateMachine CollectStateMachine(
            UnityEditor.Animations.AnimatorStateMachine stateMachine,
            Dictionary<AnimationClip, XResourceAnimationClip> clipInstanceToXResourceMap
        )
        {
            var result = new AnimatorControllerStateMachine();
            var resultStates = new List<AnimatorControllerState>();

            for (var index = 0; index < stateMachine.states.Length; index++)
            {
                var childState = stateMachine.states[index];
                resultStates.Add(
                    new AnimatorControllerState()
                    {
                        stateIndex = index,
                        Motion = CollectMotion(childState.state, clipInstanceToXResourceMap)
                    }
                );
            }

            result.states = resultStates;
            return result;
        }

        private static AnimatorControllerMotion CollectMotion(
            UnityEditor.Animations.AnimatorState state,
            Dictionary<AnimationClip, XResourceAnimationClip> clipInstanceToXResourceMap
        )
        {
            if (state.motion is AnimationClip animationClip)
            {
                return CollectAnimationClip(animationClip, clipInstanceToXResourceMap);
            }

            if (state.motion is UnityEditor.Animations.BlendTree blendTree)
            {
                return CollectBlendTree(blendTree, clipInstanceToXResourceMap);
            }

            if (state.motion == null)
            {
                return new NullMotion();
            }

            throw new InvalidDataException($"{state.name} is invalid motion type");
        }

        private static AnimationClipMotion CollectAnimationClip(
            AnimationClip clip,
            Dictionary<AnimationClip, XResourceAnimationClip> clipInstanceToXResourceMap
        )
        {
            if (!clipInstanceToXResourceMap.TryGetValue(clip, out var xResource))
            {
                xResource = UnityAnimationClipUtil.ConvertToXResourceAnimation(clip);
                clipInstanceToXResourceMap.Add(clip, xResource);
            }

            return new AnimationClipMotion() { animationClipGuid = xResource.guid };
        }

        private static BlendTreeMotion CollectBlendTree(
            UnityEditor.Animations.BlendTree blendTree,
            Dictionary<AnimationClip, XResourceAnimationClip> clipInstanceToXResourceMap
        )
        {
            var result = new BlendTreeMotion();
            var resultChildren = new List<BlendTreeChild>();
            result.name = blendTree.name;
            result.blendType = (int)blendTree.blendType;
            result.blendParameter = blendTree.blendParameter;
            result.blendParameterY = blendTree.blendParameterY;
            result.maxThreshold = blendTree.maxThreshold;
            result.minThreshold = blendTree.minThreshold;
            result.useAutomaticThresholds = blendTree.useAutomaticThresholds;

            foreach (var child in blendTree.children)
            {
                var resultChild = new BlendTreeChild()
                {
                    position = child.position,
                    threshold = child.threshold,
                    timeScale = child.timeScale,
                    cycleOffset = child.cycleOffset,
                    directBlendParameter = child.directBlendParameter,
                    mirror = child.mirror,
                };

                if (child.motion is AnimationClip animationClip)
                {
                    resultChild.Motion = CollectAnimationClip(
                        animationClip,
                        clipInstanceToXResourceMap
                    );
                }
                else if (child.motion is UnityEditor.Animations.BlendTree blendTreeMotion)
                {
                    resultChild.Motion = CollectBlendTree(
                        blendTreeMotion,
                        clipInstanceToXResourceMap
                    );
                }
                else if (child.motion == null)
                {
                    resultChild.Motion = new NullMotion();
                }
                else
                {
                    throw new InvalidDataException(
                        $"Invalid motion type detected in BlendTree in {blendTree} :{child.motion.GetType()}"
                    );
                }

                resultChildren.Add(resultChild);
            }

            result.children = resultChildren;
            return result;
        }

        public static void BuildAnimatorControllerResource(
            RuntimeAnimatorController dest,
            AnimatorControllerResource source,
            Dictionary<string, string> guidToAssetPathMap,
            Dictionary<string, AnimationClip> guidToClipAssetMap
        )
        {
            var editorAnimatorController = dest as UnityEditor.Animations.AnimatorController;
            if (editorAnimatorController == null)
            {
                throw new Exception($"{dest} is invalid AnimatorController");
            }

            if (editorAnimatorController.layers.Length != source.layers.Count)
            {
                throw new InvalidDataException("Invalid Layer Size");
            }

            var currentLayerIndex = 0;
            try
            {
                for (var i = 0; i < source.layers.Count; i++)
                {
                    currentLayerIndex = i;
                    var sourceLayer = source.layers[i];
                    var destLayer = editorAnimatorController.layers[sourceLayer.layerIndex];
                    BuildLayer(
                        destLayer,
                        sourceLayer,
                        guidToAssetPathMap: guidToAssetPathMap,
                        guidToClipAssetMap: guidToClipAssetMap
                    );
                }
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
                UnityEditor.AssetDatabase.SaveAssets();
#endif
            }
            catch (Exception)
            {
                Debug.LogError(
                    $"AnimatorController ToAssetResource Error at {dest.name}_{currentLayerIndex}"
                );
                throw;
            }
        }

        private static void BuildLayer(
            UnityEditor.Animations.AnimatorControllerLayer destLayer,
            AnimatorControllerLayer sourceLayer,
            Dictionary<string, AnimationClip> guidToClipAssetMap,
            Dictionary<string, string> guidToAssetPathMap
        )
        {
#if UNITY_EDITOR
            if (
                !string.IsNullOrEmpty(sourceLayer.avatarMaskGuid)
                && guidToAssetPathMap.TryGetValue(
                    sourceLayer.avatarMaskGuid,
                    out var avatarMaskAssetPath
                )
            )
            {
                destLayer.avatarMask =
                    UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AvatarMask>(
                        avatarMaskAssetPath
                    );
            }
#endif
            BuildStateMachine(
                destLayer.stateMachine,
                sourceLayer.animatorControllerStateMachine,
                guidToClipAssetMap
            );
        }

        private static void BuildStateMachine(
            UnityEditor.Animations.AnimatorStateMachine destStateMachine,
            AnimatorControllerStateMachine sourceStateMachine,
            Dictionary<string, AnimationClip> guidToClipAssetMap
        )
        {
            if (destStateMachine.states.Length != sourceStateMachine.states.Count)
            {
                throw new InvalidDataException("Invalid States Size");
            }

            for (var index = 0; index < destStateMachine.states.Length; index++)
            {
                var destState = destStateMachine.states[index];
                var sourceState = sourceStateMachine.states[index];
                BuildMotion(destState.state, sourceState, guidToClipAssetMap);
            }
        }

        private static void BuildMotion(
            UnityEditor.Animations.AnimatorState destState,
            AnimatorControllerState sourceState,
            Dictionary<string, AnimationClip> guidToClipAssetMap
        )
        {
            if (sourceState.Motion is AnimationClipMotion animationClipMotion)
            {
                BuildAnimationClip(destState, animationClipMotion, guidToClipAssetMap);
            }
            else if (sourceState.Motion is BlendTreeMotion blendTreeMotion)
            {
                BuildBlendTreeMotion(destState, blendTreeMotion, guidToClipAssetMap);
            }
            else if (sourceState.Motion is NullMotion)
            {
                destState.motion = null;
            }
            else
            {
                throw new InvalidDataException($"Invalid motion type. at:{destState.name}");
            }
        }

        private static void BuildAnimationClip(
            UnityEditor.Animations.AnimatorState destState,
            AnimationClipMotion sourceMotion,
            Dictionary<string, AnimationClip> guidToClipAssetMap
        )
        {
            if (!guidToClipAssetMap.TryGetValue(sourceMotion.animationClipGuid, out var clipAsset))
            {
                Debug.LogError($"{sourceMotion.animationClipGuid} is not found");
                return;
            }

            destState.motion = clipAsset;
        }

        private static void BuildBlendTreeMotion(
            UnityEditor.Animations.AnimatorState destState,
            BlendTreeMotion sourceMotion,
            Dictionary<string, AnimationClip> guidToClipAssetMap
        )
        {
            destState.motion = BuildBlendTree(sourceMotion, guidToClipAssetMap);
        }

        private static UnityEditor.Animations.BlendTree BuildBlendTree(
            BlendTreeMotion sourceMotion,
            Dictionary<string, AnimationClip> guidToClipAssetMap
        )
        {
            var destBlendTree = new UnityEditor.Animations.BlendTree();
            destBlendTree.name = sourceMotion.name;
            destBlendTree.blendType = (UnityEditor.Animations.BlendTreeType)sourceMotion.blendType;
            destBlendTree.blendParameter = sourceMotion.blendParameter;
            destBlendTree.blendParameterY = sourceMotion.blendParameterY;
            destBlendTree.maxThreshold = sourceMotion.maxThreshold;
            destBlendTree.minThreshold = sourceMotion.minThreshold;
            destBlendTree.useAutomaticThresholds = sourceMotion.useAutomaticThresholds;

            var destChildren = new UnityEditor.Animations.ChildMotion[sourceMotion.children.Count];
            for (int i = 0; i < destChildren.Length; i++)
            {
                var destChild = new UnityEditor.Animations.ChildMotion();
                var sourceChild = sourceMotion.children[i];
                if (sourceChild.Motion is AnimationClipMotion sourceChildAnimationClip)
                {
                    if (
                        !guidToClipAssetMap.TryGetValue(
                            sourceChildAnimationClip.animationClipGuid,
                            out var clipAsset
                        )
                    )
                    {
                        Debug.LogError(
                            $"{sourceChildAnimationClip.animationClipGuid} is not found"
                        );
                        return null;
                    }

                    destChild.motion = clipAsset;
                    destChild.position = sourceChild.position;
                    destChild.threshold = sourceChild.threshold;
                    destChild.timeScale = sourceChild.timeScale;
                    destChild.cycleOffset = sourceChild.cycleOffset;
                    destChild.directBlendParameter = sourceChild.directBlendParameter;
                    destChild.mirror = sourceChild.mirror;
                }
                else if (sourceChild.Motion is BlendTreeMotion blendTreeMotion)
                {
                    destChild.motion = BuildBlendTree(blendTreeMotion, guidToClipAssetMap);
                    destChild.position = sourceChild.position;
                    destChild.threshold = sourceChild.threshold;
                    destChild.timeScale = sourceChild.timeScale;
                    destChild.cycleOffset = sourceChild.cycleOffset;
                    destChild.directBlendParameter = sourceChild.directBlendParameter;
                    destChild.mirror = sourceChild.mirror;
                }
                else if (sourceChild.Motion is NullMotion) { }
                else
                {
                    throw new InvalidDataException("Invalid BlendTree");
                }

                destChildren[i] = destChild;
            }

            destBlendTree.children = destChildren;
            return destBlendTree;
        }
#endif
    }
}
