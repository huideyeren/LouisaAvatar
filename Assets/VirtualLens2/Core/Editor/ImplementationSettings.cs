using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace VirtualLens2
{
    internal class ImplementationSettings
    {
        // Build Mode
        public BuildMode BuildMode { get; }

        // Target Avatar
        public GameObject Avatar { get; }
        public GameObject MenuInstallerObject { get; }

        // Camera Object
        public GameObject CameraRoot { get; }
        public GameObject CameraNonPreviewRoot { get; }
        
        // Marker Objects
        public IImmutableList<GameObject> ScreenTouchers { get; set; }
        public GameObject DroneController { get; set; }
        public GameObject RepositionOrigin { get; set; }
        public GameObject SelfieMarkerLeft { get; set; }
        public GameObject SelfieMarkerRight { get; set; }
        
        // Focal Length
        public float FocalLengthMin { get; }
        public float FocalLengthMax { get; }
        public float FocalLengthDefault { get; }
        public bool FocalLengthSyncRemote { get; }
        
        // Aperture
        public bool ApertureEnabled { get; }
        public float ApertureFNumberMin { get; }
        public float ApertureFNumberMax { get; }
        public float ApertureFNumberDefault { get; }
        public bool ApertureFNumberSyncRemote { get; }
        
        // Manual Focusing
        public bool ManualFocusingEnabled { get; }
        public float ManualFocusingDistanceMin { get; }
        public float ManualFocusingDistanceMax { get; }
        public bool ManualFocusingDistanceSyncRemote { get; }
        
        // Exposure Compensation
        public bool ExposureEnabled { get; }
        public float ExposureRange { get; }
        public float ExposureDefault { get; }
        public bool ExposureSyncRemote { get; }
        
        // Drone
        public bool DroneEnabled { get; }
        public float DroneLinearSpeedScale { get; }
        public float DroneLinearDeadZoneSize { get; }
        public float DroneYawSpeedScale { get; }
        
        // Clipping Planes
        public float ClippingNear { get; }
        public float ClippingFar { get; }

        // Optional Objects
        internal class OptionalObject
        {
            public GameObject GameObject { get; }
            public bool DefaultState { get; }

            public OptionalObject(GameObject gameObject, bool defaultState)
            {
                GameObject = gameObject;
                DefaultState = defaultState;
            }
        }

        public IImmutableList<OptionalObject> OptionalObjects { get; }
        
        // Hideable Meshes
        public IImmutableList<GameObject> HideableMeshes { get; }
        
        // Quick Calls
        internal class QuickCallEntry
        {
            public string Name { get; }
            public float? Focal { get; }
            public float? Aperture { get; }
            public float? Exposure { get; }

            public QuickCallEntry(string name, float? focal, float? aperture, float? exposure)
            {
                Name = name;
                Focal = focal;
                Aperture = aperture;
                Exposure = exposure;
            }
        }

        public IImmutableList<QuickCallEntry> QuickCalls { get; }
        
        // Display Settings
        internal class CustomGridEntry
        {
            public string Name { get; }
            public Texture2D Texture { get; }

            public CustomGridEntry(string name, Texture2D texture)
            {
                Name = name;
                Texture = texture;
            }
        }

        public GridType DefaultGrid { get; }
        public IImmutableList<CustomGridEntry> CustomGrids { get; }
        public bool DefaultInformation { get; }
        public bool DefaultLevelMeter { get; }
        public PeakingMode DefaultPeakingMode { get; }
        
        // Default Auto Focus Settings
        public AutoFocusMode DefaultAutoFocusMode { get; }
        public AutoFocusTrackingSpeed DefaultAutoFocusTrackingSpeed { get; }
        public FocusingSpeed DefaultFocusingSpeed { get; }
        
        // Algorithm Settings
        public bool UseHighResolution { get; } 
        public int MaxBlurriness { get; }
        public int MSAASamples { get; }
        public PostAntialiasingMode AntialiasingMode { get; }
        public PostAntialiasingMode RealtimeAntialiasingMode { get; }

        // Advanced Settings
        public string ArtifactsFolder { get; }
        public bool ClearArtifactsFolder { get; }
        public float TouchScreenThickness { get; }
        public WriteDefaultsOverrideMode WriteDefaults { get; }
        public bool DefaultDepthEnabler { get; }
        public Transform ExternalPoseSource { get; }
        public bool RemoteOnly { get; }

        // Legacy Modular Avatar Integration
        public bool BuildAsModule { get; }

        // Helper Functions
        public List<float> ZoomFovs()
        {
            const int numZoomSteps = 100;
            var rcp = 1.0f / numZoomSteps;
            var values = new List<float>();
            var minLogZoom = Mathf.Log(FocalUtil.Focal2Zoom(FocalLengthMin));
            var maxLogZoom = Mathf.Log(FocalUtil.Focal2Zoom(FocalLengthMax));
            for (int i = 0; i <= numZoomSteps; ++i)
            {
                var x = i * rcp;
                var logZoom = minLogZoom + (maxLogZoom - minLogZoom) * x;
                values.Add(FocalUtil.AdjustFov(FocalUtil.Zoom2Fov(Mathf.Exp(logZoom))));
            }
            return values;
        }
        
        public float FocalToValue(float x)
        {
            var values = ZoomFovs();
            var n = values.Count - 1;
            var y = FocalUtil.AdjustFov(FocalUtil.Focal2Fov(x));
            if (y > values[0]) { return 0.0f; }
            for (int i = 0; i < n; ++i)
            {
                float lo = values[i], hi = values[i + 1];
                if (lo >= y && y > hi)
                {
                    return (i + (y - lo) / (hi - lo)) / n;
                }
            }
            return 1.0f;
        }
        
        public float ApertureMinParameter => Mathf.Log(ApertureFNumberMax);
        public float ApertureMaxParameter => Mathf.Log(ApertureFNumberMin);

        public float ApertureToValue(float x)
        {
            var y = Mathf.Log(x);
            return (y - ApertureMinParameter) / (ApertureMaxParameter - ApertureMinParameter);
        }
        
        public float ManualFocusingDistanceMinParameter => Mathf.Log(ManualFocusingDistanceMin);
        public float ManualFocusingDistanceMaxParameter => Mathf.Log(ManualFocusingDistanceMax);
        
        public float ExposureMinParameter => Mathf.Log(Mathf.Pow(2.0f, -ExposureRange));
        public float ExposureMaxParameter => Mathf.Log(Mathf.Pow(2.0f, ExposureRange));

        public float ExposureToValue(float x)
        {
            var y = Mathf.Log(Mathf.Pow(2.0f, x));
            return (y - ExposureMinParameter) / (ExposureMaxParameter - ExposureMinParameter);
        }

        public ImplementationSettings(VirtualLensSettings settings)
        {
            var hasSyncFlag = VrcExpressionParametersWrapper.SupportsNotSynchronizedVariables();
            var isDestructive = settings.buildMode == BuildMode.Destructive;

            // Build Mode
            BuildMode = settings.buildMode;
            // Target Avatar
            if (isDestructive)
            {
                Avatar = settings.avatar;
                MenuInstallerObject = null;
            }
            else
            {
                var descriptor = settings.GetComponentInParent<VRCAvatarDescriptor>();
                Avatar = descriptor == null ? null : descriptor.gameObject;
                MenuInstallerObject = settings.gameObject;
            }
            // Camera Object
            CameraRoot = settings.cameraObject;
            CameraNonPreviewRoot = settings.cameraDroppable;
            // Marker Objects
            ScreenTouchers = settings.screenTouchers.ToImmutableList();
            DroneController = settings.overrideDroneController ? settings.droneController : null;
            RepositionOrigin = settings.overrideRepositionOrigin ? settings.repositionOrigin : null;
            SelfieMarkerLeft = settings.overrideSelfieMarkers ? settings.selfieMarkerLeft : null;
            SelfieMarkerRight = settings.overrideSelfieMarkers ? settings.selfieMarkerRight : null;
            // Focal Length
            FocalLengthMin = settings.minFocalLength;
            FocalLengthMax = settings.maxFocalLength;
            FocalLengthDefault = settings.defaultFocalLength;
            FocalLengthSyncRemote = !hasSyncFlag || settings.synchronizeFocalLength;
            // Aperture
            ApertureEnabled = hasSyncFlag || settings.enableBlurring;
            ApertureFNumberMin = settings.minFNumber;
            ApertureFNumberMax = settings.maxFNumber;
            ApertureFNumberDefault = settings.defaultFNumber;
            ApertureFNumberSyncRemote = !hasSyncFlag || settings.synchronizeFNumber;
            // Manual Focusing
            ManualFocusingEnabled = hasSyncFlag || settings.enableManualFocusing;
            ManualFocusingDistanceMin = settings.minFocusDistance;
            ManualFocusingDistanceMax = settings.maxFocusDistance;
            ManualFocusingDistanceSyncRemote = !hasSyncFlag || settings.synchronizeFocusDistance;
            // Exposure Compensation
            ExposureEnabled = hasSyncFlag || settings.enableExposure;
            ExposureRange = settings.exposureRange;
            ExposureDefault = settings.defaultExposure;
            ExposureSyncRemote = !hasSyncFlag || settings.synchronizeExposure;
            // Drone
            DroneEnabled = hasSyncFlag || settings.enableDrone;
            DroneLinearSpeedScale = settings.droneLinearSpeed;
            DroneLinearDeadZoneSize = settings.droneLinearDeadZone;
            DroneYawSpeedScale = settings.droneYawSpeed;
            // Clipping Planes
            ClippingNear = settings.clippingNear;
            ClippingFar = settings.clippingFar;
            // Optional Objects
            OptionalObjects = settings.optionalObjects
                .Select(o => new OptionalObject(o.gameObject, o.defaultState))
                .ToImmutableList();
            // Hideable Meshes
            HideableMeshes = settings.hideableObjects
                .Select(o => o.gameObject)
                .ToImmutableList();
            // Quick Calls
            QuickCalls = settings.shortcuts
                .Select(e => new QuickCallEntry(
                    e.name,
                    e.hasFocal ? (float?)e.focal : null,
                    e.hasAperture ? (float?)e.aperture : null,
                    e.hasExposure ? (float?)e.exposure : null))
                .ToImmutableList();
            // Display Settings
            DefaultGrid = settings.initialGridType;
            CustomGrids = settings.customGrids
                .Select(e => new CustomGridEntry(e.name, e.texture))
                .ToImmutableList();
            DefaultInformation = settings.initialInformation;
            DefaultLevelMeter = settings.initialLevel;
            DefaultPeakingMode = settings.initialPeakingMode;
            // Auto Focus Settings
            DefaultAutoFocusMode = settings.initialAutoFocusMode;
            DefaultAutoFocusTrackingSpeed = settings.initialTrackingSpeed;
            DefaultFocusingSpeed = settings.initialAutoFocusSpeed;
            // Algorithm Settings
            UseHighResolution = settings.useHighResolution;
            MaxBlurriness = settings.logiBokehMaxBlurriness;
            MSAASamples = settings.logiBokehMSAASamples;
            AntialiasingMode = settings.logiBokehAntialiasMethod == 0
                ? PostAntialiasingMode.FXAA
                : (PostAntialiasingMode)(settings.logiBokehAntialiasMethod - 1);
            RealtimeAntialiasingMode = settings.logiBokehRealtimeAntialiasMethod == 0
                ? PostAntialiasingMode.FXAA
                : (PostAntialiasingMode)(settings.logiBokehRealtimeAntialiasMethod - 1);
            // Advanced Settings
            ArtifactsFolder = isDestructive ? settings.artifactsFolder : null;
            ClearArtifactsFolder = isDestructive && settings.clearArtifacts;
            TouchScreenThickness = settings.touchableThickness;
            WriteDefaults = settings.writeDefaults;
            DefaultDepthEnabler = settings.depthEnablerMode != 0;
            ExternalPoseSource = settings.externalPoseSource;
            if (isDestructive || settings.remoteOnlyMode == RemoteOnlyMode.ForceDisable)
            {
                RemoteOnly = false;
            }
            else if (settings.remoteOnlyMode == RemoteOnlyMode.ForceEnable)
            {
                RemoteOnly = true;
            }
            else
            {
                var target = EditorUserBuildSettings.activeBuildTarget;
                RemoteOnly = target == BuildTarget.Android || target == BuildTarget.iOS;
            }
            // Legacy Modular Avatar Integration
#if WITH_MODULAR_AVATAR
            BuildAsModule = isDestructive && settings.useModularAvatarIntegration;
#else
            BuildAsModule = false;
#endif
        }
    }
}
