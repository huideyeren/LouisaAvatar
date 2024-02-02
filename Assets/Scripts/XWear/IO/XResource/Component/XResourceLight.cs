using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Component
{
    [Serializable]
    public class XResourceLight : IXResourceComponent
    {
        public bool enabled;
        public LightType type;
        public bool useColorTemperature;

        [JsonConverter(typeof(ColorConverter))]
        public Color color;

        public float colorTemperature;

        public LightmapBakeType lightmapBakeType;
        public float intensity;
        public float bounceIntensity;
        public LightShadows shadows;
        public float shadowAngle;
        public float shadowStrength;
        public LightShadowResolution shadowResolution;
        public float shadowBias;
        public float shadowNormalBias;
        public float shadowNearPlane;

        public LightRenderMode renderMode;
        public int cullingMask;

        public float range;
        public float spotAngle;

        public ComponentType ComponentType => ComponentType.Light;

        public UnityEngine.Component AttachTo(GameObject attachTarget)
        {
            return null;
        }

        public XResourceLight() { }

        public XResourceLight(Light from)
        {
            enabled = from.enabled;
            type = from.type;
            useColorTemperature = from.useColorTemperature;
            color = from.color;
            colorTemperature = from.colorTemperature;
            /*lightmapBakeType = from.lightmapBakeType;*/
            intensity = from.intensity;
            bounceIntensity = from.bounceIntensity;
            shadows = from.shadows;
            /*shadowAngle = from.shadowAngle;*/
            shadowStrength = from.shadowStrength;
            shadowResolution = from.shadowResolution;
            shadowBias = from.shadowBias;
            shadowNormalBias = from.shadowNormalBias;
            shadowNearPlane = from.shadowNearPlane;
            renderMode = from.renderMode;
            cullingMask = from.cullingMask;
            range = from.range;
            spotAngle = from.spotAngle;
        }

        public void SetTo(Light to)
        {
            to.enabled = enabled;
            to.type = type;
            to.useColorTemperature = useColorTemperature;
            to.color = color;
            to.colorTemperature = colorTemperature;
            /*to.lightmapBakeType = lightmapBakeType;*/
            to.intensity = intensity;
            to.bounceIntensity = bounceIntensity;
            to.shadows = shadows;
            /*to.shadowAngle = shadowAngle;*/
            to.shadowStrength = shadowStrength;
            to.shadowResolution = shadowResolution;
            to.shadowBias = shadowBias;
            to.shadowNormalBias = shadowNormalBias;
            to.shadowNearPlane = shadowNearPlane;
            to.renderMode = renderMode;
            to.cullingMask = cullingMask;
            to.range = range;
            to.spotAngle = spotAngle;
        }
    }
}
