using UnityEngine;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component;

namespace XWear.IO.Editor.Mesh
{
    public static class EditorMeshOpener
    {
        public static void Open(
            XResourceContainerUtil.XResourceOpener opener,
            SkinnedMeshRenderer destSmr,
            XResourceSkinnedMeshRenderer xResource
        )
        {
            // Editor拡張では非Default(=編集済みメッシュ)を読み込む
            opener.GetAndSetCurrentMeshInfo(
                destSkinnedMeshRenderer: destSmr,
                xResourceSkinnedMeshRenderer: xResource,
                loadDefault: false
            );
        }
    }
}
