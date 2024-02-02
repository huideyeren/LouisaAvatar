using System;
using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.AvatarMeta
{
    [Serializable]
    public class VrcCollider
    {
        [Serializable]
        public struct Config
        {
            public bool isMirrored;
            public int state;
            public string transformGuid;
            public float radius;
            public float height;

            [JsonConverter(typeof(Vector3Converter))]
            public Vector3 position;

            [JsonConverter(typeof(QuaternionConverter))]
            public Quaternion rotation;
        }

        public Config head;
        public Config torso;

        public Config handL;
        public Config handR;

        public Config footL;
        public Config footR;

        public Config fingerIndexL;
        public Config fingerIndexR;

        public Config fingerMiddleL;
        public Config fingerMiddleR;

        public Config fingerRingL;
        public Config fingerRingR;

        public Config fingerLittleL;
        public Config fingerLittleR;
    }
}
