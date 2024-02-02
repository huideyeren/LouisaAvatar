using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VirtualLens2
{
    internal static class SettingsMigrator
    {
        // Migration for v2.7.0: Default distance for far plane
        private static void MigrateDefaultFarPlane(SerializedObject settings)
        {
            if (settings.FindProperty("lastVersion").intValue >= 20700) { return; }
            // Value is modified by user: do not update
            var clippingFarProp = settings.FindProperty("clippingFar");
            var clippingFar = clippingFarProp.floatValue;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (clippingFar != 1000.0f) { return; }
            clippingFarProp.floatValue = 5000.0f;
        }

        // Migration for v2.7.0: Initial grid type
        private static void MigrateInitialGridType(SerializedObject settings)
        {
            if (settings.FindProperty("lastVersion").intValue >= 20700) { return; }
            var old = settings.FindProperty("initialGrid").boolValue;
            settings.FindProperty("initialGridType").intValue = old ? 1 : 0;
        }

        // Migration for v2.8.0: Remove KinoBokeh
        private static void MigrateKinoBokehRemoval(SerializedObject settings)
        {
            if (settings.FindProperty("lastVersion").intValue >= 20800) { return; }
            var algorithmProp = settings.FindProperty("algorithm");
            if (algorithmProp.enumValueIndex == 1) // KinoBokeh
            {
                algorithmProp.enumValueIndex = 0; // Default
                settings.FindProperty("logiBokehMSAASamples").intValue =
                    settings.FindProperty("highResolutionMSAA").intValue;
            }
        }

        // Migration for v2.9.0: Drone speed
        private static void MigrateDroneSpeed(SerializedObject settings)
        {
            if (settings.FindProperty("lastVersion").intValue >= 20900) { return; }
            settings.FindProperty("droneLinearSpeed").floatValue *= 20.0f;
            settings.FindProperty("droneYawSpeed").floatValue *= 45.0f;
        }

        // Migration for v2.9.0: Depth enabler
        private static void MigrateDepthEnabler(SerializedObject settings)
        {
            if (settings.FindProperty("lastVersion").intValue >= 20900) { return; }
            settings.FindProperty("depthEnablerMode").intValue = 1;
        }

        // Migration for v2.10.0: Screen toucher
        private static void MigrateScreenTouchers(SerializedObject settings)
        {
            if (settings.FindProperty("lastVersion").intValue >= 21000) { return; }
            var avatar = settings.FindProperty("avatar").objectReferenceValue as GameObject;
            if (!avatar) { return; }
            var screenTouchers = MarkerDetector.DetectScreenTouchers(avatar);
            var listProp = settings.FindProperty("screenTouchers");
            if (screenTouchers == null || screenTouchers.All(o => o == null)) { return; }
            listProp.ClearArray();
            var index = 0;
            foreach (var marker in screenTouchers)
            {
                listProp.InsertArrayElementAtIndex(index);
                var prop = listProp.GetArrayElementAtIndex(index);
                prop.objectReferenceValue = marker;
                ++index;
            }
        }

        // Migration for v2.10.0: Drone controller marker
        private static void MigrateDroneController(SerializedObject settings)
        {
            if (settings.FindProperty("lastVersion").intValue >= 21000) { return; }
            var avatar = settings.FindProperty("avatar").objectReferenceValue as GameObject;
            if (!avatar) { return; }

            var droneController = MarkerDetector.DetectDroneController(avatar);
            var overrideProp = settings.FindProperty("overrideDroneController");
            var objectProp = settings.FindProperty("droneController");
            if (!droneController)
            {
                overrideProp.boolValue = false;
                objectProp.objectReferenceValue = null;
            }
            else
            {
                overrideProp.boolValue = true;
                objectProp.objectReferenceValue = droneController;
            }
        }

        // Migration for v2.10.0: Reposition origin marker
        private static void MigrateRepositionOrigin(SerializedObject settings)
        {
            if (settings.FindProperty("lastVersion").intValue >= 21000) { return; }
            var avatar = settings.FindProperty("avatar").objectReferenceValue as GameObject;
            if (!avatar) { return; }

            var repositionOrigin = MarkerDetector.DetectRepositionOrigin(avatar);
            var overrideProp = settings.FindProperty("overrideRepositionOrigin");
            var objectProp = settings.FindProperty("repositionOrigin");
            if (!repositionOrigin)
            {
                overrideProp.boolValue = false;
                objectProp.objectReferenceValue = null;
            }
            else
            {
                overrideProp.boolValue = true;
                objectProp.objectReferenceValue = repositionOrigin;
            }
        }

        // Migration for v2.10.0: Selfie markers
        private static void MigrateSelfieMarkers(SerializedObject settings)
        {
            if (settings.FindProperty("lastVersion").intValue >= 21000) { return; }
            var avatar = settings.FindProperty("avatar").objectReferenceValue as GameObject;
            if (!avatar) { return; }

            var selfieMarkerLeft = MarkerDetector.DetectSelfieMarkerLeft(avatar);
            var selfieMarkerRight = MarkerDetector.DetectSelfieMarkerRight(avatar);
            var overrideProp = settings.FindProperty("overrideSelfieMarkers");
            var leftObjectProp = settings.FindProperty("selfieMarkerLeft");
            var rightObjectProp = settings.FindProperty("selfieMarkerRight");
            if (!selfieMarkerLeft && !selfieMarkerRight)
            {
                overrideProp.boolValue = false;
                leftObjectProp.objectReferenceValue = null;
                rightObjectProp.objectReferenceValue = null;
            }
            else
            {
                overrideProp.boolValue = true;
                leftObjectProp.objectReferenceValue = selfieMarkerLeft;
                rightObjectProp.objectReferenceValue = selfieMarkerRight;
            }
        }

        public static void Migrate(SerializedObject settings)
        {
            MigrateDefaultFarPlane(settings);
            MigrateInitialGridType(settings);
            MigrateKinoBokehRemoval(settings);
            MigrateDroneSpeed(settings);
            MigrateDepthEnabler(settings);
            MigrateScreenTouchers(settings);
            MigrateDroneController(settings);
            MigrateRepositionOrigin(settings);
            MigrateSelfieMarkers(settings);
            settings.FindProperty("lastVersion").intValue = Constants.Version;
        }
    }
}
