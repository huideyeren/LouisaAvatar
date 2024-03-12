using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XWear.IO.Avatar.HumanoidTransfer;
using XWear.IO.Common;
using XWear.IO.XResource;
using XWear.IO.XResource.Util;
using Object = UnityEngine.Object;

namespace XWear.IO.Avatar
{
    public class XAvatarCreator
    {
        private ExportSourceBaseModel _exportSourceBaseModel;

        private readonly List<ExportSourceDressItem> _exportSourceDressItems =
            new List<ExportSourceDressItem>();

        private readonly IHumanoidReferenceTransfer[] _humanoidReferenceTransfers;
        private readonly IComponentConverter[] _componentConverters;

        private readonly Transform _exportRoot;
        private readonly bool _destroyDressWhenCreated;

        public bool IsVrmBaseModel { get; private set; }

        public XAvatarCreator(
            Transform exportRoot,
            IHumanoidReferenceTransfer[] humanoidReferenceTransfers,
            IComponentConverter[] componentConverters,
            bool destroyDressWhenCreated = false
        )
        {
            _exportRoot = exportRoot;
            _humanoidReferenceTransfers = humanoidReferenceTransfers;
            _componentConverters = componentConverters;
            _destroyDressWhenCreated = destroyDressWhenCreated;
        }

        public void SetBaseModel(
            GameObject sourceBaseModelGameObject,
            bool isVrmBaseModel = false,
            string name = ""
        )
        {
            _exportSourceBaseModel?.Destroy();
            IsVrmBaseModel = isVrmBaseModel;

            if (string.IsNullOrEmpty(name))
            {
                name = "ResultModel";
            }

            if (sourceBaseModelGameObject != null)
            {
                _exportSourceBaseModel = new ExportSourceBaseModel(
                    sourceBaseModelGameObject,
                    _exportRoot,
                    name: name
                );
            }

            foreach (var componentConverter in _componentConverters)
            {
                componentConverter.Cleanup();
            }
        }

        public void AddDressItem(HumanoidXResourceInstance[] humanoidXResourceInstances)
        {
            _exportSourceDressItems.Add(
                new ExportSourceDressItem(humanoidXResourceInstances, _exportRoot)
            );

            foreach (var componentConverter in _componentConverters)
            {
                componentConverter.Cleanup();
            }
        }

        private void Validate()
        {
            if (_exportSourceBaseModel == null)
            {
                return;
            }
            _exportSourceBaseModel.Validate();

            // 衣装パーツのExportSourceが破棄された衣装アイテムのExportSourceを削除する
            for (var index = 0; index < _exportSourceDressItems.Count; index++)
            {
                var exportSourceDressItem = _exportSourceDressItems[index];
                exportSourceDressItem.Validate();
                if (exportSourceDressItem.ExportSourceDressParts.Count == 0)
                {
                    _exportSourceDressItems.Remove(exportSourceDressItem);
                }
            }
        }

        public void RecreateAll(out ExportSourceBaseModel exportSourceBaseModel)
        {
            foreach (var componentConverter in _componentConverters)
            {
                componentConverter.Cleanup();
            }

            Validate();

            if (_exportSourceBaseModel == null)
            {
                exportSourceBaseModel = null;
                return;
            }

            _exportSourceBaseModel.Recreate();

            foreach (var exportSourceDressItem in _exportSourceDressItems)
            {
                exportSourceDressItem.Recreate();
            }

            var exportSourceDressParts = _exportSourceDressItems
                .SelectMany(x => x.ExportSourceDressParts)
                .Where(x => x.IsActive)
                .ToArray();

            foreach (var dressPartSource in exportSourceDressParts)
            {
                foreach (var smr in dressPartSource.CopiedSkinnedMeshRenderers)
                {
                    ReCalculateWeight(
                        baseModelGameObject: _exportSourceBaseModel.CopiedBaseModelGameObject,
                        baseModelHumanoidMap: _exportSourceBaseModel.CopiedBaseModelHumanoidMap,
                        skinnedMeshRenderer: smr,
                        dressPartSource: dressPartSource
                    );
                }
            }

            if (_destroyDressWhenCreated)
            {
                foreach (var exportSourceDressPart in exportSourceDressParts)
                {
                    Object.DestroyImmediate(exportSourceDressPart.CopiedDressPartGameObject);
                }
            }

            // この段階では必要なComponentは全て_exportSourceBaseModel以下に置かれている
            ConvertComponents(
                _exportSourceBaseModel.CopiedBaseModelGameObject.transform,
                _componentConverters
            );

            foreach (var componentConverter in _componentConverters)
            {
                componentConverter.Complete();
            }

            exportSourceBaseModel = _exportSourceBaseModel;
        }

