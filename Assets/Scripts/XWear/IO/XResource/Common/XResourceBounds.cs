using System;
using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Common
{
    [Serializable]
    public class XResourceBounds
    {
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 center;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 extents;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 min;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 max;
    }
}
