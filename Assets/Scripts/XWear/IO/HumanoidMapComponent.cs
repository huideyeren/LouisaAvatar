using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XWear.IO.XResource.Humanoid;

namespace XWear.IO
{
    public class HumanoidMapComponent : MonoBehaviour
    {
        [SerializeField]
        private HumanoidMap humanoidMap = new HumanoidMap();

        public HumanoidMap HumanoidMap => humanoidMap;

        [ContextMenu("Auto Assign")]
        public void AutoAssign()
        {
            HumanoidMapGenerateUtil.TryGenerate(gameObject, out var result);

            LoadHumanoidMap(result);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

        public void LoadHumanoidMap(HumanoidMap source)
        {
            humanoidMap = source;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }
    }

    [Serializable]
    public class HumanoidMap
    {
        [SerializeField]
        public List<HumanoidBone> humanoidBones = new List<HumanoidBone>();

        public Dictionary<Transform, HumanBodyBones> GetMap
        {
            get
            {
                return humanoidBones
                    .Where(x => x.bone != null)
                    .ToDictionary(x => x.bone, x => x.humanBodyBones);
            }
        }

        public void AddHumanoidBone(HumanoidBone humanoidBone)
        {
            humanoidBones.Add(humanoidBone);
        }

        public static HumanoidMap CreateFromDictionary(Dictionary<Transform, HumanBodyBones> dict)
        {
            var newMap = new HumanoidMap { humanoidBones = new List<HumanoidBone>() };

            foreach (var kvp in dict)
            {
                newMap.humanoidBones.Add(
                    new HumanoidBone() { bone = kvp.Key, humanBodyBones = kvp.Value }
                );
            }

            return newMap;
        }
    }

    [Serializable]
    public class HumanoidBone
    {
        public HumanBodyBones humanBodyBones;
        public Transform bone;
    }
}
