using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace XWear.IO.XResource.Animation
{
    public static class UnityAnimatorUtil
    {
#if UNITY_EDITOR
        private static readonly string TMPAssetDir = Path.Combine("Assets", "XWearTmp");

        public static void ConvertToXResourceAnimator(
            UnityEditor.Animations.AnimatorController source,
            Dictionary<AnimationClip, XResourceAnimationClip> clipInstanceToXResource
        )
        {
            var animatorControllerGuid = Guid.NewGuid().ToString();

            // AnimatorControllerをアセット化する
            var tmpAssetPath = Path.Combine(TMPAssetDir, $"{animatorControllerGuid}.controller");
            UnityEditor.AssetDatabase.CreateAsset(source, tmpAssetPath);
            var tmpAssetBinary = File.ReadAllBytes(tmpAssetPath);

            // AnimationClipを集めつつ、AnimatorControllerの中身の情報を抜いていく
            var sourceLayers = source.layers;
            var resultLayers = new XResourceAnimatorLayer[sourceLayers.Length];
            for (int layerIndex = 0; layerIndex < sourceLayers.Length; layerIndex++)
            {
                resultLayers[layerIndex] = ConvertToXResourceAnimatorLayer(
                    sourceLayers[layerIndex],
                    layerIndex,
                    clipInstanceToXResource
                );
            }

            var result = new XResourceAnimatorController()
            {
                guid = animatorControllerGuid,
                layers = resultLayers
            };

            /*using (var ms = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    var entry = zipArchive.CreateEntry(animatorControllerGuid);
                    using (var es = entry.Open())
                    {
                        es.Write(tmpAssetBinary, 0, tmpAssetBinary.Length);
                    }
                }
            }*/
        }

        private static XResourceAnimatorLayer ConvertToXResourceAnimatorLayer(
            UnityEditor.Animations.AnimatorControllerLayer source,
            int layerIndex,
            Dictionary<AnimationClip, XResourceAnimationClip> clipInstanceToXResource
        )
        {
            var result = new XResourceAnimatorLayer();
            result.layerIndex = layerIndex;
            result.states = ConvertToXResourceAnimatorStates(
                source.stateMachine,
                clipInstanceToXResource
            );
            return result;
        }

        private static XResourceAnimatorState[] ConvertToXResourceAnimatorStates(
            UnityEditor.Animations.AnimatorStateMachine stateMachine,
            Dictionary<AnimationClip, XResourceAnimationClip> clipInstanceToXResource
        )
        {
            var sourceStates = stateMachine.states;
            var result = new XResourceAnimatorState[sourceStates.Length];
            for (int i = 0; i < sourceStates.Length; i++)
            {
                var source = sourceStates[i].state;
                var sourceMotion = source.motion as AnimationClip;
                if (sourceMotion == null)
                {
                    continue;
                }

                XResourceAnimationClip xResourceAnimationClip;
                if (!clipInstanceToXResource.TryGetValue(sourceMotion, out xResourceAnimationClip))
                {
                    xResourceAnimationClip = UnityAnimationClipUtil.ConvertToXResourceAnimation(
                        sourceMotion
                    );
                    clipInstanceToXResource.Add(sourceMotion, xResourceAnimationClip);
                }

                var clipGuid = xResourceAnimationClip.guid;

                result[i] = new XResourceAnimatorState() { motionGuid = clipGuid, };
            }

            return result;
        }
#endif
    }
}
