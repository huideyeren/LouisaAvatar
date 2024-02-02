using System;

namespace XWear.IO.XResource.AvatarMeta
{
    [Serializable]
    public class VrcAnimationLayers
    {
        [Serializable]
        public class VrcPlayableLayers
        {
            public bool isEnabled;
            public int layerType;
            public bool isDefault;
            public string animatorGuid;
        }

        public bool customizeAnimationLayers;
        public VrcPlayableLayers[] baseAnimationLayers = Array.Empty<VrcPlayableLayers>();
        public VrcPlayableLayers[] specialAnimationLayers = Array.Empty<VrcPlayableLayers>();
    }
}
