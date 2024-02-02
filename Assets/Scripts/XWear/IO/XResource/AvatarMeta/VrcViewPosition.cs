using System;
using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.AvatarMeta
{
    [Serializable]
    public class VrcViewPosition
    {
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 position;
    }
}
