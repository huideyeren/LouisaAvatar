using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Mesh
{
    /// <summary>
    /// XResourceのSkinnedMeshRendererに保存される
    /// 同じメッシュをSkinnedMeshRenderer間で持つことは考慮していないため、
    /// 異なるSkinnedMeshRendererであればそれが参照するXResourceMeshが一つ生成される
    /// </summary>
    public class XResourceMesh
    {
        public string Name = "";
        public IndexFormat IndexFormat;
        public int VertexCount;
        public int BoneCount;

        public XResourceMesh(SkinnedMeshRenderer source)
        {
            var sharedMesh = source.sharedMesh;
            Name = sharedMesh.name;
            IndexFormat = sharedMesh.indexFormat;
            VertexCount = sharedMesh.vertexCount;
            BoneCount = source.bones.Length;
        }

        public class XResourceSubMesh
        {
            public int[] Indices;
            public MeshTopology Topology;
        }

        public class XResourceBlendShape
        {
            public string BlendShapeName;
            public List<BlendShapeFrame> BlendShapeFrames = new List<BlendShapeFrame>();
        }

        public class BlendShapeFrame
        {
            public float Weight;
            public Vector3[] Vertices;
            public Vector3[] Normals;
            public Vector3[] Tangents;
        }

        public XResourceMesh() { }
    }
}
