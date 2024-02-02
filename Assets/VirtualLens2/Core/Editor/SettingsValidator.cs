using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VirtualLens2.AV3EditorLib;

#if WITH_MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif

namespace VirtualLens2
{
    public class ValidationMessage
    {
        public MessageType Type { get; }
        public string Text { get; }

        public ValidationMessage(MessageType type, string text)
        {
            Type = type;
            Text = text;
        }
    }

    public class ValidationMessageList : IEnumerable<ValidationMessage>
    {
        private readonly List<ValidationMessage> _messages = new List<ValidationMessage>();

        public void Error(string text) { _messages.Add(new ValidationMessage(MessageType.Error, text)); }

        public void Warning(string text) { _messages.Add(new ValidationMessage(MessageType.Warning, text)); }

        public void Info(string text) { _messages.Add(new ValidationMessage(MessageType.Info, text)); }

        public IEnumerator<ValidationMessage> GetEnumerator() { return _messages.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }

    public class SettingsValidator
    {
        private const string ParameterPrefix = Constants.ParameterPrefix;

        private readonly ValidationMessageList _messages;
        private readonly ImplementationSettings _settings;

        private bool CheckUnityVersion()
        {
            var version = Application.unityVersion.Split('.');
            if (version.Length == 0 || !int.TryParse(version[0], out var year) || year < 2019)
            {
                _messages.Error("Current Unity version is no longer supported. Please upgrade your project.");
                return false;
            }
            return true;
        }

        private bool CheckAvatar()
        {
            var avatar = _settings.Avatar;
            if (avatar == null)
            {
                _messages.Error("Avatar is not selected.");
                return false;
            }
            var descriptor = avatar.GetComponent<VRCAvatarDescriptor>();
            if (descriptor == null)
            {
                _messages.Error("Avatar must have VRC Avatar Descriptor.");
                return false;
            }
            var animator = avatar.GetComponent<Animator>();
            if (animator != null && animator.avatar != null && animator.avatar.isHuman) return true;
            _messages.Error("Avatar must be a humanoid.");
            return false;
        }

        private void CheckCameraObjects()
        {
            if (_settings.CameraRoot == null)
            {
                _messages.Error($"Camera Root Object is not selected.");
            }
            if (!HierarchyUtility.IsDescendant(_settings.Avatar, _settings.CameraRoot))
            {
                _messages.Error($"Camera Root Object is not in the avatar.");
            }
            if (_settings.CameraNonPreviewRoot == null)
            {
                _messages.Error($"Camera Non-Preview Root is not selected.");
            }
            if (!HierarchyUtility.IsDescendant(_settings.CameraRoot, _settings.CameraNonPreviewRoot))
            {
                _messages.Error($"Camera Non-Preview Root is not in the camera.");
            }
        }

        private void CheckMarkers()
        {
            if (!MarkerDetector.DetectOrigin(_settings.CameraRoot))
            {
                _messages.Error("VirtualLensOrigin is not found.");
            }
            if (!MarkerDetector.DetectPreview(_settings.CameraRoot))
            {
                _messages.Warning("VirtualLensPreview is not found.");
            }
            foreach (var item in _settings.ScreenTouchers)
            {
                if (item != null && !HierarchyUtility.IsDescendant(_settings.Avatar, item))
                {
                    _messages.Error($"{item.name} is not in the avatar.");
                }
            }
            if (_settings.DroneController != null &&
                !HierarchyUtility.IsDescendant(_settings.Avatar, _settings.DroneController))
            {
                _messages.Error($"{_settings.DroneController.name} is not in the avatar.");
            }
            if (_settings.RepositionOrigin != null &&
                !HierarchyUtility.IsDescendant(_settings.Avatar, _settings.RepositionOrigin))
            {
                _messages.Error($"{_settings.RepositionOrigin.name} is not in the avatar.");
            }
            if (_settings.SelfieMarkerLeft != null &&
                !HierarchyUtility.IsDescendant(_settings.Avatar, _settings.SelfieMarkerLeft))
            {
                _messages.Error($"{_settings.SelfieMarkerLeft.name} is not in the avatar.");
            }
            if (_settings.SelfieMarkerRight != null &&
                !HierarchyUtility.IsDescendant(_settings.Avatar, _settings.SelfieMarkerRight))
            {
                _messages.Error($"{_settings.SelfieMarkerRight.name} is not in the avatar.");
            }
        }

