using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XWear.IO.Common
{
    /// <summary>
    /// Copyright (c) 2018 ousttrue
    /// The following code uses UniGLTF's code licensed under the MIT license
    /// https://github.com/vrm-c/UniVRM/blob/ffd4e63bcd499ae1a560a401b713699ba64a9ddd/Assets/UniGLTF/LICENSE.md
    /// original: https://github.com/vrm-c/UniVRM/blob/ffd4e63bcd499ae1a560a401b713699ba64a9ddd/Assets/UniGLTF/Runtime/MeshUtility/MeshExtensions.cs
    /// </summary>
    public static class MeshUtility
    {
        public static Mesh CopyMesh(Mesh src, bool copyBlendShape = true)
        {
            var dst = new Mesh();
            dst.name = src.name + "(copy)";
#if UNITY_2017_3_OR_NEWER
            dst.indexFormat = src.indexFormat;
#endif

            dst.vertices = src.vertices;
            dst.normals = src.normals;
            dst.tangents = src.tangents;
            dst.colors = src.colors;
            dst.uv = src.uv;
            dst.uv2 = src.uv2;
            dst.uv3 = src.uv3;
            dst.uv4 = src.uv4;
            dst.boneWeights = src.boneWeights;
            dst.bindposes = src.bindposes;

            dst.subMeshCount = src.subMeshCount;
            for (int i = 0; i < dst.subMeshCount; ++i)
            {
                dst.SetIndices(src.GetIndices(i), src.GetTopology(i), i);
            }

            dst.RecalculateBounds();

            if (copyBlendShape)
            {
                var vertices = src.vertices;
                var normals = src.normals;
                var tangents = src.tangents.Select(x => (Vector3)x).ToArray();

                for (int i = 0; i < src.blendShapeCount; ++i)
                {
                    src.GetBlendShapeFrameVertices(i, 0, vertices, normals, tangents);
                    dst.AddBlendShapeFrame(
                        src.GetBlendShapeName(i),
                        src.GetBlendShapeFrameWeight(i, 0),
                        vertices,
                        normals,
                        tangents
                    );
                }
            }

            return dst;
        }

        public static IEnumerable<Transform> GetChildren(this Transform parent)
        {
            foreach (Transform child in parent)
            {
                yield return child;
            }
        }

        public static IEnumerable<Transform> Traverse(this Transform parent)
        {
            yield return parent;

            foreach (Transform child in parent)
            {
                foreach (Transform descendant in Traverse(child))
                {
                    yield return descendant;
                }
            }
        }
    }
}
