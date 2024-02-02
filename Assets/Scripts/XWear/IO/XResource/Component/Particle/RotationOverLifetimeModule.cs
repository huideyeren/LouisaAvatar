using System;
using UnityEngine;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class RotationOverLifetimeModule
    {
        public bool enabled;
        public bool separateAxes;
        public XResourceMinMaxCurve x;
        public XResourceMinMaxCurve y;
        public XResourceMinMaxCurve z;
        public float xMultiplier;
        public float yMultiplier;
        public float zMultiplier;

        public RotationOverLifetimeModule() { }

        public RotationOverLifetimeModule(ParticleSystem.RotationOverLifetimeModule from)
        {
            enabled = from.enabled;
            separateAxes = from.separateAxes;
            x = new XResourceMinMaxCurve(from.x);
            y = new XResourceMinMaxCurve(from.y);
            z = new XResourceMinMaxCurve(from.z);
            xMultiplier = from.xMultiplier;
            yMultiplier = from.yMultiplier;
            zMultiplier = from.zMultiplier;
        }

        public void SetTo(ParticleSystem.RotationOverLifetimeModule to)
        {
            to.enabled = enabled;
            to.separateAxes = separateAxes;
            to.x = x.ToUnity();
            to.y = y.ToUnity();
            to.z = z.ToUnity();
            to.xMultiplier = xMultiplier;
            to.yMultiplier = yMultiplier;
            to.zMultiplier = zMultiplier;
        }
    }
}