        /// <summary>
        /// SkinnedMeshRendererのメッシュのウェイトの塗先のうち、Humanoidのものは素体側に変更する
        /// BindPoseは現在の状態で維持される
        /// </summary>
        private void ReCalculateWeight(
            GameObject baseModelGameObject,
            HumanoidMap baseModelHumanoidMap,
            SkinnedMeshRenderer skinnedMeshRenderer,
            ExportSourceDressPart dressPartSource
        )
        {
            var bakedMesh = new Mesh();
            bakedMesh.name += "Baked";
            skinnedMeshRenderer.BakeMesh(bakedMesh);

            var dressPartSourceHumanoidMap = dressPartSource.CopiedHumanoidMap;

            DressPartRestructuringUtil.RestructureBoneReferences(
                dressPartHumanoidMap: dressPartSourceHumanoidMap,
                dressPartSmr: skinnedMeshRenderer,
                baseModelHumanoidMap: baseModelHumanoidMap,
                baseModelGameObject: baseModelGameObject
            );

            MoveSourceOtherBoneToBase(
                skinnedMeshRenderer,
                baseModelHumanoidMap,
                dressPartSourceHumanoidMap
            );
            MoveSourceSkinnedMeshRendererToBase(skinnedMeshRenderer, baseModelGameObject.transform);

            // ClothItemの現在位置でのBindPoseを計算する
            skinnedMeshRenderer.sharedMesh.bindposes = ReCalcSourceBindposes(
                skinnedMeshRenderer.sharedMesh,
                baseModelGameObject.transform,
                skinnedMeshRenderer
            );

            TransferComponentsHumanoidReference(
                baseModelGameObject.transform,
                baseModelHumanoidMap,
                _humanoidReferenceTransfers,
                dressPartSourceHumanoidMap
            );
        }

        private Matrix4x4[] ReCalcSourceBindposes(
            Mesh mesh,
            Transform baseModelTransform,
            SkinnedMeshRenderer skinnedMeshRenderer
        )
        {
            var sourceBones = skinnedMeshRenderer.bones;
            var result = new Matrix4x4[sourceBones.Length];

            for (int i = 0; i < sourceBones.Length; i++)
            {
                result[i] =
                    sourceBones[i].worldToLocalMatrix * baseModelTransform.localToWorldMatrix;
            }

            return result;
        }

        /// <summary>
        /// XResourceにアタッチされているコンポーネントのうち、
        /// Humanoidを参照するコンポーネントをチェックし、必要があれば素体側のHumanoidに差し替えを行う
        /// </summary>
        private void TransferComponentsHumanoidReference(
            Transform baseModelTransform,
            HumanoidMap baseModelHumanoidMap,
            IHumanoidReferenceTransfer[] humanoidTransfers,
            HumanoidMap dressPartSourceHumanoidMap
        )
        {
            var transferComponentTargetCaches =
                new Dictionary<IHumanoidReferenceTransfer, List<Component>>();
            // 重いが、これ以外の解決策が思いつかない
            var components = baseModelTransform.GetComponentsInChildren<Component>();
            foreach (var component in components)
            {
                foreach (var humanoidTransfer in humanoidTransfers)
                {
                    if (humanoidTransfer.Check(component))
                    {
                        if (
                            !transferComponentTargetCaches.TryGetValue(
                                humanoidTransfer,
                                out var targetComponents
                            )
                        )
                        {
                            targetComponents = new List<Component>();
                            transferComponentTargetCaches.Add(humanoidTransfer, targetComponents);
                        }

                        targetComponents.Add(component);
                    }
                }
            }

            var sortedTargetCaches = transferComponentTargetCaches.OrderBy(x => x.Key.Order);
            foreach (var targetCache in sortedTargetCaches)
            {
                var transfer = targetCache.Key;
                var targets = targetCache.Value;
                foreach (var target in targets)
                {
                    transfer.Transfer(
                        target,
                        baseModelHumanoidMap: baseModelHumanoidMap,
                        dressPartSourceHumanoidMap: dressPartSourceHumanoidMap
                    );
                }
            }
        }

