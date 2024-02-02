using System;
using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Component.Collider
{
    [Serializable]
    public class XResourceCapsuleCollider : XResourceCollider<CapsuleCollider>
    {
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 center;

        public int direction;
        public float height;
        public float radius;

        public XResourceCapsuleCollider() { }

        public XResourceCapsuleCollider(CapsuleCollider collider)
            : base(collider)
        {
            center = collider.center;
            direction = collider.direction;
            height = collider.height;
            radius = collider.radius;
        }

        public override void SetTo(CapsuleCollider to)
        {
            base.SetTo(to);
            to.center = center;
            to.direction = direction;
            to.height = height;
            to.radius = radius;
        }
    }
}
