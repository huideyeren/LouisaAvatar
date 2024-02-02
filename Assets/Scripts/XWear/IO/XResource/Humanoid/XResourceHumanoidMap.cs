using System.Collections.Generic;
using UnityEngine;

namespace XWear.IO.XResource.Humanoid
{
    public class XResourceHumanoidMap
    {
        public class XResourceHumanoid
        {
            public HumanBodyBones HumanBodyBones;
            public string BoneGuid;
        }

        public readonly List<XResourceHumanoid> HumanoidBones = new List<XResourceHumanoid>();
    }
}
