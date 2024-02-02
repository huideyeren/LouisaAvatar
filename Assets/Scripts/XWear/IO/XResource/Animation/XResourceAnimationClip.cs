using System;
using UnityEngine;
using UnityEngine.Serialization;
using XWear.IO.XResource.Common;

namespace XWear.IO.XResource.Animation
{
    [Serializable]
    public class XResourceAnimationClip
    {
        public const string ZeroAnimationClipName = "ZeroFacial";
        public const string BlinkZeroAnimationClipName = "blink_Inverse";

        public string guid;
        public string name;
        public float frameRate;
        public WrapMode wrapMode;
        public XResourceBounds localBounds;
        public HideFlags hideFlags;
        public XResourceCurveBinding[] curveBindings;
        public bool loopTime;
        public bool isSmrAnimation;
    }

    [Serializable]
    public class XResourceCurveBinding
    {
        public enum CurveBindingType
        {
            Other = 0,
            Animator = 1,
            SkinnedMeshRenderer = 2,
            Transform = 3,
            GameObject = 4,
            Particle = 5,
        }

        public string path;
        public string propertyName;
        public CurveBindingType bindingType;

        // AnimationClip中にコンポーネントのアクティブなどを切り替えている場合、Typeを得る必要がある
        // Typeをそのまま持つと、コンポーネントが別アセンブリにあるなどでシリアライズに失敗するので、
        // 文字列でType名とアセンブリ名を持ち、AnimationClipの構築時に解決を試みる
        public string typeRawFullName;
        public string typeRawAssemblyFullName;
        public XResourceAnimationCurve curve;

        public static void GetBindingTypeFromObjectType(
            Type t,
            out CurveBindingType bindingType,
            out string typeRawFullName,
            out string typeRawAssemblyFullName
        )
        {
            typeRawFullName = "";
            typeRawAssemblyFullName = "";
            if (t == typeof(Animator))
            {
                bindingType = CurveBindingType.Animator;
            }
            else if (t == typeof(SkinnedMeshRenderer))
            {
                bindingType = CurveBindingType.SkinnedMeshRenderer;
            }
            else if (t == typeof(UnityEngine.Transform))
            {
                bindingType = CurveBindingType.Transform;
            }
            else if (t == typeof(GameObject))
            {
                bindingType = CurveBindingType.GameObject;
            }
            else
            {
                bindingType = CurveBindingType.Other;
                typeRawFullName = t.FullName;
                typeRawAssemblyFullName = t.Assembly.FullName;
            }
        }

        public static Type GetObjectTypeFromBindingType(XResourceCurveBinding curveBinding)
        {
            var t = curveBinding.bindingType;
            var typeFullName = curveBinding.typeRawFullName;
            var typeAssemblyFullName = curveBinding.typeRawAssemblyFullName;
            switch (t)
            {
                case CurveBindingType.Animator:
                    return typeof(Animator);
                case CurveBindingType.SkinnedMeshRenderer:
                    return typeof(SkinnedMeshRenderer);
                case CurveBindingType.Transform:
                    return typeof(UnityEngine.Transform);
                case CurveBindingType.GameObject:
                    return typeof(GameObject);
                case CurveBindingType.Particle:
                    return typeof(ParticleSystem.Particle);
                case CurveBindingType.Other:
                    return Type.GetType($"{typeFullName},{typeAssemblyFullName}");
                default:
                    throw new ArgumentOutOfRangeException(nameof(t), t, null);
            }
        }
    }
}
