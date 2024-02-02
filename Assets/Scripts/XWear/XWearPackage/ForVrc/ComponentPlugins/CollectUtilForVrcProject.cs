using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.XWearPackage.ForVrc.ComponentPlugins
{
    public static class CollectUtilForVrcProject
    {
        public static string GetCollectedTransformGuidWithCheckNull(
            UnityEngine.Transform transform,
            GameObjectWithTransformCollector gameObjectCollector
        )
        {
            if (transform == null)
            {
                return "";
            }

            var result = CollectComponentUtil.GetCollectTransformRefGuid(
                gameObjectCollector,
                transform
            );

            return result;
        }

        public static string GetCollectedSmrGuidWithCheckNull(
            UnityEngine.SkinnedMeshRenderer smr,
            SkinnedMeshRendererDataCollector smrCollector
        )
        {
            if (smr == null)
            {
                return "";
            }

            var sourceTransform = smr.transform;
            return smrCollector.SourceSkinnedMeshRendererTransformToGuidMap.TryGetValue(
                sourceTransform,
                out var guid
            )
                ? guid
                : "";
        }
    }
}
