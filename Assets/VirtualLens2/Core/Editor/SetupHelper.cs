using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VirtualLens2.AV3EditorLib;
using VRC.SDK3.Avatars.Components;

namespace VirtualLens2
{
    public class SetupHelper : EditorWindow
    {
        // VirtualLens2/Settings
        private const string SettingsFolderGuid = "041a4038b714fbc4c9127638255c75ee";

        private static SetupHelper _dialog;

        private bool _isDestructive;

        private GameObject _avatar;
        private int _targetHand;
        private GameObject _template;
        private bool _useCustomModel;
        private GameObject _customModelPrefab;
        private bool _keepCustomModelTransform;

#if WITH_NDMF && WITH_MODULAR_AVATAR
        [MenuItem("GameObject/VirtualLens2/Non-Destructive Setup", false, 20)]
        public static void NonDestructiveInitialize()
        {
            // Select VirtualLens2 object if it exists
            var avatarObject = GetAvatarFromSelection();
            if (avatarObject != null)
            {
                var components = avatarObject
                    .GetComponentsInChildren<VirtualLensSettings>(true)
                    .Where(c => c.buildMode == BuildMode.NonDestructive)
                    .ToArray();
                if (components.Length > 0)
                {
                    Selection.activeGameObject = components[0].gameObject;
                    return;
                }
            }

            // Open configuration dialog
            if (_dialog == null)
            {
                _dialog = CreateInstance<SetupHelper>();
            }
            var ps = GetProjectSettings();
            _dialog._isDestructive = false;
            _dialog._avatar = avatarObject;
            _dialog._targetHand = ps.targetHand;
            _dialog._template = ps.settingsTemplate;
            _dialog._useCustomModel = ps.useCustomModel;
            _dialog._customModelPrefab = ps.customModelPrefab;
            _dialog._keepCustomModelTransform = ps.keepCustomModelTransform;

            _dialog.titleContent.text = "VirtualLens2 Setup (Non-Destructive)";
            _dialog.ShowUtility();
        }
#endif

        [MenuItem("GameObject/VirtualLens2/Destructive Setup (Legacy)", false, 20)]
        public static void DestructiveInitialize()
        {
            // Select VirtualLens Settings object if it exists
            var avatarObject = GetAvatarFromSelection();
            if (avatarObject != null)
            {
                GameObject Recur(GameObject cur)
                {
                    var component = cur.GetComponent<VirtualLensSettings>();
                    if (component != null && component.buildMode == BuildMode.Destructive &&
                        component.avatar == avatarObject)
                    {
                        return cur;
                    }
                    foreach (Transform t in cur.transform)
                    {
                        var ret = Recur(t.gameObject);
                        if (ret != null) { return ret; }
                    }
                    return null;
                }

                GameObject found = null;
                foreach (var root in avatarObject.scene.GetRootGameObjects())
                {
                    found = Recur(root);
                    if (found != null) { break; }
                }
                if (found != null)
                {
                    Selection.activeGameObject = found;
                    return;
                }
            }

            // Open configuration dialog
            if (_dialog == null)
            {
                _dialog = CreateInstance<SetupHelper>();
            }
            var ps = GetProjectSettings();
            _dialog._isDestructive = true;
            _dialog._avatar = avatarObject;
            _dialog._targetHand = ps.targetHand;
            _dialog._template = ps.settingsTemplate;
            _dialog._useCustomModel = ps.useCustomModel;
            _dialog._customModelPrefab = ps.customModelPrefab;
            _dialog._keepCustomModelTransform = ps.keepCustomModelTransform;

            _dialog.titleContent.text = "VirtualLens2 Setup (Destructive)";
            _dialog.ShowUtility();
        }

        private static GameObject GetAvatarFromSelection()
        {
            var active = Selection.activeGameObject;
            if (active == null) { return null; }
            var descriptor = active.GetComponentInParent<VRCAvatarDescriptor>();
            if (descriptor == null) { return null; }
            return descriptor.gameObject;
        }

