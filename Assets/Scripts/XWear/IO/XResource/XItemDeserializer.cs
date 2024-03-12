using System;
using System.Collections.Generic;
using System.Linq;
using XWear.IO.XResource.Accessory;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Humanoid;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Texture;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource
{
    public static class XItemDeserializer
    {
        /// <summary>
        /// XWearInstanceへのデシリアライズとインスタンス化を行う
        /// </summary>
        /// <param name="xItem"></param>
        /// <param name="buildComponentPlugins"></param>
        /// <param name="applySavedMaterial"></param>
        /// <param name="xItemInfo"></param>
        /// <param name="meshOpenAction"></param>
        /// <param name="opener"></param>
        /// <param name="textureBuilder"></param>
        /// <param name="materialBuilder"></param>
        /// <param name="assetSaver"></param>
        /// <returns></returns>
        public static XItemInstance DeserializeAndBuild(
            XItem xItem,
            IBuildComponentPlugin[] buildComponentPlugins,
            bool applySavedMaterial,
            XItemMeta.XItemInfo xItemInfo,
            SkinnedMeshRendererDataBuilder.MeshOpenAction meshOpenAction,
            XResourceContainerUtil.XResourceOpener opener,
            TextureBuilderBase textureBuilder,
            MaterialBuilderBase materialBuilder,
            AssetSaver assetSaver = null
        )
        {
            // 描画周りはテクスチャ=>マテリアル=>SkinnedMeshRenderer とビルドされていく
            // テクスチャやマテリアルは異なるXResource間で同じ参照を持つことがあるので、先にビルドする
            var xResourceTextureInstances = textureBuilder.BuildXResourceTextures(xItem, opener);
            var xResourceMaterialInstances = materialBuilder.BuildXResourceMaterials(
                xItem,
                applySavedMaterial
            );

            var instances = new List<IXResourceInstance>();
            foreach (var xResource in xItem.XResources)
            {
                var xResourceItemInstance = BuildXResource(
                    xResource,
                    materialBuilder,
                    buildComponentPlugins,
                    xItemInfo,
                    meshOpenAction,
                    opener,
                    assetSaver
                );
                instances.Add(xResourceItemInstance);
            }

            if (
                xItemInfo.xItemType == XItemMeta.XItemType.Avatar
                || xItemInfo.xItemType == XItemMeta.XItemType.BaseModel
            )
            {
                return new AvatarXItemInstance(xItem as AvatarXItem)
                {
                    XResourceInstances = instances,
                    XResourceMaterialInstances = xResourceMaterialInstances,
                    XResourceTextureInstances = xResourceTextureInstances
                };
            }
            else
            {
                return new WearXItemInstance(xItem as WearXItem)
                {
                    XResourceInstances = instances,
                    XResourceMaterialInstances = xResourceMaterialInstances,
                    XResourceTextureInstances = xResourceTextureInstances
                };
            }
        }

        /// <summary>
        /// XResourceをビルドする
        /// </summary>
        /// <param name="xResource"></param>
        /// <param name="materialBuilder"></param>
        /// <param name="buildComponentPlugins"></param>
        /// <param name="xItemInfo"></param>
        /// <param name="meshOpenAction"></param>
        /// <param name="opener"></param>
        /// <param name="assetSaver"></param>
        private static IXResourceInstance BuildXResource(
            XResource xResource,
            MaterialBuilderBase materialBuilder,
            IBuildComponentPlugin[] buildComponentPlugins,
            XItemMeta.XItemInfo xItemInfo,
            SkinnedMeshRendererDataBuilder.MeshOpenAction meshOpenAction,
            XResourceContainerUtil.XResourceOpener opener,
            AssetSaver assetSaver = null
        )
        {
            // XResourceの中のXResourceGameObjectからヒエラルキーを構築する
            var gameObjectBuilder = new GameObjectWithTransformBuilder();
            var xResourceGameObjectRoot = gameObjectBuilder.BuildXResourceGameObjects(
                xResource.Root
            );

            // XResourceGameObjectにぶら下がっているComponentを再構築する
            var componentBuilder = new ComponentBuilder(
                gameObjectBuilder,
                materialBuilder,
                buildComponentPlugins,
                meshOpenAction,
                opener,
                assetSaver
            );
            componentBuilder.Build(out var builtComponents);

            IXResourceInstance result;
            switch (xItemInfo.xItemType)
            {
                case XItemMeta.XItemType.Avatar:
                case XItemMeta.XItemType.BaseModel:
                case XItemMeta.XItemType.Wear:
                    result = new HumanoidXResourceInstance(
                        xResource: xResource,
                        instance: xResourceGameObjectRoot.Instance,
                        components: builtComponents,
                        xResourceTransformIndexToBuiltTransformEntityMap: gameObjectBuilder.XResourceTransformIndexToBuiltTransformEntityMap
                    );
                    var humanoidXResource = (HumanoidXResource)xResource;
                    var humanoidMapBuilder = new HumanoidMapBuilder();
                    var humanoidMapComponent = humanoidMapBuilder.Build(
                        (HumanoidXResourceInstance)result,
                        humanoidXResource.HumanoidMap,
                        gameObjectBuilder,
                        xItemInfo.xItemType,
                        assetSaver
                    );

                    ((HumanoidXResourceInstance)result).HumanoidMapComponent = humanoidMapComponent;
                    return result;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(xItemInfo.xItemType),
                        xItemInfo.xItemType,
                        null
                    );
            }
        }
    }
}
