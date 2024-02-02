using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component
{
    /// <summary>
    /// IXResourceComponentからコンポーネントの実体を構築する
    /// </summary>
    public class ComponentBuilder
    {
        private readonly GameObjectWithTransformBuilder _gameObjectWithTransformBuilder;
        private readonly MaterialBuilderBase _materialBuilder;
        private readonly IBuildComponentPlugin[] _buildComponentPlugins;
        private readonly SkinnedMeshRendererDataBuilder.MeshOpenAction _meshOpenAction;
        private readonly XResourceContainerUtil.XResourceOpener _opener;
        private readonly AssetSaver _assetSaver;

        public ComponentBuilder(
            GameObjectWithTransformBuilder gameObjectWithTransformBuilder,
            MaterialBuilderBase materialBuilder,
            IBuildComponentPlugin[] buildComponentPlugins,
            SkinnedMeshRendererDataBuilder.MeshOpenAction meshOpenAction,
            XResourceContainerUtil.XResourceOpener opener,
            AssetSaver assetSaver
        )
        {
            _gameObjectWithTransformBuilder = gameObjectWithTransformBuilder;
            _materialBuilder = materialBuilder;
            _buildComponentPlugins = buildComponentPlugins;
            _meshOpenAction = meshOpenAction;
            _opener = opener;
            _assetSaver = assetSaver;
        }

        public void Build(out List<UnityEngine.Component> builtComponents)
        {
            builtComponents = new List<UnityEngine.Component>();
            var skinnedMeshRendererBuilder = new SkinnedMeshRendererDataBuilder(
                _gameObjectWithTransformBuilder,
                _materialBuilder,
                _meshOpenAction,
                _opener
            );

            // GameObjectWithTransformBuilderが構築した結果を使う
            // 構築の元になったXResourceGameObjectからアタッチされていたComponentを取得し、
            // 構築されたGameObjectのインスタンスに対してComponentのアタッチを実行する
            var gameObjectBuildResults = _gameObjectWithTransformBuilder.BuildResults;

            // 先にSkinnedMeshRendererを構築
            foreach (var gameObjectBuildResult in gameObjectBuildResults)
            {
                var sourceXResourceGameObject = gameObjectBuildResult.SourceXResourceGameObject;
                var instance = gameObjectBuildResult.Instance;

                var sourceComponents = sourceXResourceGameObject.Components;
                var smrSources = sourceComponents.OfType<XResourceSkinnedMeshRenderer>().ToArray();
                foreach (var smrSource in smrSources)
                {
                    var resultSmr = skinnedMeshRendererBuilder.Build(smrSource, instance);
                    builtComponents.Add(resultSmr);
                }
            }

            var buildComponentCaches =
                new Dictionary<
                    IBuildComponentPlugin,
                    List<(IXResourceComponent sourceComponent, GameObject instance)>
                >();

            foreach (var gameObjectBuildResult in gameObjectBuildResults)
            {
                var sourceXResourceGameObject = gameObjectBuildResult.SourceXResourceGameObject;
                var instance = gameObjectBuildResult.Instance;
                var sourceComponents = sourceXResourceGameObject.Components;
                foreach (var sourceComponent in sourceComponents)
                {
                    foreach (var buildComponentPlugin in _buildComponentPlugins)
                    {
                        if (
                            !buildComponentCaches.TryGetValue(
                                buildComponentPlugin,
                                out var componentData
                            )
                        )
                        {
                            componentData = new List<(IXResourceComponent, GameObject)>();
                            buildComponentCaches.Add(buildComponentPlugin, componentData);
                        }

                        // 揺れもののTransformなど、コンポーネント間で依存関係のある場合があるため、
                        // Pluginの対象コンポーネントを先に集めてからコピーの実施をおこなう
                        if (buildComponentPlugin.CheckIsValid(sourceComponent))
                        {
                            componentData.Add((sourceComponent, instance));
                        }
                    }
                }
            }

            var orderedCachedPlugin = buildComponentCaches.OrderBy(x => x.Key.Order);
            foreach (var kvp in orderedCachedPlugin)
            {
                var plugin = kvp.Key;
                var componentDataList = kvp.Value;
                foreach (var componentData in componentDataList)
                {
                    var instance = componentData.instance;
                    var sourceComponent = componentData.sourceComponent;
                    if (plugin.TrySetToContext(sourceComponent))
                    {
                        var attachedComponent = plugin.BuildAndAttach(
                            _gameObjectWithTransformBuilder,
                            skinnedMeshRendererBuilder,
                            _materialBuilder,
                            instance,
                            _opener,
                            _assetSaver
                        );

                        builtComponents.Add(attachedComponent);
                    }
                }
            }
        }
    }
}
