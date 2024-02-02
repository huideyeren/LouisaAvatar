using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using VRC.SDK3.Avatars.Components;
using VirtualLens2.AV3EditorLib;

namespace VirtualLens2
{
    [CustomEditor(typeof(VirtualLensSettings))]
    public class SettingsEditor : Editor
    {
        private abstract class SettingsInspectorBase
        {
            protected readonly SerializedObject SerializedObject;
            protected readonly SerializedProperty AvatarProp;
            protected readonly SerializedProperty BuildModeProp;

            protected SettingsInspectorBase(SerializedObject so)
            {
                SerializedObject = so;
                AvatarProp = so.FindProperty("avatar");
                BuildModeProp = so.FindProperty("buildMode");
            }

            protected bool IsDestructive => BuildModeProp.enumValueIndex == (int)BuildMode.Destructive;

            protected GameObject TargetAvatar
            {
                get
                {
                    if (IsDestructive) { return AvatarProp.objectReferenceValue as GameObject; }
                    var component = SerializedObject.targetObject as Component;
                    if (component == null) { return null; }
                    var descriptor = component.GetComponentInParent<VRCAvatarDescriptor>();
                    return descriptor == null ? null : descriptor.gameObject;
                }
            }

            protected Component SettingsComponent => SerializedObject.targetObject as Component;

            protected static void IntPopup(SerializedProperty prop, IEnumerable<string> options, string label)
            {
                var values = new List<int>();
                var enumerable = options as string[] ?? options.ToArray();
                for (var i = 0; i < enumerable.Length; ++i) values.Add(i);
                EditorGUILayout.IntPopup(
                    prop,
                    enumerable.Select(s => new GUIContent(s)).ToArray(),
                    values.ToArray(),
                    new GUIContent(label));
            }

            public abstract void DrawGUI();
        }

        private class GeneralSettingsInspector : SettingsInspectorBase
        {
            public GeneralSettingsInspector(SerializedObject so) : base(so) { }

            public override void DrawGUI()
            {
                var oldAvatar = TargetAvatar;
                EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
                ++EditorGUI.indentLevel;
#if WITH_NDMF && WITH_MODULAR_AVATAR
                var modes = new[] { "Destructive (Legacy)", "Non-destructive" };
#else
                var modes = new[] { "Destructive (Legacy)" };
#endif
                var oldMode = BuildModeProp.enumValueIndex;
                IntPopup(BuildModeProp, modes, "Build Mode");
                var newMode = BuildModeProp.enumValueIndex;
                if (newMode != oldMode && newMode == (int)BuildMode.NonDestructive)
                {
                    var runMigration = EditorUtility.DisplayDialog(
                        "VirtualLens2",
                        "Switching to non-destructive setup may require some modifications.\n" +
                        "Would you like to apply it automatically?",
                        "Yes", "No");
                    if (runMigration)
                    {
                        MigrateToNonDestructive(oldAvatar);
                    }
                }
                if (BuildModeProp.enumValueIndex == (int)BuildMode.Destructive)
                {
                    EditorGUILayout.PropertyField(AvatarProp, new GUIContent("Avatar"));
#if WITH_NDMF && WITH_MODULAR_AVATAR
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(15.0f);
                        if (GUILayout.Button("Migrate as a Non-destructive Module"))
                        {
                            BuildModeProp.enumValueIndex = (int)BuildMode.NonDestructive;
                            MigrateToNonDestructive(oldAvatar);
                        }
                    }
#else
                EditorGUILayout.HelpBox("Non-destructive setup requires Modular Avatar.", MessageType.Info);
#endif
                }
                else
                {
                    var descriptor = SettingsComponent.GetComponentInParent<VRCAvatarDescriptor>();
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.ObjectField(
                            new GUIContent("Avatar"), descriptor == null ? null : descriptor.gameObject,
                            typeof(GameObject), true);
                    }
                }
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
            }