        private void CheckEyeBones()
        {
            var avatar = _settings.Avatar;
            var animator = avatar.GetComponent<Animator>();
            var leftEye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
            var rightEye = animator.GetBoneTransform(HumanBodyBones.RightEye);
            if (leftEye == null || rightEye == null)
            {
                _messages.Warning("Avatar has no eye bones. Selfie focus will not work.");
            }
        }

        private void CheckPathUniqueness()
        {
            var root = _settings.Avatar.transform;

            // Construct actual parent-child relationship to care MA Bone Proxy
            var parentMap = new Dictionary<Transform, Transform>();
            var childrenMap = new Dictionary<Transform, Dictionary<string, List<Transform>>>();

            void ConstructParentMap(Transform tf)
            {
                foreach (Transform child in tf)
                {
                    var parent = tf;
#if WITH_MODULAR_AVATAR
                    var boneProxy = child.GetComponent<ModularAvatarBoneProxy>();
                    if (boneProxy != null && boneProxy.target != null) { parent = boneProxy.target; }
#endif
                    parentMap.Add(child, parent);
                    if (childrenMap.TryGetValue(parent, out var nameMap))
                    {
                        if (nameMap.TryGetValue(child.name, out var childrenList))
                        {
                            childrenList.Add(child);
                        }
                        else
                        {
                            nameMap.Add(child.name, new List<Transform> { child });
                        }
                    }
                    else
                    {
                        childrenMap.Add(parent,
                            new Dictionary<string, List<Transform>> { { child.name, new List<Transform> { child } } });
                    }
                    ConstructParentMap(child);
                }
            }

            ConstructParentMap(root);

            // Enumerate objects manipulated by VirtualLens2 animations
            var leaves = new List<Transform>();
            if (_settings.CameraRoot) { leaves.Add(_settings.CameraRoot.transform); }
            if (_settings.CameraNonPreviewRoot) { leaves.Add(_settings.CameraNonPreviewRoot.transform); }
            leaves.AddRange(_settings.HideableMeshes
                .Where(obj => obj != null)
                .Select(obj => obj.transform));
            leaves.AddRange(_settings.OptionalObjects
                .Select(elem => elem?.GameObject.transform));
            leaves.AddRange(_settings.ScreenTouchers
                .Where(obj => obj != null)
                .Select(obj => obj.transform));

            // Check path uniqueness
            var errors = new Dictionary<Transform, List<Transform>>();
            foreach (var leaf in leaves)
            {
                if (leaf == null) { continue; }
                if (!HierarchyUtility.IsDescendant(root, leaf))
                {
                    _messages.Error($"'{leaf.name}' is not in the avatar.");
                    continue;
                }
                var visited = new HashSet<Transform>();
                var cur = leaf;
                while (cur != root && !visited.Contains(cur))
                {
                    var parent = parentMap[cur];
                    visited.Add(cur);
                    var candidates = childrenMap[parent][cur.name];
                    if (candidates.Count >= 2 && !errors.ContainsKey(cur)) { errors.Add(cur, candidates); }
                    cur = parent;
                }
            }
            foreach (var elem in errors)
            {
                var path = HierarchyUtility.RelativePath(root, elem.Key);
                var listText = string.Join("\n", elem.Value
                    .Select(tf => $"- {HierarchyUtility.RelativePath(root, tf)}"));
                _messages.Error(
                    $"Object path '{path}' will not be unique. Please rename or remove objects to fix duplicate. It will conflict with following objects:\n{listText}");
            }
        }

        private void CheckExpressionMenu()
        {
            var descriptor = _settings.Avatar.GetComponent<VRCAvatarDescriptor>();
            var menu = descriptor.expressionsMenu;
            if (menu == null) { return; }

            const int maxN = 8;
            int n = menu.controls.Count;
            foreach (var item in menu.controls)
            {
                if (item.name == "VirtualLens2") { --n; }
                if (item.name == "VirtualLens2 Quick Calls") { --n; }
            }
            ++n;
            if (n > maxN)
            {
                _messages.Error("There are no enough menu slots.");
            }
        }

