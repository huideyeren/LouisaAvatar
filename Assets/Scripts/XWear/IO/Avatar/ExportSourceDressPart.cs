using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XWear.IO.Common;
using XWear.IO.XResource;

namespace XWear.IO.Avatar
{
    public class ExportSourceDressItem
    {
        public readonly List<ExportSourceDressPart> ExportSourceDressParts;

        public ExportSourceDressItem(
            HumanoidXResourceInstance[] humanoidXResourceInstances,
            Transform exportRoot
        )
        {
            ExportSourceDressParts = new List<ExportSourceDressPart>();

            for (var index = 0; index < humanoidXResourceInstances.Length; index++)
            {
                var dressPartInstance = humanoidXResourceInstances[index];
                var skinnedMeshRenderers =
                    dressPartInstance.Instance.GetComponentsInChildren<SkinnedMeshRenderer>();

                var dressPartSource = new ExportSourceDressPart(
                    dressPartInstance,
                    skinnedMeshRenderers,
                    exportRoot
                );
                ExportSourceDressParts.Add(dressPartSource);
            }
        }

        public void Validate()
        {
            // 元のGameObjectが破棄されている衣装パーツのExportSourceを削除する
            for (var index = 0; index < ExportSourceDressParts.Count; index++)
            {
                var exportSourceDressPart = ExportSourceDressParts[index];
                if (exportSourceDressPart.SourceHumanoidXResourceInstance.Instance == null)
                {
                    exportSourceDressPart.Destroy();
                    ExportSourceDressParts.Remove(exportSourceDressPart);
                }
            }
        }

        public void Recreate()
        {
            foreach (var exportSourceDressPart in ExportSourceDressParts)
            {
                if (!exportSourceDressPart.IsActive)
                {
                    exportSourceDressPart.CopiedDressPartGameObject.SetActive(false);
                    continue;
                }

                exportSourceDressPart.CopiedDressPartGameObject.SetActive(true);
                exportSourceDressPart.Recreate();
            }

            var dressPartSource = ExportSourceDressParts
                .Where(x => x.IsActive)
                .Select(
                    x =>
                        (
                            x.CopiedDressPartGameObject,
                            x.CopiedHumanoidMap,
                            x.CopiedSkinnedMeshRenderers
                        )
                )
                .ToArray();
            //DressPartRestructuringUtil.MergeBone(dressPartSource);
        }
    }

    public class ExportSourceDressPart
    {
        public GameObject CopiedDressPartGameObject { get; private set; }
        public SkinnedMeshRenderer[] CopiedSkinnedMeshRenderers { get; private set; }
        public HumanoidMap CopiedHumanoidMap { get; private set; }

        public readonly string SourceGuid;
        private readonly SkinnedMeshRenderer[] _sourceSkinnedMeshRenderers;
        private readonly Transform _exportRoot;
        public readonly HumanoidXResourceInstance SourceHumanoidXResourceInstance;
        public bool IsActive => SourceHumanoidXResourceInstance.Instance.activeSelf;

        public ExportSourceDressPart(
            HumanoidXResourceInstance sourceHumanoidXResourceInstance,
            SkinnedMeshRenderer[] skinnedMeshRenderers,
            Transform exportRoot
        )
        {
            SourceHumanoidXResourceInstance = sourceHumanoidXResourceInstance;
            _sourceSkinnedMeshRenderers = skinnedMeshRenderers;
            _exportRoot = exportRoot;

            CopiedDressPartGameObject = CopyGameObject();
            CopiedHumanoidMap = CopyHumanoidMap();
            CopiedSkinnedMeshRenderers = CopySkinnedMeshRenderers();

            SourceGuid = SourceHumanoidXResourceInstance.Guid;
        }

        private GameObject CopyGameObject()
        {
            var sourceInstance = SourceHumanoidXResourceInstance.Instance;
            var copiedDressPartGameObject = Object.Instantiate(sourceInstance, _exportRoot);
            copiedDressPartGameObject.name = $"{sourceInstance.name}";

            return copiedDressPartGameObject;
        }

        private HumanoidMap CopyHumanoidMap()
        {
            var humanoidMapComponent =
                CopiedDressPartGameObject.GetComponent<HumanoidMapComponent>();
            if (humanoidMapComponent == null)
            {
                humanoidMapComponent =
                    CopiedDressPartGameObject.AddComponent<HumanoidMapComponent>();
                humanoidMapComponent.AutoAssign();
            }

            return humanoidMapComponent.HumanoidMap;
        }

        private SkinnedMeshRenderer[] CopySkinnedMeshRenderers()
        {
            var copiedSmrs =
                CopiedDressPartGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (var index = 0; index < copiedSmrs.Length; index++)
            {
                var sourceSmr = _sourceSkinnedMeshRenderers[index];
                var sourceSmrBones = sourceSmr.bones;

                var copiedSmr = copiedSmrs[index];
                var copiedSmrTransform = copiedSmr.transform;
                copiedSmrTransform.localPosition = Vector3.zero;
                copiedSmrTransform.localRotation = Quaternion.identity;
                copiedSmrTransform.localScale = Vector3.one;

                var copiedSmrBones = copiedSmr.bones;

                copiedSmr.sharedMesh = MeshUtility.CopyMesh(copiedSmr.sharedMesh);

                var currentMesh = new Mesh();
                copiedSmr.BakeMesh(currentMesh);

                var resultBindPoses = new Matrix4x4[sourceSmrBones.Length];

                for (
                    var sourceBoneIndex = 0;
                    sourceBoneIndex < sourceSmrBones.Length;
                    sourceBoneIndex++
                )
                {
                    var sourceBone = sourceSmrBones[sourceBoneIndex];
                    var copiedBone = copiedSmrBones[sourceBoneIndex];
                    copiedBone.position = sourceBone.position;
                    copiedBone.rotation = sourceBone.rotation;
                    copiedBone.localScale = sourceBone.localScale;

                    resultBindPoses[sourceBoneIndex] =
                        copiedBone.worldToLocalMatrix * copiedSmrTransform.localToWorldMatrix;
                }

                var copiedSmrSharedMesh = copiedSmr.sharedMesh;
                copiedSmrSharedMesh.vertices = currentMesh.vertices;
                copiedSmrSharedMesh.normals = currentMesh.normals;
                copiedSmrSharedMesh.bindposes = resultBindPoses;
            }

            return copiedSmrs;
        }

        public void Recreate()
        {
            Destroy();
            CopiedDressPartGameObject = CopyGameObject();
            CopiedHumanoidMap = CopyHumanoidMap();
            CopiedSkinnedMeshRenderers = CopySkinnedMeshRenderers();
        }

        public void Destroy()
        {
            foreach (var copiedSkinnedMeshRenderer in CopiedSkinnedMeshRenderers)
            {
                if (
                    copiedSkinnedMeshRenderer != null
                    && copiedSkinnedMeshRenderer.sharedMesh != null
                )
                {
                    Object.DestroyImmediate(copiedSkinnedMeshRenderer.sharedMesh);
                }
            }

            if (CopiedDressPartGameObject != null)
            {
                Object.DestroyImmediate(CopiedDressPartGameObject);
            }
        }
    }
}
