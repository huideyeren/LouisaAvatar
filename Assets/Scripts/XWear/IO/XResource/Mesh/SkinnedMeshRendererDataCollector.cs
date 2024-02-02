using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Mesh
{
    public class SkinnedMeshRendererDataCollector
    {
        /// <summary>
        /// 元になったSkinnedMeshRendererからGuidを引くためのマップ
        /// </summary>
        /// <returns></returns>
        public readonly Dictionary<
            UnityEngine.Transform,
            string
        > SourceSkinnedMeshRendererTransformToGuidMap =
            new Dictionary<UnityEngine.Transform, string>();

        private readonly GameObjectWithTransformCollector _gameObjectCollector;
        private readonly MaterialCollectorBase _materialCollector;

        public delegate void MeshArchiveAction(
            XResourceContainerUtil.XResourceArchiver archiver,
            SkinnedMeshRenderer builtSmr,
            UnityEngine.Transform sourceTransform,
            XResourceSkinnedMeshRenderer result
        );

        private readonly MeshArchiveAction _archiveAction;

        public SkinnedMeshRendererDataCollector(
            GameObjectWithTransformCollector gameObjectCollector,
            MaterialCollectorBase materialCollector,
            MeshArchiveAction archiveAction
        )
        {
            _gameObjectCollector = gameObjectCollector;
            _materialCollector = materialCollector;
            _archiveAction = archiveAction;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="collectSource"></param>
        /// <param name="archiver"></param>
        /// <returns></returns>
        public IXResourceComponent Collect(
            XWearSourceSkinnedMeshRenderer collectSource,
            XResourceContainerUtil.XResourceArchiver archiver
        )
        {
            var skinnedMeshRenderer = collectSource.DestSmr;
            var sourceBones = skinnedMeshRenderer.bones;
            var rootBoneGuid =
                _gameObjectCollector.GetXResourceGameObjectGuidFromAvatarSourceTransform(
                    skinnedMeshRenderer.rootBone
                );

            // 実際のTransformとindexのペア配列を用意する
            // 素直にSelectを回しても問題なさそうにみえるが、SkinnedMeshRenderer.bonesのindexによって
            // Skinningが決定されるので、念ため明示的にindexを入れる
            var boneGuidsWithIndex = sourceBones
                .Select(
                    (skinnedBone, index) =>
                        (
                            guid: _gameObjectCollector.GetXResourceGameObjectGuidFromTransform(
                                skinnedBone
                            ),
                            index: index
                        )
                )
                .ToArray();

            var bones = new List<XResourceSkinnedMeshRenderer.SkinnedBone>();
            foreach (var (guid, index) in boneGuidsWithIndex)
            {
                bones.Add(
                    new XResourceSkinnedMeshRenderer.SkinnedBone()
                    {
                        Index = index,
                        BoneGuid = guid
                    }
                );
            }

            // SkinnedMeshRendererが参照するマテリアルを収集し、保存する
            // マテリアルとテクスチャはそれぞれ同じ参照を持つ場合は単一のClothItemMaterialとClothTextureItemが生成される
            var sourceMaterials = skinnedMeshRenderer.sharedMaterials;
            var refMaterialGuids = new string[sourceMaterials.Length];
            for (int i = 0; i < sourceMaterials.Length; i++)
            {
                var sourceMaterial = sourceMaterials[i];
                // 異なるSkinnedMeshRenderer間で同じマテリアルの参照を持つ可能性を考慮するため、
                // SkinnedMeshRenderer側には保存せず、MaterialBuilderに保存する
                var xResourceMaterial = _materialCollector.TryCollectAndAdd(sourceMaterial);
                refMaterialGuids[i] = xResourceMaterial.Guid;
            }

            var result = new XResourceSkinnedMeshRenderer
            {
                Bones = bones.ToArray(),
                RootBoneGuid = rootBoneGuid,
                Mesh = new XResourceMesh(skinnedMeshRenderer),
                RefMaterialGuids = refMaterialGuids
            };

            _archiveAction.Invoke(
                archiver: archiver,
                builtSmr: skinnedMeshRenderer,
                sourceTransform: collectSource.SourceSmrTransform,
                result: result
            );

            SourceSkinnedMeshRendererTransformToGuidMap.Add(
                collectSource.SourceSmrTransform,
                result.Guid
            );
            return result;
        }
    }
}
