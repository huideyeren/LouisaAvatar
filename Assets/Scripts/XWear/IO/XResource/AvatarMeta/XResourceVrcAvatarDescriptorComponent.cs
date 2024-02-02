using System;
using System.Collections.Generic;
using UnityEngine;
using XWear.IO.XResource.Animation;
using XWear.IO.XResource.AvatarMask;
using XWear.IO.XResource.Component;

namespace XWear.IO.XResource.AvatarMeta
{
    public class XResourceVrcAvatarDescriptorComponent : IXResourceComponent
    {
        public ComponentType ComponentType => ComponentType.VrcAvatarDescriptor;
        public VrcAvatarDescriptorParam Param;

        public UnityEngine.Component AttachTo(GameObject attachTarget)
        {
            // do nothing
            return null;
        }

        public XResourceVrcAvatarDescriptorComponent(VrcAvatarDescriptorParam param)
        {
            Param = param;
        }

        public XResourceVrcAvatarDescriptorComponent() { }
    }
}