        private void CheckExpressionParameters()
        {
            var descriptor = _settings.Avatar.GetComponent<VRCAvatarDescriptor>();
            var parameters = descriptor.expressionParameters;
            if (parameters == null) return;

            var wrapper = new VrcExpressionParametersWrapper(parameters);
            var costSum = wrapper.Parameters
                .Where(p => !p.Name.StartsWith(ParameterPrefix))
                .Sum(p => p.Cost);

            var forceSync = !VrcExpressionParametersWrapper.SupportsNotSynchronizedVariables();
            var intCost = VrcExpressionParametersWrapper.TypeCost(VRCExpressionParameters.ValueType.Int);
            var floatCost = VrcExpressionParametersWrapper.TypeCost(VRCExpressionParameters.ValueType.Float);
            costSum += intCost; // State
            if (_settings.FocalLengthSyncRemote || forceSync)
            {
                costSum += floatCost;
            }
            if (_settings.ApertureEnabled && (_settings.ApertureFNumberSyncRemote || forceSync))
            {
                costSum += floatCost;
            }
            if (_settings.ManualFocusingEnabled && (_settings.ManualFocusingDistanceSyncRemote || forceSync))
            {
                costSum += floatCost;
            }
            if (_settings.ExposureEnabled && (_settings.ExposureSyncRemote || forceSync))
            {
                costSum += floatCost;
            }
            if (_settings.DroneEnabled && forceSync)
            {
                costSum += floatCost;
            }
            if (costSum > VrcExpressionParametersWrapper.MaxParameterCost())
            {
                _messages.Error("Parameter cost limit will be exceeded.");
            }
        }


        private static bool IsFinite(float x) { return !(float.IsNaN(x) || float.IsInfinity(x)); }

        private static bool IsPositiveFinite(float x) { return IsFinite(x) && x > 0.0f; }

        private static bool CheckRangeMinMax(float min, float max)
        {
            if (!IsFinite(min)) { return false; }
            if (!IsFinite(max)) { return false; }
            return min < max;
        }

        private static bool CheckInRange(float x, float min, float max)
        {
            if (!IsFinite(x)) { return false; }
            return min <= x && x <= max;
        }

        private void CheckPositiveRange(string name, float min, float max, float def)
        {
            bool valid = true;
            if (!IsPositiveFinite(min))
            {
                _messages.Error($"Min {name} must be positive.");
                valid = false;
            }
            if (!IsPositiveFinite(max))
            {
                _messages.Error($"Max {name} must be positive.");
                valid = false;
            }
            if (valid && !CheckRangeMinMax(min, max))
            {
                _messages.Error($"Min {name} must be less than Max {name}.");
                valid = false;
            }
            if (valid && !CheckInRange(def, min, max))
            {
                _messages.Error($"Default {name} must be in range.");
            }
        }

        private void CheckFocalLengths()
        {
            CheckPositiveRange(
                "Focal Length",
                _settings.FocalLengthMin,
                _settings.FocalLengthMax,
                _settings.FocalLengthDefault);
        }

        private void CheckAperture()
        {
            if (!_settings.ApertureEnabled) { return; }
            CheckPositiveRange(
                "F Number",
                _settings.ApertureFNumberMin,
                _settings.ApertureFNumberMax,
                _settings.ApertureFNumberDefault);
        }

        private void CheckManualFocusing()
        {
            if (!_settings.ManualFocusingEnabled) { return; }
            CheckPositiveRange(
                "Focus Distance",
                _settings.ManualFocusingDistanceMin,
                _settings.ManualFocusingDistanceMax,
                _settings.ManualFocusingDistanceMin);
        }

        private void CheckExposure()
        {
            if (!_settings.ExposureEnabled) { return; }
            bool valid = true;
            var min = -_settings.ExposureRange;
            var max = _settings.ExposureRange;
            if (!IsPositiveFinite(max))
            {
                _messages.Error("Exposure Range must be positive.");
                valid = false;
            }
            if (valid && !CheckInRange(_settings.ExposureDefault, min, max))
            {
                _messages.Error("Default Exposure must be in range.");
            }
        }

