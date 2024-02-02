using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VirtualLens2.AV3EditorLib;

namespace VirtualLens2
{
    internal static class MarkerGenerator
    {
        private static Animator GetAvatarAnimator(GameObject obj)
        {
            if (!obj) { return null; }
            var animator = obj.GetComponent<Animator>();
            if (!animator) { return null; }
            var avatar = animator.avatar;
            if (!avatar || !avatar.isHuman) { return null; }
            return animator;
        }

        private static GameObject GenerateScreenToucher(GameObject avatarObject, int targetHand)
        {
            var animator = GetAvatarAnimator(avatarObject);
            if (!animator) { return null; }
            var distal = animator.GetBoneTransform(targetHand == 0
                ? HumanBodyBones.RightIndexDistal
                : HumanBodyBones.LeftIndexDistal);
            var hand = animator.GetBoneTransform(targetHand == 0
                ? HumanBodyBones.RightHand
                : HumanBodyBones.LeftHand);
            // VirtualLens2/Prefabs/ScreenToucher.prefab
            var prefab = AssetUtility.LoadAssetByGUID<GameObject>("7dd993771f3f7934284e57b4d2c67208");
            var toucher = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (toucher == null)
            {
                throw new ApplicationException(
                    "Failed to instantiate ScreenToucher prefab.\n" +
                    "Please reimport VirtualLens2 package.");
            }
            if (distal != null)
            {
                toucher.name = GameObjectUtility.GetUniqueNameForSibling(distal, toucher.name);
                toucher.transform.parent = distal;
                toucher.transform.localPosition = distal.transform.localPosition;
                toucher.transform.localRotation = Quaternion.identity;
                toucher.transform.localScale = Vector3.one;
            }
            else
            {
                toucher.name = GameObjectUtility.GetUniqueNameForSibling(hand, toucher.name);
                toucher.transform.parent = hand;
                toucher.transform.position = hand.transform.position;
                toucher.transform.localRotation = Quaternion.identity;
                toucher.transform.localScale = Vector3.one;
            }
            Undo.RegisterCreatedObjectUndo(toucher, "Create ScreenToucher");
            return toucher;
        }

        public static List<GameObject> GenerateScreenToucher(GameObject avatarObject)
        {
            var target = CameraMeshHelper.EstimateCurrentHand(MarkerDetector.DetectOrigin(avatarObject));
            if (target == -1)
            {
                return new List<GameObject>()
                {
                    GenerateScreenToucher(avatarObject, 0),
                    GenerateScreenToucher(avatarObject, 1)
                };
            }
            return new List<GameObject>()
            {
                GenerateScreenToucher(avatarObject, 1 - target)
            };
        }


        private static GameObject GenerateDroneController(GameObject avatarObject, int targetHand)
        {
            var animator = GetAvatarAnimator(avatarObject);
            if (!animator) { return null; }
            var hand = animator.GetBoneTransform(
                targetHand == 0 ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand);
            var index = animator.GetBoneTransform(
                targetHand == 0 ? HumanBodyBones.RightIndexProximal : HumanBodyBones.LeftIndexProximal);
            var little = animator.GetBoneTransform(
                targetHand == 0 ? HumanBodyBones.RightLittleProximal : HumanBodyBones.LeftLittleProximal);
            // VirtualLens2/Prefabs/DroneController.prefab
            var prefab = AssetUtility.LoadAssetByGUID<GameObject>("a6ce9259a21e0f04399059287bd5b875");
            var controller = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (controller == null)
            {
                throw new ApplicationException(
                    "Failed to instantiate DroneController prefab.\n" +
                    "Please reimport VirtualLens2 package.");
            }
            if (index != null && little != null)
            {
                var handPosition = hand.position;
                var indexPosition = index.position;
                var littlePosition = little.position;
                var hand2Index = indexPosition - handPosition;
                var hand2Little = littlePosition - handPosition;
                var center = (handPosition + indexPosition + littlePosition) / 3.0f;
                var direction = Vector3.Cross(hand2Little, hand2Index) * (targetHand == 0 ? 1.0f : -1.0f);
                var delta = direction.normalized * (center - handPosition).magnitude;
                controller.transform.parent = hand;
                controller.transform.position = center + delta;
                controller.transform.localRotation = Quaternion.identity;
                controller.transform.localScale = Vector3.one;
            }
            else
            {
                controller.transform.parent = hand;
                controller.transform.position = hand.position;
                controller.transform.localRotation = Quaternion.identity;
                controller.transform.localScale = Vector3.one;
            }
            Undo.RegisterCreatedObjectUndo(controller, "Create DroneController");
            return controller;
        }

