using System.Collections.Generic;
using UnityEngine;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component;

namespace XWear.IO.Editor.Mesh
{
    public static class EditorMeshArchiver
    {
        public static void Archive(
            XResourceContainerUtil.XResourceArchiver archiver,
            SkinnedMeshRenderer builtSmr,
            Transform sourceTransform,
            XResourceSkinnedMeshRenderer result
        )
        {
            // 現在のメッシュの状態を書き込む
            archiver.AddMesh(result, builtSmr, isDefault: false);

            // Editor拡張からのエクスポートの場合、デフォルトとしても書き込みをおこなう
            if (archiver.ArchiveFromPackage)
            {
                archiver.AddMesh(result, builtSmr, isDefault: true);
            }

            // メッシュが何も削除されていない状態(AddVertexActiveMarksが全てtrue)として保存しておく
            var activeMarks = new List<bool>();
            for (int i = 0; i < builtSmr.sharedMesh.vertexCount; i++)
            {
                activeMarks.Add(true);
            }

            archiver.AddVertexActiveMarks(result, activeMarks);
        }
    }
}