        /// <summary>
        /// SkinnedMeshRenderer.bonesのうち、非Humanoidなボーンを素体側に移植する
        /// </summary>
        private void MoveSourceOtherBoneToBase(
            SkinnedMeshRenderer skinnedMeshRenderer,
            HumanoidMap baseModelHumanoidMap,
            HumanoidMap dressPartSourceHumanoidMap
        )
        {
            var transformToHumanBodyBones = dressPartSourceHumanoidMap.GetMap;
            foreach (var transformToHumanBodyBone in transformToHumanBodyBones)
            {
                var humanoidBone = transformToHumanBodyBone.Key;
                var humanBodyBones = transformToHumanBodyBone.Value;
                // HumanoidBoneの子階層(孫を含めず)をみて、以下の条件に当てはまる階層をコピーし、
                // コピー先をSkinnedMeshRenderer.bonesに入れる
                // - 非HumanoidBoneのTransform
                // - SkinnedMeshRenderer.bonesに含まれるTransformが含まれる

                var humanoidChildren = humanoidBone.GetChildren().ToArray();
                foreach (var humanoidChild in humanoidChildren)
                {
                    // Humanoidに含まれるBoneはみない
                    if (transformToHumanBodyBones.TryGetValue(humanoidChild, out _))
                    {
                        continue;
                    }

                    // 孫を含めて子が全てSkinnedMeshRendererに入っていなければ移動させない
                    if (humanoidChild.Traverse().All(x => !skinnedMeshRenderer.bones.Contains(x)))
                    {
                        continue;
                    }

                    // 素体のどの骨の下にアサインするか
                    if (
                        !baseModelHumanoidMap.GetMap
                            .FlipKvp()
                            .TryGetValue(humanBodyBones, out var injectRoot)
                    )
                    {
                        injectRoot = IOParentBoneMapper.GetValidHumanoidParent(
                            baseModelHumanoidMap,
                            humanBodyBones
                        );
                    }

                    // 衣装揺れものボーンの親変更後の位置を変更する
                    var currentLocalPosition = humanoidChild.position;
                    var currentWorldRotation = humanoidChild.rotation;
                    var currentLocalScale = humanoidChild.localScale;
                    humanoidChild.parent = injectRoot.transform;
                    // 衣装の揺れものボーンの座標なので、位置や回転はずらさない
                    humanoidChild.position = currentLocalPosition;
                    humanoidChild.rotation = currentWorldRotation;
                    humanoidChild.localScale = currentLocalScale;
                }
            }
        }

        /// <summary>
        /// SkinnedMeshRendererを素体側に移植する
        /// </summary>
        /// <param name="skinnedMeshRenderer"></param>
        /// <param name="baseModelTransform"></param>
        private void MoveSourceSkinnedMeshRendererToBase(
            SkinnedMeshRenderer skinnedMeshRenderer,
            Transform baseModelTransform
        )
        {
            skinnedMeshRenderer.gameObject.transform.SetParent(baseModelTransform, true);
        }

        private IEnumerable<Transform> GetChildrenFrom(Transform parent)
        {
            return parent.Cast<Transform>();
        }

        private void ConvertComponents(
            Transform baseModelTransform,
            IComponentConverter[] converters
        )
        {
            foreach (var converter in converters)
            {
                converter.Setup(baseModelTransform, IsVrmBaseModel);
            }

            var transferComponentTargetCaches =
                new Dictionary<IComponentConverter, List<Component>>();
            // 重いが、これ以外の解決策が思いつかない
            var components = baseModelTransform.GetComponentsInChildren<Component>();
            foreach (var component in components)
            {
                foreach (var converter in converters)
                {
                    if (converter.Check(component))
                    {
                        if (
                            !transferComponentTargetCaches.TryGetValue(
                                converter,
                                out var targetComponents
                            )
                        )
                        {
                            targetComponents = new List<Component>();
                            transferComponentTargetCaches.Add(converter, targetComponents);
                        }

                        targetComponents.Add(component);
                    }
                }
            }

            var sortedTargetCaches = transferComponentTargetCaches.OrderBy(x => x.Key.Order);
            foreach (var targetCache in sortedTargetCaches)
            {
                var converter = targetCache.Key;
                var targets = targetCache.Value;
                foreach (var target in targets)
                {
                    converter.Convert(baseModelTransform, target);
                }
            }
        }
    }
}
