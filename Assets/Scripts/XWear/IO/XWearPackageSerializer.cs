using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace XWear.IO
{
    public class XWearPackageSerializer { }

    [Serializable]
    public class XWearData { }

    [Serializable]
    public class XWearSkinnedMesh
    {
        public XWearMesh xWearMesh;
    }

    [Serializable]
    public class XWearMesh
    {
        public List<XWearMaterial> materials = new List<XWearMaterial>();
    }

    [Serializable]
    public class XWearTransform { }

    [Serializable]
    public class XWearMaterial
    {
        public enum XWearMaterialParamType
        {
            Texture,
            Color,
            Float,
            Int,
        }

        public interface IXWearMaterialParam
        {
            XWearMaterialParamType ParamType { get; set; }
        }

        [Serializable]
        public class TextureParam : IXWearMaterialParam
        {
            public XWearMaterialParamType ParamType { get; set; } = XWearMaterialParamType.Texture;

            // ここでXWearTextureを直接持たないのは同じテクスチャを利用するマテリアルも存在するため
            public string textureRef = "";
        }

        [Serializable]
        public class ColorParam : IXWearMaterialParam
        {
            public XWearMaterialParamType ParamType { get; set; } = XWearMaterialParamType.Color;
        }
    }

    [Serializable]
    public class XWearBone
    {
        public int index = 0;

        public string rawName = "";

        // いらんかも？
        public string guid = "";

        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
        public Vector3 scale = Vector3.one;

        public Vector3 localPosition = Vector3.zero;
        public Quaternion localRotation = Quaternion.identity;
        public Vector3 localScale = Vector3.one;

        public List<XWearBone> children = new List<XWearBone>();
    }

    public class XWearShader { }

    [Serializable]
    public class XWearTexture
    {
        public string rawName = "";
        public string guid = "";
    }
}
