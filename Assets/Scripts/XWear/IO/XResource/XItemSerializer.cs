using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using XWear.IO.XResource.Accessory;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Humanoid;
using XWear.IO.XResource.Licensing;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Texture;
using XWear.IO.XResource.Transform;
using XWear.IO.XResource.Util;
using Object = UnityEngine.Object;

namespace XWear.IO.XResource
{
    [Serializable]
    public class XItemMeta
    {
        public const int CurrentVersion = 1;

        public enum XItemType
        {
            Avatar,
            BaseModel,
            Wear,
            Accessory,
        }

        [Serializable]
        public class XItemInfo
        {
            public string name;
            public string guid;
            public XItemType xItemType;
        }

        public int version;
        public XItemType type;

        public XItemInfo[] infoList = Array.Empty<XItemInfo>();
    }

    public class XItemSerializeSource
    {
        public readonly string XItemName;
        public readonly GameObject RootGameObject;
        public readonly HumanoidMap SourceHumanoidMap;
        public readonly ICollectComponentPlugin[] ComponentPlugins;
        public readonly ExportContext.ExportType ExportType;

        public XItemSerializeSource(
            GameObject rootGameObject,
            HumanoidMap sourceHumanoidMap,
            ICollectComponentPlugin[] componentPlugins,
            ExportContext.ExportType exportType,
            string xItemName = ""
        )
        {
            RootGameObject = rootGameObject;
            SourceHumanoidMap = sourceHumanoidMap;
            ComponentPlugins = componentPlugins;
            ExportType = exportType;

            XItemName = string.IsNullOrEmpty(xItemName) ? RootGameObject.name : xItemName;
        }
    }

    public static class XItemSerializer
    {
        public static AvatarXItem SerializeAsAvatar(
            XItemSerializeSource avatarSerializeSource,
            TextureCollectorBase textureCollector,
            MaterialCollectorBase materialCollector,
            bool saveAsDefault,
            SkinnedMeshRendererDataCollector.MeshArchiveAction meshArchiveAction,
            XResourceContainerUtil.XResourceArchiver archiver
        )
        {
            var avatar =
                Serialize(
                    name: avatarSerializeSource.XItemName,
                    rootGameObject: avatarSerializeSource.RootGameObject,
                    sourceHumanoidMap: avatarSerializeSource.SourceHumanoidMap,
                    componentPlugins: avatarSerializeSource.ComponentPlugins,
                    exportType: ExportContext.ExportType.Avatar,
                    textureCollector: textureCollector,
                    materialCollector: materialCollector,
                    saveAsDefault: saveAsDefault,
                    meshArchiveAction: meshArchiveAction,
                    archiver: archiver
                ) as AvatarXItem;

            if (avatar == null)
            {
                throw new InvalidDataException();
            }

            return avatar;
        }

        public static WearXItem SerializeAsWear(
            XItemSerializeSource wearSerializeSource,
            TextureCollectorBase textureCollector,
            MaterialCollectorBase materialCollector,
            bool saveAsDefault,
            SkinnedMeshRendererDataCollector.MeshArchiveAction meshArchiveAction,
            XResourceContainerUtil.XResourceArchiver archiver
        )
        {
            var wear =
                Serialize(
                    name: wearSerializeSource.XItemName,
                    rootGameObject: wearSerializeSource.RootGameObject,
                    sourceHumanoidMap: wearSerializeSource.SourceHumanoidMap,
                    componentPlugins: wearSerializeSource.ComponentPlugins,
                    exportType: ExportContext.ExportType.Wear,
                    textureCollector: textureCollector,
                    materialCollector: materialCollector,
                    saveAsDefault: saveAsDefault,
                    meshArchiveAction: meshArchiveAction,
                    archiver: archiver
                ) as WearXItem;

            if (wear == null)
            {
                throw new InvalidDataException();
            }

            return wear;
        }

