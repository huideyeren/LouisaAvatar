using System;
using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class ColorBySpeedModule
    {
        public bool enabled;
        public XResourceMinMaxGradient color;

        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 range;

        public ColorBySpeedModule() { }

        public ColorBySpeedModule(ParticleSystem.ColorBySpeedModule from)
        {
            enabled = from.enabled;
            color = new XResourceMinMaxGradient(from.color);
            range = from.range;
        }

        public void SetTo(ParticleSystem.ColorBySpeedModule to)
        {
            to.enabled = enabled;
            to.color = color.ToUnity();
            to.range = range;
        }
    }
}
