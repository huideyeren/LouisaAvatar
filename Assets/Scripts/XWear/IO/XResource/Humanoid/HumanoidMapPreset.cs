using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace XWear.IO.XResource.Humanoid
{
    public class HumanoidMapPreset : ScriptableObject
    {
        [Serializable]
        public class HumanoidBoneDefine
        {
            public string name;
            public HumanBodyBones humanBodyBones;
        }

        public List<HumanoidBoneDefine> humanoidBoneDefines = new List<HumanoidBoneDefine>();

        public static HumanoidMapPreset[] LoadPresets()
        {
            return Resources.LoadAll<HumanoidMapPreset>("HumanoidPresets");
        }
    }
}
