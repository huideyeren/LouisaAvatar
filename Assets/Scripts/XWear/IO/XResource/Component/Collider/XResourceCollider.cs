using System;
using UnityEngine;
using XWear.IO.XResource.Common;

namespace XWear.IO.XResource.Component.Collider
{
    [Serializable]
    public abstract class XResourceCollider<T> : IXResourceComponent
        where T : UnityEngine.Collider
    {
        public bool enabled;
        public XResourceBounds bounds;
        public ComponentType ComponentType => ComponentType.Collider;

        public UnityEngine.Component AttachTo(GameObject attachTarget)
        {
            return null;
        }

        protected XResourceCollider() { }

        protected XResourceCollider(UnityEngine.Collider collider)
        {
            enabled = collider.enabled;
            var b = collider.bounds;
            bounds = new XResourceBounds()
            {
                center = b.center,
                extents = b.extents,
                max = b.max,
                min = b.min,
            };
        }

        public virtual void SetTo(T to)
        {
            to.enabled = enabled;
            var b = to.bounds;
            b.center = bounds.center;
            b.extents = bounds.extents;
            b.max = bounds.max;
            b.min = bounds.min;
        }
    }
}
