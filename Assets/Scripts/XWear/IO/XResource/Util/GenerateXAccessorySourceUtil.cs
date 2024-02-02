using UnityEngine;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Util
{
    public static class GenerateXAccessorySourceUtil
    {
        public static XSourceBase GenerateXSourceForAccessory(
            GameObject rootGameObject,
            AccessoryMap sourceAccessoryMap,
            ICollectComponentPlugin[] collectComponentPlugins
        )
        {
            // RootからすべてのTransformを構築する
            var rootTransform = rootGameObject.transform;
            var transformBuilder = new EditTransformBuilder();

            var builtRootTransform = transformBuilder.BuildTransforms(rootTransform);

            // ここで外部コンポーネントのコピーを実施する
            // この段階ではTransformの参照はSourceTransformのままなので注意
            foreach (var builtTransform in transformBuilder.SourceToEditTransformMemo)
            {
                var sourceAttachedComponents =
                    builtTransform.Key.gameObject.GetComponents<UnityEngine.MonoBehaviour>();
                var destEditTransform = builtTransform.Value;
                foreach (var attachedComponent in sourceAttachedComponents)
                {
                    foreach (var collectComponentPlugin in collectComponentPlugins)
                    {
                        collectComponentPlugin.CheckIsValid(attachedComponent);
                    }
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

            var xWearSource = new XAccessorySource(
                resultSkinnedMeshRenderers,
                builtRootTransform.gameObject,
                accessoryMap: sourceAccessoryMap,
                transformBuilder.SourceToEditTransformMemo
            );

            return xWearSource;
        }
    }
}
