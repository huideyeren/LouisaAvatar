using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component
{
    /// <summary>
    /// GameObjectの実体に当たっているComponentを収集し、XResourceGameObjectに保存する
    /// </summary>
    public class ComponentCollector
    {
        private readonly XItem _xItem;
        private readonly XSourceBase _xSource;
        private readonly GameObjectWithTransformCollector _gameObjectCollector;
        private readonly MaterialCollectorBase _materialCollector;
        private readonly ICollectComponentPlugin[] _componentPlugins;
        private readonly XResourceContainerUtil.XResourceArchiver _archiver;
        private readonly SkinnedMeshRendererDataCollector.MeshArchiveAction _meshArchiveAction;

        public ComponentCollector(
            XItem xItem,
            XSourceBase xSource,
            GameObjectWithTransformCollector gameObjectCollector,
            MaterialCollectorBase materialCollector,
            ICollectComponentPlugin[] componentPlugins,
            SkinnedMeshRendererDataCollector.MeshArchiveAction meshArchiveAction,
            XResourceContainerUtil.XResourceArchiver archiver
        )
        {
            _xItem = xItem;
            _xSource = xSource;
            _gameObjectCollector = gameObjectCollector;
            _materialCollector = materialCollector;
            _componentPlugins = componentPlugins;
            _meshArchiveAction = meshArchiveAction;
            _archiver = archiver;
        }

        public void CollectComponentsData()
        {
            var skinnedMeshRendererCollector = new SkinnedMeshRendererDataCollector(
                _gameObjectCollector,
                _materialCollector,
                _meshArchiveAction
            );

            // GameObjectWithTransformCollectorが収集した結果を使う
            // 収集のもとになったGameObjectにアタッチされているComponentを取得し、
            // 必要なIXResourceComponentを保存する
            var guidToXResourceGameObjectMemo = _gameObjectCollector.GuidToXResourceGameObjectMemo;
            var sb = new StringBuilder();
            sb.AppendLine("CollectTargetGameObjects");
            foreach (var kvp in guidToXResourceGameObjectMemo)
            {
                sb.AppendLine($"{kvp.Value.Name}");
            }

            Debug.Log(sb.ToString());

            // SkinnedMeshRendererの収集とデータ化を先に実施する
            // SkinnedMeshRendererがほかのコンポーネントから参照されるケースが存在するため
            var sourceSmrComponentToSmrDataMap =
                new Dictionary<SkinnedMeshRenderer, IXResourceComponent>();
            foreach (var collectSource in _xSource.SourceSmrs)
            {
                var smr = collectSource.DestSmr;

                var smrComponentData = skinnedMeshRendererCollector.Collect(
                    collectSource,
                    _archiver
                );

                sourceSmrComponentToSmrDataMap.Add(smr, smrComponentData);
            }

            // さらに各XResourceGameObjectにアタッチされているコンポーネントを見に行き、
            // SkinnedMeshRendererがアタッチされているものについては、先にデータ化したIResourceComponentを追加しておく
            foreach (var kvp in guidToXResourceGameObjectMemo)
            {
                var xResourceGameObjectId = kvp.Key;
                var xResourceGameObject = kvp.Value;

                var components = _gameObjectCollector.GetAttachedComponents(xResourceGameObjectId);
                var skinnedMeshRenderers = components.OfType<SkinnedMeshRenderer>().ToArray();
                foreach (var smr in skinnedMeshRenderers)
                {
                    if (sourceSmrComponentToSmrDataMap.TryGetValue(smr, out var componentData))
                    {
                        xResourceGameObject.Components.Add(componentData);
                    }
                }
            }

            // SkinnedMeshRenderer以外のコンポーネントを収集する
            foreach (var kvp in guidToXResourceGameObjectMemo)
            {
                var xResourceGameObjectId = kvp.Key;
                var xResourceGameObject = kvp.Value;

                var components = _gameObjectCollector.GetAttachedComponents(xResourceGameObjectId);
                var result = new List<IXResourceComponent>();

                foreach (var component in components)
                {
                    foreach (var componentPlugin in _componentPlugins)
                    {
                        if (componentPlugin.TrySetToContext(component))
                        {
                            var xResourceComponent = componentPlugin.Collect(
                                xItem: _xItem,
                                materialCollector: _materialCollector,
                                _gameObjectCollector,
                                skinnedMeshRendererCollector,
                                _archiver
                            );
                            if (xResourceComponent != null)
                            {
                                result.Add(xResourceComponent);
                            }
                        }
                    }
                }

                xResourceGameObject.Components.AddRange(result);
            }
        }
    }
}
