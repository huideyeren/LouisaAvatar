using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using VirtualLens2.AV3EditorLib;
using VRC.SDKBase.Editor.BuildPipeline;

namespace VirtualLens2
{
    public class VirtualLensBuildHook : IVRCSDKPreprocessAvatarCallback
    {
        // It will run between NDMF preprocess and optimization (-5000 and -1025)
        public int callbackOrder => -4500;
        
        public bool OnPreprocessAvatar(GameObject avatar)
        {
            var root = HierarchyUtility.PathToObject(avatar, "_VirtualLens_Root");
            // Pass if VirtualLens2 is not implemented for the avatar.
            if (root == null) { return true; }

            // Search version marker in the root of generated objects.
            var version = "";
            var re = new Regex(@"^_Version(\d+)$");
            foreach (Transform child in root.transform)
            {
                var match = re.Match(child.name);
                if (match.Success)
                {
                    version = match.Groups[1].Value;
                    break;
                }
            }
            if (string.IsNullOrEmpty(version) || int.Parse(version) != Constants.Version)
            {
                EditorUtility.DisplayDialog(
                    "VirtualLens2 Validator",
                    "Older VirtualLens2 is implemented for this avatar.\n" +
                    "You have to apply VirtualLensSettings again before building the avatar.",
                    "Cancel build");
                return false;
            }

            return true;
        }
    }
}