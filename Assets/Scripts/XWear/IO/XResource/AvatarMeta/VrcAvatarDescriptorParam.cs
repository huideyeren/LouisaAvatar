using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace XWear.IO.XResource.AvatarMeta
{
    [Serializable]
    public class VrcAvatarDescriptorParam
    {
        public VrcViewPosition viewPosition = new VrcViewPosition();
        public VrcLipSync lipSync = new VrcLipSync();
        public VrcEyeLook eyeLook = new VrcEyeLook();
        public VrcAnimationLayers animationLayers = new VrcAnimationLayers();
        public VrcLowerBody lowerBody = new VrcLowerBody();
        public VrcCollider collider = new VrcCollider();
        public VrcExpressions expressions = new VrcExpressions();

        [SerializeReference]
        public List<VrcAssetResource> vrcAssetResources = new List<VrcAssetResource>();
    }
}
