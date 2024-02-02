using UnityEngine;

namespace XWear.IO.XResource.Texture.Util
{
    /// <summary>
    /// The following code is a modified version of VRMShader's code licensed under the MIT License
    /// Copyright (c) 2020 VRM Consortium Copyright (c) 2018 Masataka SUMI for MToon
    /// https://github.com/vrm-c/UniVRM/blob/v0.108.0/Assets/VRMShaders/LICENSE.md
    /// original: https://github.com/vrm-c/UniVRM/blob/v0.108.0/Assets/VRMShaders/GLTF/IO/Runtime/Texture/Converter/NormalConverter.cs
    /// </summary>
    public static class NormalConverter
    {
        private static UnityEngine.Material _exporter;

        private static UnityEngine.Material Exporter
        {
            get
            {
                if (_exporter == null)
                {
                    _exporter = new UnityEngine.Material(
                        Shader.Find("Hidden/XWear/NormalMapExporter")
                    );
                }

                return _exporter;
            }
        }

        public static void Cleanup()
        {
            if (_exporter != null)
            {
                Object.DestroyImmediate(_exporter);
            }
        }

        public static byte[] Export(UnityEngine.Texture texture)
        {
            var normalTexture = texture.CopyTexture(TextureFormat.RGB24, Exporter);
            var result = normalTexture.EncodeToPNG();
            Object.DestroyImmediate(normalTexture);
            return result;
        }
    }
}
