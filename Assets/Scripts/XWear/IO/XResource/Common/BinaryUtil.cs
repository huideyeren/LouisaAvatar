using System.IO;
using UnityEngine;

namespace XWear.IO.XResource.Common
{
    public static class BinaryUtil
    {
        public static void Write(this BinaryWriter bw, Vector2 vector2)
        {
            bw.Write(vector2.x);
            bw.Write(vector2.y);
        }

        public static void Write(this BinaryWriter bw, Vector3 vector3)
        {
            bw.Write(vector3.x);
            bw.Write(vector3.y);
            bw.Write(vector3.z);
        }

        public static void Write(this BinaryWriter bw, Vector4 vector4)
        {
            bw.Write(vector4.x);
            bw.Write(vector4.y);
            bw.Write(vector4.z);
            bw.Write(vector4.w);
        }

        public static void Write(this BinaryWriter bw, Quaternion quaternion)
        {
            bw.Write(quaternion.x);
            bw.Write(quaternion.y);
            bw.Write(quaternion.z);
            bw.Write(quaternion.w);
        }

        public static void Write(this BinaryWriter bw, Matrix4x4 matrix4X4)
        {
            bw.Write(matrix4X4.m00);
            bw.Write(matrix4X4.m10);
            bw.Write(matrix4X4.m20);
            bw.Write(matrix4X4.m30);

            bw.Write(matrix4X4.m01);
            bw.Write(matrix4X4.m11);
            bw.Write(matrix4X4.m21);
            bw.Write(matrix4X4.m31);

            bw.Write(matrix4X4.m02);
            bw.Write(matrix4X4.m12);
            bw.Write(matrix4X4.m22);
            bw.Write(matrix4X4.m32);

            bw.Write(matrix4X4.m03);
            bw.Write(matrix4X4.m13);
            bw.Write(matrix4X4.m23);
            bw.Write(matrix4X4.m33);
        }

        public static Vector2 ReadVector2(this BinaryReader br)
        {
            return new Vector2(x: br.ReadSingle(), y: br.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader br)
        {
            return new Vector3(x: br.ReadSingle(), y: br.ReadSingle(), z: br.ReadSingle());
        }

        public static Vector4 ReadVector4(this BinaryReader br)
        {
            return new Vector4(
                x: br.ReadSingle(),
                y: br.ReadSingle(),
                z: br.ReadSingle(),
                w: br.ReadSingle()
            );
        }

        public static Quaternion ReadQuaternion(this BinaryReader br)
        {
            return new Quaternion(
                x: br.ReadSingle(),
                y: br.ReadSingle(),
                z: br.ReadSingle(),
                w: br.ReadSingle()
            );
        }

        public static Matrix4x4 ReadMatrix4X4(this BinaryReader br)
        {
            return new Matrix4x4()
            {
                m00 = br.ReadSingle(),
                m10 = br.ReadSingle(),
                m20 = br.ReadSingle(),
                m30 = br.ReadSingle(),
                m01 = br.ReadSingle(),
                m11 = br.ReadSingle(),
                m21 = br.ReadSingle(),
                m31 = br.ReadSingle(),
                m02 = br.ReadSingle(),
                m12 = br.ReadSingle(),
                m22 = br.ReadSingle(),
                m32 = br.ReadSingle(),
                m03 = br.ReadSingle(),
                m13 = br.ReadSingle(),
                m23 = br.ReadSingle(),
                m33 = br.ReadSingle(),
            };
        }
    }
}
