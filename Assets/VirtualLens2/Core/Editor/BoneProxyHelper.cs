using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

#if WITH_MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
using UnityEditor;
#endif

namespace VirtualLens2
{
    internal static class BoneProxyHelper
    {
        public static bool AddBoneProxy(GameObject obj)
        {
#if WITH_MODULAR_AVATAR
            var descriptor = obj.GetComponentInParent<VRCAvatarDescriptor>();
            if (descriptor == null) return false;
            var animator = descriptor.GetComponentInParent<Animator>();
            if (animator == null || !animator.isHuman) return false;
            
            var boneDistance = int.MaxValue;
            var boneReference = HumanBodyBones.LastBone;
            var subPath = "";
            for (var i = 0; i < (int)HumanBodyBones.LastBone; ++i)
            {
                var bone = (HumanBodyBones)i;
                var transform = animator.GetBoneTransform(bone);
                var tokens = new List<string>();
                var t = obj.transform.parent;
                while (t != transform && t != animator.transform)
                {
                    tokens.Add(t.name);
                    t = t.parent;
                }
                if (t == animator.transform) continue;
                tokens.Reverse();
                var distance = tokens.Count;
                if (distance < boneDistance)
                {
                    boneDistance = distance;
                    boneReference = bone;
                    subPath = string.Join("/", tokens);
                }
            }
            if (boneReference == HumanBodyBones.LastBone) return false;
            var component = obj.GetComponent<ModularAvatarBoneProxy>();
            if (component == null)
            {
                component = Undo.AddComponent<ModularAvatarBoneProxy>(obj);
            }
            else
            {
                Undo.RecordObject(component, "Update Bone Proxy Parameters");
            }
            component.attachmentMode = BoneProxyAttachmentMode.AsChildKeepWorldPose;
            component.boneReference = boneReference;
            component.subPath = subPath;
            return true;
#else
            return false;
#endif
        }
    }

}
