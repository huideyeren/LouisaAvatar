using UnityEngine;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component.ComponentPlugin
{
    /// <summary>
    /// 外部パッケージに依存するComponentを収集するためのプラグイン
    /// 対応するIXResourceComponentを記述しておくことで、
    /// Componentの構築時に対象のIXResourceComponentから
    /// もともとのComponentを再現することができる
    /// </summary>
    public interface ICollectComponentPlugin
    {
        int Order { get; }

        bool CheckIsValid(UnityEngine.Component attachSource);

        /// <summary>
        /// Componentの実体が収集対象かどうかを判定する
        /// 判定処理は実装に任せる
        /// </summary>
        /// <param name="sourceComponent"></param>
        /// <returns></returns>
        bool TrySetToContext(UnityEngine.Component sourceComponent);

        void CopyComponent(
            UnityEngine.Transform attachTarget,
            UnityEngine.Component sourceComponent
        );

        /// <summary>
        /// 実際に収集し、結果をIXResourceComponentとして返す
        /// </summary>
        /// <param name="materialCollector"></param>
        /// <param name="gameObjectCollector"></param>
        /// <param name="xItem"></param>
        /// <param name="skinnedMeshRendererCollector"></param>
        /// <param name="archiver"></param>
        /// <returns></returns>
        IXResourceComponent Collect(
            XItem xItem,
            MaterialCollectorBase materialCollector,
            GameObjectWithTransformCollector gameObjectCollector,
            SkinnedMeshRendererDataCollector skinnedMeshRendererCollector,
            XResourceContainerUtil.XResourceArchiver archiver
        );
    }
}
