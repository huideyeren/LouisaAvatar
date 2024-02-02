using System.IO;
using UnityEditor;

namespace XWear.XWearPackage.Editor.Util
{
    public static class FileUtil
    {
        private static string GetLibraryFolder()
        {
            var projDir = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));
            return Path.Combine(projDir, "Library");
        }

        public static string GetValidXWearPackagePreferenceFolder()
        {
            var folder = Path.Combine(GetLibraryFolder(), "XWearPreference");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }

        private static bool CreateDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                return true;
            }

            Directory.CreateDirectory(directoryPath);
            return true;
        }

        public static T CopyAsset<T>(T sourceObject, string distDirectory, string distFileName)
            where T : UnityEngine.Object
        {
            CreateDirectory(distDirectory);

            var sourcePath = AssetDatabase.GetAssetPath(sourceObject);
            var newPath = Path.Combine(distDirectory, distFileName);

            if (!AssetDatabase.CopyAsset(sourcePath, newPath))
            {
                throw new System.Exception();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var copiedAsset = AssetDatabase.LoadAssetAtPath<T>(newPath);
            return copiedAsset;
        }

        public static T SaveAsset<T>(T sourceObject, string distDirectory, string distFileName)
            where T : UnityEngine.Object
        {
            var path = Path.Combine(distDirectory, distFileName);

            CreateDirectory(distDirectory);
            AssetDatabase.CreateAsset(sourceObject, path);
            AssetDatabase.SaveAssets();

            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}
