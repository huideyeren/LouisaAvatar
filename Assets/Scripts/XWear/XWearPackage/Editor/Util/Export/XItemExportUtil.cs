using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using UnityEditor;
using UnityEngine;
using XWear.IO;
using XWear.IO.Editor.Material;
using XWear.IO.Editor.Mesh;
using XWear.IO.Editor.Texture;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Component.UnityConstraint.Aim;
using XWear.IO.XResource.Component.UnityConstraint.LookAt;
using XWear.IO.XResource.Component.UnityConstraint.Parent;
using XWear.IO.XResource.Component.UnityConstraint.Position;
using XWear.IO.XResource.Component.UnityConstraint.Rotation;
using XWear.IO.XResource.Component.UnityConstraint.Scale;
using XWear.IO.XResource.Material;
using XWear.XWearPackage.ForVrc.ComponentPlugins;
using Object = UnityEngine.Object;

namespace XWear.XWearPackage.Editor.Util.Export
{
    public static class XItemExportUtil
    {
        public static void Export(
            string savePath,
            ExportContext exportContext,
            GameObject exportTarget,
            out List<GameObject> copied
        )
        {
            copied = new List<GameObject>();

            var plugins = new List<ICollectComponentPlugin>()
            {
                new PhysBoneCollectPlugin(),
                new PhysBoneColliderCollectPlugin(),
                new AimConstraintCollectComponentPlugin(),
                new LookAtConstraintCollectComponentPlugin(),
                new ParentConstraintCollectComponentPlugin(),
                new PositionConstraintCollectComponentPlugin(),
                new RotationConstraintCollectComponentPlugin(),
                new ScaleConstraintCollectComponentPlugin()
            };

            if (exportContext.exportType == ExportContext.ExportType.Avatar)
            {
                plugins.Add(new VrcAvatarDescriptorCollectPlugin());
            }

            switch (exportContext.exportType)
            {
                case ExportContext.ExportType.Avatar:
                {
                    var copiedBaseModel = CopyExportGameObject(exportTarget);
                    var baseModelHumanoidMapComponent = GetHumanoidMapComponentAndAutoAssignIfNull(
                        copiedBaseModel
                    );

                    RemoveUnusedBones(copiedBaseModel, baseModelHumanoidMapComponent);

                    var wearSources =
                        new List<(
                            GameObject wearRootGameObject,
                            HumanoidMapComponent wearHumanoidMapComponent
                        )>();

                    if (
                        exportContext.exportChildren != null
                        && exportContext.exportChildren.Count != 0
                    )
                    {
                        var validChildren = exportContext.exportChildren
                            .Where(x => x != null)
                            .Distinct()
                            .ToArray();

                        foreach (var exportChild in validChildren)
                        {
                            var copiedWearSourceRoot = CopyExportGameObject(exportChild);
                            var copiedWearHumanoidMapComponent =
                                GetHumanoidMapComponentAndAutoAssignIfNull(copiedWearSourceRoot);
                            RemoveUnusedBones(copiedWearSourceRoot, copiedWearHumanoidMapComponent);
                            wearSources.Add((copiedWearSourceRoot, copiedWearHumanoidMapComponent));

                            copied.Add(copiedWearSourceRoot);
                        }
                    }

                    var textureCollector = new EditorTextureCollector();
                    var materialCollector = new EditorMaterialCollector(textureCollector);
                    var exporter = new XAvatarExporter(
                        savePath: savePath,
                        baseModelRoot: copiedBaseModel,
                        baseModelHumanoidMapComponent: baseModelHumanoidMapComponent,
                        wearSources: wearSources.ToArray(),
                        textureCollector: textureCollector,
                        materialCollector: materialCollector,
                        meshArchiveAction: EditorMeshArchiver.Archive
                    );

                    exporter.Run(exportContext, plugins.ToArray(), exportFromPackage: true);
                    copied.Add(copiedBaseModel);
                    break;
                }
                case ExportContext.ExportType.Wear:
                {
                    var copiedRoot = CopyExportGameObject(exportTarget);
                    copied.Add(copiedRoot);
                    var textureCollector = new EditorTextureCollector();
                    var materialCollector = new EditorMaterialCollector(textureCollector);
                    var exporter = new XWearExporter(
                        savePath,
                        textureCollector: textureCollector,
                        materialCollector: materialCollector,
                        copiedRoot
                    );

                    var humanoidMapComponent = GetHumanoidMapComponentAndAutoAssignIfNull(
                        copiedRoot
                    );
                    RemoveUnusedBones(copiedRoot, humanoidMapComponent);
                    exporter.Run(
                        exportContext,
                        plugins.ToArray(),
                        humanoidMapComponent.HumanoidMap,
                        exportFromPackage: true,
                        meshArchiveAction: EditorMeshArchiver.Archive
                    );
                    break;
                }
            }
        }