        private void CheckDrone()
        {
            if (!_settings.DroneEnabled) { return; }
            if (!IsPositiveFinite(_settings.DroneLinearSpeedScale))
            {
                _messages.Error("Drone Speed Scale must be positive.");
            }
            if (!IsPositiveFinite(_settings.DroneLinearDeadZoneSize))
            {
                _messages.Error("Dead Zone of Drone Speed Controller must be positive.");
            }
            if (!IsPositiveFinite(_settings.DroneYawSpeedScale))
            {
                _messages.Error("Drone Yaw Speed must be positive.");
            }
            if (_settings.DroneYawSpeedScale >= 1500.0f)
            {
                _messages.Error("Drone Yaw Speed must be less than 1500.");
            }
        }

        private void CheckClippingPlanes()
        {
            bool valid = true;
            var min = _settings.ClippingNear;
            var max = _settings.ClippingFar;
            if (!IsPositiveFinite(min) || !IsPositiveFinite(max))
            {
                _messages.Error("Distance for clipping planes must be positive.");
                valid = false;
            }
            if (valid && !CheckRangeMinMax(min, max))
            {
                _messages.Error("Near Clipping Plane must be near than Far Clipping Plane.");
            }
        }

        private void CheckOptionalObjects()
        {
            var avatar = _settings.Avatar;
            foreach (var item in _settings.OptionalObjects)
            {
                var obj = item.GameObject;
                if (obj == null) { continue; }
                if (!HierarchyUtility.IsDescendant(avatar, obj))
                {
                    _messages.Error($"'{obj.name}' is not in the avatar.");
                }
            }
        }

        private void CheckHideableObjects()
        {
            var avatar = _settings.Avatar;
            foreach (var obj in _settings.HideableMeshes)
            {
                if (obj == null) { continue; }
                if (!HierarchyUtility.IsDescendant(avatar, obj))
                {
                    _messages.Error($"'{obj.name}' is not in the avatar.");
                }
                if (!obj.GetComponent<MeshRenderer>() && !obj.GetComponent<SkinnedMeshRenderer>())
                {
                    _messages.Warning($"'{obj.name}' doesn't have either Mesh Renderer or Skinned Mesh Renderer.");
                }
            }
        }

        private void CheckQuickCalls()
        {
            if (_settings.QuickCalls.Count > Constants.NumQuickCalls)
            {
                _messages.Error("There are too many quick calls.");
            }
            var minFocal = _settings.FocalLengthMin;
            var maxFocal = _settings.FocalLengthMax;
            var minAperture = _settings.ApertureFNumberMin;
            var maxAperture = _settings.ApertureFNumberMax;
            var minExposure = -_settings.ExposureRange;
            var maxExposure = _settings.ExposureRange;
            foreach (var item in _settings.QuickCalls)
            {
                var prefix = "'" + item.Name + "': ";
                if (item.Focal != null && !CheckInRange((float)item.Focal, minFocal, maxFocal))
                {
                    _messages.Error(prefix + "Focal length must be in range.");
                }
                if (item.Aperture != null && !CheckInRange((float)item.Aperture, minAperture, maxAperture))
                {
                    _messages.Error(prefix + "F number must be in range.");
                }
                if (item.Exposure != null && !CheckInRange((float)item.Exposure, minExposure, maxExposure))
                {
                    _messages.Error(prefix + "Exposure must be in range.");
                }
            }
        }

        private void CheckCustomGrids()
        {
            if(_settings.CustomGrids.Count > Constants.NumCustomGrids)
            {
                _messages.Error("There are too many quick calls.");
            }
            foreach (var item in _settings.CustomGrids)
            {
                var texture = item.Texture;
                if (texture == null)
                {
                    _messages.Error("Custom grid texture is not selected.");
                    continue;
                }
                if (texture.width != 1280 || texture.height != 720)
                {
                    _messages.Warning($"The resolution of custom grid texture {texture.name} is not 1280x720.");
                }
                var path = AssetDatabase.GetAssetPath(texture);
                if (string.IsNullOrEmpty(path)) { continue; }
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) { continue; }
                if (importer.npotScale != TextureImporterNPOTScale.None)
                {
                    _messages.Warning($"Custom grid texture {texture.name} scaled to power of 2.");
                }
                if (!importer.DoesSourceTextureHaveAlpha())
                {
                    _messages.Warning($"Custom grid texture {texture.name} does not have alpha.");
                }
                if (importer.mipmapEnabled)
                {
                    _messages.Warning($"Mipmap is enabled for custom grid texture {texture.name}.");
                }
            }
        }

