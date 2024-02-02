using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using XWear.IO.XResource.Material.Shader;
using XWear.IO.XResource.Texture;

namespace XWear.IO.XResource.Material
{
    public abstract class MaterialCollectorBase
    {
        protected readonly TextureCollectorBase TextureCollector;

        /// <summary>
        /// マテリアルの実体から収集したXResourceMaterialを引くためのメモ
        /// </summary>
        public readonly Dictionary<
            UnityEngine.Material,
            XResourceMaterial
        > MaterialToXResourceMaterialMemo =
            new Dictionary<UnityEngine.Material, XResourceMaterial>();

        public readonly List<XResourceMaterial> CurrentContextReferenced =
            new List<XResourceMaterial>();

        protected MaterialCollectorBase(TextureCollectorBase textureCollector)
        {
            TextureCollector = textureCollector;
        }

        public List<XResourceMaterial> GetCollectedXResourceMaterial()
        {
            return MaterialToXResourceMaterialMemo.Select(x => x.Value).ToList();
        }

        public XResourceMaterial TryGetCollectedXResourceMaterial(UnityEngine.Material material)
        {
            if (MaterialToXResourceMaterialMemo.TryGetValue(material, out var collected))
            {
                return collected;
            }

            throw new DirectoryNotFoundException($"{material.name}が追加されていません");
        }

        /// <summary>
        /// マテリアルの実体からXResourceMaterialを生成する。
        /// 生成した結果は_materialToXResourceMaterialMemoに保存される
        /// すでにsourceMaterialを元に生成したXResourceMaterialが存在する場合は、
        /// _materialToXResourceMaterialMemoからその結果を返す。
        /// </summary>
        /// <param name="sourceMaterial"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public abstract XResourceMaterial TryCollectAndAdd(UnityEngine.Material sourceMaterial);

        public abstract void CollectDefaultMaterials(XItem xItem);

        protected XResourceMaterial CollectAsNew(UnityEngine.Material sourceMaterial)
        {
            var materialTags = MaterialTagUtil.CollectMaterialTags(sourceMaterial);
            var shaderKeywords = sourceMaterial.shaderKeywords;

            var shader = sourceMaterial.shader;
            var shaderName = shader.name;
            var propertyCount = shader.GetPropertyCount();
            var shaderPropertyMemos =
                new List<(string propertyName, ShaderPropertyType propertyType)>();
            for (int propertyIndex = 0; propertyIndex < propertyCount; propertyIndex++)
            {
                var propertyName = shader.GetPropertyName(propertyIndex);
                var propertyType = shader.GetPropertyType(propertyIndex);
                shaderPropertyMemos.Add((propertyName, propertyType));
            }

            var shaderProperties = new List<IXResourceShaderProperty>();
            foreach (var shaderPropertyMemo in shaderPropertyMemos)
            {
                IXResourceShaderProperty property = null;
                var propertyType = shaderPropertyMemo.propertyType;
                var propertyName = shaderPropertyMemo.propertyName;

                switch (propertyType)
                {
                    case ShaderPropertyType.Color:
                        property = new ShaderColorProperty(
                            propertyName,
                            sourceMaterial.GetColor(propertyName)
                        );
                        break;
                    case ShaderPropertyType.Vector:
                        property = new ShaderVectorProperty(
                            propertyName,
                            sourceMaterial.GetVector(propertyName)
                        );
                        break;
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        property = new ShaderFloatProperty(
                            propertyName,
                            sourceMaterial.GetFloat(propertyName)
                        );
                        break;
                    case ShaderPropertyType.Texture:
                        var sourceTexture = sourceMaterial.GetTexture(propertyName);
                        if (sourceTexture == null)
                        {
                            continue;
                        }

                        // おなじテクスチャを共有する場合があるので、Textureの実体からClothItemTextureを引くマップをTextureCollectorが保持する
                        var xResourceTexture = TextureCollector.TryCollect(sourceTexture);
                        property = new ShaderTextureProperty(
                            propertyName: propertyName,
                            textureGuid: xResourceTexture.Guid
                        );
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (property != null)
                {
                    shaderProperties.Add(property);
                }
            }

            var xResourceMaterial = new XResourceMaterial(
                name: sourceMaterial.name,
                guid: Guid.NewGuid().ToString(),
                shaderName: shaderName,
                shaderProperties,
                materialTags,
                shaderKeywords,
                sourceMaterial.renderQueue
            );

            MaterialToXResourceMaterialMemo.Add(sourceMaterial, xResourceMaterial);
            if (!CurrentContextReferenced.Contains(xResourceMaterial))
            {
                CurrentContextReferenced.Add(xResourceMaterial);
            }

            return xResourceMaterial;
        }

        public void Flush()
        {
            CurrentContextReferenced.Clear();
        }
    }
}