        public static GameObject GenerateDroneController(GameObject avatarObject)
        {
            var target = CameraMeshHelper.EstimateCurrentHand(MarkerDetector.DetectOrigin(avatarObject));
            return target == -1
                ? GenerateDroneController(avatarObject, 1)
                : GenerateDroneController(avatarObject, 1 - target);
        }


        public static GameObject GenerateRepositionOrigin(GameObject avatarObject)
        {
            var animator = GetAvatarAnimator(avatarObject);
            if (!animator) { return null; }
            var leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
            var leftLower = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).position;
            var leftUpper = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position;
            var leftShoulder = animator.GetBoneTransform(HumanBodyBones.LeftShoulder).position;
            var rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand).position;
            var rightLower = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position;
            var rightUpper = animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position;
            var rightShoulder = animator.GetBoneTransform(HumanBodyBones.RightShoulder).position;
            var length =
                (leftShoulder - rightShoulder).magnitude
                + (leftHand - leftLower).magnitude
                + (leftLower - leftUpper).magnitude
                + (leftUpper - leftShoulder).magnitude
                + (rightHand - rightLower).magnitude
                + (rightLower - rightUpper).magnitude
                + (rightUpper - rightShoulder).magnitude;
            var chest = animator.GetBoneTransform(HumanBodyBones.Chest);
            var neck = animator.GetBoneTransform(HumanBodyBones.Neck);
            var position = neck.position + new Vector3(0.0f, -length / 20.0f, length / 4.0f);
            // VirtualLens2/Prefabs/RepositionOrigin.prefab
            var prefab = AssetUtility.LoadAssetByGUID<GameObject>("a3582bbce3991544cb8d3121d2cdd91e");
            var origin = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (origin == null)
            {
                throw new ApplicationException(
                    "Failed to instantiate RepositionOrigin prefab.\n" +
                    "Please reimport VirtualLens2 package.");
            }
            origin.transform.parent = chest;
            origin.transform.position = position;
            origin.transform.rotation = Quaternion.identity;
            origin.transform.localScale = Vector3.one;
            Undo.RegisterCreatedObjectUndo(origin, "Create RepositionOrigin");
            return origin;
        }


        public static GameObject[] GenerateEyeMarkers(GameObject avatarObject, bool generateLeft, bool generateRight)
        {
            var pair = new GameObject[] { null, null };
            var animator = GetAvatarAnimator(avatarObject);
            if (!animator) { return pair; }
            var avatar = animator.avatar;

            var leftTransform = animator.GetBoneTransform(HumanBodyBones.LeftEye);
            var rightTransform = animator.GetBoneTransform(HumanBodyBones.RightEye);
            if (leftTransform == null || rightTransform == null)
            {
                Debug.LogWarning("Avatar doesn't have left or right eye bone.");
                return pair;
            }
            var leftDescendants = new HashSet<Transform>(leftTransform.GetComponentsInChildren<Transform>());
            var rightDescendants = new HashSet<Transform>(rightTransform.GetComponentsInChildren<Transform>());

            var avatarType = typeof(Avatar);
            var avatarGetPostRotationMethod = avatarType.GetMethod(
                "GetPostRotation", BindingFlags.NonPublic | BindingFlags.Instance);
            if (avatarGetPostRotationMethod == null)
            {
                Debug.LogWarning("UnityEngine.Avatar.GetPostRotation() is not found.");
                return pair;
            }

            var leftPostQ = (Quaternion)avatarGetPostRotationMethod.Invoke(
                avatar, new object[] { (int)HumanBodyBones.LeftEye });
            var rightPostQ = (Quaternion)avatarGetPostRotationMethod.Invoke(
                avatar, new object[] { (int)HumanBodyBones.LeftEye });
            var leftOrigin = leftTransform.position;
            var rightOrigin = rightTransform.position;
            var leftNormal = leftTransform.rotation * leftPostQ * Vector3.right;
            var rightNormal = rightTransform.rotation * rightPostQ * Vector3.right;

            var leftSum = Vector3.zero;
            var rightSum = Vector3.zero;
            float leftWeightSum = 0;
            float rightWeightSum = 0;
            foreach (var renderer in avatarObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var bones = renderer.bones;
                var leftDescendantIndices = new HashSet<int>();
                var rightDescendantIndices = new HashSet<int>();
                for (var i = 0; i < bones.Length; ++i)
                {
                    var bone = renderer.bones[i];
                    if (leftDescendants.Contains(bone)) { leftDescendantIndices.Add(i); }
                    if (rightDescendants.Contains(bone)) { rightDescendantIndices.Add(i); }
                }
                if (leftDescendantIndices.Count == 0 && rightDescendantIndices.Count == 0) { continue; }

                var mesh = renderer.sharedMesh;
                var baked = new Mesh();
                renderer.BakeMesh(baked);
                var numVertices = mesh.vertexCount;
                var vertices = baked.vertices;
                var boneWeights = mesh.boneWeights;
                if (boneWeights.Length != numVertices) { continue; }

                var leftVertexWeights = new float[numVertices];
                var rightVertexWeights = new float[numVertices];
                var vertexPositions = new Vector3[numVertices];
                for (var i = 0; i < numVertices; ++i)
                {
                    var bw = boneWeights[i];
                    float leftWeight = 0;
                    float rightWeight = 0;
                    if (leftDescendantIndices.Contains(bw.boneIndex0)) leftWeight += bw.weight0;
                    if (leftDescendantIndices.Contains(bw.boneIndex1)) leftWeight += bw.weight1;
                    if (leftDescendantIndices.Contains(bw.boneIndex2)) leftWeight += bw.weight2;
                    if (leftDescendantIndices.Contains(bw.boneIndex3)) leftWeight += bw.weight3;
                    if (rightDescendantIndices.Contains(bw.boneIndex0)) rightWeight += bw.weight0;
                    if (rightDescendantIndices.Contains(bw.boneIndex1)) rightWeight += bw.weight1;
                    if (rightDescendantIndices.Contains(bw.boneIndex2)) rightWeight += bw.weight2;
                    if (rightDescendantIndices.Contains(bw.boneIndex3)) rightWeight += bw.weight3;

                    var tf = renderer.transform;
                    var v = tf.rotation * vertices[i] + tf.position;
                    var leftDot = Vector3.Dot(v - leftOrigin, leftNormal);
                    var rightDot = Vector3.Dot(v - rightOrigin, rightNormal);
                    leftVertexWeights[i] = leftDot * leftWeight;
                    rightVertexWeights[i] = rightDot * rightWeight;
                    vertexPositions[i] = v;
                }

                var triangles = mesh.triangles;
                for (var i = 0; i < triangles.Length; i += 3)
                {
                    var ai = triangles[i + 0];
                    var bi = triangles[i + 1];
                    var ci = triangles[i + 2];
                    var a = vertexPositions[ai];
                    var b = vertexPositions[bi];
                    var c = vertexPositions[ci];
                    var area = 0.5f * Vector3.Magnitude(Vector3.Cross(b - a, c - a));
                    var lwa = Mathf.Max(0.0f, area * leftVertexWeights[ai]);
                    var lwb = Mathf.Max(0.0f, area * leftVertexWeights[bi]);
                    var lwc = Mathf.Max(0.0f, area * leftVertexWeights[ci]);
                    var rwa = Mathf.Max(0.0f, area * rightVertexWeights[ai]);
                    var rwb = Mathf.Max(0.0f, area * rightVertexWeights[bi]);
                    var rwc = Mathf.Max(0.0f, area * rightVertexWeights[ci]);
                    leftSum += a * lwa + b * lwb + c * lwc;
                    rightSum += a * rwa + b * rwb + c * rwc;
                    leftWeightSum += lwa + lwb + lwc;
                    rightWeightSum += rwa + rwb + rwc;
                }
            }

            if (generateLeft && leftWeightSum > 0)
            {
                // VirtualLens2/Prefabs/SelfieMarkerLeft.prefab
                var prefab = AssetUtility.LoadAssetByGUID<GameObject>("30135137a812d024e8faa8a1ce836c87");
                var origin = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (origin == null)
                {
                    throw new ApplicationException(
                        "Failed to instantiate SelfieMarkerLeft prefab.\n" +
                        "Please reimport VirtualLens2 package.");
                }
                origin.transform.parent = leftTransform;
                origin.transform.position = leftSum / leftWeightSum;
                origin.transform.rotation = Quaternion.identity;
                origin.transform.localScale = Vector3.one;
                Undo.RegisterCreatedObjectUndo(origin, "Create SelfieMarkerLeft");
                pair[0] = origin;
            }
            if (generateRight && rightWeightSum > 0)
            {
                // VirtualLens2/Prefabs/SelfieMarkerRight.prefab
                var prefab = AssetUtility.LoadAssetByGUID<GameObject>("0454613d1827826448648c3eb74bd7f1");
                var origin = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (origin == null)
                {
                    throw new ApplicationException(
                        "Failed to instantiate SelfieMarkerRight prefab.\n" +
                        "Please reimport VirtualLens2 package.");
                }
                origin.transform.parent = rightTransform;
                origin.transform.position = rightSum / rightWeightSum;
                origin.transform.rotation = Quaternion.identity;
                origin.transform.localScale = Vector3.one;
                Undo.RegisterCreatedObjectUndo(origin, "Create SelfieMarkerRight");
                pair[1] = origin;
            }
            return pair;
        }
    }
}
