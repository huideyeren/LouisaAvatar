using System;
using UnityEditor;
using UnityEngine;
using VirtualLens2.AV3EditorLib;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;

#if WITH_MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif

namespace VirtualLens2
{
    internal static class ExpressionParametersGenerator
    {
        private const string ParameterPrefix = Constants.ParameterPrefix;

        private static VRCExpressionParameters GetOrCreateParameters(
            ImplementationSettings settings, ArtifactsFolder folder)
        {
            var descriptor = settings.Avatar.GetComponent<VRCAvatarDescriptor>();
            if (descriptor.expressionParameters) { return descriptor.expressionParameters; }
            // VirtualLens2/Core/Expressions/DefaultParameters.asset
            var parameters = Object.Instantiate(
                AssetUtility.LoadAssetByGUID<VRCExpressionParameters>("0af21823883325441ac6fdaa3ca5d41d"));
            folder.CreateAsset(parameters);
            descriptor.expressionParameters = parameters;
            return parameters;
        }

        public static void Generate(ImplementationSettings settings, GeneratedObjectSet objectSet,
            ArtifactsFolder folder)
        {
            Clear(settings);
            if (settings.BuildAsModule || settings.BuildMode == BuildMode.NonDestructive)
            {
                UpdateModularAvatarParameters(settings, objectSet.VirtualLensRoot);
            }
            else
            {
                UpdateNativeParameters(settings, folder);
            }
        }

        private static void UpdateNativeParameters(ImplementationSettings settings, ArtifactsFolder folder)
        {
            var ps = GetOrCreateParameters(settings, folder);
            Undo.RecordObject(ps, "Update expression parameters");
            var wrapper = new VrcExpressionParametersWrapper(ps);
            wrapper.AddParameter(ParameterPrefix + "Control", VRCExpressionParameters.ValueType.Int, false);
            wrapper.AddParameter(
                ParameterPrefix + "Zoom", VRCExpressionParameters.ValueType.Float, false,
                0.0f, settings.FocalLengthSyncRemote);
            if (settings.ApertureEnabled)
            {
                wrapper.AddParameter(
                    ParameterPrefix + "Aperture", VRCExpressionParameters.ValueType.Float, false,
                    0.0f, settings.ApertureFNumberSyncRemote);
            }
            if (settings.ApertureEnabled && settings.ManualFocusingEnabled)
            {
                wrapper.AddParameter(
                    ParameterPrefix + "Distance", VRCExpressionParameters.ValueType.Float, false,
                    0.0f, settings.ManualFocusingDistanceSyncRemote);
            }
            if (settings.ExposureEnabled)
            {
                wrapper.AddParameter(
                    ParameterPrefix + "Exposure", VRCExpressionParameters.ValueType.Float, false,
                    0.0f, settings.ExposureSyncRemote);
            }
            if (settings.DroneEnabled)
            {
                wrapper.AddParameter(
                    ParameterPrefix + "X", VRCExpressionParameters.ValueType.Float, false,
                    0.0f, false);
            }
            if (VrcExpressionParametersWrapper.SupportsNotSynchronizedVariables())
            {
                // Expose internal parameters
                void AddIntParameter(string name)
                {
                    wrapper.AddParameter(
                        ParameterPrefix + name, VRCExpressionParameters.ValueType.Int,
                        false, 0, false);
                }

                void AddFloatParameter(string name)
                {
                    wrapper.AddParameter(
                        ParameterPrefix + name, VRCExpressionParameters.ValueType.Float,
                        false, 0, false);
                }

                AddIntParameter("Enable");
                AddIntParameter("AltMesh");
                AddIntParameter("PositionMode");
                AddIntParameter("AutoLeveler");
                AddIntParameter("Stabilizer");
                AddIntParameter("RepositionScale");
                AddIntParameter("LoadPin");
                AddIntParameter("StorePin");
                for (var i = 1; i <= Constants.NumPins; ++i)
                {
                    AddIntParameter($"ExistPin{i}");
                }
                AddIntParameter("ExternalPose");
                AddIntParameter("AFMode");
                AddIntParameter("AFSpeed");
                AddIntParameter("TrackingSpeed");
                AddIntParameter("FocusLock");
                AddIntParameter("Grid");
                AddIntParameter("Information");
                AddIntParameter("Level");
                AddIntParameter("Peaking");
                AddIntParameter("Hide");
                AddIntParameter("FarPlane");
                AddIntParameter("DepthEnabler");
                AddIntParameter("LocalPlayerMask");
                AddIntParameter("RemotePlayerMask");
                AddIntParameter("UIMask");
                AddIntParameter("PreviewHUD");
                
                // Expose configuration parameters
                AddIntParameter("Version");
                AddFloatParameter("Zoom Min");
                AddFloatParameter("Zoom Max");
                AddFloatParameter("Aperture Min");
                AddFloatParameter("Aperture Max");
                AddFloatParameter("Exposure Range");
                AddIntParameter("Resolution X");
                AddIntParameter("Resolution Y");
            }
            wrapper.Cleanup();
            EditorUtility.SetDirty(ps);
        }

