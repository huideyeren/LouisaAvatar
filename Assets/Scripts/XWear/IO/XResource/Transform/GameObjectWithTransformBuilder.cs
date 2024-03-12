using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace XWear.IO.XResource.Transform
{
    public class GameObjectWithTransformBuilder
    {
        public class XResourceGameObjectBuildResult
        {
            public readonly XResourceGameObject SourceXResourceGameObject;
            public readonly GameObject Instance;

            public XResourceGameObjectBuildResult(
                XResourceGameObject sourceXResourceGameObject,
                GameObject instance
            )
            {
                SourceXResourceGameObject = sourceXResourceGameObject;
                Instance = instance;
            }
        }

        // guidから構築したTransformの実体を引くためのメモ
        private readonly Dictionary<string, UnityEngine.Transform> _guidToBuildTransformMemo =
            new Dictionary<string, UnityEngine.Transform>();

        // 構築したXResourceGameObjectとGameObjectの実体を保存する
        public readonly List<XResourceGameObjectBuildResult> BuildResults =
            new List<XResourceGameObjectBuildResult>();

        public readonly Dictionary<
            int,
            UnityEngine.Transform
        > XResourceTransformIndexToBuiltTransformEntityMap =
            new Dictionary<int, UnityEngine.Transform>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="sourceXResourceGameObject"></param>
        /// <param name="parent"></param>
        public XResourceGameObjectBuildResult BuildXResourceGameObjects(
            XResourceGameObject sourceXResourceGameObject,
            UnityEngine.Transform parent = null
        )
        {
            // XResourceGameObjectに対応するGameObjectの実体を生成する
            var instance = new GameObject(sourceXResourceGameObject.Name);
            instance.SetActive(sourceXResourceGameObject.ActiveSelf);
            instance.tag = sourceXResourceGameObject.Tag;

            _guidToBuildTransformMemo.Add(sourceXResourceGameObject.Guid, instance.transform);

            var result = new XResourceGameObjectBuildResult(sourceXResourceGameObject, instance);

            XResourceTransformIndexToBuiltTransformEntityMap.Add(
                sourceXResourceGameObject.Transform.Index,
                instance.transform
            );

            BuildResults.Add(result);

            // Transform値を設定
            var sourceXResourceTransform = sourceXResourceGameObject.Transform;
            SetTransform(instance, sourceXResourceTransform, parent);

            // 子に対しても再帰的に実施
            for (int i = 0; i < sourceXResourceGameObject.Children.Count; i++)
            {
                BuildXResourceGameObjects(
                    sourceXResourceGameObject.Children[i],
                    instance.transform
                );
            }

            return result;
        }

        /// <summary>
        /// XResourceTransformを元にしてTransformの実体の値を設定する
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="xResourceTransform"></param>
        /// <param name="parent"></param>
        private void SetTransform(
            GameObject instance,
            XResourceTransform xResourceTransform,
            UnityEngine.Transform parent
        )
        {
            // world
            instance.transform.SetPositionAndRotation(
                xResourceTransform.Position,
                xResourceTransform.Rotation
            );
            if (parent != null)
            {
                instance.transform.SetParent(parent.transform, true);
            }

            // local
            instance.transform.localScale = xResourceTransform.LocalScale;
        }

        /// <summary>
        /// guidから構築したTransformの実体を引く
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public UnityEngine.Transform GetBuildTransformFromGuid(string guid)
        {
            if (_guidToBuildTransformMemo.TryGetValue(guid, out var transform))
            {
                return transform;
            }

            throw new DirectoryNotFoundException($"{guid} is not found in build TransformMemo");
        }
    }
}
