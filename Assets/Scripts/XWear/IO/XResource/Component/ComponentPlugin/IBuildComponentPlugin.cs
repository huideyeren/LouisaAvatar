using UnityEngine;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component.ComponentPlugin
{
    /// <summary>
    /// 外部パッケージに依存するComponentをビルドするためのプラグイン
    /// 対応するIXResourceComponentを記述しておくことで、
    /// Componentの構築時に対象のIXResourceComponentから
    /// もともとのComponentを再現することができる
    /// </summary>
    public interface IBuildComponentPlugin
    {
        int Order { get; }

        bool CheckIsValid(IXResourceComponent sourceComponent);

        /// <summary>
        /// Componentのビルド時に対象のIXResourceComponentかどうかを判定する
        /// 対応するIXResourceComponentを実装するクラスを別途用意する必要がある
        /// </summary>
        /// <param name="sourceComponent"></param>
        /// <returns></returns>
        bool TrySetToContext(IXResourceComponent sourceComponent);

        /// <summary>
        /// 実際にComponentの中身をビルドし、結果をもともとComponentがアタッチされていた
        /// GameObjectに対してアタッチする
        /// </summary>
        /// <param name="gameObjectBuilder"></param>
        /// <param name="skinnedMeshRendererBuilder"></param>
        /// <param name="materialBuilder"></param>
        /// <param name="attachTarget"></param>
        /// <param name="opener"></param>
        /// <param name="assetSaver"></param>
        UnityEngine.Component BuildAndAttach(
            GameObjectWithTransformBuilder gameObjectBuilder,
            SkinnedMeshRendererDataBuilder skinnedMeshRendererBuilder,
            MaterialBuilderBase materialBuilder,
            GameObject attachTarget,
            XResourceContainerUtil.XResourceOpener opener,
            AssetSaver assetSaver
        );
    }
}
