using System.Collections.Generic;
using UnityEngine;

namespace XWear.IO.XResource.Transform
{
    public class EditTransformBuilder
    {
        /// <summary>
        /// 構築の元になったTransformと対応するTransformのマップ
        /// SourceTransform,NewTransform
        /// </summary>
        public readonly Dictionary<
            UnityEngine.Transform,
            UnityEngine.Transform
        > SourceToEditTransformMemo =
            new Dictionary<UnityEngine.Transform, UnityEngine.Transform>();

        /// <summary>
        /// 以下の条件でSkinnedMeshRenderer用のTransformが重複して作られてしまうので、その対策としてマップを作っておく
        /// BuildTransform()が実行されたとき、sourceTransformの子に直接
        /// SkinnedMeshRendererがアタッチされていたTransformを重複して構築しないために使う
        /// </summary>
        public readonly Dictionary<
            SkinnedMeshRenderer,
            UnityEngine.Transform
        > SkinnedMeshRendererTransformMemo =
            new Dictionary<SkinnedMeshRenderer, UnityEngine.Transform>();

        /// <summary>
        /// 再帰的にBone構造を再構築する
        /// </summary>
        /// <param name="sourceTransform"></param>
        /// <param name="parent"></param>
        /// <param name="ignoreSkinnedMeshRendererTransform"></param>
        public UnityEngine.Transform BuildTransforms(
            UnityEngine.Transform sourceTransform,
            GameObject parent = null,
            bool ignoreSkinnedMeshRendererTransform = false
        )
        {
            var newTransformGameObject = new GameObject(sourceTransform.gameObject.name);
            if (!SourceToEditTransformMemo.ContainsKey(sourceTransform))
            {
                SourceToEditTransformMemo.Add(sourceTransform, newTransformGameObject.transform);
            }

            // world
            newTransformGameObject.transform.SetPositionAndRotation(
                sourceTransform.position,
                sourceTransform.rotation
            );
            if (parent != null)
            {
                newTransformGameObject.transform.SetParent(parent.transform, true);
            }

            // local
            newTransformGameObject.transform.localScale = sourceTransform.localScale;

            // sourceTransformにSkinnedMeshRendererがアタッチされている場合、
            // ビルドしたTransformの参照を保存しておく
            var attachedSkinnedMeshRenderer = sourceTransform.GetComponent<SkinnedMeshRenderer>();
            var iSkinnedMeshRendererTransform = attachedSkinnedMeshRenderer != null;
            if (iSkinnedMeshRendererTransform)
            {
                SkinnedMeshRendererTransformMemo.Add(
                    attachedSkinnedMeshRenderer,
                    newTransformGameObject.transform
                );
            }

            if (sourceTransform.childCount > 0)
            {
                for (int i = 0; i < sourceTransform.childCount; i++)
                {
                    var childTransform = sourceTransform.GetChild(i);
                    BuildTransforms(childTransform, newTransformGameObject);
                }
            }

            newTransformGameObject.SetActive(sourceTransform.gameObject.activeSelf);
            return newTransformGameObject.transform;
        }

        public void ForceBuild(
            UnityEngine.Transform sourceTransform,
            GameObject newTransformGameObject
        )
        {
            if (!SourceToEditTransformMemo.ContainsKey(sourceTransform))
            {
                SourceToEditTransformMemo.Add(sourceTransform, newTransformGameObject.transform);
            }
        }
    }
}
