using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Transform
{
    public class XResourceTransform
    {
        public string Name;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 Position;

        [JsonConverter(typeof(QuaternionConverter))]
        public Quaternion Rotation;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 Scale;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 LocalPosition;

        [JsonConverter(typeof(QuaternionConverter))]
        public Quaternion LocalRotation;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 LocalScale;

        public int Index;
    }
}
