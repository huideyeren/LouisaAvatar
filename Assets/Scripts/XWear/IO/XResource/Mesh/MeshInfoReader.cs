using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XWear.IO.XResource.Common;

namespace XWear.IO.XResource.Mesh
{
    public class MeshInfoReader : IDisposable
    {
        private readonly MemoryStream _ms;
        private readonly BinaryReader _br;

        public MeshInfoReader(byte[] meshData)
        {
            _ms = new MemoryStream(meshData);
            _br = new BinaryReader(_ms);
        }

        public void ReadTo(SkinnedMeshRenderer destSkinnedMeshRenderer)
        {
            var destMesh = destSkinnedMeshRenderer.sharedMesh;
            ReadTo(destMesh, out var currentBlendShapeWeights);

            if (currentBlendShapeWeights != null)
            {
                foreach (var currentBlendShapeWeight in currentBlendShapeWeights)
                {
                    var index = currentBlendShapeWeight.blendShapeIndex;
                    var weight = currentBlendShapeWeight.value;
                    destSkinnedMeshRenderer.SetBlendShapeWeight(index, weight);
                }
            }

            destMesh.RecalculateBounds();
        }

        public void ReadTo(
            UnityEngine.Mesh destMesh,
            out (int blendShapeIndex, float value)[] currentBlendShapeWeights
        )
        {
            var header = ReadHeader();
            var vertices = ReadVertices();
            var normals = ReadNormals();
            var tangents = ReadTangents();
            var colors = ReadColors();
            var uvs = ReadUvs();
            var uv1 = uvs[0];
            var uv2 = uvs[1];
            var uv3 = uvs[2];
            var uv4 = uvs[3];
            var boneWeights = ReadBoneWeights();
            var bindPoses = ReadBindPoses();
            var subMeshes = ReadSubMeshIndices();
            var blendShapes = ReadBlendShapes(header.VertexCount);
            currentBlendShapeWeights = ReadCurrentBlendShapeWeights();

            destMesh.vertices = vertices;
            destMesh.normals = normals;
            destMesh.tangents = tangents;
            destMesh.colors = colors;
            destMesh.SetVertices(vertices);
            destMesh.SetNormals(normals);
            destMesh.SetTangents(tangents);

            destMesh.uv = uv1;
            destMesh.uv2 = uv2;
            destMesh.uv3 = uv3;
            destMesh.uv4 = uv4;

            destMesh.boneWeights = boneWeights;
            destMesh.bindposes = bindPoses;

            destMesh.subMeshCount = subMeshes.Length;
            for (var subMeshIndex = 0; subMeshIndex < subMeshes.Length; subMeshIndex++)
            {
                var subMesh = subMeshes[subMeshIndex];
                destMesh.SetIndices(subMesh.Indices, subMesh.Topology, subMeshIndex);
            }

            destMesh.RecalculateBounds();

            foreach (var blendShape in blendShapes)
            {
                foreach (var frame in blendShape.BlendShapeFrames)
                {
                    destMesh.AddBlendShapeFrame(
                        shapeName: blendShape.BlendShapeName,
                        frameWeight: frame.Weight,
                        deltaVertices: frame.Vertices,
                        deltaNormals: frame.Normals,
                        deltaTangents: frame.Tangents
                    );
                }
            }

            destMesh.name = header.Name;
        }

        private MeshInfoCommon.Header ReadHeader()
        {
            var version = _br.ReadInt32();
            var meshName = _br.ReadString();

            var vertexCount = _br.ReadInt32();
            return new MeshInfoCommon.Header()
            {
                Version = version,
                Name = meshName,
                VertexCount = vertexCount,
            };
        }

        private Vector3[] ReadVertices()
        {
            var count = _br.ReadInt32();
            var result = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = _br.ReadVector3();
            }

            return result;
        }

        private Vector3[] ReadNormals()
        {
            var count = _br.ReadInt32();
            var result = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = _br.ReadVector3();
            }

