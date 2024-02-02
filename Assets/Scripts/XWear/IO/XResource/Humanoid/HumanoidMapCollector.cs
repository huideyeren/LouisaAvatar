using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Humanoid
{
    public class HumanoidMapCollector
    {
        public XResourceHumanoidMap Collect(
            HumanoidMap sourceHumanoidMap,
            GameObjectWithTransformCollector gameObjectWithTransformCollector
        )
        {
            var result = new XResourceHumanoidMap();

            var sourceHumanoidBones = sourceHumanoidMap.GetMap;
            foreach (var kvp in sourceHumanoidBones)
            {
                var sourceHumanoidTransform = kvp.Key;
                var humanBodyBones = kvp.Value;

                var guid = gameObjectWithTransformCollector.GetXResourceGameObjectGuidFromTransform(
                    sourceHumanoidTransform
                );

                gameObjectWithTransformCollector.MarkIsHumanoidTransform(
                    sourceHumanoidTransform,
                    humanBodyBones
                );

                result.HumanoidBones.Add(
                    new XResourceHumanoidMap.XResourceHumanoid()
                    {
                        HumanBodyBones = humanBodyBones,
                        BoneGuid = guid
                    }
                );
            }

            return result;
        }
    }
}
