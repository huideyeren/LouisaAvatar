using System;
using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class RotationBySpeedModule
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

        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 range;

        public RotationBySpeedModule() { }

        public RotationBySpeedModule(ParticleSystem.RotationBySpeedModule from)
        {
            enabled = from.enabled;
            separateAxes = from.separateAxes;
            x = new XResourceMinMaxCurve(from.x);
            y = new XResourceMinMaxCurve(from.y);
            z = new XResourceMinMaxCurve(from.z);
            xMultiplier = from.xMultiplier;
            yMultiplier = from.yMultiplier;
            zMultiplier = from.zMultiplier;
            range = from.range;
        }

        public void SetTo(ParticleSystem.RotationBySpeedModule to)
        {
            to.enabled = enabled;
            to.separateAxes = separateAxes;
            to.x = x.ToUnity();
            to.y = y.ToUnity();
            to.z = z.ToUnity();
            to.xMultiplier = xMultiplier;
            to.yMultiplier = yMultiplier;
            to.zMultiplier = zMultiplier;
            to.range = range;
        }
    }
}
