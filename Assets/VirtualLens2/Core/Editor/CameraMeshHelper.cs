using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VirtualLens2.AV3EditorLib;
using VRC.SDK3.Avatars.Components;

#if WITH_MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif

namespace VirtualLens2
{
    internal static class CameraMeshHelper
    {
        public static bool EstimateTransform(GameObject obj, int targetHand)
        {
            var descriptor = obj.GetComponentInParent<VRCAvatarDescriptor>();
            if (!descriptor) { return false; }
            var animator = descriptor.GetComponent<Animator>();
            if (!animator) { return false; }
            var avatar = animator.avatar;
            if (!avatar || !avatar.isHuman) { return false; }

            var origin = MarkerDetector.DetectOrigin(obj);
            if (origin == null) { return false; }

            var handBone = targetHand == 0 ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand;
            var hand = animator.GetBoneTransform(handBone);

            var avatarType = typeof(Avatar);
            var avatarGetPreRotationMethod = avatarType.GetMethod(
                "GetPreRotation", BindingFlags.NonPublic | BindingFlags.Instance);
            var avatarGetPostRotationMethod = avatarType.GetMethod(
                "GetPostRotation", BindingFlags.NonPublic | BindingFlags.Instance);
            var avatarGetLimitSignMethod = avatarType.GetMethod(
                "GetLimitSign", BindingFlags.NonPublic | BindingFlags.Instance);
            if (avatarGetPreRotationMethod == null || avatarGetPostRotationMethod == null ||
                avatarGetLimitSignMethod == null)
            {
                Debug.LogError("Failed to get internal methods from UnityEngine.Avatar.");
                return false;
            }

            var rootRotation = obj.transform.rotation;
            var rootPosition = obj.transform.position;
            var originRotation = origin.transform.rotation;
            var originPosition = origin.transform.position;
            var leftDir = originRotation * Vector3.left;
            var rightDir = originRotation * Vector3.right;
            var backDir = originRotation * Vector3.back;

            float leftFarthest = 0;
            float rightFarthest = 0;
            float backFarthest = 0;
            foreach (var mf in obj.GetComponentsInChildren<MeshFilter>(true))
            {
                var transform = mf.transform;
                var mesh = mf.sharedMesh;
                foreach (var input in mesh.vertices)
                {
                    var v = transform.TransformPoint(input) - originPosition;
                    leftFarthest = Mathf.Max(leftFarthest, Vector3.Dot(v, leftDir));
                    rightFarthest = Mathf.Max(rightFarthest, Vector3.Dot(v, rightDir));
                    backFarthest = Mathf.Max(backFarthest, Vector3.Dot(v, backDir));
                }
            }
            foreach (var smr in obj.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var transform = smr.transform;
                var mesh = smr.sharedMesh;
                var baked = new Mesh();
                smr.BakeMesh(baked);
                var numVertices = mesh.vertexCount;
                var vertices = baked.vertices;
                for (var i = 0; i < numVertices; ++i)
                {
                    var v = transform.TransformPoint(vertices[i]) - originPosition;
                    leftFarthest = Mathf.Max(leftFarthest, Vector3.Dot(v, leftDir));
                    rightFarthest = Mathf.Max(rightFarthest, Vector3.Dot(v, rightDir));
                    backFarthest = Mathf.Max(backFarthest, Vector3.Dot(v, backDir));
                }
            }

            var middleBone = targetHand == 0
                ? HumanBodyBones.RightMiddleProximal
                : HumanBodyBones.LeftMiddleProximal;
            var middle = animator.GetBoneTransform(middleBone);

            float offsetX = targetHand == 0 ? rightFarthest : leftFarthest;
            float offsetZ = backFarthest;
            if (middle)
            {
                var mag = Vector3.Magnitude(middle.position - hand.position);
                offsetX += 0.5f * mag;
                offsetZ += 0.5f * mag;
            }

            var preQ = (Quaternion)avatarGetPreRotationMethod.Invoke(avatar, new object[] { (int)handBone });
            var postQ = (Quaternion)avatarGetPostRotationMethod.Invoke(avatar, new object[] { (int)handBone });
            var signs = (Vector3)avatarGetLimitSignMethod.Invoke(avatar, new object[] { (int)handBone });

            var forward = hand.rotation * postQ * Vector3.right;
            var upward = hand.parent.rotation * preQ * Vector3.forward;
            var inner = Vector3.Cross(forward, upward) * signs.z;
            var targetRotation = Quaternion.LookRotation(forward, upward);
            var deltaX = inner * offsetX;
            var deltaZ = forward * offsetZ;

            Undo.RecordObject(obj, "Automatic Camera Mesh Placement");
            var lossyScale = hand.lossyScale;
            Undo.SetTransformParent(obj.transform, hand, "Automatic Camera Mesh Placement");
            obj.transform.position =
                targetRotation * Quaternion.Inverse(originRotation) * (rootPosition - originPosition)
                + hand.position + deltaX + deltaZ;
            obj.transform.rotation = targetRotation * Quaternion.Inverse(originRotation) * rootRotation;
            obj.transform.localScale =
                new Vector3(1.0f / lossyScale.x, 1.0f / lossyScale.y, 1.0f / lossyScale.z);
            return true;
        }

