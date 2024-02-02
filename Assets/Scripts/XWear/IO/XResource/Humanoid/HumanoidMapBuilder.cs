using UnityEngine;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Humanoid
{
    public class HumanoidMapBuilder
    {
        public HumanoidMapComponent Build(
            HumanoidXResourceInstance xResourceInstance,
            XResourceHumanoidMap xResourceHumanoidMap,
            GameObjectWithTransformBuilder gameObjectBuilder,
            XItemMeta.XItemType xItemType,
            AssetSaver assetSaver = null
        )
        {
            var rootInstance = xResourceInstance.Instance;
            var result = new HumanoidMap();
            var sourceBones = xResourceHumanoidMap.HumanoidBones;
            foreach (var sourceBone in sourceBones)
            {
                var guid = sourceBone.BoneGuid;
                var destTransform = gameObjectBuilder.GetBuildTransformFromGuid(guid);
                var humanBodyBones = sourceBone.HumanBodyBones;
                result.AddHumanoidBone(
                    new HumanoidBone() { bone = destTransform, humanBodyBones = humanBodyBones, }
                );
            }

            var animator = rootInstance.AddComponent<Animator>();
            var attachedHumanoidMap = rootInstance.AddComponent<HumanoidMapComponent>();
            attachedHumanoidMap.LoadHumanoidMap(result);

            // XItemTypeがAvatarの場合、Humanoidなアバターを構築してアサインする
            if (xItemType == XItemMeta.XItemType.BaseModel)
            {
                var avatar = HumanoidAvatarUtil.GenerateFromHumanoidMap(
                    rootInstance.transform,
                    attachedHumanoidMap.HumanoidMap
                );

                avatar.name = $"Avatar-{rootInstance.name}";
                animator.avatar = avatar;

                if (assetSaver != null)
                {
                    animator.avatar = (UnityEngine.Avatar)assetSaver.CreateAsset(avatar);
                }
            }

            return attachedHumanoidMap;
        }
    }
}
