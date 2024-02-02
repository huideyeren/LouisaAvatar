using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XWear.IO.XResource.Material.Shader;
using XWear.IO.XResource.Texture;

namespace XWear.IO.XResource.Material
{
    public abstract class MaterialBuilderBase
    {
        /// <summary>
        /// Guidから構築したマテリアルの実体を引くためのマップ
        /// </summary>
        protected readonly Dictionary<string, XResourceMaterialInstance> GuidToMaterial =
            new Dictionary<string, XResourceMaterialInstance>();

        public readonly List<XResourceMaterialInstance> MaterialInstances = new();

        protected readonly TextureBuilderBase TextureBuilder;

        protected MaterialBuilderBase(TextureBuilderBase textureBuilder)
        {
            TextureBuilder = textureBuilder;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="xItem"></param>
        /// <param name="applySavedMaterial"></param>
        public abstract List<XResourceMaterialInstance> BuildXResourceMaterials(
            XItem xItem,
            bool applySavedMaterial
        );

        /// <summary>
        /// XResourceMaterialのGuidから構築されたMaterialの実体を取得する
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="material"></param>
        public bool TryGetMaterialFromGuid(string guid, out UnityEngine.Material material)
        {
            if (GuidToMaterial.TryGetValue(guid, out var instance))
            {
                material = instance.MaterialInstance;
                return true;
            }

            Debug.LogError($"Material:{guid} is not found");
            material = null;
            return false;
        }
    }
}
