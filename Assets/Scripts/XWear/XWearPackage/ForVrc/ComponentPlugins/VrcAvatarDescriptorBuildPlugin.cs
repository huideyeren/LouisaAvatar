using UnityEngine;
using VRC.SDK3.Avatars.Components;
using XWear.IO;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.AvatarMeta;
using XWear.IO.XResource.Component;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.SwingParameter;
using XWear.IO.XResource.Transform;

namespace XWear.XWearPackage.ForVrc.ComponentPlugins
{
    public class VrcAvatarDescriptorBuildPlugin
        : DefaultBuildComponentPluginBase<XResourceVrcAvatarDescriptorComponent>
    {
        public override int Order => 10;

        public override Component BuildAndAttach(
            GameObjectWithTransformBuilder gameObjectBuilder,
            SkinnedMeshRendererDataBuilder skinnedMeshRendererBuilder,
            MaterialBuilderBase materialBuilder,
            GameObject attachTarget,
            XResourceContainerUtil.XResourceOpener opener,
            AssetSaver assetSaver
        )
        {
            var vrcAvatarDescriptor = attachTarget.AddComponent<VRCAvatarDescriptor>();
            var param = Context.Get().Param;

            VrcAvatarDescriptorBuildUtil.SetFromXResource(
                param,
                vrcAvatarDescriptor,
                gameObjectBuilder,
                skinnedMeshRendererBuilder,
                opener,
                assetSaver
            );

            return vrcAvatarDescriptor;
        }
    }
}
