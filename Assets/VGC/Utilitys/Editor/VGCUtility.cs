using Gatosyocora.VRCAvatars3Tools.Utilitys;
using UnityEditor;
using UnityEditor.Animations;

// ver 0.0.1
// Copyright (c) 2021 TKSP


// 名前空間の定義
namespace VGC
{
    // TODO: いずれはAnimatorControllerUtilityと同等のものを自作したい。
    public static class VGCUtility
    {
        /// <summary>
        /// アニメーションコントローラーを結合する
        /// </summary>
        /// <param name="srcController">追加するコントローラー</param>
        /// <param name="dstController">対象</param>
        public static void MargeAnimatorController(AnimatorController srcController, AnimatorController dstController)
        {
            var dstControllerPath = AssetDatabase.GetAssetPath(dstController);

            // 先にパラメータを追加
            foreach (var parameter in srcController.parameters)
            {
                AnimatorControllerUtility.AddParameter(dstController, parameter);
            }

            EditorUtility.SetDirty(dstController);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            for (int i = 0; i < srcController.layers.Length; i++)
            {
                bool contain = false;
                for (int j = 0; j < dstController.layers.Length; j++)
                {
                    if (srcController.layers[i].name == dstController.layers[j].name)
                        contain = true;
                }

                // 同名が無ければ追加
                if (!contain)
                {
                    AnimatorControllerUtility.AddLayer(dstController, srcController.layers[i], i == 0, dstControllerPath);
                }

            }

            EditorUtility.SetDirty(dstController);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
