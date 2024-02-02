using System;
using XWear.IO.XResource.Material.Shader;
using XWear.IO.XResource.Material.Shader.Fallback;
using XWear.IO.XResource.Texture;

namespace XWear.IO.XResource.Material
{
    public static class MaterialBuildUtil
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="xResourceMaterial"></param>
        /// <param name="textureBuilder"></param>
        /// <param name="applySavedMaterial"></param>
        /// <returns></returns>
        public static UnityEngine.Material Build(
            XResourceMaterial xResourceMaterial,
            TextureBuilderBase textureBuilder,
            bool applySavedMaterial
        )
        {
            var shaderName = xResourceMaterial.ShaderName;
            var shader = UnityEngine.Shader.Find(shaderName);
            var isNeedFallback = false;
            if (shader == null && !applySavedMaterial)
            {
                shader = UnityEngine.Shader.Find("Hidden/lilToonOutline");
                isNeedFallback = true;
            }
            else if (shader == null && applySavedMaterial)
            {
                // applySavedMaterialがtrueのとき、フォールバックを考慮せず、もともとのShaderをあてる
                throw new Exception("対応するシェーダーが見つかりませんでした");
            }

            var newMaterial = new UnityEngine.Material(shader);
            newMaterial.name = xResourceMaterial.name;
            if (isNeedFallback)
            {
                ShaderFallbackUtil.FallbackToLilToon(
                    xResourceMaterial,
                    shader,
                    newMaterial,
                    textureBuilder
                );
            }

            var shaderProperties = xResourceMaterial.ShaderProperties;
            foreach (var shaderProperty in shaderProperties)
            {
                var propertyName = shaderProperty.PropertyName;
                if (shaderProperty is ShaderTextureProperty textureProperty)
                {
                    if (
                        textureBuilder.TryGetTextureFromGuid(
                            textureProperty.TextureGuid,
                            out var texture
                        )
                    )
                    {
                        newMaterial.SetTexture(propertyName, texture);
                    }
                }

                if (shaderProperty is ShaderColorProperty colorProperty)
                {
                    newMaterial.SetColor(propertyName, colorProperty.Color);
                }

                if (shaderProperty is ShaderFloatProperty floatProperty)
                {
                    newMaterial.SetFloat(propertyName, floatProperty.Value);
                }

                if (shaderProperty is ShaderVectorProperty vectorProperty)
                {
                    newMaterial.SetVector(propertyName, vectorProperty.Vector);
                }
                // todo other properties
            }

            MaterialTagUtil.BuildMaterialTags(xResourceMaterial.MaterialTags, newMaterial);
            newMaterial.shaderKeywords = xResourceMaterial.ShaderKeywords;
            newMaterial.renderQueue = xResourceMaterial.RenderQueue;

            return newMaterial;
        }
    }
}
