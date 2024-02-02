using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using XWear.IO.XResource;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.AvatarMask;
using XWear.IO.XResource.AvatarMeta;
using XWear.IO.XResource.Component;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.SwingParameter;
using XWear.IO.XResource.Transform;

namespace XWear.XWearPackage.ForVrc.ComponentPlugins
{
    public class VrcAvatarDescriptorCollectPlugin
        : DefaultCollectComponentPluginBase<VRCAvatarDescriptor>
    {
        private readonly Dictionary<AvatarMask, XResourceAvatarMask> _avatarMaskGenerateCache;

        public override int Order => 10;

        protected override void CopyComponent(
            Transform attachTarget,
            VRCAvatarDescriptor sourceComponent
        )
        {
            var result = attachTarget.gameObject.AddComponent<VRCAvatarDescriptor>();
            VrcAvatarDescriptorCopyUtil.Copy(source: sourceComponent, dest: result);
        }

        public override IXResourceComponent Collect(
            XItem xItem,
            MaterialCollectorBase materialCollector,
            GameObjectWithTransformCollector gameObjectCollector,
            SkinnedMeshRendererDataCollector skinnedMeshRendererCollector,
            XResourceContainerUtil.XResourceArchiver archiver
        )
        {
            var descriptor = Context.Get();
            return VrcAvatarDescriptorCollectUtil.ToXResource(
                descriptor,
                gameObjectCollector,
                skinnedMeshRendererCollector,
                archiver
            );
        }
    }
}
