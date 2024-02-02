#if VL2_DEVELOPMENT

using System.Collections.Generic;
using System.Linq;
using AnimatorAsCode.V0;
using UnityEditor.Animations;
using UnityEngine;

namespace VirtualLens2.Generators
{
    internal class BlendTreeEditor
    {
        private readonly BlendTree _tree;
        private readonly SortedDictionary<float, Motion> _children;

        public BlendTreeEditor(AacFlBase aac, AacFlFloatParameter parameter)
        {
            _tree = aac.NewBlendTreeAsRaw();
            _tree.hideFlags = HideFlags.HideInHierarchy;
            _tree.blendType = BlendTreeType.Simple1D;
            _tree.blendParameter = parameter.Name;
            _tree.blendType = BlendTreeType.Simple1D;
            _tree.useAutomaticThresholds = false;
            _tree.minThreshold = 0.0f;
            _tree.maxThreshold = 0.0f;
            _tree.children = new ChildMotion[] { };
            _children = new SortedDictionary<float, Motion>();
        }

        public BlendTreeEditor AddChild(float threshold, Motion motion)
        {
            _children.Add(threshold, motion);
            _tree.minThreshold = _children.Keys.Min();
            _tree.maxThreshold = _children.Keys.Max();
            _tree.children = _children
                .Select(pair => new ChildMotion() { motion = pair.Value, threshold = pair.Key, timeScale = 1.0f })
                .ToArray();
            return this;
        }

        public BlendTreeEditor AddChild(float threshold, AacFlClip clip) { return AddChild(threshold, clip.Clip); }

        public BlendTreeEditor AddChild(float threshold, BlendTreeEditor child)
        {
            return AddChild(threshold, child.Motion);
        }

        public Motion Motion => _tree;
    }
}

#endif
