using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XWear.IO.XResource.Transform
{
    /// <summary>
    /// XResourceとして保存したいGameObjectとそのTransformを収集するクラス
    /// XResourceGameObjectとXResourceTransformを保持する
    /// </summary>
    public class GameObjectWithTransformCollector
    {
        /// <summary>
        /// 収集したTransformを持つGameObjectの箱をguidから引くためのメモ
        /// </summary>
        public readonly Dictionary<string, XResourceGameObject> GuidToXResourceGameObjectMemo =
            new Dictionary<string, XResourceGameObject>();

        /// <summary>
        /// 収集前のTransformにアタッチされているComponentの実体をGameObjectのguidから引くためのメモ
        /// </summary>
        private readonly Dictionary<string, UnityEngine.Component[]> _guidToAttachedComponentsMemo =
            new Dictionary<string, UnityEngine.Component[]>();

        /// <summary>
        /// 収集後のTransformの実体からXResourceGameObjectのguidを取得するためのメモ
        /// </summary>
        private readonly Dictionary<UnityEngine.Transform, string> _collectedTransformToGuidMemo =
            new Dictionary<UnityEngine.Transform, string>();

        /// <summary>
        /// AvatarSourceのTransformからCollect対象のTransformを引くためのマップ
        /// たとえばPhysBoneやVRMSpringBoneのTransformの参照を更新したりするときに利用する
        /// </summary>
        private readonly Dictionary<
            UnityEngine.Transform,
            UnityEngine.Transform
        > _avatarSourceToEditTransformMap;

        public GameObjectWithTransformCollector(
            Dictionary<UnityEngine.Transform, UnityEngine.Transform> avatarSourceToEditTransformMap
        )
        {
            _avatarSourceToEditTransformMap = avatarSourceToEditTransformMap;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="collectTargetGameObject">収集対象のGameObject</param>
        /// <param name="isFirst"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public XResourceGameObject Collect(
            UnityEngine.GameObject collectTargetGameObject,
            bool isFirst,
            ref int index
        )
        {
            var collectTargetTransform = collectTargetGameObject.transform;
            var guid = Guid.NewGuid().ToString();

            // 再構築用のGameObjectの箱をつくる
            var parentClothItemGameObject = new XResourceGameObject()
            {
                Guid = guid,
                Name = collectTargetGameObject.name,
                Tag = collectTargetGameObject.tag,
                ActiveSelf = collectTargetGameObject.activeSelf
            };

            // Transformを保存
            var parentClothItemTransform = new XResourceTransform()
            {
                Name = collectTargetTransform.name,
                Position = collectTargetTransform.position,
                Rotation = collectTargetTransform.rotation,
                Scale = collectTargetTransform.lossyScale,
                LocalPosition = collectTargetTransform.localPosition,
                LocalRotation = collectTargetTransform.localRotation,
                LocalScale = collectTargetTransform.localScale,
                Index = index
            };

            index++;

            parentClothItemGameObject.Transform = parentClothItemTransform;

            if (!isFirst) { }

            GuidToXResourceGameObjectMemo.Add(guid, parentClothItemGameObject);

            _collectedTransformToGuidMemo.Add(collectTargetTransform, guid);

            // アタッチされているコンポーネントをメモに追加しておく
            var components = collectTargetTransform.GetComponents<UnityEngine.Component>();
            if (components != null && components.Length != 0)
            {
                _guidToAttachedComponentsMemo.Add(guid, components);
            }

            // 再帰的に収集
            if (collectTargetTransform.childCount > 0)
            {
                for (int i = 0; i < collectTargetTransform.childCount; i++)
                {
                    var child = Collect(
                        collectTargetGameObject: collectTargetTransform.GetChild(i).gameObject,
                        isFirst: false,
                        ref index
                    );
                    parentClothItemGameObject.Children.Add(child);
                }
            }

            return parentClothItemGameObject;
        }

        /// <summary>
        /// guidからXResourceGameObjectを取得する
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public XResourceGameObject GetCollectedDressGameObject(string guid)
        {
            if (GuidToXResourceGameObjectMemo.TryGetValue(guid, out var xResourceGameObject))
            {
                return xResourceGameObject;
            }

            throw new DirectoryNotFoundException(
                $"{guid} is not found in collected XResourceGameObjects"
            );
        }

        /// <summary>
        /// guidから元となったUnityEngine.GameObjectにアタッチされていたコンポーネントの実体を引く
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public UnityEngine.Component[] GetAttachedComponents(string guid)
        {
            if (_guidToAttachedComponentsMemo.TryGetValue(guid, out var components))
            {
                return components;
            }

            throw new DirectoryNotFoundException(
                $"{guid} is not found in collected AttachedComponentsMap"
            );
        }

        /// <summary>
        /// UnityEngine.Transformの実体から、収集結果のXResourceGameObjectのguidを引く
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public string GetXResourceGameObjectGuidFromTransform(UnityEngine.Transform transform)
        {
            if (_collectedTransformToGuidMemo.TryGetValue(transform, out var guid))
            {
                return guid;
            }

            throw new DirectoryNotFoundException(
                $"{transform.name} is not found in collected TransformMemo"
            );
        }

        /// <summary>
        /// XResourceGameObjectがHumanoidBoneであることをマークする
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="humanBodyBones"></param>
        public void MarkIsHumanoidTransform(
            UnityEngine.Transform transform,
            UnityEngine.HumanBodyBones humanBodyBones
        )
        {
            var guid = GetXResourceGameObjectGuidFromTransform(transform);
            GuidToXResourceGameObjectMemo[guid].IsHumanoidBone = true;
            GuidToXResourceGameObjectMemo[guid].HumanBodyBones = humanBodyBones;
        }

        /// <summary>
        /// AvatarSourceのTransformから収集結果のXResourceGameObjectのguidを取得する
        /// </summary>
        /// <param name="avatarSourceTransform"></param>
        /// <returns></returns>
        public string GetXResourceGameObjectGuidFromAvatarSourceTransform(
            UnityEngine.Transform avatarSourceTransform
        )
        {
            var collectedTransform = GetCollectedTransformFromAvatarSourceTransform(
                avatarSourceTransform
            );
            return GetXResourceGameObjectGuidFromTransform(collectedTransform);
        }

        /// <summary>
        /// AvatarSourceのTransformから収集結果のTransformの実体を引く
        /// </summary>
        /// <param name="avatarSourceTransform"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public UnityEngine.Transform GetCollectedTransformFromAvatarSourceTransform(
            UnityEngine.Transform avatarSourceTransform
        )
        {
            if (
                _avatarSourceToEditTransformMap.TryGetValue(
                    avatarSourceTransform,
                    out var editTransform
                )
            )
            {
                return editTransform;
            }

            throw new DirectoryNotFoundException(
                $"{avatarSourceTransform.name} not found in editTransformBuilderResult"
            );
        }

        public List<XResourceGameObject> GetCollectedXResourceGameObjects()
        {
            return GuidToXResourceGameObjectMemo.Select(x => x.Value).ToList();
        }
    }
}