        public static int EstimateCurrentHand(GameObject currentObject)
        {
            const int defaultValue = 0; // Right hand

            if (currentObject == null) return defaultValue;
            var descriptor = currentObject.GetComponentInParent<VRCAvatarDescriptor>();
            if (descriptor == null) return defaultValue;
            var animator = descriptor.GetComponentInParent<Animator>();
            if (animator == null || !animator.isHuman) return defaultValue;

            var lh = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            var rh = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            var t = currentObject.transform;
            var visited = new HashSet<Transform> { lh, rh, animator.transform };
            while (!visited.Contains(t))
            {
                visited.Add(t);
#if WITH_MODULAR_AVATAR
                var proxy = t.gameObject.GetComponent<ModularAvatarBoneProxy>();
                if (proxy && proxy.target)
                {
                    t = proxy.target;
                    continue;
                }
#endif
                t = t.parent;
            }

            if (t == lh) return 1;
            if (t == rh) return 0;
            return defaultValue;
        }

        public static GameObject DetectCameraDroppable(GameObject root)
        {
            var preview = MarkerDetector.DetectPreview(root);
            if (preview == null) { return root; }

            var ancestors = new HashSet<Transform> { root.transform };
            for (var t = preview.transform; t != root.transform; t = t.parent) { ancestors.Add(t); }

            var origin = MarkerDetector.DetectOrigin(root);
            for (var t = origin.transform; t != root.transform; t = t.parent)
            {
                if (ancestors.Contains(t.parent)) { return t.gameObject; }
            }
            return root;
        }

        public static GameObject PutCameraMesh(GameObject root, GameObject prefab, int targetHand, bool keepTransform)
        {
            if (!root) { return null; }
            var animator = root.GetComponent<Animator>();
            if (!animator) { return null; }
            var avatar = animator.avatar;
            if (!avatar || !avatar.isHuman) { return null; }

            var handBone = targetHand == 0 ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand;
            var hand = animator.GetBoneTransform(handBone);

            if (prefab == null)
            {
                prefab = AssetUtility.LoadAssetByGUID<GameObject>("4e7ba2446d1e2fd44ac344d66e6d6018");
                keepTransform = false;
            }

            var origin = MarkerDetector.DetectOrigin(prefab);
            if (origin == null) return null;

            if (!keepTransform)
            {
                var obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (obj == null)
                {
                    throw new ApplicationException("Failed to instantiate the camera mesh prefab.");
                }
                obj.transform.parent = root.transform;
                EstimateTransform(obj, targetHand);
                Undo.RegisterCreatedObjectUndo(obj, "Create Mesh Object");
                return obj;
            }
            else
            {
                var position = prefab.transform.localPosition;
                var rotation = prefab.transform.localRotation;
                var scale = prefab.transform.localScale;
                var obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (obj == null)
                {
                    throw new ApplicationException("Failed to instantiate the camera mesh prefab.");
                }
                obj.transform.parent = hand.transform;
                obj.transform.localPosition = position;
                obj.transform.localRotation = rotation;
                obj.transform.localScale = scale;
                Undo.RegisterCreatedObjectUndo(obj, "Create Mesh Object");
                return obj;
            }
        }
    }
}