        private static ProjectSettings GetProjectSettings()
        {
            var path = Path.Combine(AssetDatabase.GUIDToAssetPath(SettingsFolderGuid), "ProjectSettings.asset");
            var obj = AssetDatabase.LoadAssetAtPath<ProjectSettings>(path);
            if (obj != null) { return obj; }
            obj = CreateInstance<ProjectSettings>();
            AssetDatabase.CreateAsset(obj, path);
            return obj;
        }

        private void SaveSettings()
        {
            var settings = GetProjectSettings();
            settings.targetHand = _targetHand;
            settings.settingsTemplate = _template;
            settings.useCustomModel = _useCustomModel;
            settings.customModelPrefab = _customModelPrefab;
            settings.keepCustomModelTransform = _keepCustomModelTransform;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        private void OnGUI()
        {
            if (_dialog != this) { Close(); }
            var rect = EditorGUILayout.BeginVertical();
            _avatar = EditorGUILayout.ObjectField(
                    new GUIContent("Avatar"),
                    _avatar,
                    typeof(GameObject),
                    true)
                as GameObject;
            _targetHand = EditorGUILayout.IntPopup(
                new GUIContent("Target Hand"), _targetHand,
                new[] { new GUIContent("Right"), new GUIContent("Left") },
                new[] { 0, 1 });
            _template = EditorGUILayout.ObjectField(
                    new GUIContent("Settings Template"),
                    _template,
                    typeof(GameObject),
                    false)
                as GameObject;
            _useCustomModel = EditorGUILayout.Toggle("Use Custom Model", _useCustomModel);
            if (_useCustomModel)
            {
                _customModelPrefab = EditorGUILayout.ObjectField(
                    "Custom Model Prefab", _customModelPrefab, typeof(GameObject), false) as GameObject;
                _keepCustomModelTransform = EditorGUILayout.Toggle(
                    "Keep Root Transform", _keepCustomModelTransform);
            }
            EditorGUILayout.Space();

            var validity = ValidateSettings();
            using (new EditorGUILayout.HorizontalScope())
            {
                var isWindows = Application.platform == RuntimePlatform.WindowsEditor;
                if (!isWindows)
                {
                    if (GUILayout.Button("Cancel")) _dialog.Close();
                }
                using (new EditorGUI.DisabledScope(!validity))
                {
                    if (GUILayout.Button("Apply"))
                    {
                        Apply();
                        SaveSettings();
                        _dialog.Close();
                    }
                }
                if (isWindows)
                {
                    if (GUILayout.Button("Cancel")) _dialog.Close();
                }
            }

            EditorGUILayout.EndVertical();
            if (rect.height > 0)
            {
                var height = rect.height + 4;
                minSize = new Vector2(minSize.x, height);
                maxSize = new Vector2(maxSize.x, height);
            }
        }

        private bool IsSettingsTemplate(GameObject obj)
        {
            if (obj == null) { return true; }
            if (PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.NotAPrefab) { return false; }
            var settings = obj.GetComponent<VirtualLensSettings>();
            if (settings == null) { return false; }
            return true;
        }

        private bool ValidateSettings()
        {
            if (_avatar == null)
            {
                EditorGUILayout.HelpBox("Avatar is not specified.", MessageType.Error);
                return false;
            }
            var descriptor = _avatar.GetComponent<VRCAvatarDescriptor>();
            if (descriptor == null)
            {
                EditorGUILayout.HelpBox("Avatar does not have VRC Avatar Descriptor.", MessageType.Error);
                return false;
            }
            var animator = _avatar.GetComponent<Animator>();
            if (animator.avatar == null || !animator.avatar.isHuman)
            {
                EditorGUILayout.HelpBox("Avatar is not a humanoid.", MessageType.Error);
                return false;
            }
            var rigValidity =
                animator.GetBoneTransform(HumanBodyBones.Chest) != null
                && animator.GetBoneTransform(HumanBodyBones.Neck) != null
                && animator.GetBoneTransform(HumanBodyBones.LeftShoulder) != null
                && animator.GetBoneTransform(HumanBodyBones.LeftUpperArm) != null
                && animator.GetBoneTransform(HumanBodyBones.LeftLowerArm) != null
                && animator.GetBoneTransform(HumanBodyBones.LeftHand) != null
                && animator.GetBoneTransform(HumanBodyBones.RightShoulder) != null
                && animator.GetBoneTransform(HumanBodyBones.RightUpperArm) != null
                && animator.GetBoneTransform(HumanBodyBones.RightLowerArm) != null
                && animator.GetBoneTransform(HumanBodyBones.RightHand) != null;
            if (!rigValidity)
            {
                EditorGUILayout.HelpBox(
                    "Your rig must have mappings for neck, shoulders, arms and hands.", MessageType.Error);
                return false;
            }
            if (MarkerDetector.DetectOrigin(_avatar))
            {
                EditorGUILayout.HelpBox(
                    "Your may already contain a Camera Object. You might have to remove it before applying.",
                    MessageType.Warning);
            }
            if (!IsSettingsTemplate(_template))
            {
                EditorGUILayout.HelpBox("Settings template is not a valid prefab.", MessageType.Error);
                return false;
            }
            if (_template != null && !SettingsPrefabMigrator.IsVariantOfDefaultPrefab(_template))
            {
                EditorGUILayout.HelpBox("Settings template is not a variant of DefaultSettings.", MessageType.Warning);
            }
            if (_useCustomModel && _customModelPrefab != null && !MarkerDetector.DetectOrigin(_customModelPrefab))
            {
                EditorGUILayout.HelpBox("Custom Model is not built for VirtualLens2.", MessageType.Error);
                return false;
            }
            return true;
        }

        private void Apply()
        {
            // Get template prefab
            var prefab = _template;
            if (prefab == null)
            {
                // VirtualLens2/Prefabs/DefaultSettings.prefab
                prefab = AssetUtility.LoadAssetByGUID<GameObject>("3c8a8e9b6ad5718489e23501fa495408");
            }
            // Instantiate settings template
            var obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (obj == null)
            {
                throw new ApplicationException(
                    "Failed to instantiate DefaultSettings prefab.\n" +
                    "Please reimport VirtualLens2 package.");
            }
            obj.name = _isDestructive ? $"{_avatar.name}_VirtualLensSettings" : "VirtualLens2";
            // Put camera model
            var customModel = _useCustomModel ? _customModelPrefab : null;
            var keepTransform = _useCustomModel && _keepCustomModelTransform;
            var cameraMesh = CameraMeshHelper.PutCameraMesh(_avatar, customModel, _targetHand, keepTransform);
            // (Non-Destructive) Set parent of settings object
            if (!_isDestructive)
            {
                Undo.SetTransformParent(obj.transform, _avatar.transform, "Create VirtualLens Settings");
                BoneProxyHelper.AddBoneProxy(cameraMesh);
                Undo.SetTransformParent(cameraMesh.transform, obj.transform, "Create VirtualLens Settings");
            }
            // Assign setting parameters
            var component = obj.GetComponent<VirtualLensSettings>();
            component.buildMode = _isDestructive ? BuildMode.Destructive : BuildMode.NonDestructive;
            component.avatar = _avatar;
            component.cameraObject = cameraMesh;
            component.cameraDroppable = CameraMeshHelper.DetectCameraDroppable(cameraMesh);
            Debug.Log(component.cameraObject, component.cameraDroppable);
            Undo.RegisterCreatedObjectUndo(obj, "Create VirtualLensSettings");
            Selection.activeGameObject = obj;
        }
    }
}
