using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XWear.IO.XResource;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Texture;

namespace XWear.IO
{
    public class XItemImporter
    {
        private readonly string _loadPath;

        public XItemImporter(string loadPath)
        {
            _loadPath = loadPath;
        }

        public async Task<(List<XItemInstance> xItemInstances, XItemMeta meta)> Run(
            IBuildComponentPlugin[] buildComponentPlugins,
            ImportContext.ImportType importType,
            TextureBuilderBase textureBuilder,
            MaterialBuilderBase materialBuilder,
            SkinnedMeshRendererDataBuilder.MeshOpenAction meshOpenAction,
            bool applySavedMaterial = true,
            AssetSaver assetSaver = null,
            bool saveExternal = false
        )
        {
            await Task.CompletedTask;

            var result = new List<XItemInstance>();
            XItemMeta meta;
            using (var fs = File.OpenRead(_loadPath))
            {
                using (var opener = new XResourceContainerUtil.XResourceOpener(fs))
                {
                    meta = opener.GetXItemMeta();

                    // 先に展開しておいてほしいアセットを保存する
                    if (assetSaver != null)
                    {
                        // todo Metaのルートに名前を入れる
                        assetSaver.SetXItemName(meta.infoList.FirstOrDefault()?.name);
#if UNITY_EDITOR
                        var clipResources = opener.GetAllXResourceAnimationClips();
                        foreach (var clipResource in clipResources)
                        {
                            assetSaver.CreateAnimationClipAssets(clipResource);
                        }
#endif
                    }

                    var infoList = meta.infoList;
                    foreach (var info in infoList)
                    {
                        var xItem = opener.GetXItem(info);
                        if (xItem == null)
                        {
                            continue;
                        }

                        var instance = XItemDeserializer.DeserializeAndBuild(
                            xItem,
                            buildComponentPlugins,
                            applySavedMaterial: applySavedMaterial,
                            xItemInfo: info,
                            opener: opener,
                            textureBuilder: textureBuilder,
                            materialBuilder: materialBuilder,
                            assetSaver: assetSaver,
                            meshOpenAction: meshOpenAction
                        );
                        result.Add(instance);
                    }
                }
            }

            return (result, meta);
        }
    }
}