        /// <summary>
        /// 与えられたRootGameObjectとHumanoidMapからXWearを生成する
        /// この関数は、アバター向けの出力と衣装向けの出力の両者を担うため、Serialize対象に無関心である
        /// (例: Serialize対象にXResourceを含むかどうか？の情報を持たない)
        /// 代わりに、Serialize/Deserializeの相互で保持しておきたいパラメータなどが存在する場合、
        /// Pluginを介して情報の保存をおこなう
        /// </summary>
        /// <param name="name"></param>
        /// <param name="rootGameObject"></param>
        /// <param name="sourceHumanoidMap"></param>
        /// <param name="componentPlugins">コンポーネント</param>
        /// <param name="exportType"></param>
        /// <param name="textureCollector"></param>
        /// <param name="materialCollector"></param>
        /// <param name="saveAsDefault"></param>
        /// <param name="meshArchiveAction"></param>
        /// <param name="archiver"></param>
        /// <returns></returns>
        private static XItem Serialize(
            string name,
            GameObject rootGameObject,
            HumanoidMap sourceHumanoidMap,
            ICollectComponentPlugin[] componentPlugins,
            ExportContext.ExportType exportType,
            TextureCollectorBase textureCollector,
            MaterialCollectorBase materialCollector,
            SkinnedMeshRendererDataCollector.MeshArchiveAction meshArchiveAction,
            XResourceContainerUtil.XResourceArchiver archiver,
            bool saveAsDefault
        )
        {
            List<XWearSource> xWearSources;
            switch (exportType)
            {
                case ExportContext.ExportType.Avatar:
                    xWearSources = GetXWearSource(
                        rootGameObject,
                        sourceHumanoidMap,
                        componentPlugins
                    );
                    break;
                case ExportContext.ExportType.Wear:
                    xWearSources = GetXWearSourceSplitBySkinnedMeshRenderer(
                        rootGameObject,
                        sourceHumanoidMap,
                        componentPlugins
                    );
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(exportType), exportType, null);
            }

            UnityEngine.Debug.Log(exportType);

            if (exportType == ExportContext.ExportType.Avatar)
            {
                return GenerateXItem<AvatarXItem>(
                    xWearSources,
                    componentPlugins,
                    exportType,
                    archiver,
                    name,
                    textureCollector,
                    materialCollector: materialCollector,
                    meshArchiveAction: meshArchiveAction,
                    rootGameObject
                );
            }

            return GenerateXItem<WearXItem>(
                xWearSources,
                componentPlugins,
                exportType,
                archiver,
                name,
                textureCollector: textureCollector,
                materialCollector: materialCollector,
                meshArchiveAction: meshArchiveAction
            );
        }

        public static T GenerateXItem<T>(
            List<XWearSource> xWearSources,
            ICollectComponentPlugin[] componentPlugins,
            ExportContext.ExportType exportType,
            XResourceContainerUtil.XResourceArchiver archiver,
            string name,
            TextureCollectorBase textureCollector,
            MaterialCollectorBase materialCollector,
            SkinnedMeshRendererDataCollector.MeshArchiveAction meshArchiveAction,
            GameObject rootGameObject = null
        )
            where T : XItem, new()
        {
            var xItem = new T();
            Debug.Log("Debug2");
            foreach (var xWearSource in xWearSources)
            {
                var collectedXResource = CollectXResource(
                    xItem,
                    xWearSource,
                    materialCollector,
                    componentPlugins,
                    exportType,
                    meshArchiveAction,
                    archiver
                );
                xItem.XResources.Add(collectedXResource);
            }

            Debug.Log("Debug7");

            // すべてのテクスチャとマテリアルは共通参照を考慮し、XWearの直下に保存しておく
            // 参照渡しでは壊れるので、必ず再度ToList()する
            xItem.XResourceMaterials = materialCollector.CurrentContextReferenced.ToList();
            materialCollector.CollectDefaultMaterials(xItem);

            // テクスチャがフォールバック先Shaderから参照されていない場合、CollectDefaultMaterialsで
            // TextureCollectorへTryCollectされるので、必ずCollectDefaultMaterialsよりもあとに
            // XResourceTexturesへのアサインを実行する必要がある
            xItem.XResourceTextures = textureCollector.CurrentContextReferenced.ToList();

            foreach (var xWearSource in xWearSources)
            {
                Debug.Log(xWearSource.RootGameObject.name);
                Object.DestroyImmediate(xWearSource.RootGameObject);
            }

            Debug.Log("Debug9");

            // Guidの収集をクリアする
            // XAvatarにXWearが梱包される場合、全体としては共通のテクスチャを保持したいが、
            // それぞれのXItemでどのテクスチャを参照したか？という情報を保持する必要がある
            // そのため、XItemのシリアライズごとにGuidの収集とクリアを実施し、
            // 現在シリアライズ中のXItemが参照したGuidだけを確保できるようにする
            textureCollector.Flush();
            materialCollector.Flush();

            xItem.Guid = Guid.NewGuid().ToString();
            xItem.Name = name;
            return xItem;
        }

