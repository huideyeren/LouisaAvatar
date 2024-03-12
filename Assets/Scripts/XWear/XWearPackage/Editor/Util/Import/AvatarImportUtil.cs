using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using XWear.IO;
using XWear.IO.Avatar;
using XWear.IO.Avatar.HumanoidTransfer;
using XWear.IO.Editor.Material;
using XWear.IO.Editor.Mesh;
using XWear.IO.Editor.Texture;
using XWear.IO.XResource;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Component.UnityConstraint.Aim;
using XWear.IO.XResource.Component.UnityConstraint.LookAt;
using XWear.IO.XResource.Component.UnityConstraint.Parent;
using XWear.IO.XResource.Component.UnityConstraint.Position;
using XWear.IO.XResource.Component.UnityConstraint.Rotation;
using XWear.IO.XResource.Component.UnityConstraint.Scale;
using XWear.IO.XResource.Material;
using XWear.XWearPackage.ForVrc.ComponentPlugins;
using XWear.XWearPackage.ForVrc.HumanoidTransfer;

namespace XWear.XWearPackage.Editor.Util.Import
{
    public static class AvatarImportUtil
    {
        public static async Task<string[]> CheckShaderIsExist(
            string loadPath,
            bool applySavedMaterial
        )
        {
            var result = new List<string>();
            await using (var fs = File.OpenRead(loadPath))
            {
                using (var opener = new XResourceContainerUtil.XResourceOpener(fs))
                {
                    var meta = opener.GetXItemMeta();
                    foreach (var info in meta.infoList)
                    {
                        var xItem = opener.GetXItem(info);
                        var materials = applySavedMaterial
                            ? xItem.DefaultXResourceMaterials
                            : xItem.XResourceMaterials;
                        var notFoundShaders = materials
                            .Select(x => x.ShaderName)
                            .Distinct()
                            .Where(x => Shader.Find(x) == null)
                            .ToArray();

                        result.AddRange(notFoundShaders);
                    }
                }
            }

            return result.Distinct().ToArray();
        }

        public static async Task RunImport(
            string saveFolder,
            string loadPath,
            bool applySaveMaterial
        )
        {
            var assetSaver = new AssetSaver(rootFolderPath: saveFolder);

            var importer = new XItemImporter(loadPath);
            var componentPlugins = new IBuildComponentPlugin[]
            {
                new PhysBoneBuildPlugin(),
                new PhysBoneColliderBuildPlugin(),
                new VrcAvatarDescriptorBuildPlugin(),
                new AimConstraintBuildComponentPlugin(),
                new LookAtConstraintBuildComponentPlugin(),
                new ParentConstraintBuildComponentPlugin(),
                new PositionConstraintBuildComponentPlugin(),
                new RotationConstraintBuildComponentPlugin(),
                new ScaleConstraintBuildComponentPlugin()
            };

            var editorTextureBuilder = new EditorTextureBuilder(assetSaver);
            var materialBuilder = new EditorMaterialBuilder(editorTextureBuilder, assetSaver);

            var importResult = await importer.Run(
                componentPlugins,
                ImportContext.ImportType.Avatar,
                materialBuilder: materialBuilder,
                textureBuilder: editorTextureBuilder,
                applySavedMaterial: applySaveMaterial,
                assetSaver: assetSaver,
                meshOpenAction: EditorMeshOpener.Open
            );

            CreateRecalculatedAvatarPrefab(importResult, assetSaver);
        }

        private static void CreateRecalculatedAvatarPrefab(
            (List<XItemInstance> xItemInstances, XItemMeta meta) importResult,
            AssetSaver assetSaver
        )
        {
            CreateRecalculatedAvatar(importResult, assetSaver, out var exportResult);
            var smrs = exportResult.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var smr in smrs)
            {
                smr.sharedMesh = (Mesh)assetSaver.CreateAsset(smr.sharedMesh);
            }

            // このタイミングではHumanoidMapComponentは既に不要なので剥がす
            var humanoidMapComponent = exportResult.GetComponent<HumanoidMapComponent>();
            Object.DestroyImmediate(humanoidMapComponent);

            assetSaver.CreateAsset(exportResult);

            Object.DestroyImmediate(exportResult);

            foreach (var xItemInstance in importResult.xItemInstances)
            {
                foreach (var instance in xItemInstance.XResourceInstances)
                {
                    if (instance != null)
                    {
                        Object.DestroyImmediate(instance.Instance);
                    }
                }
            }
        }

        private static void CreateRecalculatedAvatar(
            (List<XItemInstance> xItemInstances, XItemMeta meta) importResult,
            AssetSaver assetSaver,
            out GameObject resultModel
        )
        {
            resultModel = null;
            var baseModelInfo = importResult.meta.infoList.FirstOrDefault(
                x => x.xItemType == XItemMeta.XItemType.BaseModel
            );

            if (baseModelInfo == null)
            {
                return;
            }

            // 素体のGuidと衣装のGuidからそれぞれのXItemを集める
            var baseModelGuid = baseModelInfo.guid;
            var baseModelXItemInstance = importResult.xItemInstances.FirstOrDefault(
                x => x.Guid == baseModelGuid
            );
            if (baseModelXItemInstance == null)
            {
                return;
            }

            if (baseModelXItemInstance.XItem is AvatarXItem avatarXItem && avatarXItem.FromVrm)
            {
                VrmBaseModelConvert(baseModelXItemInstance, assetSaver);
            }

            var wearGuids = importResult.meta.infoList
                .Select(x => x.guid)
                .Where(x => x != baseModelGuid)
                .ToArray();

            var wearXItemInstances = wearGuids
                .Select(
                    wearGuid => importResult.xItemInstances.FirstOrDefault(x => x.Guid == wearGuid)
                )
                .Where(wearXItemInstance => wearXItemInstance != null)
                .ToArray();

            var avatarCreator = new XAvatarCreator(
                null,
                new IHumanoidReferenceTransfer[]
                {
                    new PhysBoneHumanoidTransfer(),
                    new PhysBoneColliderHumanoidTransfer()
                },
                new IComponentConverter[] { },
                destroyDressWhenCreated: true
            );

            avatarCreator.SetBaseModel(
                baseModelXItemInstance.XResourceInstances[0].Instance,
                name: baseModelInfo.name
            );

            avatarCreator.AddDressItem(
                wearXItemInstances
                    .SelectMany(x => x.XResourceInstances)
                    .Select(x => x as HumanoidXResourceInstance)
                    .ToArray()
            );

            avatarCreator.RecreateAll(out var exportSourceBaseModel);
            resultModel = exportSourceBaseModel.CopiedBaseModelGameObject;
        }

        private static void VrmBaseModelConvert(XItemInstance xItemInstance, AssetSaver assetSaver)
        {
            VrmBaseModelConversion.Convert(xItemInstance, assetSaver);
        }
    }
}
