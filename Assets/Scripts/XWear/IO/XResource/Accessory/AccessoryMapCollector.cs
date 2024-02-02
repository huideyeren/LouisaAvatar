using System.Linq;
using UnityEngine;
using XWear.IO.XResource.Humanoid;

namespace XWear.IO.XResource.Accessory
{
    public class AccessoryMapCollector
    {
        public XResourceAccessoryMap Collect(AccessoryMap accessoryMap)
        {
            var resultBones = new HumanBodyBones[accessoryMap.recommendHumanBodyBones.Length];
            for (int i = 0; i < resultBones.Length; i++)
            {
                resultBones[i] = accessoryMap.recommendHumanBodyBones[i];
            }

            return new XResourceAccessoryMap() { RecommendedHumanBodyBones = resultBones.ToList() };
        }
    }
}
