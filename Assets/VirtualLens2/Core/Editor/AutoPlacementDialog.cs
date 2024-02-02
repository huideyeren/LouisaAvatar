using System;
using UnityEditor;
using UnityEngine;
using VirtualLens2.AV3EditorLib;

namespace VirtualLens2
{
    internal class AutoPlacementDialog : EditorWindow
    {
        private static AutoPlacementDialog _dialog;

        private SerializedObject _serializedObject;
        private SerializedProperty _cameraObjectProp;
        private SerializedProperty _cameraDroppableProp;

        private GameObject _avatar;
        private GameObject _parent;
        private int _targetHand;
        private GameObject _customModel;
        private bool _removeExistingModel = true;

        public static void Open(SerializedObject so, GameObject avatar, GameObject parent)
        {
            if (!_dialog)
            {
                _dialog = CreateInstance<AutoPlacementDialog>();
            }

            _dialog._serializedObject = so;
            _dialog._cameraObjectProp = so.FindProperty("cameraObject");
            _dialog._cameraDroppableProp = so.FindProperty("cameraDroppable");

            _dialog._avatar = avatar;
            _dialog._parent = parent;
            _dialog._targetHand = CameraMeshHelper.EstimateCurrentHand(
                _dialog._cameraObjectProp.objectReferenceValue as GameObject);
            _dialog._removeExistingModel = true;

            _dialog.titleContent.text = "VirtualLens2 Auto Placement";
            _dialog.ShowUtility();
        }

        private void OnGUI()
        {
            if (_dialog != this) Close();
            var rect = EditorGUILayout.BeginVertical();

            _targetHand = EditorGUILayout.IntPopup(
                new GUIContent("Target hand"), _targetHand,
                new[] { new GUIContent("Right"), new GUIContent("Left") },
                new[] { 0, 1 });

            _customModel = EditorGUILayout.ObjectField(
                    new GUIContent("Custom model prefab"),
                    _customModel,
                    typeof(GameObject),
                    false)
                as GameObject;

            if (!_customModel)
            {
                _removeExistingModel = EditorGUILayout.Toggle("Remove existing model", _removeExistingModel);
            }

            EditorGUILayout.Space();

            var valid = Validate();
            if (!valid)
            {
                EditorGUILayout.Space();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                var isWindows = Application.platform == RuntimePlatform.WindowsEditor;
                if (!isWindows)
                {
                    if (GUILayout.Button("Cancel")) Close();
                }
                using (new EditorGUI.DisabledScope(!valid))
                {
                    if (GUILayout.Button("Apply"))
                    {
                        Apply();
                        Close();
                    }
                }
                if (isWindows)
                {
                    if (GUILayout.Button("Cancel")) Close();
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

        private bool Validate()
        {
            var result = true;
            if (!_avatar)
            {
                EditorGUILayout.HelpBox("VirtualLens object is not in an avatar.", MessageType.Error);
                result = false;
            }
            else
            {
                var animator = _avatar.GetComponent<Animator>();
                if (!animator || !animator.isHuman)
                {
                    EditorGUILayout.HelpBox("The avatar is not a humanoid.", MessageType.Error);
                    result = false;
                }
            }
            if (_customModel)
            {
                if (MarkerDetector.DetectOrigin(_customModel) == null)
                {
                    EditorGUILayout.HelpBox(
                        "The prefab is not built for VirtualLens2.", MessageType.Error);
                    result = false;
                }
            }
            return result;
        }

        private void Apply()
        {
            Undo.SetCurrentGroupName("VirtualLens2 Auto Placement");
            var currentObject = _cameraObjectProp.objectReferenceValue as GameObject;
            if (!_customModel)
            {
                if (currentObject == null)
                {
                    // VirtualLens2/Prefabs/CompactCamera.prefab
                    var prefab = AssetUtility.LoadAssetByGUID<GameObject>("4e7ba2446d1e2fd44ac344d66e6d6018");
                    currentObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    if (currentObject == null)
                    {
                        throw new ApplicationException(
                            "Failed to instantiate CompactCamera prefab.\n" +
                            "Please reimport the VirtualLens2 package.");
                    }
                    currentObject.transform.parent = _avatar.transform;
                    Undo.RegisterCreatedObjectUndo(currentObject, "Instantiate Camera Mesh");
                    _cameraObjectProp.objectReferenceValue = currentObject;
                    _cameraDroppableProp.objectReferenceValue =
                        HierarchyUtility.PathToObject(currentObject, "CompactCameraMesh");
                    _serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                if (_removeExistingModel && !currentObject)
                {
                    Undo.DestroyObjectImmediate(currentObject);
                }
                currentObject = PrefabUtility.InstantiatePrefab(_customModel) as GameObject;
                if (currentObject == null)
                {
                    throw new ApplicationException("Failed to instantiate custom model prefab.");
                }
                currentObject.transform.parent = _avatar.transform;
                Undo.RegisterCreatedObjectUndo(currentObject, "Instantiate Camera Mesh");
                _cameraObjectProp.objectReferenceValue = currentObject;
                _cameraDroppableProp.objectReferenceValue =
                    CameraMeshHelper.DetectCameraDroppable(currentObject);
                _serializedObject.ApplyModifiedProperties();
            }
            CameraMeshHelper.EstimateTransform(currentObject, _targetHand);
#if WITH_MODULAR_AVATAR
            if (_parent)
            {
                BoneProxyHelper.AddBoneProxy(currentObject);
                Undo.SetTransformParent(currentObject.transform, _parent.transform, "Estimate Camera Placement");
            }
#endif
        }
    }
}