        private static void UpdateModularAvatarParameters(ImplementationSettings settings, GameObject obj)
        {
#if WITH_MODULAR_AVATAR
            var component = obj.GetComponent<ModularAvatarParameters>();
            if (component != null)
            {
                Undo.RecordObject(component, "Update modular avatar parameters");
            }
            else
            {
                component = obj.AddComponent<ModularAvatarParameters>();
                Undo.RegisterCreatedObjectUndo(component, "Update modular avatar parameters");
            }
            var so = new SerializedObject(component);
            var parameters = so.FindProperty("parameters");

            void AddParameter(string name, ParameterSyncType type, bool local)
            {
                var index = parameters.arraySize;
                parameters.InsertArrayElementAtIndex(index);
                var elem = parameters.GetArrayElementAtIndex(index);
                elem.FindPropertyRelative("nameOrPrefix").stringValue = ParameterPrefix + name;
                elem.FindPropertyRelative("remapTo").stringValue = ParameterPrefix + name;
                elem.FindPropertyRelative("defaultValue").floatValue = 0.0f;
                elem.FindPropertyRelative("internalParameter").boolValue = false;
                elem.FindPropertyRelative("isPrefix").boolValue = false;
                elem.FindPropertyRelative("saved").boolValue = false;
                elem.FindPropertyRelative("syncType").enumValueIndex = (int)type;
                elem.FindPropertyRelative("localOnly").boolValue = local;
            }

            AddParameter("Control", ParameterSyncType.Int, false);
            AddParameter("Zoom", ParameterSyncType.Float, !settings.FocalLengthSyncRemote);
            if (settings.ApertureEnabled)
            {
                AddParameter("Aperture", ParameterSyncType.Float, !settings.ApertureFNumberSyncRemote);
            }
            if (settings.ApertureEnabled && settings.ManualFocusingEnabled)
            {
                AddParameter("Distance", ParameterSyncType.Float, !settings.ManualFocusingDistanceSyncRemote);
            }
            if (settings.ExposureEnabled)
            {
                AddParameter("Exposure", ParameterSyncType.Float, !settings.ExposureSyncRemote);
            }
            AddParameter("X", ParameterSyncType.Float, true);

            if (VrcExpressionParametersWrapper.SupportsNotSynchronizedVariables())
            {
                // Expose internal parameters
                void AddIntParameter(string name)
                {
                    AddParameter(name, ParameterSyncType.Int, true);
                }

                void AddFloatParameter(string name)
                {
                    AddParameter(name, ParameterSyncType.Float, true);
                }

                AddIntParameter("Enable");
                AddIntParameter("AltMesh");
                AddIntParameter("PositionMode");
                AddIntParameter("AutoLeveler");
                AddIntParameter("Stabilizer");
                AddIntParameter("RepositionScale");
                AddIntParameter("LoadPin");
                AddIntParameter("StorePin");
                for (var i = 1; i <= Constants.NumPins; ++i)
                {
                    AddIntParameter($"ExistPin{i}");
                }
                AddIntParameter("ExternalPose");
                AddIntParameter("AFMode");
                AddIntParameter("AFSpeed");
                AddIntParameter("TrackingSpeed");
                AddIntParameter("FocusLock");
                AddIntParameter("Grid");
                AddIntParameter("Information");
                AddIntParameter("Level");
                AddIntParameter("Peaking");
                AddIntParameter("Hide");
                AddIntParameter("FarPlane");
                AddIntParameter("DepthEnabler");
                AddIntParameter("LocalPlayerMask");
                AddIntParameter("RemotePlayerMask");
                AddIntParameter("UIMask");
                AddIntParameter("PreviewHUD");
                
                // Expose configuration parameters
                AddIntParameter("Version");
                AddFloatParameter("Zoom Min");
                AddFloatParameter("Zoom Max");
                AddFloatParameter("Aperture Min");
                AddFloatParameter("Aperture Max");
                AddFloatParameter("Exposure Range");
                AddIntParameter("Resolution X");
                AddIntParameter("Resolution Y");
            }
            so.ApplyModifiedProperties();
#else
            throw new ApplicationException("Modular Avatar is not installed for this project.");
#endif
        }

        public static void Clear(GameObject avatar)
        {
            if (!avatar) { return; }
            var descriptor = avatar.GetComponent<VRCAvatarDescriptor>();
            if (!descriptor) { return; }
            var parameters = descriptor.expressionParameters;
            if (!parameters) { return; }
            Undo.RecordObject(parameters, "Remove expression parameters");
            var wrapper = new VrcExpressionParametersWrapper(parameters);
            wrapper.RemoveParameters(p => p.Name.StartsWith(ParameterPrefix));
            EditorUtility.SetDirty(parameters);
        }

        private static void Clear(ImplementationSettings settings) { Clear(settings.Avatar); }
    }
}
