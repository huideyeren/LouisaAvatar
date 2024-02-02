using System;
using UnityEngine;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class SizeOverLifetimeModule
    {
        public bool enabled;
        public bool separateAxes;
        public XResourceMinMaxCurve x;
        public XResourceMinMaxCurve y;
        public XResourceMinMaxCurve z;
        public float xMultiplier;
        public float yMultiplier;
        public float zMultiplier;
        public XResourceMinMaxCurve size;
        public float sizeMultiplier;

        public SizeOverLifetimeModule() { }

        public SizeOverLifetimeModule(ParticleSystem.SizeOverLifetimeModule from)
        {
            enabled = from.enabled;
            separateAxes = from.separateAxes;
            x = new XResourceMinMaxCurve(from.x);
            y = new XResourceMinMaxCurve(from.y);
            z = new XResourceMinMaxCurve(from.z);
            xMultiplier = from.xMultiplier;
            yMultiplier = from.yMultiplier;
            zMultiplier = from.zMultiplier;

            size = new XResourceMinMaxCurve(from.size);
            sizeMultiplier = from.sizeMultiplier;
        }

        public void SetTo(ParticleSystem.SizeOverLifetimeModule to)
        {
            to.enabled = enabled;
            to.separateAxes = separateAxes;
            to.x = x.ToUnity();
            to.y = y.ToUnity();
            to.z = z.ToUnity();
            to.xMultiplier = xMultiplier;
            to.yMultiplier = yMultiplier;
            to.zMultiplier = zMultiplier;

            to.size = size.ToUnity();
            to.sizeMultiplier = sizeMultiplier;
        }
    }
}
