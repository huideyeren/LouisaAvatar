using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using XWear.IO.XResource.Common;

namespace XWear.IO.XResource.Mesh
{
    public class MeshInfoCommon
    {
        public const int FileVersion = 0;

        public struct Header
        {
            public int Version;
            public string Name;
            public int VertexCount;
        }
    }

    public class MeshInfoWriter : IDisposable
    {
        private readonly MemoryStream _ms;
        private readonly BinaryWriter _bw;

        public MeshInfoWriter()
        {
            _ms = new MemoryStream();
            _bw = new BinaryWriter(_ms);
        }

        public void Write(SkinnedMeshRenderer skinnedMeshRenderer, bool setZeroBlendShape)
        {
            var mesh = skinnedMeshRenderer.sharedMesh;
            WriteHeader(mesh);
            WriteVertices(mesh);
            WriteNormals(mesh);
            WriteTangents(mesh);
            WriteColors(mesh);
            WriteUvs(mesh);
            WriteBoneWeights(mesh);
            WriteBindPoses(mesh);
            WriteSubMeshes(mesh);
            WriteBlendShapes(mesh);
            WriteBlendShapeWeights(skinnedMeshRenderer, setZeroBlendShape);
        }

        public void Write(UnityEngine.Mesh mesh)
        {
            WriteHeader(mesh);
            WriteVertices(mesh);
            WriteNormals(mesh);
            WriteTangents(mesh);
            WriteColors(mesh);
            WriteUvs(mesh);
            WriteBoneWeights(mesh);
            WriteBindPoses(mesh);
            WriteSubMeshes(mesh);
            WriteBlendShapes(mesh);
            WriteBlendShapeWeights(mesh);
        }

        public byte[] GetCurrent()
        {
            return _ms.ToArray();
        }

        private void WriteHeader(UnityEngine.Mesh mesh)
        {
            _bw.Write(MeshInfoCommon.FileVersion);
            _bw.Write(mesh.name);
            _bw.Write(mesh.vertexCount);
        }

        private void WriteVertices(UnityEngine.Mesh mesh)
        {
            _bw.Write(mesh.vertices.Length);
            foreach (var vertex in mesh.vertices)
            {
                _bw.Write(vertex);
            }
        }

        private void WriteNormals(UnityEngine.Mesh mesh)
        {
            _bw.Write(mesh.normals.Length);
            foreach (var normal in mesh.normals)
            {
                _bw.Write(normal);
            }
        }

        private void WriteTangents(UnityEngine.Mesh mesh)
        {
            _bw.Write(mesh.tangents.Length);
            foreach (var tangent in mesh.tangents)
            {
                _bw.Write(tangent);
            }
        }

        private void WriteColors(UnityEngine.Mesh mesh)
        {
            _bw.Write(mesh.colors.Length);
            foreach (var color in mesh.colors)
            {
                _bw.Write(color);
            }
        }

        private void WriteUvs(UnityEngine.Mesh mesh)
        {
            _bw.Write(mesh.uv.Length);
            foreach (var uv in mesh.uv)
            {
                _bw.Write(uv);
            }

            _bw.Write(mesh.uv2.Length);
            foreach (var uv in mesh.uv2)
            {
                _bw.Write(uv);
            }

            _bw.Write(mesh.uv3.Length);
            foreach (var uv in mesh.uv3)
            {
                _bw.Write(uv);
            }

            _bw.Write(mesh.uv4.Length);
            foreach (var uv in mesh.uv4)
            {
                _bw.Write(uv);
            }
        }

        private void WriteBoneWeights(UnityEngine.Mesh mesh)
        {
            _bw.Write(mesh.boneWeights.Length);
            foreach (var boneWeight in mesh.boneWeights)
            {
                _bw.Write(boneWeight.weight0);
                _bw.Write(boneWeight.weight1);
                _bw.Write(boneWeight.weight2);
                _bw.Write(boneWeight.weight3);

                _bw.Write(boneWeight.boneIndex0);
                _bw.Write(boneWeight.boneIndex1);
                _bw.Write(boneWeight.boneIndex2);
                _bw.Write(boneWeight.boneIndex3);
            }
        }

        private void WriteBindPoses(UnityEngine.Mesh mesh)
        {
            _bw.Write(mesh.bindposes.Length);
            foreach (var bindPose in mesh.bindposes)
            {
                _bw.Write(bindPose);
            }
        }

        private void WriteSubMeshes(UnityEngine.Mesh mesh)
        {
            var subMeshCount = mesh.subMeshCount;
            _bw.Write(subMeshCount);

            for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
            {
                _bw.Write((int)mesh.GetTopology(subMeshIndex));

                var indices = mesh.GetIndices(subMeshIndex);
                _bw.Write(indices.Length);

                foreach (var index in indices)
                {
                    _bw.Write(index);
                }
            }
        }

        private void WriteBlendShapes(UnityEngine.Mesh mesh)
        {
            var vertexCount = mesh.vertexCount;
            var blendShapeCount = mesh.blendShapeCount;
            _bw.Write(blendShapeCount);
            for (int blendShapeIndex = 0; blendShapeIndex < blendShapeCount; blendShapeIndex++)
            {
                _bw.Write(mesh.GetBlendShapeName(blendShapeIndex));

                var frameCount = mesh.GetBlendShapeFrameCount(blendShapeIndex);
                _bw.Write(frameCount);

                for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    var weight = mesh.GetBlendShapeFrameWeight(blendShapeIndex, frameIndex);
                    _bw.Write(weight);

                    var deltaVertices = new Vector3[vertexCount];
                    var deltaNormals = new Vector3[vertexCount];
                    var deltaTangents = new Vector3[vertexCount];
                    mesh.GetBlendShapeFrameVertices(
                        blendShapeIndex,
                        frameIndex,
                        deltaVertices,
                        deltaNormals,
                        deltaTangents
                    );

                    for (var index = 0; index < vertexCount; index++)
                    {
                        var deltaVertex = deltaVertices[index];
                        var deltaNormal = deltaNormals[index];
                        var deltaTangent = deltaTangents[index];
                        _bw.Write(deltaVertex);
                        _bw.Write(deltaNormal);
                        _bw.Write(deltaTangent);
                    }
                }
            }
        }

        private void WriteBlendShapeWeights(
            SkinnedMeshRenderer skinnedMeshRenderer,
            bool setZeroBlendShape
        )
        {
            var blendShapeCount = skinnedMeshRenderer.sharedMesh.blendShapeCount;
            _bw.Write(blendShapeCount);
            for (int blendShapeIndex = 0; blendShapeIndex < blendShapeCount; blendShapeIndex++)
            {
                _bw.Write(blendShapeIndex);
                var weight = 0.0f;
                if (!setZeroBlendShape)
                {
                    weight = skinnedMeshRenderer.GetBlendShapeWeight(blendShapeIndex);
                }

                _bw.Write(weight);
            }
        }

        private void WriteBlendShapeWeights(UnityEngine.Mesh mesh)
        {
            var blendShapeCount = mesh.blendShapeCount;
            _bw.Write(blendShapeCount);

            for (int blendShapeIndex = 0; blendShapeIndex < blendShapeCount; blendShapeIndex++)
            {
                _bw.Write(blendShapeIndex);
                var weight = 0.0f;
                _bw.Write(weight);
            }
        }

        public void Dispose()
        {
            _ms?.Dispose();
            _bw?.Dispose();
        }
    }
}
