using System.Collections.Generic;
using UnityEngine;

namespace VirtualLens2
{
    internal static class MarkerDetector
    {
        public static GameObject DetectOrigin(GameObject avatar)
        {
            if (!avatar) { return null; }
            return HierarchyUtil.FindGameObject(avatar, "VirtualLensOrigin");
        }
        
        public static GameObject DetectOrigin(ImplementationSettings settings)
        {
            return DetectOrigin(settings.Avatar);
        }
        
        
        public static GameObject DetectPreview(GameObject avatar)
        {
            if (!avatar) { return null; }
            return HierarchyUtil.FindGameObject(avatar, "VirtualLensPreview");
        }

        public static GameObject DetectPreview(ImplementationSettings settings)
        {
            return DetectPreview(settings.Avatar);
        }
        
        
        public static IList<GameObject> DetectScreenTouchers(GameObject avatar)
        {
            if (!avatar) { return new List<GameObject>(); }
            // VirtualLens2/Prefabs/ScreenToucher.prefab
            return HierarchyUtil.FindPrefabInstances(avatar, "7dd993771f3f7934284e57b4d2c67208");
        }

        public static IList<GameObject> DetectScreenTouchers(VirtualLensSettings settings)
        {
            return DetectScreenTouchers(settings.avatar);
        }
        
        
        public static GameObject DetectDroneController(GameObject avatar)
        {
            if (!avatar) { return null; }
            // VirtualLens2/Prefabs/DroneController.prefab
            return HierarchyUtil.FindPrefabInstance(avatar, "a6ce9259a21e0f04399059287bd5b875");
        }
        
        public static GameObject DetectDroneController(VirtualLensSettings settings)
        {
            return DetectDroneController(settings.avatar);
        }

        
        public static GameObject DetectRepositionOrigin(GameObject avatar)
        {
            if (!avatar) { return null; }
            // VirtualLens2/Prefabs/RepositionOrigin.prefab
            return HierarchyUtil.FindPrefabInstance(avatar, "a3582bbce3991544cb8d3121d2cdd91e");
        }
        
        public static GameObject DetectRepositionOrigin(VirtualLensSettings settings)
        {
            return DetectRepositionOrigin(settings.avatar);
        }


        public static GameObject DetectSelfieMarkerLeft(GameObject avatar)
        {
            if (!avatar) { return null; }
            // VirtualLens2/Prefabs/SelfieMarkerLeft.prefab
            return HierarchyUtil.FindPrefabInstance(avatar, "30135137a812d024e8faa8a1ce836c87");
        }

        public static GameObject DetectSelfieMarkerLeft(VirtualLensSettings settings)
        {
            return DetectSelfieMarkerLeft(settings.avatar);
        }


        public static GameObject DetectSelfieMarkerRight(GameObject avatar)
        {
            if (!avatar) { return null; }
            // VirtualLens2/Prefabs/SelfieMarkerRight.prefab
            return HierarchyUtil.FindPrefabInstance(avatar, "0454613d1827826448648c3eb74bd7f1");
        }

        public static GameObject DetectSelfieMarkerRight(VirtualLensSettings settings)
        {
            return DetectSelfieMarkerRight(settings.avatar);
        }
    }
}
