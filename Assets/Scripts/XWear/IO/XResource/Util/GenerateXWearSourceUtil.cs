using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Util
{
    public static class GenerateXWearSourceUtil
    {
        /// <summary>
        /// 着せ替え後のアバターとして出力するためのXWearSourceを生成する
        /// ここではSkinnedMeshRendererの分割は実行されない
        /// </summary>
        /// <param name="rootGameObject"></param>
        /// <param name="sourceHumanoidMap"></param>
        /// <param name="collectComponentPlugins"></param>
        /// <returns></returns>
        public static XWearSource GenerateWithNoSplitSkinnedMeshRenderers(
            GameObject rootGameObject,
            HumanoidMap sourceHumanoidMap,
            ICollectComponentPlugin[] collectComponentPlugins
        )
        {
            // RootからすべてのTransformを構築する
            var rootTransform = rootGameObject.transform;
            var transformBuilder = new EditTransformBuilder();

            var builtRootTransform = transformBuilder.BuildTransforms(rootTransform);

            // ビルド後のHumanoidMapを構築する
            var destHumanoidMap = new HumanoidMap();
            var sourceMap = sourceHumanoidMap.GetMap;

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

            // ここで外部コンポーネントのコピーを実施する
            // この段階ではTransformの参照はSourceTransformのままなので注意
            var collectedComponentCaches =
                new Dictionary<
                    ICollectComponentPlugin,
                    List<(
                        UnityEngine.Component sourceComponent,
                        UnityEngine.Transform destEditTransform
                    )>
                >();
            foreach (var builtTransform in transformBuilder.SourceToEditTransformMemo)
            {
                var sourceAttachedComponents =
                    builtTransform.Key.gameObject.GetComponents<UnityEngine.Component>();
                var destEditTransform = builtTransform.Value;
                foreach (var sourceComponent in sourceAttachedComponents)
                {
                    foreach (var collectComponentPlugin in collectComponentPlugins)
                    {
                        if (
                            !collectedComponentCaches.TryGetValue(
                                collectComponentPlugin,
                                out var componentData
                            )
                        )
                        {
                            componentData =
                                new List<(UnityEngine.Component, UnityEngine.Transform)>();
                            collectedComponentCaches.Add(collectComponentPlugin, componentData);
                        }

                        if (collectComponentPlugin.CheckIsValid(sourceComponent))
                        {
                            componentData.Add((sourceComponent, destEditTransform));
                        }
                    }
                }
            }

            var orderedCachedPlugin = collectedComponentCaches.OrderBy(x => x.Key.Order);
            foreach (var kvp in orderedCachedPlugin)
            {
                var plugin = kvp.Key;
                var componentDataList = kvp.Value;
                foreach (var componentData in componentDataList)
                {
                    var attachTarget = componentData.destEditTransform;
                    var sourceComponent = componentData.sourceComponent;
                    plugin.CopyComponent(attachTarget, sourceComponent);
                }
            }

            var skinnedMeshRenderers = rootTransform.GetComponentsInChildren<SkinnedMeshRenderer>();
            var resultSkinnedMeshRenderers = new XWearSourceSkinnedMeshRenderer[
                skinnedMeshRenderers.Length
            ];
            for (var index = 0; index < skinnedMeshRenderers.Length; index++)
            {
                var skinnedMeshRenderer = skinnedMeshRenderers[index];
                var sourceBones = skinnedMeshRenderer.bones;
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

                // todo ここの分岐はいらないかも
                // なぜならアバター出力時はすでにSkinnedMeshRendererTransformは移動済みなので
                // 対象のSkinnedMeshRendererのTransformが構築されていなければ、
                // SkinnedMeshRendererのGameObjectをSplitRootの子に移す
                if (
                    !transformBuilder.SkinnedMeshRendererTransformMemo.TryGetValue(
                        skinnedMeshRenderer,
                        out var buildSmrTransform
                    )
                )
                {
                    skinnedMeshRenderer.gameObject.transform.parent = builtRootTransform;
                    resultSkinnedMeshRenderers[index] = new XWearSourceSkinnedMeshRenderer(
                        sourceSmrTransform: skinnedMeshRenderer.transform,
                        destSmr: skinnedMeshRenderer
                    );
                    // todo rootBone
                }
                else
                {
                    // 構築されている場合、コンポーネントをビルド結果にアタッチする
                    var newAttachedSmr =
                        buildSmrTransform.gameObject.CopyAndAttachSkinnedMeshRenderer(
                            skinnedMeshRenderer
                        );
                    resultSkinnedMeshRenderers[index] = new XWearSourceSkinnedMeshRenderer(
                        sourceSmrTransform: skinnedMeshRenderer.transform,
                        destSmr: newAttachedSmr
                    );
                }
            }

            var xWearSource = new XWearSource(
                resultSkinnedMeshRenderers,
                builtRootTransform.gameObject,
                destHumanoidMap,
                transformBuilder.SourceToEditTransformMemo
            );

            return xWearSource;
        }
    }

    internal static class ComponentUtil
    {
        public static void CopyBuiltComponentsWithOrder(
            EditTransformBuilder transformBuilder,
            ICollectComponentPlugin[] collectComponentPlugins
        )
        {
            // ここで外部コンポーネントのコピーを実施する
            // この段階ではTransformの参照はSourceTransformのままなので注意
            var collectedComponentCaches =
                new Dictionary<
                    ICollectComponentPlugin,
                    List<(
                        UnityEngine.Component sourceComponent,
                        UnityEngine.Transform destEditTransform
                    )>
                >();

            foreach (var builtTransform in transformBuilder.SourceToEditTransformMemo)
            {
                var sourceAttachedComponents =
                    builtTransform.Key.gameObject.GetComponents<UnityEngine.Component>();
                var destEditTransform = builtTransform.Value;
                foreach (var sourceComponent in sourceAttachedComponents)
                {
                    foreach (var collectComponentPlugin in collectComponentPlugins)
                    {
                        if (
                            !collectedComponentCaches.TryGetValue(
                                collectComponentPlugin,
                                out var componentData
                            )
                        )
                        {
                            componentData =
                                new List<(UnityEngine.Component, UnityEngine.Transform)>();
                            collectedComponentCaches.Add(collectComponentPlugin, componentData);
                        }

                        if (collectComponentPlugin.CheckIsValid(sourceComponent))
                        {
                            componentData.Add((sourceComponent, destEditTransform));
                        }
                    }
                }
            }

            var orderedCachedPlugin = collectedComponentCaches.OrderBy(x => x.Key.Order);
            foreach (var kvp in orderedCachedPlugin)
            {
                var plugin = kvp.Key;
                var componentDataList = kvp.Value;
                foreach (var componentData in componentDataList)
                {
                    var attachTarget = componentData.destEditTransform;
                    var sourceComponent = componentData.sourceComponent;
                    plugin.CopyComponent(attachTarget, sourceComponent);
                }
            }
        }
    }
}
