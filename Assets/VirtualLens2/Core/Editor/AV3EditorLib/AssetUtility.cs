using System;
using System.Linq;
using UnityEditor;

namespace VirtualLens2.AV3EditorLib
{
    /// <summary>
    /// Utility functions for asset manipulation.
    /// </summary>
    public static class AssetUtility
    {
        /// <summary>
        /// Loads an asset by GUID.
        /// </summary>
        /// <param name="guid">GUID of the target asset.</param>
        /// <typeparam name="T">Expected type of the target asset.</typeparam>
        /// <returns>Loaded asset object.</returns>
        // Resharper disable once InconsistentNaming
        public static T LoadAssetByGUID<T>(string guid) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }

        /// <summary>
        /// Removes all sub-assets satisfying a condition from an asset.
        /// </summary>
        /// <param name="asset">The asset to be modified.</param>
        /// <param name="pred">The function to test each sub-asset for a condition.</param>
        /// <exception cref="ArgumentNullException"><c>controller</c> or <c>pred</c> is <c>null</c>.</exception>
        public static void RemoveSubAssets(UnityEngine.Object asset, Predicate<UnityEngine.Object> pred)
        {
            if (asset == null) { throw new ArgumentNullException(nameof(asset)); }
            if (pred == null) { throw new ArgumentNullException(nameof(pred)); }
            
            var path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path)) { return; }
            bool modified = false;
            foreach (var subAsset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (subAsset == null) { continue; }
                // AssetDatabase.IsSubAsset returns false always if asset is hidden in hierarchy.
                if (AssetDatabase.IsMainAsset(subAsset)) { continue; }
                if (pred(subAsset))
                {
                    UnityEngine.Object.DestroyImmediate(subAsset, true);
                    modified = true;
                }
            }
            if (modified)
            {
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }
    }

}