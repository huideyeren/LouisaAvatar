#if VL2_DEVELOPMENT

using UnityEditor;

namespace VirtualLens2.Generators
{
    public static class Packager
    {
        [MenuItem("Window/Logilabo/VirtualLens2/Generate Package")]
        static void ExportPackage()
        {
            // Remove unnecessary files
            AssetDatabase.DeleteAsset("Assets/VirtualLens2/Settings/ProjectSettings.asset");
            foreach (var s in AssetDatabase.GetSubFolders("Assets/VirtualLens2/Artifacts"))
            {
                AssetDatabase.DeleteAsset(s);
            }
            // Export files as an unitypackage
            string[] files = { "Assets/VirtualLens2" };
            AssetDatabase.ExportPackage(
                files, "VirtualLens2_v2.x.x.unitypackage",
                ExportPackageOptions.Recurse | ExportPackageOptions.Default);
        }
    }
}

#endif