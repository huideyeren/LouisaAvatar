using System;
using System.Linq;
using UnityEngine;
using XWear.IO.XResource.Common;

namespace XWear.IO.XResource.Animation
{
    public static class UnityAnimationClipUtil
    {
        public static AnimationClip ConvertToAnimationClip(XResourceAnimationClip source)
        {
            var result = new AnimationClip();
            result.name = source.name;
            result.frameRate = source.frameRate;
            result.wrapMode = source.wrapMode;
            if (source.localBounds != null)
            {
                result.localBounds = new Bounds()
                {
                    center = source.localBounds.center,
                    extents = source.localBounds.extents,
                    min = source.localBounds.min,
                    max = source.localBounds.max
                };
            }

            result.hideFlags = source.hideFlags;

            var curveBindings = source.curveBindings;
            foreach (var curveBinding in curveBindings)
            {
                SetCurveBindingsToAnimationClip(result, curveBinding);
            }

            return result;
        }

        private static void SetCurveBindingsToAnimationClip(
            AnimationClip targetClip,
            XResourceCurveBinding source
        )
        {
            var path = source.path;
            var propertyName = source.propertyName;
            var type = XResourceCurveBinding.GetObjectTypeFromBindingType(source);

            // コンポーネントがアサインされたカーブがAnimationClipの構築を実行した環境下に無い可能性がある
            if (source.bindingType == XResourceCurveBinding.CurveBindingType.Other && type == null)
            {
                Debug.LogWarning(
                    $"Type:{source.typeRawFullName},{source.typeRawAssemblyFullName} is not found."
                        + $"Skip set curve. "
                        + $"in: {targetClip.name}"
                );
                return;
            }

            var curve = source.curve.GetUnityAnimationCurve();

            targetClip.SetCurve(
                relativePath: path,
                type: type,
                propertyName: propertyName,
                curve: curve
            );
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor Only
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static XResourceAnimationClip ConvertToXResourceAnimation(AnimationClip source)
        {
            var result = new XResourceAnimationClip();
            result.guid = Guid.NewGuid().ToString();
            result.frameRate = source.frameRate;
            result.name = source.name;
            result.wrapMode = source.wrapMode;
            result.localBounds = new XResourceBounds()
            {
                center = source.localBounds.center,
                extents = source.localBounds.extents,
                min = source.localBounds.min,
                max = source.localBounds.max
            };

            var settings = UnityEditor.AnimationUtility.GetAnimationClipSettings(source);
            result.loopTime = settings.loopTime;

            result.hideFlags = source.hideFlags;

            if (source.events.Length > 0)
            {
                Debug.LogWarning("Animation Events are not supported.");
            }

            var curveBindings = UnityEditor.AnimationUtility.GetCurveBindings(source);
            result.curveBindings = new XResourceCurveBinding[curveBindings.Length];
            for (
                var curveBindingIndex = 0;
                curveBindingIndex < curveBindings.Length;
                curveBindingIndex++
            )
            {
                var curveBinding = curveBindings[curveBindingIndex];
                var xResourceCurveBinding = ConvertToXResourceCurveBinding(source, curveBinding);
                result.curveBindings[curveBindingIndex] = xResourceCurveBinding;
            }

            result.isSmrAnimation = curveBindings.All(x => x.type == typeof(SkinnedMeshRenderer));
            return result;
        }

        private static XResourceCurveBinding ConvertToXResourceCurveBinding(
            AnimationClip clipSource,
            UnityEditor.EditorCurveBinding source
        )
        {
            var result = new XResourceCurveBinding();
            XResourceCurveBinding.GetBindingTypeFromObjectType(
                source.type,
                out result.bindingType,
                out result.typeRawFullName,
                out result.typeRawAssemblyFullName
            );

            result.path = source.path;
            result.propertyName = source.propertyName;
            var curve = UnityEditor.AnimationUtility.GetEditorCurve(clipSource, source);
            result.curve = new XResourceAnimationCurve(curve);

            return result;
        }
#endif
    }
}
