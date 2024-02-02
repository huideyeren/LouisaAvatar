using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XWear.IO.XResource.Material.Shader;

namespace XWear.IO.XResource.Material
{
    [Serializable]
    public class XResourceMaterial
    {
        public string name;
        public string Guid;
        public string ShaderName;

        [SerializeReference]
        public List<IXResourceShaderProperty> ShaderProperties =
            new List<IXResourceShaderProperty>();

        public List<MaterialTag> MaterialTags = new List<MaterialTag>();
        public string[] ShaderKeywords = Array.Empty<string>();
        public int RenderQueue;

        public XResourceMaterial(
            string name,
            string guid,
            string shaderName,
            List<IXResourceShaderProperty> shaderProperties,
            List<MaterialTag> materialTags,
            string[] shaderKeywords,
            int renderQueue
        )
        {
            this.name = name;
            Guid = guid;
            ShaderName = shaderName;
            ShaderProperties = shaderProperties;
            MaterialTags = materialTags;
            ShaderKeywords = shaderKeywords;
            RenderQueue = renderQueue;
        }

        public XResourceMaterial() { }

        public XResourceMaterial Copy()
        {
            var copiedShaderProperties = new List<IXResourceShaderProperty>();
            foreach (var shaderProperty in ShaderProperties)
            {
                copiedShaderProperties.Add(shaderProperty.Copy());
            }

            var copiedShaderKeyWords = new string[ShaderKeywords.Length];
            for (int i = 0; i < ShaderKeywords.Length; i++)
            {
                copiedShaderKeyWords[i] = ShaderKeywords[i];
            }

            var copiedMaterialTags = new List<MaterialTag>();
            foreach (var materialTag in MaterialTags)
            {
                copiedMaterialTags.Add(
                    new MaterialTag()
                    {
                        MaterialTagKey = materialTag.MaterialTagKey,
                        TagValue = materialTag.TagValue
                    }
                );
            }

            return new XResourceMaterial(
                name: name,
                guid: Guid,
                shaderName: ShaderName,
                shaderProperties: copiedShaderProperties,
                materialTags: copiedMaterialTags,
                shaderKeywords: copiedShaderKeyWords,
                renderQueue: RenderQueue
            );
        }
    }

    public class XResourceMaterialInstance
    {
        public readonly string Guid;
        public readonly XResourceMaterial SourceXResourceMaterial;
        public readonly UnityEngine.Material MaterialInstance;
        public readonly List<IXResourceShaderProperty> XResourceShaderProperties;

        public XResourceMaterialInstance(
            XResourceMaterial xResourceMaterial,
            UnityEngine.Material instance
        )
        {
            SourceXResourceMaterial = xResourceMaterial;
            Guid = xResourceMaterial.Guid;
            MaterialInstance = instance;
            XResourceShaderProperties = xResourceMaterial.ShaderProperties;
        }
    }
}
