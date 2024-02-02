using System.Collections.Generic;
using UnityEngine;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Mesh
{
    public class SkinnedMeshRendererDataBuilder
    {
        /// <summary>
        /// Guidから構築したSkinnedMeshRendererを引くためのマップ
        /// </summary>
        /// <returns></returns>
        public readonly Dictionary<string, SkinnedMeshRenderer> GuidToBuiltSkinnedMeshRenderer =
            new Dictionary<string, SkinnedMeshRenderer>();

        private readonly GameObjectWithTransformBuilder _gameObjectBuilder;
        private readonly MaterialBuilderBase _materialBuilder;
        private readonly XResourceContainerUtil.XResourceOpener _opener;

        public delegate void MeshOpenAction(
            XResourceContainerUtil.XResourceOpener opener,
            SkinnedMeshRenderer destSmr,
            XResourceSkinnedMeshRenderer xResource
        );

        private readonly MeshOpenAction _meshOpenAction;

        public SkinnedMeshRendererDataBuilder(
            GameObjectWithTransformBuilder gameObjectBuilder,
            MaterialBuilderBase materialBuilder,
            MeshOpenAction meshOpenAction,
            XResourceContainerUtil.XResourceOpener opener
        )
        {
            _gameObjectBuilder = gameObjectBuilder;
            _materialBuilder = materialBuilder;
            _meshOpenAction = meshOpenAction;
            _opener = opener;
        }

        public SkinnedMeshRenderer Build(
            XResourceSkinnedMeshRenderer xResourceSkinnedMeshRenderer,
            GameObject instance
        )
        {
            var smr = (SkinnedMeshRenderer)xResourceSkinnedMeshRenderer.AttachTo(instance);
            smr.rootBone = _gameObjectBuilder.GetBuildTransformFromGuid(
                xResourceSkinnedMeshRenderer.RootBoneGuid
            );

            _meshOpenAction.Invoke(_opener, smr, xResourceSkinnedMeshRenderer);

            smr.sharedMesh.RecalculateBounds();

            // アタッチされたSkinnedMeshRenderer.bonesに構築後のTransformを入れる
            // 構築後のTransformはguidToBuildTransformMemoからguidで引かれる
            var componentBones = xResourceSkinnedMeshRenderer.Bones;
            var boneCount = componentBones.Length;
            var resultBones = new UnityEngine.Transform[boneCount];
            for (int i = 0; i < boneCount; i++)
            {
                var index = componentBones[i].Index;
                var guid = componentBones[i].BoneGuid;
                var buildTransform = _gameObjectBuilder.GetBuildTransformFromGuid(guid);

                resultBones[index] = buildTransform;
            }

            smr.bones = resultBones;

            // 事前にビルドされたマテリアルから参照を引く
            var materialRefs = xResourceSkinnedMeshRenderer.RefMaterialGuids;
            var refMaterials = new UnityEngine.Material[materialRefs.Length];
            for (int i = 0; i < materialRefs.Length; i++)
            {
                var refGuid = materialRefs[i];

                // 異なるSkinnedMeshRenderer間で同じマテリアルの参照を持つ可能性を考慮するため、
                // MaterialBuilderが保存するMaterialから参照を引く
                if (_materialBuilder.TryGetMaterialFromGuid(refGuid, out var material))
                {
                    refMaterials[i] = material;
                }
            }

            smr.sharedMaterials = refMaterials;

            GuidToBuiltSkinnedMeshRenderer.Add(xResourceSkinnedMeshRenderer.Guid, smr);

            return smr;
        }
    }
}
