using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XWear.IO.Thumbnail;
using XWear.IO.XResource;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.AvatarMeta;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Texture;

namespace XWear.IO
{
    public class XAvatarExporter
    {
        private readonly string _savePath;
        private readonly TextureCollectorBase _textureCollector;
        private readonly MaterialCollectorBase _materialCollector;
        private readonly GameObject _baseModelRoot;
        private readonly HumanoidMapComponent _baseModelHumanoidMapComponent;
        private readonly SkinnedMeshRendererDataCollector.MeshArchiveAction _meshArchiveAction;

        private readonly (
            GameObject wearRootGameObject,
            HumanoidMapComponent wearHumanoidMapComponent
        )[] _wearSources;

        public XAvatarExporter(
            string savePath,
            TextureCollectorBase textureCollector,
            MaterialCollectorBase materialCollector,
            SkinnedMeshRendererDataCollector.MeshArchiveAction meshArchiveAction,
            GameObject baseModelRoot,
            HumanoidMapComponent baseModelHumanoidMapComponent,
            (
                GameObject wearRootGameObject,
                HumanoidMapComponent wearHumanoidMapComponent
            )[] wearSources
        )
        {
            _savePath = savePath;
            _textureCollector = textureCollector;
            _materialCollector = materialCollector;
            _meshArchiveAction = meshArchiveAction;
            _baseModelRoot = baseModelRoot;
            _baseModelHumanoidMapComponent = baseModelHumanoidMapComponent;
            _wearSources = wearSources;
        }

        public void Run(
            ExportContext exportContext,
            ICollectComponentPlugin[] componentPlugins,
            bool exportFromPackage
        )
        {
            using (var fs = new FileStream(_savePath, FileMode.Create))
            {
                using (
                    var archiver = new XResourceContainerUtil.XResourceArchiver(
                        fs,
                        archiveFromPackage: exportFromPackage
                    )
                )
                {
                    var metaInfoList = new List<XItemMeta.XItemInfo>();

                    var avatarSerializeSource = new XItemSerializeSource(
                        _baseModelRoot,
                        _baseModelHumanoidMapComponent.HumanoidMap,
                        componentPlugins,
                        exportType: exportContext.exportType
                    );

                    var avatarXItem = XItemSerializer.SerializeAsAvatar(
                        avatarSerializeSource,
                        textureCollector: _textureCollector,
                        materialCollector: _materialCollector,
                        saveAsDefault: exportFromPackage,
                        meshArchiveAction: _meshArchiveAction,
                        archiver
                    );

                    metaInfoList.Add(
                        new XItemMeta.XItemInfo()
                        {
                            name = avatarXItem.Name,
                            guid = avatarXItem.Guid,
                            xItemType = XItemMeta.XItemType.BaseModel
                        }
                    );

                    foreach (var wearSource in _wearSources)
                    {
                        var wearRootGameObject = wearSource.wearRootGameObject;
                        var wearHumanoidMapComponent = wearSource
                            .wearHumanoidMapComponent
                            .HumanoidMap;

                        var wearSerializeSource = new XItemSerializeSource(
                            wearRootGameObject,
                            wearHumanoidMapComponent,
                            componentPlugins,
                            exportType: exportContext.exportType
                        );

                        var wearXItem = XItemSerializer.SerializeAsWear(
                            wearSerializeSource,
                            textureCollector: _textureCollector,
                            materialCollector: _materialCollector,
                            saveAsDefault: exportFromPackage,
                            meshArchiveAction: _meshArchiveAction,
                            archiver
                        );

                        archiver.AddXItem(wearXItem);
                        metaInfoList.Add(
                            new XItemMeta.XItemInfo()
                            {
                                name = wearXItem.Name,
                                guid = wearXItem.Guid,
                                xItemType = XItemMeta.XItemType.Wear
                            }
                        );
                    }

                    _textureCollector.Archive(archiver);
                    archiver.AddThumbnail(_baseModelRoot);

                    var meta = new XItemMeta
                    {
                        type = XItemMeta.XItemType.Avatar,
                        infoList = metaInfoList.ToArray()
                    };

                    archiver.AddMeta(meta);
                    archiver.AddXItem(avatarXItem);
                }
            }
        }
    }
}
