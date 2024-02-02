using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.AvatarMeta
{
    [Serializable]
    public class VrcLipSync
    {
        public enum LipSyncStyle
        {
            Default,
            JawFlapBone,
            JawFlapBlendShape,
            VisemeBlendShape,
            VisemeParameterOnly,
        }

        public enum Viseme
        {
            sil,
            PP,
            FF,
            TH,
            DD,
            kk,
            CH,
            SS,
            nn,
            RR,
            aa,
            E,
            ih,
            oh,
            ou,
            Count,
        }

        public LipSyncStyle lipSyncStyle;
        public string visemeSkinnedMeshRendererGuid;
        public string[] visemeBlendShapes = Array.Empty<string>();

        public string jawFlapBoneGuid;

        [JsonConverter(typeof(QuaternionConverter))]
        public Quaternion jawOpen;

        [JsonConverter(typeof(QuaternionConverter))]
        public Quaternion jawClose;

        public string mouthOpenBlendShapeName;
    }
}
