using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Animation
{
    [Serializable]
    public class XResourceAnimationCurve
    {
        [JsonProperty(ItemConverterType = typeof(KeyframeConverter))]
        public Keyframe[] KeyFrames = Array.Empty<Keyframe>();

        public WrapMode preWrapMode = WrapMode.Default;
        public WrapMode postWrapMode = WrapMode.Default;

        public XResourceAnimationCurve(AnimationCurve animationCurve)
        {
            if (animationCurve == null)
            {
                return;
            }

            if (animationCurve.keys != null)
            {
                KeyFrames = animationCurve.keys.ToArray();
            }

            preWrapMode = animationCurve.preWrapMode;
            postWrapMode = animationCurve.postWrapMode;
        }

        public AnimationCurve GetUnityAnimationCurve()
        {
            var result = new AnimationCurve();
            result.preWrapMode = preWrapMode;
            result.postWrapMode = postWrapMode;

            foreach (var keyframe in KeyFrames)
            {
                result.AddKey(keyframe);
            }

            return result;
        }

        public XResourceAnimationCurve() { }

        public XResourceAnimationCurve DeepCopy()
        {
            return new XResourceAnimationCurve()
            {
                KeyFrames = (Keyframe[])KeyFrames.Clone(),
                preWrapMode = preWrapMode,
                postWrapMode = postWrapMode
            };
        }
    }
}
