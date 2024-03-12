using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace XWear.XWearPackage.Editor.Util.Import
{
    [CreateAssetMenu(menuName = nameof(VrmAnimatorPreset))]
    public class VrmAnimatorPreset : ScriptableObject
    {
        public AnimatorController vrcFaceAnimatorControllerTemplate;
        public AnimatorController vrcHandAnimatorControllerTemplate;
        public VRCExpressionParameters vrcExpressionParametersTemplate;
        public VRCExpressionsMenu vrcExpressionsMenuTemplate;
    }
}
