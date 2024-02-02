using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component.ComponentPlugin
{
    public static class CollectComponentUtil
    {
        public static string GetCollectTransformRefGuid(
            GameObjectWithTransformCollector gameObjectCollector,
            UnityEngine.Transform avatarSourceTransform
        )
        {
            return gameObjectCollector.GetXResourceGameObjectGuidFromAvatarSourceTransform(
                avatarSourceTransform
            );
        }
    }
}
