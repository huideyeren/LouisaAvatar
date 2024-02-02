using System;
using UnityEngine;
using XWear.IO.XResource.Mesh;

namespace XWear.IO.XResource.Component
{
    /// <summary>
    /// XResourceに保存されるSkinnedMeshRenderer
    /// </summary>
    public class XResourceSkinnedMeshRenderer : IXResourceComponent
    {
        public class SkinnedBone
        {
            public int Index;
            public string BoneGuid;
        }

        public ComponentType ComponentType => ComponentType.SkinnedMeshRenderer;

        public string Guid = System.Guid.NewGuid().ToString();
        public XResourceMesh Mesh = new XResourceMesh();
        public string RootBoneGuid = "";
        public SkinnedBone[] Bones = Array.Empty<SkinnedBone>();
        public string[] RefMaterialGuids = Array.Empty<string>();

        public UnityEngine.Component AttachTo(GameObject attachTarget)
        {
            // GameObjectへのアタッチ時に、保存されたXResourceMeshからMeshを再現する
            var xResourceMesh = Mesh;
            var newMesh = new UnityEngine.Mesh()
            {
                name = xResourceMesh.Name,
                indexFormat = xResourceMesh.IndexFormat,
            };

            var smr = attachTarget.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = newMesh;
            return smr;
        }
    }
}
