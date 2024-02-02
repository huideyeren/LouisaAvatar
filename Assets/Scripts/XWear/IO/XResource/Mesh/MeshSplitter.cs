using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using XWear.IO.Common;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Transform;
using XWear.IO.XResource.Util;

namespace XWear.IO.XResource.Mesh
{
    public static class MeshSplitter
    {
        /// <summary>
        /// SkinnedMeshRenderer単位でGameObjectを分割し、XWearSourceを生成する
        /// 生成されるGameObjectはSkinnedMeshRendererを一つだけ持つ
        /// 各衣装パーツ間で同一のHumanoidMapを保持すること、および、PhysBoneなどの参照を破壊しない目的で
        /// ここでボーンを抜くような処理を実行してはならない
        /// ボーンを抜くなどの処理はアバターの合成処理側、あるいはエクスポートが実際に実行される前段階で実行される必要がある
        /// </summary>
        /// <param name="sourceSkinnedMeshRenderers"></param>
        /// <param name="sourceHumanoidMap"></param>
        /// <param name="collectComponentPlugins"></param>
        public static List<XWearSource> GenerateXWearSourcesWithSplitBySkinnedMeshRenderer(
            SkinnedMeshRenderer[] sourceSkinnedMeshRenderers,
            HumanoidMap sourceHumanoidMap,
            ICollectComponentPlugin[] collectComponentPlugins
        )
        {
            var result = new List<XWearSource>();
            for (var index = 0; index < sourceSkinnedMeshRenderers.Length; index++)
            {
                var skinnedMeshRenderer = sourceSkinnedMeshRenderers[index];

                var buildGameObjectResult = BuildDressPartGameObject(
                    skinnedMeshRenderer,
                    sourceHumanoidMap,
                    collectComponentPlugins
                );
                var dressPartRootGameObject = buildGameObjectResult.clothPartGameObject;

                // ウェイトの塗られていないHumanoidなボーンに対してPhysBoneColliderの参照がある、みたいなケースを考慮し、
                // たとえボーンウェイトが塗られていないBoneであっても削除や抜く処理を行わない

                result.Add(
                    new XWearSource(
                        new[]
                        {
                            new XWearSourceSkinnedMeshRenderer(
                                sourceSmrTransform: skinnedMeshRenderer.transform,
                                destSmr: skinnedMeshRenderer
                            )
                        },
                        dressPartRootGameObject,
                        //weightedHumanoidMap,
                        buildGameObjectResult.clothPartHumanoidMap,
                        buildGameObjectResult.sourceToEditTransformMap
                    )
                );
            }

            return result;
        }

