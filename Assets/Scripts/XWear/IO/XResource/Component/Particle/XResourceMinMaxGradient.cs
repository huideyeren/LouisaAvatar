using System;
using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class XResourceMinMaxGradient
    {
        public ParticleSystemGradientMode mode;
        public XResourceGradient gradientMin;
        public XResourceGradient gradientMax;

        [JsonConverter(typeof(ColorConverter))]
        public Color colorMin = Color.white;

        [JsonConverter(typeof(ColorConverter))]
        public Color colorMax = Color.white;

        public XResourceMinMaxGradient() { }

        public XResourceMinMaxGradient(ParticleSystem.MinMaxGradient from)
        {
            mode = from.mode;
            gradientMin = new XResourceGradient(from.gradientMin);
            gradientMax = new XResourceGradient(from.gradientMax);
            colorMin = from.colorMin;
            colorMax = from.colorMax;
        }

        public ParticleSystem.MinMaxGradient ToUnity()
        {
            return new ParticleSystem.MinMaxGradient()
            {
                mode = mode,
                gradientMin = gradientMin.ToUnity(),
                gradientMax = gradientMax.ToUnity(),
                colorMin = colorMin,
                colorMax = colorMax,
            };
        }
    }
}
