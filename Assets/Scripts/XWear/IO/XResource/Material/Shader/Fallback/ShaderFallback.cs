using System.Linq;
using XWear.IO.XResource.Texture;

namespace XWear.IO.XResource.Material.Shader.Fallback
{
    public static class ShaderFallbackUtil
    {
        public static void FallbackToLilToon(
            XResourceMaterial xResource,
            UnityEngine.Shader lilToonShader,
            UnityEngine.Material resultMaterial,
            TextureBuilderBase textureBuilder
        )
        {
            var shaderName = xResource.ShaderName;
            if (shaderName.Contains("poiyomi"))
            {
                FallbackPoiyomi(xResource, lilToonShader, resultMaterial, textureBuilder);
            }
        }

        private static void FallbackPoiyomi(
            XResourceMaterial xResource,
            UnityEngine.Shader lilToonShader,
            UnityEngine.Material resultMaterial,
            TextureBuilderBase textureBuilder
        )
        {
            resultMaterial.shader = lilToonShader;

            var poiyomiOutLineColor = xResource.ShaderProperties.FirstOrDefault(
                x => x.PropertyName == PoiyomiShaderProperties.OutlineColor
            );

            if (
                poiyomiOutLineColor != null
                && poiyomiOutLineColor is ShaderColorProperty outLineColorProperty
            )
            {
                resultMaterial.SetColor(
                    LilToonShaderProperties.LilToonOutlineColorId,
                    outLineColorProperty.Color
                );
            }

            var poiyomiOutLineTexture = xResource.ShaderProperties.FirstOrDefault(
                x => x.PropertyName == PoiyomiShaderProperties.OutlineTexture
            );

            if (
                poiyomiOutLineTexture != null
                && poiyomiOutLineTexture is ShaderTextureProperty outLineTextureProperty
            )
            {
                if (!string.IsNullOrEmpty(outLineTextureProperty.TextureGuid))
                {
                    textureBuilder.TryGetTextureFromGuid(
                        outLineTextureProperty.TextureGuid,
                        out var outlineTexture
                    );
                    resultMaterial.SetTexture(
                        LilToonShaderProperties.LilToonOutlineTextureId,
                        outlineTexture
                    );
                }
            }

            var poiyomiOutlineWidthMask = xResource.ShaderProperties.FirstOrDefault(
                x => x.PropertyName == PoiyomiShaderProperties.OutlineWidthMask
            );

            if (
                poiyomiOutlineWidthMask != null
                && poiyomiOutlineWidthMask is ShaderTextureProperty outLineWidthMaskTextureProperty
            )
            {
                if (!string.IsNullOrEmpty(outLineWidthMaskTextureProperty.TextureGuid))
                {
                    textureBuilder.TryGetTextureFromGuid(
                        outLineWidthMaskTextureProperty.TextureGuid,
                        out var outlineWidthTexture
                    );
                    resultMaterial.SetTexture(
                        LilToonShaderProperties.LilToonOutlineWidthMaskId,
                        outlineWidthTexture
                    );
                }
            }

            var poiyomiOutlineWidth = xResource.ShaderProperties.FirstOrDefault(
                x => x.PropertyName == PoiyomiShaderProperties.OutlineWidth
            );

            if (
                poiyomiOutlineWidth != null
                && poiyomiOutlineWidth is ShaderFloatProperty outLineWidthProperty
            )
            {
                resultMaterial.SetFloat(
                    LilToonShaderProperties.LilToonOutlineWidthId,
                    outLineWidthProperty.Value
                );
            }
        }

        private struct LilToonShaderProperties
        {
            private const string OutlineColor = "_OutlineColor";
            private const string OutlineTexture = "_OutlineTex";
            private const string OutlineWidthMask = "_OutlineWidthMask";
            private const string OutlineWidth = "_OutlineWidth";

            public static readonly int LilToonOutlineColorId = UnityEngine.Shader.PropertyToID(
                OutlineColor
            );

            public static readonly int LilToonOutlineTextureId = UnityEngine.Shader.PropertyToID(
                OutlineTexture
            );

            public static readonly int LilToonOutlineWidthMaskId = UnityEngine.Shader.PropertyToID(
                OutlineWidthMask
            );

            public static readonly int LilToonOutlineWidthId = UnityEngine.Shader.PropertyToID(
                OutlineWidth
            );
        }

        private struct PoiyomiShaderProperties
        {
            public const string OutlineColor = "_LineColor";
            public const string OutlineTexture = "_OutlineTexture";
            public const string OutlineWidthMask = "_OutlineMask";
            public const string OutlineWidth = "_LineWidth";
        }
    }
}
