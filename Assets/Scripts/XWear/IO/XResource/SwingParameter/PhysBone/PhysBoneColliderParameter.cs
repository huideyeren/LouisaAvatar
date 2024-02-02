using System;
using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.SwingParameter.PhysBone
{
    [Serializable]
    public class PhysBoneColliderParameter
    {
        public string rootTransformRef;
        public bool isAtHumanoidBone;
        public HumanBodyBones humanBodyBones;

        #region Shape

        public int shapeType;
        public float radius;
        public float height;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 position;

        [JsonConverter(typeof(QuaternionConverter))]
        public Quaternion rotation;

        public bool insideBounds;
        public bool bonesAsSpheres;

        #endregion

        public PhysBoneColliderParameter() { }

        public PhysBoneColliderParameter(PhysBoneColliderParameter source)
        {
            rootTransformRef = source.rootTransformRef;
            shapeType = source.shapeType;
            radius = source.radius;
            height = source.height;
            position = source.position;
            rotation = source.rotation;
            insideBounds = source.insideBounds;
            bonesAsSpheres = source.bonesAsSpheres;
        }
    }
}
