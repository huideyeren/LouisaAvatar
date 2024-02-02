using System;
using System.IO;
using UnityEngine;
using XWear.IO.XResource;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Texture;

namespace XWear.IO
{
    public class XWearExporter
    {
        private readonly string _savePath;
        private readonly TextureCollectorBase _textureCollector;
        private readonly MaterialCollectorBase _materialCollector;
        private readonly GameObject _rootGameObject;

        public XWearExporter(
            string savePath,
            TextureCollectorBase textureCollector,
            MaterialCollectorBase materialCollector,
            GameObject rootGameObject
        )
        {
            _savePath = savePath;
            _textureCollector = textureCollector;
            _materialCollector = materialCollector;
            _rootGameObject = rootGameObject;
        }

        public void Run(
            ExportContext exportContext,
            ICollectComponentPlugin[] componentPlugins,
            HumanoidMap sourceHumanoidMap,
            SkinnedMeshRendererDataCollector.MeshArchiveAction meshArchiveAction,
            bool exportFromPackage
        )
        {
            if (exportContext.exportType != ExportContext.ExportType.Wear)
            {
                throw new InvalidDataException();
            }

            using (var fs = new FileStream(_savePath, FileMode.Create))
            {
                using (
                    var archiver = new XResourceContainerUtil.XResourceArchiver(
                        fs,
                        archiveFromPackage: exportFromPackage
                    )
                )
                {
                    var serializeSource = new XItemSerializeSource(
                        _rootGameObject,
                        sourceHumanoidMap,
                        componentPlugins,
                        exportType: exportContext.exportType
                    );

                    archiver.AddThumbnail(_rootGameObject);

                    var result = XItemSerializer.SerializeAsWear(
                        serializeSource,
                        _textureCollector,
                        _materialCollector,
                        saveAsDefault: exportFromPackage,
                        meshArchiveAction: meshArchiveAction,
                        archiver
                    );

                    _textureCollector.Archive(archiver);
                    var meta = new XItemMeta()
                    {
                        infoList = new[]
                        {
                            new XItemMeta.XItemInfo()
                            {
                                name = result.Name,
                                guid = result.Guid,
                                xItemType = XItemMeta.XItemType.Wear
                            }
                        }
                    };

                    archiver.AddMeta(meta);
                    archiver.AddXItem(result);
                }
            }
        }

        /// <summary>
        /// アクセサリ出力用
        /// アクセサリはHumanoidMapを必要とせず、代わりにAccessoryMapを必要とする
        /// </summary>
        /// <param name="exportContext"></param>
        /// <param name="componentPlugins"></param>
        /// <param name="accessoryMap"></param>
        /// <param name="archiver"></param>
        public void Run(
            ExportContext exportContext,
            ICollectComponentPlugin[] componentPlugins,
            AccessoryMap accessoryMap,
            XResourceContainerUtil.XResourceArchiver archiver
        )
        {
            if (exportContext.exportType != ExportContext.ExportType.Accessory)
            {
                throw new InvalidDataException("Invalid Export Type.");
            }

            var serializeResult = XItemSerializer.Serialize(
                _rootGameObject,
                accessoryMap,
                componentPlugins,
                exportType: exportContext.exportType,
                textureCollector: _textureCollector,
                archiver
            );

            File.WriteAllText(_savePath, serializeResult);
        }
    }
}
