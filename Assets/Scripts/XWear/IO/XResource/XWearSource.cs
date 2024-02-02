using System.Collections.Generic;
using UnityEngine;

namespace XWear.IO.XResource
{
    public class XWearSourceSkinnedMeshRenderer
    {
        public readonly UnityEngine.Transform SourceSmrTransform;
        public readonly SkinnedMeshRenderer DestSmr;

        public XWearSourceSkinnedMeshRenderer(
            UnityEngine.Transform sourceSmrTransform,
            SkinnedMeshRenderer destSmr
        )
        {
            SourceSmrTransform = sourceSmrTransform;
            DestSmr = destSmr;
        }
    }

    public abstract class XSourceBase
    {
        public readonly XWearSourceSkinnedMeshRenderer[] SourceSmrs;
        public readonly GameObject RootGameObject;

        public readonly Dictionary<
            UnityEngine.Transform,
            UnityEngine.Transform
        > SourceToEditTransformMap;

        protected XSourceBase(
            XWearSourceSkinnedMeshRenderer[] sourceSmrs,
            GameObject rootGameObject,
            Dictionary<UnityEngine.Transform, UnityEngine.Transform> sourceToEditTransformMap
        )
        {
            SourceSmrs = sourceSmrs;
            RootGameObject = rootGameObject;
            SourceToEditTransformMap = sourceToEditTransformMap;
        }
    }

    public class XAccessorySource : XSourceBase
    {
        public AccessoryMap AccessoryMap;

        public XAccessorySource(
            XWearSourceSkinnedMeshRenderer[] sourceSmrs,
            GameObject rootGameObject,
            AccessoryMap accessoryMap,
            Dictionary<UnityEngine.Transform, UnityEngine.Transform> sourceToEditTransformMap
        )
            : base(sourceSmrs, rootGameObject, sourceToEditTransformMap)
        {
            AccessoryMap = accessoryMap;
        }
    }

    public class XWearSource : XSourceBase
    {
        public HumanoidMap HumanoidMap;

        public XWearSource(
            XWearSourceSkinnedMeshRenderer[] sourceSmrs,
            GameObject rootGameObject,
            HumanoidMap humanoidMap,
            Dictionary<UnityEngine.Transform, UnityEngine.Transform> sourceToEditTransformMap
        )
            : base(sourceSmrs, rootGameObject, sourceToEditTransformMap)
        {
            HumanoidMap = humanoidMap;
        }
    }
}
