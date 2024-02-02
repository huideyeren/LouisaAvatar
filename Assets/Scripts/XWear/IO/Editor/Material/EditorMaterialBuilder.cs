using System.Collections.Generic;
using XWear.IO.XResource;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Texture;

namespace XWear.IO.Editor.Material
{
    public class EditorMaterialBuilder : MaterialBuilderBase
    {
        private readonly AssetSaver _assetSaver;

        public EditorMaterialBuilder(TextureBuilderBase textureBuilder, AssetSaver assetSaver)
            : base(textureBuilder)
        {
            _assetSaver = assetSaver;
        }

        public override List<XResourceMaterialInstance> BuildXResourceMaterials(
            XItem xItem,
            bool applySavedMaterial
        )
        {
            var result = new List<XResourceMaterialInstance>();

            var xResourceMaterials = applySavedMaterial
                ? xItem.DefaultXResourceMaterials
                : xItem.XResourceMaterials;

            foreach (var xResourceMaterial in xResourceMaterials)
            {
                // 異なるDressItem間で共通のマテリアルが使われている場合があるので重複を考慮する
                if (!GuidToMaterial.TryGetValue(xResourceMaterial.Guid, out var instance))
                {
                    var material = MaterialBuildUtil.Build(
                        xResourceMaterial,
                        TextureBuilder,
                        applySavedMaterial
                    );

                    if (_assetSaver != null)
                    {
                        material = (UnityEngine.Material)_assetSaver.CreateAsset(material);
                    }

                    instance = new XResourceMaterialInstance(xResourceMaterial, material);
                    GuidToMaterial.Add(xResourceMaterial.Guid, instance);
                    MaterialInstances.Add(instance);
                }

                result.Add(instance);
            }

            return result;
        }
    }
}