        private void CheckPlayableLayers()
        {
            var descriptor = _settings.Avatar.GetComponent<VRCAvatarDescriptor>();
            var layers = descriptor.baseAnimationLayers;
            var numFXLayers = layers.Count(layer => layer.type == VRCAvatarDescriptor.AnimLayerType.FX);
            if (numFXLayers == 1) { return; }

            var isFX3 = layers.Length == 5 && layers[3].type == VRCAvatarDescriptor.AnimLayerType.FX;
            var isFX4 = layers.Length == 5 && layers[4].type == VRCAvatarDescriptor.AnimLayerType.FX;
            if (numFXLayers == 2 && isFX3 && isFX4)
            {
                // https://feedback.vrchat.com/sdk-bug-reports/p/sdk202009250008-switching-a-rig-from-generic-to-the-humanoid-rig-type-causes-dup
                _messages.Warning("The number of FX layers is wrong. VirtualLens2 will fix it automatically.");
            }
            else
            {
                // Unknown layer configuration
                _messages.Error(
                    "The number of FX layers is wrong. Please update VRCSDK and reset playable layers to default.");
            }
        }

        private AnimatorController FindFXAnimatorController()
        {
            var descriptor = _settings.Avatar.GetComponent<VRCAvatarDescriptor>();
            foreach (var layer in descriptor.baseAnimationLayers)
            {
                if (layer.type != VRCAvatarDescriptor.AnimLayerType.FX) { continue; }
                return layer.animatorController as AnimatorController;
            }
            return null;
        }

        private void CheckWriteDefaults()
        {
            if (_settings.WriteDefaults == WriteDefaultsOverrideMode.Auto) { return; }
            var expect = _settings.WriteDefaults == WriteDefaultsOverrideMode.ForceEnable;
            var controller = FindFXAnimatorController();
            if (controller == null) { return; }
            bool valid = true;
            foreach (var layer in controller.layers)
            {
                if (layer == null) { continue; }
                if (layer.name.StartsWith("VirtualLens2 ")) { continue; }
                if (layer.stateMachine == null || layer.stateMachine.states == null) { continue; }
                foreach (var state in layer.stateMachine.states)
                {
                    if (state.state.name.Contains("(WD On)")) continue;
                    if (state.state.name.Contains("(WD Off)")) continue;
                    if (state.state.writeDefaultValues != expect) valid = false;
                }
            }
            if (!valid)
            {
                _messages.Warning(
                    "Configurations of Write Defaults will be mixed. Please check or uncheck 'Enable Write Defaults' if necessary.");
            }
        }

        private void CheckDuplicatedParameters()
        {
            // https://feedback.vrchat.com/avatar-30/p/bug-vrc-avatar-parameter-driver-does-not-work-when-there-are-duplicated-paramete
            var controller = FindFXAnimatorController();
            if (controller == null) { return; }

            var history = new HashSet<string>();
            foreach (var p in controller.parameters)
            {
                if (history.Contains(p.name))
                {
                    _messages.Warning($"Duplicated parameter '{p.name}' was found. It will be removed.");
                }
                history.Add(p.name);
            }
        }

        private bool InArtifactsFolder(Object asset)
        {
            if (string.IsNullOrEmpty(_settings.ArtifactsFolder)) { return false; }
            return AssetDatabase.GetAssetPath(asset).StartsWith(_settings.ArtifactsFolder);
        }