        /// <summary>
        /// 与えられたSkinnedMeshRendererのbonesから構造を再構築し、編集用のGameObjectをつくる
        /// </summary>
        /// <param name="skinnedMeshRenderer"></param>
        /// <param name="sourceHumanoidMap">_humanoidMap.GetMap()から得られるマップ。
        /// {もともとのTransform,HumanBodyBones}のマップ。</param>
        /// <param name="collectComponentPlugins"></param>
        private static (
            GameObject clothPartGameObject,
            HumanoidMap clothPartHumanoidMap,
            Dictionary<UnityEngine.Transform, UnityEngine.Transform> sourceToEditTransformMap
        ) BuildDressPartGameObject(
            SkinnedMeshRenderer skinnedMeshRenderer,
            HumanoidMap sourceHumanoidMap,
            ICollectComponentPlugin[] collectComponentPlugins
        )
        {
            var weightedBones = GetWeightedBones(skinnedMeshRenderer);
            var sourceMap = sourceHumanoidMap.GetMap;
            // <src,dst>
            var splitRoot = new GameObject(skinnedMeshRenderer.name);
            var rootTrs = splitRoot.transform;
            var sourceBones = skinnedMeshRenderer.bones;

            // SkinnedMeshRenderer.bonesからヒエラルキー構造を再構築する
            var transformBuilder = new EditTransformBuilder();

            UnityEngine.Transform topBone = null;
            // ウェイトが塗られているSkinnedMeshRenderer.bonesの中で最も階層の高いボーンを取得する
            foreach (var bone in skinnedMeshRenderer.bones)
            {
                if (!weightedBones.Contains(bone))
                {
                    continue;
                }

                if (topBone == null)
                {
                    topBone = bone;
                    continue;
                }

                var grandChildren = bone.GetComponentsInChildren<UnityEngine.Transform>();
                if (grandChildren.Contains(topBone))
                {
                    topBone = bone;
                }
            }

            if (topBone == null)
            {
                throw new InvalidDataException("対応していない階層構造です");
            }

            // smr.rootBoneがtopBoneの階層以下に無かった場合、smr.rootBoneをtopBoneとして設定する
            // たとえば、靴のSkinnedMeshRendererのRootBoneがHipsに当たっているが、
            // Hipsにインフルエンスが設定されていない場合にRootBoneを参照することができなくなってしまうのを防ぐ
            // 単純にSmr.rootBoneをtopBoneに差し替えないのは、袖などが左右1メッシュであるときなどを考慮するため
            var topBoneChildren = topBone.GetComponentsInChildren<UnityEngine.Transform>();
            var rootBone = skinnedMeshRenderer.rootBone;
            if (!topBoneChildren.Contains(rootBone))
            {
                topBone = skinnedMeshRenderer.rootBone;
            }

            var topBoneParent = topBone.parent ? topBone.parent : topBone;
            transformBuilder.BuildTransforms(
                // SkinnedMeshRenderer.bonesの上にさらにTransformが設定されている場合を考慮する
                topBoneParent,
                rootTrs.gameObject
            );

            // ビルド後のHumanoidMapを構築する
            var destHumanoidMap = new HumanoidMap();

            // buildTransformMemoのうち、HumanoidBoneを再構築したTransformとHumanBodyBonesを対応させたHumanoidMapを作成する
            foreach (var builtTransform in transformBuilder.SourceToEditTransformMemo)
            {
                var buildSourceTransform = builtTransform.Key;
                var buildDestTransform = builtTransform.Value;
                if (sourceMap.TryGetValue(buildSourceTransform, out var humanBodyBones))
                {
                    destHumanoidMap.AddHumanoidBone(
                        new HumanoidBone()
                        {
                            bone = buildDestTransform,
                            humanBodyBones = humanBodyBones
                        }
                    );
                }
            }

            ComponentUtil.CopyBuiltComponentsWithOrder(transformBuilder, collectComponentPlugins);

            // 新たに構築したTransformの一覧をSkinnedMeshRenderer.bonesにアサインする
            var newBones = new UnityEngine.Transform[sourceBones.Length];
            for (int i = 0; i < sourceBones.Length; i++)
            {
                if (
                    transformBuilder.SourceToEditTransformMemo.TryGetValue(
                        sourceBones[i],
                        out var builtTransform
                    )
                )
                {
                    newBones[i] = builtTransform;
                }
            }

            skinnedMeshRenderer.bones = newBones;

            // 対象のSkinnedMeshRendererのTransformが構築されていなければ、
            // SkinnedMeshRendererのGameObjectをSplitRootの子に移す
            if (!transformBuilder.SkinnedMeshRendererTransformMemo.ContainsKey(skinnedMeshRenderer))
            {
                var smrTransform = skinnedMeshRenderer.transform;
                smrTransform.parent = splitRoot.transform;

                // TransformBuilder側に追加を強制する
                // SkinnedMeshRendererはボーン参照の解決の問題で、コンポーネントの親をすり替えるだけであるので、
                // sourceTransformとnewTransformGameObjectは同じものを指すことになる
                foreach (var t in smrTransform.Traverse())
                {
                    transformBuilder.ForceBuild(t, t.gameObject);
                }
            }

            return (
                clothPartGameObject: splitRoot,
                clothPartHumanoidMap: destHumanoidMap,
                sourceToEditTransformMap: transformBuilder.SourceToEditTransformMemo
            );
        }

        private static UnityEngine.Transform[] GetWeightedBones(SkinnedMeshRenderer smr)
        {
            var result = new List<UnityEngine.Transform>();
            var mesh = smr.sharedMesh;
            var boneWeights = mesh.boneWeights;
            foreach (var boneWeight in boneWeights)
            {
                if (boneWeight.weight0 > 0)
                {
                    var bone = smr.bones[boneWeight.boneIndex0];
                    if (!result.Contains(bone))
                    {
                        result.Add(bone);
                    }
                }

                if (boneWeight.weight1 > 0)
                {
                    var bone = smr.bones[boneWeight.boneIndex1];
                    if (!result.Contains(bone))
                    {
                        result.Add(bone);
                    }
                }

                if (boneWeight.weight2 > 0)
                {
                    var bone = smr.bones[boneWeight.boneIndex2];
                    if (!result.Contains(bone))
                    {
                        result.Add(bone);
                    }
                }

                if (boneWeight.weight3 > 0)
                {
                    var bone = smr.bones[boneWeight.boneIndex3];
                    if (!result.Contains(bone))
                    {
                        result.Add(bone);
                    }
                }
            }

            return result.Distinct().ToArray();
        }
    }
}
