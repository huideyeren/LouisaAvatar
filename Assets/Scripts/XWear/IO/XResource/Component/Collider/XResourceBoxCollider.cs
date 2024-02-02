using System;
using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Component.Collider
{
    [Serializable]
    public class XResourceBoxCollider : XResourceCollider<BoxCollider>
    {
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 center;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 size;

        public XResourceBoxCollider() { }

        public XResourceBoxCollider(BoxCollider collider)
            : base(collider)
        {
            center = collider.center;
            size = collider.size;
        }

        public override void SetTo(BoxCollider to)
        {
            base.SetTo(to);
            to.center = center;
            to.size = size;
        }
    }
}