        public static string Serialize(
            GameObject rootGameObject,
            AccessoryMap accessoryMap,
            ICollectComponentPlugin[] componentPlugins,
            ExportContext.ExportType exportType,
            TextureCollectorBase textureCollector,
            XResourceContainerUtil.XResourceArchiver archiver
        )
        {
            throw new NotImplementedException();
        }

        private static List<XWearSource> GetXWearSourceSplitBySkinnedMeshRenderer(
            GameObject rootGameObject,
            HumanoidMap sourceHumanoidMap,
            ICollectComponentPlugin[] collectComponentPlugins
        )
        {
            // 与えられたRootとなるGameObjectからSkinnedMeshRendererの一覧を収集し、
            // SkinnedMeshRendererを一つしか持たない階層つきのGameObjectを作成する
            var sourceSkinnedMeshRenderers =
                rootGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();

            // SkinnedMeshRenderer単位でXWearSourceのリストを作る
            var xWearSources = MeshSplitter.GenerateXWearSourcesWithSplitBySkinnedMeshRenderer(
                sourceSkinnedMeshRenderers,
                sourceHumanoidMap,
                collectComponentPlugins
            );

            return xWearSources;
        }

        private static List<XWearSource> GetXWearSource(
            GameObject rootGameObject,
            HumanoidMap sourceHumanoidMap,
            ICollectComponentPlugin[] collectComponentPlugins
        )
        {
            var xWearSource = GenerateXWearSourceUtil.GenerateWithNoSplitSkinnedMeshRenderers(
                rootGameObject,
                sourceHumanoidMap,
                collectComponentPlugins
            );
            return new List<XWearSource>() { xWearSource };
        }

        /// <summary>
        /// XResourceの構築に必要な要素の収集をする
        /// </summary>
        /// <param name="xItem"></param>
        /// <param name="xSource"></param>
        /// <param name="materialCollector"></param>
        /// <param name="componentPlugins"></param>
        /// <param name="exportType"></param>
        /// <param name="meshArchiveAction"></param>
        /// <param name="archiver"></param>
        private static XResource CollectXResource(
            XItem xItem,
            XSourceBase xSource,
            MaterialCollectorBase materialCollector,
            ICollectComponentPlugin[] componentPlugins,
            ExportContext.ExportType exportType,
            SkinnedMeshRendererDataCollector.MeshArchiveAction meshArchiveAction,
            XResourceContainerUtil.XResourceArchiver archiver
        )
        {
            XResource result;
            switch (exportType)
            {
                case ExportContext.ExportType.Avatar:
                case ExportContext.ExportType.Wear:
                    result = new HumanoidXResource();
                    break;
                case ExportContext.ExportType.Accessory:
                    result = new AccessoryXResource();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(exportType), exportType, null);
            }

            var xWearRoot = xSource.RootGameObject.transform;

            // GameObject,Transform,アタッチされているコンポーネントを収集する
            var gameObjectCollector = new GameObjectWithTransformCollector(
                xSource.SourceToEditTransformMap
            );

            Debug.Log("Collect GameObject");

            var index = 0;
            var rootXResourceGameObject = gameObjectCollector.Collect(
                xSource.RootGameObject,
                isFirst: true,
                ref index
            );

            result.Root = rootXResourceGameObject;

            Debug.Log("Collect Humanoid");

            if (xSource is XWearSource xWearSource && result is HumanoidXResource humanoidXResource)
            {
                var humanoidMapCollector = new HumanoidMapCollector();
                humanoidXResource.HumanoidMap = humanoidMapCollector.Collect(
                    xWearSource.HumanoidMap,
                    gameObjectCollector
                );
            }
            else if (
                xSource is XAccessorySource xAccessorySource
                && result is AccessoryXResource accessoryXResource
            )
            {
                var accessoryMapCollector = new AccessoryMapCollector();
                accessoryXResource.AccessoryMap = accessoryMapCollector.Collect(
                    xAccessorySource.AccessoryMap
                );
            }

            Debug.Log("Collect Component");
            // コンポーネントはマテリアル・テクスチャ・Humanoidなどと依存関係が強い可能性があるので、必ず最後に収集する
            var componentCollector = new ComponentCollector(
                xItem,
                xSource,
                gameObjectCollector,
                materialCollector,
                componentPlugins,
                meshArchiveAction,
                archiver
            );

            componentCollector.CollectComponentsData();

            result.GameObjects = gameObjectCollector.GetCollectedXResourceGameObjects();

            Debug.Log("Debug6");

            return result;
        }
    }
}