            return result;
        }

        private Vector4[] ReadTangents()
        {
            var count = _br.ReadInt32();
            var result = new Vector4[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = _br.ReadVector4();
            }

            return result;
        }

        private Color[] ReadColors()
        {
            var count = _br.ReadInt32();
            var result = new Color[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = _br.ReadVector4();
            }

            return result;
        }

        private Vector2[][] ReadUvs()
        {
            var result = new Vector2[4][];

            var uv1Length = _br.ReadInt32();
            result[0] = new Vector2[uv1Length];
            for (int i = 0; i < uv1Length; i++)
            {
                result[0][i] = _br.ReadVector2();
            }

            var uv2Length = _br.ReadInt32();
            result[1] = new Vector2[uv2Length];
            for (int i = 0; i < uv2Length; i++)
            {
                result[1][i] = _br.ReadVector2();
            }

            var uv3Length = _br.ReadInt32();
            result[2] = new Vector2[uv3Length];
            for (int i = 0; i < uv3Length; i++)
            {
                result[2][i] = _br.ReadVector2();
            }

            var uv4Length = _br.ReadInt32();
            result[3] = new Vector2[uv4Length];
            for (int i = 0; i < uv4Length; i++)
            {
                result[3][i] = _br.ReadVector2();
            }

            return result;
        }

        private BoneWeight[] ReadBoneWeights()
        {
            var count = _br.ReadInt32();
            var result = new BoneWeight[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = new BoneWeight()
                {
                    weight0 = _br.ReadSingle(),
                    weight1 = _br.ReadSingle(),
                    weight2 = _br.ReadSingle(),
                    weight3 = _br.ReadSingle(),
                    boneIndex0 = _br.ReadInt32(),
                    boneIndex1 = _br.ReadInt32(),
                    boneIndex2 = _br.ReadInt32(),
                    boneIndex3 = _br.ReadInt32(),
                };
            }

            return result;
        }

        private Matrix4x4[] ReadBindPoses()
        {
            var count = _br.ReadInt32();
            var result = new Matrix4x4[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = _br.ReadMatrix4X4();
            }

            return result;
        }

        private XResourceMesh.XResourceSubMesh[] ReadSubMeshIndices()
        {
            var subMeshCount = _br.ReadInt32();
            var result = new XResourceMesh.XResourceSubMesh[subMeshCount];
            for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
            {
                var topology = (MeshTopology)_br.ReadInt32();

                var indicesCount = _br.ReadInt32();
                var indices = new int[indicesCount];

                for (int indicesIndex = 0; indicesIndex < indices.Length; indicesIndex++)
                {
                    indices[indicesIndex] = _br.ReadInt32();
                }

                result[subMeshIndex] = new XResourceMesh.XResourceSubMesh()
                {
                    Topology = topology,
                    Indices = indices
                };
            }

            return result;
        }

        private XResourceMesh.XResourceBlendShape[] ReadBlendShapes(int vertexCount)
        {
            var blendShapeCount = _br.ReadInt32();
            var result = new XResourceMesh.XResourceBlendShape[blendShapeCount];
            for (int blendShapeIndex = 0; blendShapeIndex < blendShapeCount; blendShapeIndex++)
            {
                var blendShapeName = _br.ReadString();
                var frameCount = _br.ReadInt32();

                var frames = new List<XResourceMesh.BlendShapeFrame>();

                for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    var weight = _br.ReadSingle();

                    var deltaVertices = new Vector3[vertexCount];
                    var deltaNormals = new Vector3[vertexCount];
                    var deltaTangents = new Vector3[vertexCount];

                    for (var vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
                    {
                        deltaVertices[vertexIndex] = _br.ReadVector3();
                        deltaNormals[vertexIndex] = _br.ReadVector3();
                        deltaTangents[vertexIndex] = _br.ReadVector3();
                    }

                    frames.Add(
                        new XResourceMesh.BlendShapeFrame()
                        {
                            Weight = weight,
                            Vertices = deltaVertices,
                            Normals = deltaNormals,
                            Tangents = deltaTangents
                        }
                    );
                }

                result[blendShapeIndex] = new XResourceMesh.XResourceBlendShape()
                {
                    BlendShapeName = blendShapeName,
                    BlendShapeFrames = frames
                };
            }

            return result;
        }

        private (int blendShapeIndex, float value)[] ReadCurrentBlendShapeWeights()
        {
            if (_br.BaseStream.Position == _br.BaseStream.Length)
            {
                return null;
            }

            var blendShapeCount = _br.ReadInt32();
            var result = new (int, float)[blendShapeCount];
            for (int blendShapeIndex = 0; blendShapeIndex < blendShapeCount; blendShapeIndex++)
            {
                var index = _br.ReadInt32();
                var weight = _br.ReadSingle();
                result[blendShapeIndex] = (index, weight);
            }

            return result;
        }

        private List<bool> ReadDeletedVertexInfo(int vertexCount)
        {
            if (_br.BaseStream.Position == _br.BaseStream.Length)
            {
                return null;
            }

            var result = new List<bool>();
            for (int i = 0; i < vertexCount; i++)
            {
                result.Add(_br.ReadBoolean());
            }

            return result;
        }

        public void Dispose()
        {
            _ms.Dispose();
            _br.Dispose();
        }
    }
}