            private void MigrateToNonDestructive(GameObject avatar)
            {
#if WITH_NDMF && WITH_MODULAR_AVATAR
                var component = SettingsComponent;
                if (!component) return;
                if (avatar)
                {
                    Undo.SetTransformParent(component.transform, avatar.transform, "Move objects");
                }

                var so = SerializedObject;
                var cameraObjectProp = so.FindProperty("cameraObject");
                var cameraDroppableProp = so.FindProperty("cameraDroppable");
                var overrideDroneControllerProp = so.FindProperty("overrideDroneController");
                var overrideRepositionOriginProp = so.FindProperty("overrideRepositionOrigin");
                var overrideSelfieMarkersProp = so.FindProperty("overrideSelfieMarkers");
                var screenTouchersProp = so.FindProperty("screenTouchers");
                var droneControllerProp = so.FindProperty("droneController");
                var repositionOriginProp = so.FindProperty("repositionOrigin");
                var selfieMarkerLeftProp = so.FindProperty("selfieMarkerLeft");
                var selfieMarkerRightProp = so.FindProperty("selfieMarkerRight");
                var artifactsFolderProp = so.FindProperty("artifactsFolder");
                var cameraRoot = cameraObjectProp.objectReferenceValue as GameObject;
                if (cameraRoot != null && BoneProxyHelper.AddBoneProxy(cameraRoot))
                {
                    Undo.SetTransformParent(cameraRoot.transform, component.transform, "Move objects");
                }
                for (var i = 0; i < screenTouchersProp.arraySize; ++i)
                {
                    var item = screenTouchersProp.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                    if (item != null && BoneProxyHelper.AddBoneProxy(item))
                    {
                        Undo.SetTransformParent(item.transform, component.transform, "Move objects");
                    }
                }
                if (overrideDroneControllerProp.boolValue)
                {
                    var item = droneControllerProp.objectReferenceValue as GameObject;
                    if (item != null && BoneProxyHelper.AddBoneProxy(item))
                    {
                        Undo.SetTransformParent(item.transform, component.transform, "Move objects");
                    }
                }
                if (overrideRepositionOriginProp.boolValue)
                {
                    var item = repositionOriginProp.objectReferenceValue as GameObject;
                    if (item != null && BoneProxyHelper.AddBoneProxy(item))
                    {
                        Undo.SetTransformParent(item.transform, component.transform, "Move objects");
                    }
                }
                if (overrideSelfieMarkersProp.boolValue)
                {
                    var left = selfieMarkerLeftProp.objectReferenceValue as GameObject;
                    var right = selfieMarkerRightProp.objectReferenceValue as GameObject;
                    if (left != null && BoneProxyHelper.AddBoneProxy(left))
                    {
                        Undo.SetTransformParent(left.transform, component.transform, "Move objects");
                    }
                    if (right != null && BoneProxyHelper.AddBoneProxy(right))
                    {
                        Undo.SetTransformParent(right.transform, component.transform, "Move objects");
                    }
                }
                var nonPreviewRoot = cameraDroppableProp.objectReferenceValue as GameObject;
                Applier.Remove(TargetAvatar, cameraRoot, nonPreviewRoot, artifactsFolderProp.stringValue);
#endif
            }
        }

        private class CameraObjectInspector : SettingsInspectorBase
        {
            private readonly SerializedProperty _cameraObjectProp;
            private readonly SerializedProperty _cameraDroppableProp;

            public CameraObjectInspector(SerializedObject so) : base(so)
            {
                _cameraObjectProp = so.FindProperty("cameraObject");
                _cameraDroppableProp = so.FindProperty("cameraDroppable");
            }

            public override void DrawGUI()
            {
                EditorGUILayout.LabelField("Camera Object", EditorStyles.boldLabel);
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(_cameraObjectProp, new GUIContent("Root Object"));
                EditorGUILayout.PropertyField(_cameraDroppableProp, new GUIContent("Non-Preview Root"));
                --EditorGUI.indentLevel;
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(15.0f);
                    if (GUILayout.Button("Auto Placement"))
                    {
                        var component = SettingsComponent;
                        var parent = component == null ? null : component.gameObject;
                        AutoPlacementDialog.Open(SerializedObject, TargetAvatar, IsDestructive ? null : parent);
                    }
                }
                ++EditorGUI.indentLevel;
                if (_cameraObjectProp.objectReferenceValue != null && IsDestructive)
                {
                    EditorGUILayout.HelpBox(
                        "Root object will be disabled after applying. It will be enabled in VRChat by animations.",
                        MessageType.Info);
                }
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
            }
        }

        private class MarkerObjectsInspector : SettingsInspectorBase
        {
            private readonly SerializedProperty _overrideDroneControllerProp;
            private readonly SerializedProperty _overrideRepositionOriginProp;
            private readonly SerializedProperty _overrideSelfieMarkersProp;

            private readonly SerializedProperty _screenTouchersProp;
            private readonly ReorderableList _screenTouchersList;
            private readonly SerializedProperty _droneControllerProp;
            private readonly SerializedProperty _repositionOriginProp;
            private readonly SerializedProperty _selfieMarkerLeftProp;
            private readonly SerializedProperty _selfieMarkerRightProp;

            public MarkerObjectsInspector(SerializedObject so) : base(so)
            {
                _overrideDroneControllerProp = so.FindProperty("overrideDroneController");
                _overrideRepositionOriginProp = so.FindProperty("overrideRepositionOrigin");
                _overrideSelfieMarkersProp = so.FindProperty("overrideSelfieMarkers");
                _screenTouchersProp = so.FindProperty("screenTouchers");
                _screenTouchersList = new ReorderableList(so, _screenTouchersProp);
                _screenTouchersList.drawHeaderCallback += rect => { EditorGUI.LabelField(rect, "Screen Touchers"); };
                _screenTouchersList.drawElementCallback += (rect, index, selected, focused) =>
                {
                    var property = _screenTouchersList.serializedProperty.GetArrayElementAtIndex(index);
                    var h = EditorGUIUtility.singleLineHeight;
                    var py = (rect.height - h) / 2;
                    var objectRect = new Rect(rect.x, rect.y + py, rect.width, h);
                    EditorGUI.ObjectField(objectRect, property, GUIContent.none);
                };
                _droneControllerProp = so.FindProperty("droneController");
                _repositionOriginProp = so.FindProperty("repositionOrigin");
                _selfieMarkerLeftProp = so.FindProperty("selfieMarkerLeft");
                _selfieMarkerRightProp = so.FindProperty("selfieMarkerRight");
            }

            public override void DrawGUI()
            {
                if (!SettingsComponent) { return; }
                var componentObject = SettingsComponent.gameObject;
                var avatar = TargetAvatar;

                var miniButtonStyle = new GUIStyle(EditorStyles.miniButton);
                var singleCancelButtonStyle = new GUIStyle(EditorStyles.miniButton) { fixedWidth = 30.0f };
                var doubleCancelButtonStyle = new GUIStyle(EditorStyles.miniButton) { fixedWidth = 30.0f };
                doubleCancelButtonStyle.fixedHeight =
                    doubleCancelButtonStyle.fixedHeight * 2.0f + 2.0f;

                EditorGUILayout.LabelField("Marker Objects", EditorStyles.boldLabel);
                ++EditorGUI.indentLevel;
                if (_screenTouchersProp.arraySize == 0)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PrefixLabel(new GUIContent("Screen Touchers"));
                        if (GUILayout.Button(new GUIContent("Auto"), miniButtonStyle))
                        {
                            if (avatar != null)
                            {
                                var markers = MarkerGenerator.GenerateScreenToucher(avatar);
                                var index = 0;
                                foreach (var marker in markers)
                                {
                                    if (!IsDestructive)
                                    {
                                        BoneProxyHelper.AddBoneProxy(marker);
                                        Undo.SetTransformParent(marker.transform, componentObject.transform,
                                            "Create ScreenTouchers");
                                    }
                                    _screenTouchersProp.InsertArrayElementAtIndex(index);
                                    var prop = _screenTouchersProp.GetArrayElementAtIndex(index);
                                    prop.objectReferenceValue = marker;
                                    ++index;
                                }
                            }
                        }
                    }
                }
                else
                {
                    _screenTouchersList.DoLayoutList();
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PrefixLabel(new GUIContent("Drone Controller"));
                    if (_overrideDroneControllerProp.boolValue)
                    {
                        --EditorGUI.indentLevel;
                        EditorGUILayout.PropertyField(_droneControllerProp, GUIContent.none);
                        if (GUILayout.Button(new GUIContent("×"), singleCancelButtonStyle))
                        {
                            _overrideDroneControllerProp.boolValue = false;
                        }
                        ++EditorGUI.indentLevel;
                    }
                    else
                    {
                        if (GUILayout.Button(new GUIContent("Auto"), miniButtonStyle))
                        {
                            _overrideDroneControllerProp.boolValue = true;
                            if (avatar != null && _droneControllerProp.objectReferenceValue == null)
                            {
                                var marker = MarkerGenerator.GenerateDroneController(avatar);
                                if (!IsDestructive)
                                {
                                    BoneProxyHelper.AddBoneProxy(marker);
                                    Undo.SetTransformParent(marker.transform, componentObject.transform,
                                        "Create DroneController");
                                }
                                _droneControllerProp.objectReferenceValue = marker;
                            }
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PrefixLabel(new GUIContent("Reposition Origin"));
                    if (_overrideRepositionOriginProp.boolValue)
                    {
                        --EditorGUI.indentLevel;
                        EditorGUILayout.PropertyField(_repositionOriginProp, GUIContent.none);
                        if (GUILayout.Button(new GUIContent("×"), singleCancelButtonStyle))
                        {
                            _overrideRepositionOriginProp.boolValue = false;
                        }
                        ++EditorGUI.indentLevel;
                    }
                    else
                    {
                        if (GUILayout.Button(new GUIContent("Auto"), miniButtonStyle))
                        {
                            _overrideRepositionOriginProp.boolValue = true;
                            if (avatar != null && _repositionOriginProp.objectReferenceValue == null)
                            {
                                var marker = MarkerGenerator.GenerateRepositionOrigin(avatar);
                                if (!IsDestructive)
                                {
                                    BoneProxyHelper.AddBoneProxy(marker);
                                    Undo.SetTransformParent(marker.transform, componentObject.transform,
                                        "Create RepositionOrigin");
                                }
                                _repositionOriginProp.objectReferenceValue = marker;
                            }
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (_overrideSelfieMarkersProp.boolValue)
                    {
                        using (new EditorGUILayout.VerticalScope())
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.PrefixLabel(new GUIContent("Selfie Marker (Left)"));
                                --EditorGUI.indentLevel;
                                EditorGUILayout.PropertyField(_selfieMarkerLeftProp, GUIContent.none);
                                ++EditorGUI.indentLevel;
                            }
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.PrefixLabel(new GUIContent("Selfie Marker (Right)"));
                                --EditorGUI.indentLevel;
                                EditorGUILayout.PropertyField(_selfieMarkerRightProp, GUIContent.none);
                                ++EditorGUI.indentLevel;
                            }
                        }
                        if (GUILayout.Button(new GUIContent("×"), doubleCancelButtonStyle))
                        {
                            _overrideSelfieMarkersProp.boolValue = false;
                        }
                    }
                    else
                    {
                        EditorGUILayout.PrefixLabel(new GUIContent("Selfie Markers"));
                        if (GUILayout.Button(new GUIContent("Auto"), miniButtonStyle))
                        {
                            _overrideSelfieMarkersProp.boolValue = true;
                            if (avatar != null)
                            {
                                var pair = MarkerGenerator.GenerateEyeMarkers(
                                    avatar,
                                    _selfieMarkerLeftProp.objectReferenceValue == null,
                                    _selfieMarkerRightProp.objectReferenceValue == null);
                                if (pair[0] != null)
                                {
                                    if (!IsDestructive)
                                    {
                                        BoneProxyHelper.AddBoneProxy(pair[0]);
                                        Undo.SetTransformParent(pair[0].transform, componentObject.transform,
                                            "Create SelfieMarkers");
                                    }
                                    _selfieMarkerLeftProp.objectReferenceValue = pair[0];
                                }
                                if (pair[1] != null)
                                {
                                    if (!IsDestructive)
                                    {
                                        BoneProxyHelper.AddBoneProxy(pair[1]);
                                        Undo.SetTransformParent(pair[1].transform, componentObject.transform,
                                            "Create SelfieMarkers");
                                    }
                                    _selfieMarkerRightProp.objectReferenceValue = pair[1];
                                }
                            }
                        }
                    }
                }
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
            }
        }

        private class FocalLengthInspector : SettingsInspectorBase
        {
            private readonly SerializedProperty _minFocalLengthProp;
            private readonly SerializedProperty _maxFocalLengthProp;
            private readonly SerializedProperty _defaultFocalLengthProp;
            private readonly SerializedProperty _synchronizeFocalLengthProp;

            public FocalLengthInspector(SerializedObject so) : base(so)
            {
                _minFocalLengthProp = so.FindProperty("minFocalLength");
                _maxFocalLengthProp = so.FindProperty("maxFocalLength");
                _defaultFocalLengthProp = so.FindProperty("defaultFocalLength");
                _synchronizeFocalLengthProp = so.FindProperty("synchronizeFocalLength");
            }

            public override void DrawGUI()
            {
                EditorGUILayout.LabelField("Focal Length", EditorStyles.boldLabel);
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(_minFocalLengthProp, new GUIContent("Min Focal Length [mm]"));
                EditorGUILayout.PropertyField(_maxFocalLengthProp, new GUIContent("Max Focal Length [mm]"));
                EditorGUILayout.PropertyField(_defaultFocalLengthProp, new GUIContent("Default Focal Length [mm]"));
                if (VrcExpressionParametersWrapper.SupportsNotSynchronizedVariables())
                {
                    EditorGUILayout.PropertyField(_synchronizeFocalLengthProp,
                        new GUIContent("Sync Parameter for Remote"));
                }
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
            }
        }

        private class ApertureInspector : SettingsInspectorBase
        {
            private readonly SerializedProperty _enableBlurringProp;
            private readonly SerializedProperty _minFNumberProp;
            private readonly SerializedProperty _maxFNumberProp;
            private readonly SerializedProperty _defaultFNumberProp;
            private readonly SerializedProperty _synchronizeFNumberProp;

            public ApertureInspector(SerializedObject so) : base(so)
            {
                _enableBlurringProp = so.FindProperty("enableBlurring");
                _minFNumberProp = so.FindProperty("minFNumber");
                _maxFNumberProp = so.FindProperty("maxFNumber");
                _defaultFNumberProp = so.FindProperty("defaultFNumber");
                _synchronizeFNumberProp = so.FindProperty("synchronizeFNumber");
            }

            public override void DrawGUI()
            {
                EditorGUILayout.LabelField("Aperture", EditorStyles.boldLabel);
                ++EditorGUI.indentLevel;
                var hasSyncFlag = VrcExpressionParametersWrapper.SupportsNotSynchronizedVariables();
                var enableProp = _enableBlurringProp;
                var enabled = true;
                if (!hasSyncFlag)
                {
                    EditorGUILayout.PropertyField(enableProp, new GUIContent("Enable DoF Simulation"));
                    enabled = enableProp.boolValue;
                }
                using (new EditorGUI.DisabledScope(!enabled))
                {
                    EditorGUILayout.PropertyField(_minFNumberProp, new GUIContent("Min F Number"));
                    EditorGUILayout.PropertyField(_maxFNumberProp, new GUIContent("Max F Number"));
                    EditorGUILayout.PropertyField(_defaultFNumberProp, new GUIContent("Default F Number"));
                    if (hasSyncFlag)
                    {
                        EditorGUILayout.PropertyField(_synchronizeFNumberProp,
                            new GUIContent("Sync Parameter for Remote"));
                    }
                }
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
            }
        }

        private class ManualFocusingInspector : SettingsInspectorBase
        {
            private readonly SerializedProperty _enableManualFocusingProp;
            private readonly SerializedProperty _minFocusDistanceProp;
            private readonly SerializedProperty _maxFocusDistanceProp;
            private readonly SerializedProperty _synchronizeFocusDistanceProp;

            public ManualFocusingInspector(SerializedObject so) : base(so)
            {
                _enableManualFocusingProp = so.FindProperty("enableManualFocusing");
                _minFocusDistanceProp = so.FindProperty("minFocusDistance");
                _maxFocusDistanceProp = so.FindProperty("maxFocusDistance");
                _synchronizeFocusDistanceProp = so.FindProperty("synchronizeFocusDistance");
            }

            public override void DrawGUI()
            {
                EditorGUILayout.LabelField("Manual Focusing", EditorStyles.boldLabel);
                ++EditorGUI.indentLevel;
                var hasSyncFlag = VrcExpressionParametersWrapper.SupportsNotSynchronizedVariables();
                var enableProp = _enableManualFocusingProp;
                var enabled = true;
                if (!hasSyncFlag)
                {
                    EditorGUILayout.PropertyField(enableProp, new GUIContent("Enable Manual Focusing"));
                    enabled = enableProp.boolValue;
                }
                using (new EditorGUI.DisabledScope(!enabled))
                {
                    EditorGUILayout.PropertyField(_minFocusDistanceProp, new GUIContent("Min Focus Distance [m]"));
                    EditorGUILayout.PropertyField(_maxFocusDistanceProp, new GUIContent("Max Focus Distance [m]"));
                    if (hasSyncFlag)
                    {
                        EditorGUILayout.PropertyField(
                            _synchronizeFocusDistanceProp, new GUIContent("Sync Parameter for Remote"));
                    }
                }
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
            }
        }

        private class ExposureInspector : SettingsInspectorBase
        {
            private readonly SerializedProperty _enableExposureProp;
            private readonly SerializedProperty _exposureRangeProp;
            private readonly SerializedProperty _defaultExposureProp;
            private readonly SerializedProperty _synchronizeExposureProp;

            public ExposureInspector(SerializedObject so) : base(so)
            {
                _enableExposureProp = so.FindProperty("enableExposure");
                _exposureRangeProp = so.FindProperty("exposureRange");
                _defaultExposureProp = so.FindProperty("defaultExposure");
                _synchronizeExposureProp = so.FindProperty("synchronizeExposure");
            }

            public override void DrawGUI()
            {
                EditorGUILayout.LabelField("Exposure Compensation", EditorStyles.boldLabel);
                ++EditorGUI.indentLevel;
                var hasSyncFlag = VrcExpressionParametersWrapper.SupportsNotSynchronizedVariables();
                var enableProp = _enableExposureProp;
                var enabled = true;
                if (!hasSyncFlag)
                {
                    EditorGUILayout.PropertyField(enableProp, new GUIContent("Enable Exposure Compensation"));
                    enabled = enableProp.boolValue;
                }
                using (new EditorGUI.DisabledScope(!enabled))
                {
                    EditorGUILayout.PropertyField(_exposureRangeProp, new GUIContent("Exposure Range [EV]"));
                    EditorGUILayout.PropertyField(_defaultExposureProp, new GUIContent("Default Exposure [EV]"));
                    if (hasSyncFlag)
                    {
                        EditorGUILayout.PropertyField(
                            _synchronizeExposureProp, new GUIContent("Sync Parameter for Remote"));
                    }
                }
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
            }
        }

        private class DroneInspector : SettingsInspectorBase
        {
            private readonly SerializedProperty _enableDroneProp;
            private readonly SerializedProperty _droneLinearSpeedProp;
            private readonly SerializedProperty _droneLinearDeadZoneProp;
            private readonly SerializedProperty _droneYawSpeedProp;

            public DroneInspector(SerializedObject so) : base(so)
            {
                _enableDroneProp = so.FindProperty("enableDrone");
                _droneLinearSpeedProp = so.FindProperty("droneLinearSpeed");
                _droneLinearDeadZoneProp = so.FindProperty("droneLinearDeadZone");
                _droneYawSpeedProp = so.FindProperty("droneYawSpeed");
            }

            public override void DrawGUI()
            {
                EditorGUILayout.LabelField("Drone", EditorStyles.boldLabel);
                ++EditorGUI.indentLevel;
                var hasSyncFlag = VrcExpressionParametersWrapper.SupportsNotSynchronizedVariables();
                var enableProp = _enableDroneProp;
                var enabled = true;
                if (!hasSyncFlag)
                {
                    EditorGUILayout.PropertyField(enableProp, new GUIContent("Enable Drone"));
                    enabled = enableProp.boolValue;
                }
                using (new EditorGUI.DisabledScope(!enabled))
                {
                    EditorGUILayout.PropertyField(_droneLinearSpeedProp, new GUIContent("Speed Scale"));
                    EditorGUILayout.PropertyField(_droneLinearDeadZoneProp,
                        new GUIContent("Dead Zone of Speed Controller"));
                    EditorGUILayout.PropertyField(_droneYawSpeedProp, new GUIContent("Yaw Speed Scale"));

                }
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
            }
        }

        private class ClippingPlanesInspector : SettingsInspectorBase
        {
            private readonly SerializedProperty _clippingNearProp;
            private readonly SerializedProperty _clippingFarProp;

            public ClippingPlanesInspector(SerializedObject so) : base(so)
            {
                _clippingNearProp = so.FindProperty("clippingNear");
                _clippingFarProp = so.FindProperty("clippingFar");
            }

            public override void DrawGUI()
            {
                EditorGUILayout.LabelField("Clipping Planes", EditorStyles.boldLabel);
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(_clippingNearProp, new GUIContent("Near [m]"));
                EditorGUILayout.PropertyField(_clippingFarProp, new GUIContent("Far [m]"));
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
            }
        }

        private class OptionalObjectsInspector : SettingsInspectorBase
        {
            private readonly ReorderableList _optionalObjectList;

            public OptionalObjectsInspector(SerializedObject so) : base(so)
            {
                _optionalObjectList = new ReorderableList(so, so.FindProperty("optionalObjects"))
                {
                    headerHeight = 2
                };
                _optionalObjectList.drawElementCallback += (rect, index, selected, focused) =>
                {
                    var property = _optionalObjectList.serializedProperty.GetArrayElementAtIndex(index);
                    var h = EditorGUIUtility.singleLineHeight;
                    var px = 5.0f;
                    var py = (rect.height - h) / 2;
                    var stateRect = new Rect(rect.x, rect.y + py, h * 2, h);
                    var objectRect = new Rect(rect.x + h + px, rect.y + py, rect.width - h - px, h);
                    var gameObject = property.FindPropertyRelative("gameObject");
                    var defaultState = property.FindPropertyRelative("defaultState");
                    defaultState.boolValue = EditorGUI.Toggle(stateRect, defaultState.boolValue);
                    EditorGUI.ObjectField(objectRect, gameObject, GUIContent.none);
                };
            }

            public override void DrawGUI()
            {
                EditorGUILayout.LabelField("Optional Objects", EditorStyles.boldLabel);
                ++EditorGUI.indentLevel;
                _optionalObjectList.DoLayoutList();
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
            }
        }

        private class HideableMeshesInspector : SettingsInspectorBase
        {
            private readonly ReorderableList _hideableObjectList;

            public HideableMeshesInspector(SerializedObject so) : base(so)
            {
                _hideableObjectList = new ReorderableList(so, so.FindProperty("hideableObjects"))
                {
                    headerHeight = 2
                };
                _hideableObjectList.drawElementCallback += (rect, index, selected, focused) =>
                {
                    var property = _hideableObjectList.serializedProperty.GetArrayElementAtIndex(index);
                    var h = EditorGUIUtility.singleLineHeight;
                    var py = (rect.height - h) / 2;
                    var objectRect = new Rect(rect.x, rect.y + py, rect.width, h);
                    var gameObject = property.FindPropertyRelative("gameObject");
                    EditorGUI.ObjectField(objectRect, gameObject, GUIContent.none);
                };
            }

            public override void DrawGUI()
            {
                EditorGUILayout.LabelField("Hideable Meshes", EditorStyles.boldLabel);
                ++EditorGUI.indentLevel;
                _hideableObjectList.DoLayoutList();
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
            }
        }

        private class QuickCallsInspector : SettingsInspectorBase
        {
            private readonly ReorderableList _shortcutList;

            public QuickCallsInspector(SerializedObject so) : base(so)
            {
                _shortcutList = new ReorderableList(so, so.FindProperty("shortcuts"));
                _shortcutList.drawHeaderCallback += rect =>
                {
                    const int x0 = 34, px = 5;
                    var baseWidth = (rect.width - (x0 - rect.x) - 4 * px) / 5;
                    var nameWidth = baseWidth * 2 + px;
                    var offset = baseWidth + px;
                    var nameRect = new Rect(x0 + 0 * offset, rect.y, nameWidth, rect.height);
                    var focalRect = new Rect(x0 + 2 * offset, rect.y, baseWidth, rect.height);
                    var apertureRect = new Rect(x0 + 3 * offset, rect.y, baseWidth, rect.height);
                    var exposureRect = new Rect(x0 + 4 * offset, rect.y, baseWidth, rect.height);
                    EditorGUI.LabelField(nameRect, "Name");
                    EditorGUI.LabelField(focalRect, "Focal Length");
                    EditorGUI.LabelField(apertureRect, "Aperture");
                    EditorGUI.LabelField(exposureRect, "Exposure");
                };
                _shortcutList.drawElementCallback += (rect, index, selected, focused) =>
                {
                    var property = _shortcutList.serializedProperty.GetArrayElementAtIndex(index);
                    int x0 = (int)rect.x, px = 5;
                    var h = EditorGUIUtility.singleLineHeight;
                    var y = rect.y + (rect.height - h) / 2;
                    var baseWidth = (rect.width - 4 * px) / 5;
                    var offset = baseWidth + px;
                    var nameWidth = baseWidth * 2 + px;
                    var checkWidth = h * 2;
                    var paramWidth = baseWidth - h - px;
                    var nameRect = new Rect(x0, y, nameWidth, h);
                    var hasFocalRect = new Rect(x0 + 2 * offset, y, checkWidth, h);
                    var focalRect = new Rect(x0 + 2 * offset + h + px, y, paramWidth, h);
                    var hasApertureRect = new Rect(x0 + 3 * offset, y, checkWidth, h);
                    var apertureRect = new Rect(x0 + 3 * offset + h + px, y, paramWidth, h);
                    var hasExposureRect = new Rect(x0 + 4 * offset, y, checkWidth, h);
                    var exposureRect = new Rect(x0 + 4 * offset + h + px, y, paramWidth, h);
                    var shortcutName = property.FindPropertyRelative("name");
                    var hasFocal = property.FindPropertyRelative("hasFocal");
                    var focal = property.FindPropertyRelative("focal");
                    var hasAperture = property.FindPropertyRelative("hasAperture");
                    var aperture = property.FindPropertyRelative("aperture");
                    var hasExposure = property.FindPropertyRelative("hasExposure");
                    var exposure = property.FindPropertyRelative("exposure");
                    shortcutName.stringValue = EditorGUI.TextField(nameRect, shortcutName.stringValue);
                    hasFocal.boolValue = EditorGUI.Toggle(hasFocalRect, hasFocal.boolValue);
                    using (new EditorGUI.DisabledScope(!hasFocal.boolValue))
                    {
                        focal.floatValue = EditorGUI.FloatField(focalRect, focal.floatValue);
                    }
                    hasAperture.boolValue = EditorGUI.Toggle(hasApertureRect, hasAperture.boolValue);
                    using (new EditorGUI.DisabledScope(!hasAperture.boolValue))
                    {
                        aperture.floatValue = EditorGUI.FloatField(apertureRect, aperture.floatValue);
                    }
                    hasExposure.boolValue = EditorGUI.Toggle(hasExposureRect, hasExposure.boolValue);
                    using (new EditorGUI.DisabledScope(!hasExposure.boolValue))
                    {
                        exposure.floatValue = EditorGUI.FloatField(exposureRect, exposure.floatValue);
                    }
                };
            }

            public override void DrawGUI()
            {
                EditorGUILayout.LabelField("Quick Calls", EditorStyles.boldLabel);
                ++EditorGUI.indentLevel;
                _shortcutList.DoLayoutList();
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
            }
        }

        private class DisplaySettingsInspector : SettingsInspectorBase
        {
            private readonly SerializedProperty _initialGridTypeProp;
            private readonly SerializedProperty _customGridsProp;
            private readonly ReorderableList _customGridsList;
            private readonly SerializedProperty _initialInformationProp;
            private readonly SerializedProperty _initialLevelProp;
            private readonly SerializedProperty _initialPeakingModeProp;

            public DisplaySettingsInspector(SerializedObject so) : base(so)
            {
                _initialGridTypeProp = so.FindProperty("initialGridType");

                _customGridsProp = so.FindProperty("customGrids");
                _customGridsList = new ReorderableList(so, _customGridsProp) { headerHeight = 2 };
                _customGridsList.drawElementCallback += (rect, index, selected, focused) =>
                {
                    var property = _customGridsList.serializedProperty.GetArrayElementAtIndex(index);
                    var h = EditorGUIUtility.singleLineHeight;
                    var nw = rect.width * 0.4f;
                    var tw = rect.width - nw;
                    var py = (rect.height - h) / 2;
                    var nameRect = new Rect(rect.x, rect.y + py, nw, h);
                    var textureRect = new Rect(rect.x + nw, rect.y + py, tw, h);
                    var nameProp = property.FindPropertyRelative("name");
                    var textureProp = property.FindPropertyRelative("texture");
                    EditorGUI.PropertyField(nameRect, nameProp, GUIContent.none);
                    EditorGUI.PropertyField(textureRect, textureProp, GUIContent.none);
                };

                _initialInformationProp = so.FindProperty("initialInformation");
                _initialLevelProp = so.FindProperty("initialLevel");
                _initialPeakingModeProp = so.FindProperty("initialPeakingMode");
            }

            public override void DrawGUI()
            {
                EditorGUILayout.LabelField("Display Settings", EditorStyles.boldLabel);
                ++EditorGUI.indentLevel;
                var gridTypes = new List<string> { "None", "3x3", "3x3 + Diagonal", "6x4" };
                for (var i = 0; i < _customGridsProp.arraySize; ++i)
                {
                    var customGridProp = _customGridsProp.GetArrayElementAtIndex(i);
                    var nameProp = customGridProp.FindPropertyRelative("name");
                    gridTypes.Add($"Custom Grid {i + 1} ({nameProp.stringValue})");
                }
                IntPopup(_initialGridTypeProp, gridTypes, "Grid");
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PrefixLabel(new GUIContent("Custom Grids"));
                    using (new EditorGUILayout.VerticalScope())
                    {
                        _customGridsList.DoLayoutList();
                    }
                }
                EditorGUILayout.PropertyField(_initialInformationProp, new GUIContent("Information"));
                EditorGUILayout.PropertyField(_initialLevelProp, new GUIContent("Level"));
                var peakingModes = new[] { "None", "MF only", "Always" };
                IntPopup(_initialPeakingModeProp, peakingModes, "Peaking");
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
            }
        }

        private class AutoFocusSettingsInspector : SettingsInspectorBase
        {
            private readonly SerializedProperty _initialAutoFocusModeProp;
            private readonly SerializedProperty _initialTrackingSpeedProp;
            private readonly SerializedProperty _initialAutoFocusSpeedProp;

            public AutoFocusSettingsInspector(SerializedObject so) : base(so)
            {
                _initialAutoFocusModeProp = so.FindProperty("initialAutoFocusMode");
                _initialTrackingSpeedProp = so.FindProperty("initialTrackingSpeed");
                _initialAutoFocusSpeedProp = so.FindProperty("initialAutoFocusSpeed");
            }

            public override void DrawGUI()
            {
                EditorGUILayout.LabelField("Default Auto Focus Settings", EditorStyles.boldLabel);
                ++EditorGUI.indentLevel;
                var autoFocusModes = new[] { "Point AF", "Face AF", "Selfie AF" };
                var autoFocusSpeeds = new[] { "Immediate", "Fast", "Medium", "Slow" };
                IntPopup(_initialAutoFocusModeProp, autoFocusModes, "Mode");
                IntPopup(_initialTrackingSpeedProp, autoFocusSpeeds, "Tracking Speed");
                IntPopup(_initialAutoFocusSpeedProp, autoFocusSpeeds, "Focusing Speed");
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
            }
        }

        private class AlgorithmSettingsInspector : SettingsInspectorBase
        {
            private readonly SerializedProperty _useHighResolutionProp;
            private readonly SerializedProperty _logiBokehMaxBlurrinessProp;
            private readonly SerializedProperty _logiBokehAntialiasMethodProp;
            private readonly SerializedProperty _logiBokehRealtimeAntialiasMethodProp;
            private readonly SerializedProperty _logiBokehMSAASamplesProp;

            public AlgorithmSettingsInspector(SerializedObject so) : base(so)
            {
                _useHighResolutionProp = so.FindProperty("useHighResolution");
                _logiBokehMaxBlurrinessProp = so.FindProperty("logiBokehMaxBlurriness");
                _logiBokehAntialiasMethodProp = so.FindProperty("logiBokehAntialiasMethod");
                _logiBokehRealtimeAntialiasMethodProp = so.FindProperty("logiBokehRealtimeAntialiasMethod");
                _logiBokehMSAASamplesProp = so.FindProperty("logiBokehMSAASamples");
            }

            public override void DrawGUI()
            {
                EditorGUILayout.LabelField("Algorithm Settings", EditorStyles.boldLabel);
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(_useHighResolutionProp, new GUIContent("Enable 4K Capturing"));
                var msaaSamples = new[] { "None", "2x", "4x", "8x" };
                EditorGUILayout.IntSlider(_logiBokehMaxBlurrinessProp, 3, 12, new GUIContent("Max Blurriness"));
                IntPopup(_logiBokehMSAASamplesProp, msaaSamples, "MSAA Samples");
                var antialiasingMethods = new[]
                {
                    "Default (FXAA)", "None (low quality, faster)", "FXAA (intermediate)", "SMAA (high quality, slower)"
                };
                IntPopup(_logiBokehAntialiasMethodProp, antialiasingMethods, "Post Antialiasing");
                if (_useHighResolutionProp.boolValue)
                {
                    IntPopup(_logiBokehRealtimeAntialiasMethodProp, antialiasingMethods, "Realtime Post Antialiasing");
                }
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
            }
        }

        private class AdvancedSettingsInspector : SettingsInspectorBase
        {
            private readonly SerializedProperty _artifactsFolderProp;
            private readonly SerializedProperty _clearArtifactsProp;
            private readonly SerializedProperty _touchableThicknessProp;
            private readonly SerializedProperty _writeDefaultsProp;
            private readonly SerializedProperty _depthEnablerModeProp;
            private readonly SerializedProperty _externalPoseSourceProp;
            private readonly SerializedProperty _remoteOnlyModeProp;

            public AdvancedSettingsInspector(SerializedObject so) : base(so)
            {
                _artifactsFolderProp = so.FindProperty("artifactsFolder");
                _clearArtifactsProp = so.FindProperty("clearArtifacts");
                _touchableThicknessProp = so.FindProperty("touchableThickness");
                _writeDefaultsProp = so.FindProperty("writeDefaults");
                _depthEnablerModeProp = so.FindProperty("depthEnablerMode");
                _externalPoseSourceProp = so.FindProperty("externalPoseSource");
                _remoteOnlyModeProp = so.FindProperty("remoteOnlyMode");
            }

            public override void DrawGUI()
            {
                EditorGUILayout.LabelField("Advanced Settings", EditorStyles.boldLabel);
                ++EditorGUI.indentLevel;
                if (IsDestructive)
                {
                    EditorGUILayout.PropertyField(_artifactsFolderProp, new GUIContent("Artifacts Folder"));
                    EditorGUILayout.PropertyField(_clearArtifactsProp, new GUIContent("Clear Artifacts Folder"));
                }
                EditorGUILayout.PropertyField(_touchableThicknessProp, new GUIContent("Thickness of Touch Screen"));
                var writeDefaultsModes = new[] { "Off", "On", "Auto" };
                IntPopup(_writeDefaultsProp, writeDefaultsModes, "Enable Write Defaults");
                var depthEnablerModes = new[] { "Off by default", "On by default" };
                IntPopup(_depthEnablerModeProp, depthEnablerModes, "Depth Enabler");
                EditorGUILayout.PropertyField(_externalPoseSourceProp, new GUIContent("External Pose Source"));
                if (!IsDestructive)
                {
                    var remoteOnlyModes = new[] { "Always Disable", "Always Enable", "Mobile Only" };
                    IntPopup(_remoteOnlyModeProp, remoteOnlyModes, "Remote Only Mode");
                }
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
            }
        }

        private class ModularAvatarIntegrationInspector : SettingsInspectorBase
        {
            private readonly SerializedProperty _modularAvatarIntegrationProp;

            public ModularAvatarIntegrationInspector(SerializedObject so) : base(so)
            {
                _modularAvatarIntegrationProp = so.FindProperty("useModularAvatarIntegration");
            }

            public override void DrawGUI()
            {
#if WITH_MODULAR_AVATAR
                if (!IsDestructive) { return; }
                EditorGUILayout.LabelField("Modular Avatar Integration", EditorStyles.boldLabel);
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(
                    _modularAvatarIntegrationProp, new GUIContent("Enable Legacy Modular Avatar Integration"));
                if (_modularAvatarIntegrationProp.boolValue)
                {
                    EditorGUILayout.HelpBox(
                        "Legacy Modular Avatar integration is deprecated. Please consider to use non-destructive setup with Modular Avatar >= 1.8.0.",
                        MessageType.Info);
                }
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
#endif
            }
        }

        // Sub-inspectors
        private IEnumerable<SettingsInspectorBase> _inspectors;

        // Cache
        private bool _isDefaultPrefabInstance;

        private void GenerateMarkers(GameObject avatar)
        {
            var so = serializedObject;
            var overrideDroneControllerProp = so.FindProperty("overrideDroneController");
            var overrideRepositionOriginProp = so.FindProperty("overrideRepositionOrigin");
            var overrideSelfieMarkersProp = so.FindProperty("overrideSelfieMarkers");
            var screenTouchersProp = so.FindProperty("screenTouchers");
            var droneControllerProp = so.FindProperty("droneController");
            var repositionOriginProp = so.FindProperty("repositionOrigin");
            var selfieMarkerLeftProp = so.FindProperty("selfieMarkerLeft");
            var selfieMarkerRightProp = so.FindProperty("selfieMarkerRight");

            var hasScreenToucher = false;
            for (var i = 0; i < screenTouchersProp.arraySize; ++i)
            {
                var item = screenTouchersProp.GetArrayElementAtIndex(i);
                if (item.objectReferenceValue != null) hasScreenToucher = true;
            }
            if (!hasScreenToucher)
            {
                var markers = MarkerGenerator.GenerateScreenToucher(avatar);
                var index = 0;
                screenTouchersProp.ClearArray();
                foreach (var marker in markers)
                {
                    screenTouchersProp.InsertArrayElementAtIndex(index);
                    var prop = screenTouchersProp.GetArrayElementAtIndex(index);
                    prop.objectReferenceValue = marker;
                    ++index;
                }
            }
            if (!overrideDroneControllerProp.boolValue || droneControllerProp.objectReferenceValue == null)
            {
                overrideDroneControllerProp.boolValue = true;
                droneControllerProp.objectReferenceValue = MarkerGenerator.GenerateDroneController(avatar);
            }
            if (!overrideRepositionOriginProp.boolValue || repositionOriginProp.objectReferenceValue == null)
            {
                overrideRepositionOriginProp.boolValue = true;
                repositionOriginProp.objectReferenceValue = MarkerGenerator.GenerateRepositionOrigin(avatar);
            }
            if (!overrideSelfieMarkersProp.boolValue ||
                selfieMarkerLeftProp.objectReferenceValue == null ||
                selfieMarkerRightProp.objectReferenceValue == null)
            {
                var pair = MarkerGenerator.GenerateEyeMarkers(
                    avatar,
                    !overrideSelfieMarkersProp.boolValue || selfieMarkerLeftProp.objectReferenceValue == null,
                    !overrideSelfieMarkersProp.boolValue || selfieMarkerRightProp.objectReferenceValue == null);
                overrideSelfieMarkersProp.boolValue = true;
                if (pair[0] != null) selfieMarkerLeftProp.objectReferenceValue = pair[0];
                if (pair[1] != null) selfieMarkerRightProp.objectReferenceValue = pair[1];
            }
            so.ApplyModifiedProperties();
        }

        private void FixPlayableLayers(VirtualLensSettings settings)
        {
            // Check isHuman
            var animator = settings.avatar.GetComponent<Animator>();
            if (!animator || !animator.avatar || !animator.avatar.isHuman)
            {
                return;
            }
            // Check number of FX layers
            var descriptor = settings.avatar.GetComponent<VRCAvatarDescriptor>();
            var numFXLayers = descriptor.baseAnimationLayers.Count(
                layer => layer.type == VRCAvatarDescriptor.AnimLayerType.FX);
            if (numFXLayers == 1)
            {
                return;
            }
            // Fix layer types
            // https://feedback.vrchat.com/sdk-bug-reports/p/sdk202009250008-switching-a-rig-from-generic-to-the-humanoid-rig-type-causes-dup
            descriptor.baseAnimationLayers[3].type = VRCAvatarDescriptor.AnimLayerType.Action;
            descriptor.baseAnimationLayers[4].type = VRCAvatarDescriptor.AnimLayerType.FX;
        }

        private void ApplyDestructive(SerializedObject so)
        {
            // Create artifacts folder if required
            var folderProp = so.FindProperty("artifactsFolder");
            if (string.IsNullOrEmpty(folderProp.stringValue))
            {
                // VirtualLens2/Artifacts/Empty
                folderProp.stringValue = ArtifactsFolder
                    .FromEmptyFile("5f0e868d5be4749409cbf0a4719ec55a")
                    .Path;
                so.ApplyModifiedProperties();
            }

            // Get component properties as VirtualLensSettings
            var rawSettings = so.targetObject as VirtualLensSettings;
            if (rawSettings == null)
            {
                Debug.LogError("Failed to convert component to VirtualLensSettings.");
                return;
            }

            // Create markers if required
            GenerateMarkers(rawSettings.avatar);
            FixPlayableLayers(rawSettings);

            // Apply settings
            var settings = new ImplementationSettings(rawSettings);
            Applier.Apply(settings);
        }

        private void Remove(VirtualLensSettings rawSettings)
        {
            var settings = new ImplementationSettings(rawSettings);
            Applier.Remove(
                settings.Avatar, settings.CameraRoot, settings.CameraNonPreviewRoot,
                settings.ArtifactsFolder);
        }

        private void OnEnable()
        {
            _inspectors = new SettingsInspectorBase[]
            {
                new GeneralSettingsInspector(serializedObject),
                new CameraObjectInspector(serializedObject),
                new MarkerObjectsInspector(serializedObject),
                new FocalLengthInspector(serializedObject),
                new ApertureInspector(serializedObject),
                new ManualFocusingInspector(serializedObject),
                new ExposureInspector(serializedObject),
                new DroneInspector(serializedObject),
                new ClippingPlanesInspector(serializedObject),
                new OptionalObjectsInspector(serializedObject),
                new HideableMeshesInspector(serializedObject),
                new QuickCallsInspector(serializedObject),
                new DisplaySettingsInspector(serializedObject),
                new AutoFocusSettingsInspector(serializedObject),
                new AlgorithmSettingsInspector(serializedObject),
                new AdvancedSettingsInspector(serializedObject),
                new ModularAvatarIntegrationInspector(serializedObject),
            };
            var settings = (VirtualLensSettings)target;
            _isDefaultPrefabInstance = SettingsPrefabMigrator.IsInstanceOfDefaultPrefab(settings.gameObject);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SettingsMigrator.Migrate(serializedObject);

            foreach (var inspector in _inspectors) inspector.DrawGUI();
            serializedObject.ApplyModifiedProperties();

            var settings = (VirtualLensSettings)serializedObject.targetObject;
            var valid = true;
            foreach (var msg in SettingsValidator.Validate(settings))
            {
                if (msg.Type == MessageType.Error) valid = false;
                EditorGUILayout.HelpBox(msg.Text, msg.Type);
            }
            EditorGUILayout.Space();

            if (settings.buildMode == BuildMode.Destructive)
            {
                using (new EditorGUI.DisabledScope(!valid))
                {
                    var label = settings.useModularAvatarIntegration ? "Apply as a Modular Avatar Module" : "Apply";
                    if (GUILayout.Button(label))
                    {
                        ApplyDestructive(serializedObject);
                    }
                }
                EditorGUILayout.Space();
                if (GUILayout.Button("Remove Autogenerated Objects"))
                {
                    Remove(settings);
                }
                EditorGUILayout.Space();
            }

            if (!EditorApplication.isPlaying && !_isDefaultPrefabInstance)
            {
                EditorGUILayout.Space();
                const string message =
                    "This object is not an instance of DefaultSettings prefab or its variant. " +
                    "It is recommended to replace by an instance of the prefab for future compatibility.";
                EditorGUILayout.HelpBox(message, MessageType.Info);
                if (GUILayout.Button("Replace by Prefab Instance"))
                {
                    SettingsPrefabMigrator.ReplaceByDefaultPrefabInstance(settings.gameObject);
                }
            }
        }
    }
}
