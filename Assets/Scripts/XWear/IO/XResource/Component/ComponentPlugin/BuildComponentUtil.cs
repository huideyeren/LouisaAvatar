using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component.ComponentPlugin
{
    public static class BuildComponentUtil
    {
        public static UnityEngine.Transform GetBuildTransformRefFromGuid(
            GameObjectWithTransformBuilder gameObjectBuilder,
            string guid
        )
        {
            return gameObjectBuilder.GetBuildTransformFromGuid(guid);
        }
    }
}