        private bool IsMerged(AnimatorController controller)
        {
            if (controller.layers.Length == 0) { return true; }
            var baseLayer = controller.layers[0];
            if (baseLayer.stateMachine.states.Length != 0) { return true; }
            for (var i = 1; i < controller.layers.Length; ++i)
            {
                var layer = controller.layers[i];
                if (!layer.name.StartsWith(ParameterPrefix)) { return true; }
            }
            return controller.parameters
                .Any(p =>
                    p.name != "IsLocal" &&
                    p.name != "VRMode" &&
                    p.name != "TrackingType" &&
                    !p.name.StartsWith(ParameterPrefix));
        }

        private bool IsMerged(VRCExpressionsMenu menu)
        {
            return menu.controls.Any(item => item.name != "VirtualLens2");
        }

        private bool IsMerged(VRCExpressionParameters parameters)
        {
            var items = parameters.parameters;
            if (items.Length < 3) { return true; }
            if (items[0].name != "VRCEmote") { return true; }
            if (items[1].name != "VRCFaceBlendH") { return true; }
            if (items[2].name != "VRCFaceBlendV") { return true; }
            for (var i = 3; i < items.Length; ++i)
            {
                if (!items[i].name.StartsWith(ParameterPrefix)) { return true; }
            }
            return false;
        }

        private void CheckAutoGeneratedObjectsInPrefabRecur(GameObject obj, Regex regex)
        {
            if (!obj) return;
            foreach (Transform transform in obj.transform)
            {
                var child = transform.gameObject;
                if (regex.IsMatch(child.name))
                {
                    var root = PrefabUtility.GetOutermostPrefabInstanceRoot(child);
                    if (root && root != child)
                    {
                        _messages.Error(
                            $"An auto generated object is found in a prefab. Please remove '{child.name}' from the prefab '{root.name}'.");
                    }
                }
                else
                {
                    CheckAutoGeneratedObjectsInPrefabRecur(child, regex);
                }
            }
        }

        private void CheckAutoGeneratedObjectsInPrefab()
        {
            CheckAutoGeneratedObjectsInPrefabRecur(_settings.Avatar, new Regex(@"^_VirtualLens_.*$"));
        }

        private void CheckModifiedArtifacts()
        {
            var controller = FindFXAnimatorController();
            if (InArtifactsFolder(controller) && IsMerged(controller))
            {
                _messages.Warning(
                    "Your changes on FX layer will be discarded. You should move it out of artifacts folder before applying.");
            }
            var descriptor = _settings.Avatar.GetComponent<VRCAvatarDescriptor>();
            var menu = descriptor.expressionsMenu;
            if (InArtifactsFolder(menu) && IsMerged(menu))
            {
                _messages.Warning(
                    "Your changes on expressions menu will be discarded. You should move it out of artifacts folder before applying.");
            }
            var parameters = descriptor.expressionParameters;
            if (InArtifactsFolder(parameters) && IsMerged(parameters))
            {
                _messages.Warning(
                    "Your changes on expression parameters will be discarded. You should move it out of artifacts folder before applying.");
            }
        }

        private SettingsValidator(VirtualLensSettings raw)
        {
            _settings = new ImplementationSettings(raw);
            _messages = new ValidationMessageList();
            var isDestructive = _settings.BuildMode == BuildMode.Destructive;

            if (!CheckUnityVersion()) { return; }
            if (!CheckAvatar()) { return; }
            CheckPlayableLayers();
            if (isDestructive) { CheckExpressionMenu(); }
            if (isDestructive) { CheckExpressionParameters(); }
            CheckCameraObjects();
            CheckMarkers();
            CheckEyeBones();
            CheckPathUniqueness();
            CheckFocalLengths();
            CheckAperture();
            CheckManualFocusing();
            CheckExposure();
            CheckDrone();
            CheckClippingPlanes();
            CheckOptionalObjects();
            CheckHideableObjects();
            CheckQuickCalls();
            CheckCustomGrids();
            if (isDestructive) { CheckWriteDefaults(); }
            if (isDestructive) { CheckDuplicatedParameters(); }
            CheckAutoGeneratedObjectsInPrefab();
            if (isDestructive) { CheckModifiedArtifacts(); }
        }

        public static ValidationMessageList Validate(VirtualLensSettings settings)
        {
            var self = new SettingsValidator(settings);
            return self._messages;
        }
    }
}
