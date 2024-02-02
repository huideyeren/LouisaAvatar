#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace XWear.IO.Editor
{
    public static class UnityBuiltInAssetUtil
    {
        private static readonly Dictionary<string, Type> BuiltInAssetPathToType = new Dictionary<
            string,
            Type
        >()
        {
            { "Default-Line.mat", typeof(UnityEngine.Material) },
            { "Default-Material.mat", typeof(UnityEngine.Material) },
            { "Default-Particle.mat", typeof(UnityEngine.Material) },
            { "Default-ParticleSystem.mat", typeof(UnityEngine.Material) },
            { "Default-Skybox.mat", typeof(UnityEngine.Material) },
            { "Default-Terrain-Diffuse.mat", typeof(UnityEngine.Material) },
            { "Default-Terrain-Specular.mat", typeof(UnityEngine.Material) },
            { "Default-Terrain-Standard.mat", typeof(UnityEngine.Material) },
            { "VR/Materials/SpatialMappingOcclusion.mat", typeof(UnityEngine.Material) },
            { "VR/Materials/SpatialMappingWireframe.mat", typeof(UnityEngine.Material) },
            { "Sprites-Default.mat", typeof(UnityEngine.Material) },
            { "Sprites-Mask.mat", typeof(UnityEngine.Material) },
            { "UI/Skin/Background.psd", typeof(Sprite) },
            { "UI/Skin/Checkmark.psd", typeof(Sprite) },
            { "Default-Checker.png", typeof(Texture2D) },
            { "Default-Checker-Gray.png", typeof(Texture2D) },
            { "Default-Particle.psd", typeof(Texture2D) },
            { "Default-ParticleSystem.psd", typeof(Texture2D) },
            { "UI/Skin/DropdownArrow.psd", typeof(Sprite) },
            { "UI/Skin/InputFieldBackground.psd", typeof(Sprite) },
            { "UI/Skin/Knob.psd", typeof(Sprite) },
            { "UI/Skin/UIMask.psd", typeof(Sprite) },
            { "UI/Skin/UISprite.psd", typeof(Sprite) },
        };

        private static readonly Dictionary<string, string> BuiltInMaterialAssetNameToPath =
            new Dictionary<string, string>()
            {
                { "Default-Line", "Default-Line.mat" },
                { "Default-Material", "Default-Material.mat" },
                { "Default-Particle", "Default-Particle.mat" },
                { "Default-ParticleSystem", "Default-ParticleSystem.mat" },
                { "Default-Skybox", "Default-Skybox.mat" },
                { "Default-Terrain-Diffuse", "Default-Terrain-Diffuse.mat" },
                { "Default-Terrain-Specular", "Default-Terrain-Specular.mat" },
                { "Default-Terrain-Standard", "Default-Terrain-Standard.mat" },
                {
                    "VR/Materials/SpatialMappingOcclusion",
                    "VR/Materials/SpatialMappingOcclusion.mat"
                },
                {
                    "VR/Materials/SpatialMappingWireframe",
                    "VR/Materials/SpatialMappingWireframe.mat"
                },
                { "Sprites-Default", "Sprites-Default.mat" },
                { "Sprites-Mask", "Sprites-Mask.mat" },
            };

        private static readonly Dictionary<string, string> BuiltInTextureAssetNameToPath =
            new Dictionary<string, string>()
            {
                { "Background", "UI/Skin/Background.psd" },
                { "Checkmark", "UI/Skin/Checkmark.psd" },
                { "Default-Checker", "Default-Checker.png" },
                { "Default-Checker-Gray", "Default-Checker-Gray.png" },
                { "Default-Particle", "Default-Particle.psd" },
                { "Default-ParticleSystem", "Default-ParticleSystem.psd" },
                { "DropdownArrow", "UI/Skin/DropdownArrow.psd" },
                { "InputFieldBackground", "UI/Skin/InputFieldBackground.psd" },
                { "Knob", "UI/Skin/Knob.psd" },
                { "UIMask", "UI/Skin/UIMask.psd" },
                { "UISprite", "UI/Skin/UISprite.psd" },
            };

        private static string[] _builtInExtraAllNames;

        private static string[] BuiltInExtraAllNames
        {
            get
            {
                if (_builtInExtraAllNames == null)
                {
                    _builtInExtraAllNames = UnityEditor.AssetDatabase
                        .LoadAllAssetsAtPath("Resources/unity_builtin_extra")
                        .Select(x => x.name)
                        .ToArray();
                }

                return _builtInExtraAllNames;
            }
        }

        public static bool IsBuiltInAsset(string assetName)
        {
            return BuiltInExtraAllNames.Contains(assetName);
        }

        public static bool GetBuiltInMaterial(
            string assetName,
            out UnityEngine.Object asset,
            out string ext
        )
        {
            asset = null;
            ext = "";

            if (!BuiltInMaterialAssetNameToPath.TryGetValue(assetName, out var assetPath))
            {
                return false;
            }

            ext = Path.GetExtension(assetPath);
            return GetBuiltInAsset(assetPath, out asset);
        }

        public static bool GetBuiltInTexture(
            string assetName,
            out UnityEngine.Object asset,
            out string ext
        )
        {
            asset = null;
            ext = "";

            if (!BuiltInTextureAssetNameToPath.TryGetValue(assetName, out var assetPath))
            {
                return false;
            }

            ext = Path.GetExtension(assetPath);
            return GetBuiltInAsset(assetPath, out asset);
        }

        private static bool GetBuiltInAsset(string assetPath, out UnityEngine.Object asset)
        {
            asset = null;
            if (!BuiltInAssetPathToType.TryGetValue(assetPath, out var t))
            {
                return false;
            }

            asset = UnityEditor.AssetDatabase.GetBuiltinExtraResource(t, assetPath);
            return true;
        }
    }
}
#endif
