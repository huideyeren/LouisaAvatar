using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace VirtualLens2
{
    [DisallowMultipleComponent]
    [AddComponentMenu("VirtualLens2/VirtualLens Settings")]
    [HelpURL("https://vlens2.logilabo.dev/")]
    public class VirtualLensSettings : MonoBehaviour, IEditorOnly
    {
        [Serializable]
        public class OptionalObject
        {
            public GameObject gameObject = null;
            public bool defaultState = false;
        }

        [Serializable]
        public class HideableObject
        {
            public GameObject gameObject = null;
        }

        [Serializable]
        public class ShortcutEntry
        {
            public string name = "";
            public bool hasFocal = false;
            public float focal = 0.0f;
            public bool hasAperture = false;
            public float aperture = 0.0f;
            public bool hasExposure = false;
            public float exposure = 0.0f;
        }

        [Serializable]
        public class CustomGrid
        {
            public string name = "";
            public Texture2D texture = null;
        }

    [SerializeField] public int lastVersion = 0;

        [SerializeField] public BuildMode buildMode = BuildMode.Destructive;

        // [Destructive only] Target avatar
        [SerializeField] public GameObject avatar = null;

        // Camera object
        [SerializeField] public GameObject cameraObject = null;
        [SerializeField] public GameObject cameraDroppable = null;

        // Marker Objects
        [SerializeField] public bool overrideDroneController = false;
        [SerializeField] public bool overrideRepositionOrigin = false;
        [SerializeField] public bool overrideSelfieMarkers = false;
        [SerializeField] public List<GameObject> screenTouchers = new List<GameObject>();
        [SerializeField] public GameObject droneController = null;
        [SerializeField] public GameObject repositionOrigin = null;
        [SerializeField] public GameObject selfieMarkerLeft = null;
        [SerializeField] public GameObject selfieMarkerRight = null;

        // Focal length
        [SerializeField] public float minFocalLength = 12.0f;
        [SerializeField] public float maxFocalLength = 300.0f;
        [SerializeField] public float defaultFocalLength = 50.0f;
        [SerializeField] public bool synchronizeFocalLength = false;

        // Aperture
        [SerializeField] public bool enableBlurring = true;
        [SerializeField] public float minFNumber = 1.0f;
        [SerializeField] public float maxFNumber = 22.0f;
        [SerializeField] public float defaultFNumber = 22.0f;
        [SerializeField] public bool synchronizeFNumber = false;

        // Manual focusing
        [SerializeField] public bool enableManualFocusing = true;
        [SerializeField] public float minFocusDistance =  0.5f;
        [SerializeField] public float maxFocusDistance = 40.0f;
        [SerializeField] public bool synchronizeFocusDistance = false;

        // Exposure
        [SerializeField] public bool enableExposure = true;
        [SerializeField] public float exposureRange = 3.0f;
        [SerializeField] public float defaultExposure = 0.0f;
        [SerializeField] public bool synchronizeExposure = false;

        // Drone
        [SerializeField] public bool enableDrone = true;
        [SerializeField] public float droneLinearSpeed = 1.0f;
        [SerializeField] public float droneLinearDeadZone = 0.02f;
        [SerializeField] public float droneYawSpeed = 4.0f;

        // Clipping planes
        [SerializeField] public float clippingNear = 0.05f;
        [SerializeField] public float clippingFar = 5000.0f;

        // Optional objects
        [SerializeField] public List<OptionalObject> optionalObjects = new List<OptionalObject>();
        
        // Hideable meshes
        [SerializeField] public List<HideableObject> hideableObjects = new List<HideableObject>();

        // Quick calls
        [SerializeField] public List<ShortcutEntry> shortcuts = new List<ShortcutEntry>();

        // Display settings
        [SerializeField] public GridType initialGridType = GridType.None;
        [SerializeField] public List<CustomGrid> customGrids = new List<CustomGrid>();
        [SerializeField] public bool initialInformation = false;
        [SerializeField] public bool initialLevel = false;
        [SerializeField] public PeakingMode initialPeakingMode = PeakingMode.None;

        // Auto focusing
        [SerializeField] public AutoFocusMode initialAutoFocusMode = AutoFocusMode.Point;
        [SerializeField] public AutoFocusTrackingSpeed initialTrackingSpeed = AutoFocusTrackingSpeed.Medium;
        [SerializeField] public FocusingSpeed initialAutoFocusSpeed = FocusingSpeed.Immediate;

        // Depth-of-field algorithm
        [SerializeField] public bool useHighResolution = false;
        [SerializeField] public int logiBokehMaxBlurriness = 9;
        [SerializeField] public int logiBokehAntialiasMethod = 0;
        [SerializeField] public int logiBokehRealtimeAntialiasMethod = 0;
        [SerializeField] public int logiBokehMSAASamples = 2;

        // Advanced settings
        [SerializeField] public string artifactsFolder = "";
        [SerializeField] public bool clearArtifacts = true;
        [SerializeField] public float touchableThickness = 1.0f;
        [SerializeField] public WriteDefaultsOverrideMode writeDefaults = WriteDefaultsOverrideMode.Auto;
        [SerializeField] public int depthEnablerMode = 0;
        [SerializeField] public Transform externalPoseSource = null;
        [SerializeField] public RemoteOnlyMode remoteOnlyMode = RemoteOnlyMode.MobileOnly;
        
        // Modular Avatar integration
        [SerializeField] public bool useModularAvatarIntegration = false;
        
        // Obsolete in v2.6.0. Kept for migration.
        [SerializeField] public List<GameObject> droppableObjects = null;
        [SerializeField] public GameObject stabilizerTarget = null;
        
        // Obsolete in v2.7.0. Kept for migration.
        [SerializeField] public bool initialGrid = false;
        
        // Obsolete in v2.8.0. Kept for migration.
        [SerializeField] public int highResolutionMSAA = 2; // KinoBokeh configurations
        
        // Obsolete in v2.10.0 Kept for migration.
        [SerializeField] public int algorithm = 0;
    }

}
