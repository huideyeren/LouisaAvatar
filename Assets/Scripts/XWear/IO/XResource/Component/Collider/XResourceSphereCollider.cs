using System;
using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Component.Collider
{
    [Serializable]
    public class XResourceSphereCollider : XResourceCollider<SphereCollider>
    {
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 center;

        public float radius;

        public XResourceSphereCollider() { }

        public XResourceSphereCollider(SphereCollider collider)
            : base(collider)
        {
            center = collider.center;
            radius = collider.radius;
        }

        public override void SetTo(SphereCollider to)
        {
            base.SetTo(to);
            to.center = center;
            to.radius = radius;
        }
    }
}
