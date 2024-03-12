using UnityEngine;

namespace XWear.IO.XResource.Mesh
{
    public static class SkinnedMeshRendererUtil
    {
        /// <summary>
        /// sourceのSkinnedMeshRendererの内容をコピーしてtargetにアタッチする
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        public static SkinnedMeshRenderer CopyAndAttachSkinnedMeshRenderer(
            this GameObject target,
            SkinnedMeshRenderer source
        )
        {
            target.tag = source.gameObject.tag;
            var attachedSmr = target.AddComponent<SkinnedMeshRenderer>();
            attachedSmr.bones = source.bones;
            attachedSmr.sharedMesh = source.sharedMesh;
            attachedSmr.sharedMaterials = source.sharedMaterials;
            attachedSmr.rootBone = source.rootBone;

            var blendShapeCount = source.sharedMesh.blendShapeCount;
            for (int blendShapeIndex = 0; blendShapeIndex < blendShapeCount; blendShapeIndex++)
            {
                var weight = source.GetBlendShapeWeight(blendShapeIndex);
                attachedSmr.SetBlendShapeWeight(blendShapeIndex, weight);
            }

            return attachedSmr;
        }
    }
}
