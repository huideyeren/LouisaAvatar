using System;
using UnityEngine;

namespace XWear.IO.XResource.Animation
{
    [Serializable]
    public class XResourceAnimatorController
    {
        public string guid;
        public string name;
        public XResourceAnimatorLayer[] layers;
    }

    [Serializable]
    public class XResourceAnimatorLayer
    {
        public int layerIndex;
        public XResourceAnimatorState[] states;
    }

    [Serializable]
    public class XResourceAnimatorState
    {
        public string motionGuid;
    }
}
