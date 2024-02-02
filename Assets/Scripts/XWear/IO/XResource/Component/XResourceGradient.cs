using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Component
{
    [Serializable]
    public class XResourceGradient
    {
        [Serializable]
        public class XResourceGradientColorKey
        {
            [JsonConverter(typeof(ColorConverter))]
            public Color color = Color.white;

            public float time;
        }

        [Serializable]
        public class XResourceGradientAlphaKey
        {
            public float alpha;
            public float time;
        }

        public GradientMode mode;
        public XResourceGradientColorKey[] colorKeys = Array.Empty<XResourceGradientColorKey>();
        public XResourceGradientAlphaKey[] alphaKeys = Array.Empty<XResourceGradientAlphaKey>();

        public XResourceGradient() { }

        public XResourceGradient(Gradient from)
        {
            if (from == null)
            {
                return;
            }

            mode = from.mode;
            colorKeys = from.colorKeys
                .Select(x => new XResourceGradientColorKey() { color = x.color, time = x.time, })
                .ToArray();

            alphaKeys = from.alphaKeys
                .Select(x => new XResourceGradientAlphaKey() { alpha = x.alpha, time = x.time })
                .ToArray();
        }

        public Gradient ToUnity()
        {
            return new Gradient()
            {
                mode = mode,
                colorKeys = colorKeys.Select(x => new GradientColorKey(x.color, x.time)).ToArray(),
                alphaKeys = alphaKeys.Select(x => new GradientAlphaKey(x.alpha, x.time)).ToArray(),
            };
        }
    }
}
