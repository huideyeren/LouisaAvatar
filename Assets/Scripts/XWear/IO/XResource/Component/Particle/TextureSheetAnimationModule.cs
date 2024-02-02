using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class TextureSheetAnimationModule
    {
        public bool enabled;
        public int numTilesX;
        public int numTilesY;
        public ParticleSystemAnimationMode mode;
        public ParticleSystemAnimationType animation;
        public ParticleSystemAnimationTimeMode timeMode;
        public XResourceMinMaxCurve frameOverTime;
        public float frameOverTimeMultiplier;
        public XResourceMinMaxCurve startFrame;
        public float startFrameMultiplier;
        public int cycleCount;
        public UVChannelFlags uvChannelMask;

        public TextureSheetAnimationModule() { }

        public TextureSheetAnimationModule(ParticleSystem.TextureSheetAnimationModule from)
        {
            enabled = from.enabled;
            mode = from.mode;
            numTilesX = from.numTilesX;
            numTilesY = from.numTilesY;
            animation = from.animation;
            timeMode = from.timeMode;
            frameOverTime = new XResourceMinMaxCurve(from.frameOverTime);
            frameOverTimeMultiplier = from.frameOverTimeMultiplier;
            startFrame = new XResourceMinMaxCurve(from.startFrame);
            startFrameMultiplier = from.startFrameMultiplier;
            cycleCount = from.cycleCount;
            uvChannelMask = from.uvChannelMask;
        }

        public void SetTo(ParticleSystem.TextureSheetAnimationModule to)
        {
            to.enabled = enabled;
            to.mode = mode;
            to.numTilesX = numTilesX;
            to.numTilesY = numTilesY;
            to.animation = animation;
            to.timeMode = timeMode;
            to.frameOverTime = frameOverTime.ToUnity();
            to.frameOverTimeMultiplier = frameOverTimeMultiplier;
            to.startFrame = startFrame.ToUnity();
            to.startFrameMultiplier = startFrameMultiplier;
            to.cycleCount = cycleCount;
            to.uvChannelMask = uvChannelMask;
        }
    }
}