        private static HumanoidMapComponent GetHumanoidMapComponentAndAutoAssignIfNull(
            GameObject from
        )
        {
            var humanoidMapComponent = from.GetComponent<HumanoidMapComponent>();
            if (humanoidMapComponent == null)
            {
                humanoidMapComponent = from.AddComponent<HumanoidMapComponent>();
                humanoidMapComponent.AutoAssign();
            }

            return humanoidMapComponent;
        }

        private static GameObject CopyExportGameObject(GameObject target)
        {
            var copied = Object.Instantiate(target);
            target.transform.position = Vector3.zero;
            target.transform.rotation = Quaternion.identity;
            copied.name = target.name;

            // Exporterでメッシュを触るため、元データを壊さないためにコピーする
            var skinnedMeshRenderers = copied.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                var sourceMesh = skinnedMeshRenderer.sharedMesh;
                var destMesh = CopyMesh(sourceMesh);
                skinnedMeshRenderer.sharedMesh = destMesh;
            }

            return copied;
        }

        private static void RemoveUnusedBones(
            GameObject copiedGameObject,
            HumanoidMapComponent humanoidMapComponent
        )
        {
            var smrs = copiedGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            RemoveBonesUtil.RemoveUnusedBones(humanoidMapComponent, smrs);
        }

        /// <summary>
        /// This code is a modified version of the following code.
        /// Copyright (c) 2018 ousttrue
        /// https://github.com/vrm-c/UniVRM/blob/ffd4e63bcd499ae1a560a401b713699ba64a9ddd/Assets/UniGLTF/LICENSE.md
        /// original: https://github.com/vrm-c/UniVRM/blob/ffd4e63bcd499ae1a560a401b713699ba64a9ddd/Assets/UniGLTF/Runtime/MeshUtility/MeshExtensions.cs
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static Mesh CopyMesh(Mesh source)
        {
            var dest = new Mesh
            {
                name = source.name,
                indexFormat = source.indexFormat,
                vertices = source.vertices,
                normals = source.normals,
                tangents = source.tangents,
                colors = source.colors,
                uv = source.uv,
                uv2 = source.uv2,
                uv3 = source.uv3,
                uv4 = source.uv4,
                boneWeights = source.boneWeights,
                bindposes = source.bindposes
            };

            var subMeshCount = source.subMeshCount;
            dest.subMeshCount = subMeshCount;
            for (var i = 0; i < subMeshCount; ++i)
            {
                dest.SetIndices(source.GetIndices(i), source.GetTopology(i), i);
            }

            dest.RecalculateBounds();
            var vertices = new Vector3[source.vertices.Length];
            var normals = new Vector3[source.vertices.Length];
            var tangents = new Vector3[source.vertices.Length];
            for (int i = 0; i < source.blendShapeCount; ++i)
            {
                source.GetBlendShapeFrameVertices(i, 0, vertices, normals, tangents);
                dest.AddBlendShapeFrame(
                    source.GetBlendShapeName(i),
                    source.GetBlendShapeFrameWeight(i, 0),
                    vertices,
                    normals,
                    tangents
                );
            }

            return dest;
        }
    }
}
