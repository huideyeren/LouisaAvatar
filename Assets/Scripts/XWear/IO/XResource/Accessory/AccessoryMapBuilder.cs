using UnityEngine;
using XWear.IO.XResource.Humanoid;

namespace XWear.IO.XResource.Accessory
{
    public class AccessoryMapBuilder
    {
        public AccessoryMapComponent Build(
            AccessoryXResourceInstance xResourceInstance,
            XResourceAccessoryMap xResourceAccessoryMap,
            ExportContext.ExportType exportType
        )
        {
            var rootInstance = xResourceInstance.Instance;
            var resultMap = new AccessoryMap();
            resultMap.recommendHumanBodyBones = new HumanBodyBones[
                xResourceAccessoryMap.RecommendedHumanBodyBones.Count
            ];
            for (
                var index = 0;
                index < xResourceAccessoryMap.RecommendedHumanBodyBones.Count;
                index++
            )
            {
                var sourceBone = xResourceAccessoryMap.RecommendedHumanBodyBones[index];
                resultMap.recommendHumanBodyBones[index] = sourceBone;
            }

            var newComponent = rootInstance.AddComponent<AccessoryMapComponent>();
            newComponent.LoadAccessoryMap(resultMap);
            return newComponent;
        }
    }
}
