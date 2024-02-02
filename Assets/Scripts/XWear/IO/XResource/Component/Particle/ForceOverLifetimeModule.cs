using System;
using UnityEngine;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class ForceOverLifetimeModule
    {
        public bool enabled;
        public XResourceMinMaxCurve x;
        public XResourceMinMaxCurve y;
        public XResourceMinMaxCurve z;
        public float xMultiplier;
        public float yMultiplier;
        public float zMultiplier;
        public ParticleSystemSimulationSpace space;
        public bool randomized;

        public ForceOverLifetimeModule() { }

        public ForceOverLifetimeModule(ParticleSystem.ForceOverLifetimeModule from)
        {
            enabled = from.enabled;
            x = new XResourceMinMaxCurve(from.x);
            y = new XResourceMinMaxCurve(from.y);
            z = new XResourceMinMaxCurve(from.z);
            xMultiplier = from.xMultiplier;
            yMultiplier = from.yMultiplier;
            zMultiplier = from.zMultiplier;
            space = from.space;
            randomized = from.randomized;
        }

        public void SetTo(ParticleSystem.ForceOverLifetimeModule to)
        {
            to.enabled = enabled;
            to.x = x.ToUnity();
            to.y = y.ToUnity();
            to.z = z.ToUnity();
            to.xMultiplier = xMultiplier;
            to.yMultiplier = yMultiplier;
            to.zMultiplier = zMultiplier;
            to.space = space;
            to.randomized = randomized;
        }
    }
}
