using System;
using UnityEngine;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class InheritVelocityModule
    {
        public bool enabled;
        public ParticleSystemInheritVelocityMode mode;
        public XResourceMinMaxCurve curve;
        public float curveMultiplier;

        public InheritVelocityModule() { }

        public InheritVelocityModule(ParticleSystem.InheritVelocityModule from)
        {
            enabled = from.enabled;
            mode = from.mode;
            curve = new XResourceMinMaxCurve(from.curve);
            curveMultiplier = from.curveMultiplier;
        }

        public void SetTo(ParticleSystem.InheritVelocityModule to)
        {
            to.enabled = enabled;
            to.mode = mode;
            to.curve = curve.ToUnity();
            to.curveMultiplier = curveMultiplier;
        }
    }
}
