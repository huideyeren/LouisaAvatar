using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using VirtualLens2.AV3EditorLib;

#if WITH_MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif

namespace VirtualLens2
{
    internal static class AnimatorControllerGenerator
    {
        private const string ParameterPrefix = Constants.ParameterPrefix;

        public static void Generate(ImplementationSettings settings, ArtifactsFolder folder,
            GeneratedObjectSet objectSet)
        {
            Clear(settings);

            var avatar = settings.Avatar;
            var descriptor = avatar.GetComponent<VRCAvatarDescriptor>();

            AnimatorController controller;
            if (settings.BuildAsModule)
            {
                var path = folder.GenerateAssetPath<AnimatorController>();
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);
                Undo.RegisterCreatedObjectUndo(controller, "Create animator controller");
                controller.RemoveLayer(0); // Remove Base Layer
#if WITH_MODULAR_AVATAR
                var component = objectSet.VirtualLensRoot.GetComponent<ModularAvatarMergeAnimator>();
                Undo.RecordObject(component, "Update modular avatar merge animator");
                var so = new SerializedObject(component);
                so.FindProperty("animator").objectReferenceValue = controller;
                so.ApplyModifiedProperties();
#endif
            }
            else
            {
                controller = AvatarDescriptorUtility.GetOrCreatePlayableLayer(
                    descriptor, VRCAvatarDescriptor.AnimLayerType.FX, folder);
            }

            // VirtualLens2/Core/Animations/FX.controller
            var source = AnimatorControllerEditor.Clone(
                AssetUtility.LoadAssetByGUID<AnimatorController>("21f6fdb6102ff1045a1eb146ac402102"));

            var motions = new MotionTemplateParameters();
            RegisterZoomMotion(motions, settings, folder);
            RegisterApertureMotion(motions, settings, folder);
            RegisterDistanceMotion(motions, settings, folder);
            RegisterExposureMotion(motions, settings, folder);
            RegisterDroneYawSpeedMotion(motions, settings, folder);
            RegisterFarPlaneMotion(motions, settings, folder);
            RegisterAvatarScalingMotion(motions, settings, folder);
            RegisterMaxBlurrinessMotion(motions, settings, folder);
            RegisterPreviewMaterialsMotion(motions, settings, folder);
            AnimatorControllerEditor.ProcessMotionTemplate(source, motions);

            var objects = new AnimationTemplateParameters();
            objects.Add("VirtualLensOrigin", MarkerDetector.DetectOrigin(settings));
            var preview = MarkerDetector.DetectPreview(settings);
            if (preview)
            {
                objects.Add("VirtualLensPreview", MarkerDetector.DetectPreview(settings));
            }
            objects.Add("CameraRoot", settings.CameraRoot);
            objects.Add("CameraNonPreviewRoot", settings.CameraNonPreviewRoot);

            var hideableMeshes = settings.HideableMeshes
                .Select(obj => obj.GetComponent<MeshRenderer>())
                .Where(renderer => renderer != null)
                .ToArray();
            var hideableSkinnedMeshes = settings.HideableMeshes
                .Select(obj => obj.GetComponent<SkinnedMeshRenderer>())
                .Where(renderer => renderer != null)
                .ToArray();

            var nonPreviewMeshes = settings.CameraNonPreviewRoot
                .GetComponentsInChildren<MeshRenderer>(true)
                .Where(c => c.enabled)
                .ToArray();
            var nonPreviewSkinnedMeshes = settings.CameraNonPreviewRoot
                .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(c => c.enabled)
                .ToArray();

            var previewMeshes = settings.CameraRoot
                .GetComponentsInChildren<MeshRenderer>(true)
                .Except(nonPreviewMeshes)
                .Where(c => c.enabled)
                .ToArray();
            var previewSkinnedMeshes = settings.CameraRoot
                .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Except(nonPreviewSkinnedMeshes)
                .Where(c => c.enabled)
                .ToArray();

            var simpleHideableMeshes = hideableMeshes
                .Except(nonPreviewMeshes)
                .Except(previewMeshes)
                .ToArray();
            var simpleHideableSkinnedMeshes = hideableSkinnedMeshes
                .Except(nonPreviewSkinnedMeshes)
                .Except(previewSkinnedMeshes)
                .ToArray();

            foreach (var renderer in nonPreviewMeshes.Except(hideableMeshes))
            {
                objects.Add("NonPreviewMeshes", renderer.gameObject);
            }
            foreach (var renderer in nonPreviewSkinnedMeshes.Except(hideableSkinnedMeshes))
            {
                objects.Add("NonPreviewSkinnedMeshes", renderer.gameObject);
            }
            foreach (var renderer in nonPreviewMeshes.Intersect(hideableMeshes))
            {
                objects.Add("HideableNonPreviewMeshes", renderer.gameObject);
            }
            foreach (var renderer in nonPreviewSkinnedMeshes.Intersect(hideableSkinnedMeshes))
            {
                objects.Add("HideableNonPreviewSkinnedMeshes", renderer.gameObject);
            }

            foreach (var renderer in previewMeshes.Except(hideableMeshes))
            {
                objects.Add("PreviewMeshes", renderer.gameObject);
            }
            foreach (var renderer in previewSkinnedMeshes.Except(hideableSkinnedMeshes))
            {
                objects.Add("PreviewSkinnedMeshes", renderer.gameObject);
            }
            foreach (var renderer in previewMeshes.Intersect(hideableMeshes))
            {
                objects.Add("HideablePreviewMeshes", renderer.gameObject);
            }
            foreach (var renderer in previewSkinnedMeshes.Intersect(hideableSkinnedMeshes))
            {
                objects.Add("HideablePreviewSkinnedMeshes", renderer.gameObject);
            }

            foreach (var renderer in simpleHideableMeshes)
            {
                objects.Add("HideableMeshes", renderer.gameObject);
            }
            foreach (var renderer in simpleHideableSkinnedMeshes)
            {
                objects.Add("HideableSkinnedMeshes", renderer.gameObject);
            }

            if (objectSet.SelfieDetectorMarkers)
            {
                objects.Add("SelfieMarker", objectSet.SelfieDetectorMarkers);
            }
            foreach (var container in settings.ScreenTouchers)
            {
                var toucher = HierarchyUtility.PathToObject(container, "_VirtualLens_ScreenToucher");
                if (toucher)
                {
                    objects.Add("ScreenTouchers", toucher);
                }
            }
            foreach (var optional in settings.OptionalObjects)
            {
                var key = optional.DefaultState ? "OptionalObjectsNegated" : "OptionalObjects";
                objects.Add(key, optional.GameObject);
            }

            AnimatorControllerEditor.ProcessAnimationTemplates(source, settings.Avatar, objects, folder);

            var writeDefaults = AV3EditorLib.WriteDefaultsOverrideMode.None;
            switch (settings.WriteDefaults)
            {
                case WriteDefaultsOverrideMode.Auto:
                    writeDefaults = SelectWriteDefaultsMode(controller);
                    break;
                case WriteDefaultsOverrideMode.ForceDisable:
                    writeDefaults = AV3EditorLib.WriteDefaultsOverrideMode.ForceDisable;
                    break;
                case WriteDefaultsOverrideMode.ForceEnable:
                    writeDefaults = AV3EditorLib.WriteDefaultsOverrideMode.ForceEnable;
                    break;
            }
            AnimatorControllerEditor.Merge(controller, source, writeDefaults);

            // Controller must be persistent to add StateMachineBehaviour
            RegisterDefaultParameters(controller, settings);
            RegisterQuickCalls(controller, settings);

            EditorUtility.SetDirty(controller);
        }


        public static void Clear(GameObject avatar)
        {
            if (!avatar) { return; }
            var descriptor = avatar.GetComponent<VRCAvatarDescriptor>();
            if (!descriptor) { return; }
            var controller = AvatarDescriptorUtility.GetPlayableLayer(descriptor, VRCAvatarDescriptor.AnimLayerType.FX);
            if (!controller) { return; }
            AssetUtility.RemoveSubAssets(controller, o => o.name.StartsWith("zAutogenerated__VirtualLens2__"));
            var re = new Regex(@"^VirtualLens2 .*");
            AnimatorControllerEditor.RemoveLayers(controller, re, descriptor);
            AnimatorControllerEditor.RemoveParameters(controller, re);
            EditorUtility.SetDirty(controller);
        }

        private static void Clear(ImplementationSettings settings) { Clear(settings.Avatar); }


        #region Object Paths

        private static readonly string[] CaptureCameraPaths = new[]
        {
            "_VirtualLens_Root/Local/Capture/Camera/RGB",
            "_VirtualLens_Root/Local/Capture/Camera/Depth",
            "_VirtualLens_Root/Local/Capture/Camera/AvatarDepth",
            "_VirtualLens_Root/Local/Capture/Camera/SelfieDetector",
        };

        private static readonly string[] ComputeRendererPaths = new[]
        {
            "_VirtualLens_Root/Local/Compute/FaceFocusCompute/Quad",
            "_VirtualLens_Root/Local/Compute/StateUpdater/Quad",
            "_VirtualLens_Root/Local/Compute/DisplayRenderer/Quad",
            "_VirtualLens_Root/Local/Compute/Normal/ComputeCoc/Quad",
            "_VirtualLens_Root/Local/Compute/Normal/ComputeTiles/Quad",
            "_VirtualLens_Root/Local/Compute/Normal/Compute/Quad",
            "_VirtualLens_Root/Local/Compute/HighResolution/ComputeCoc/Quad",
            "_VirtualLens_Root/Local/Compute/HighResolution/ComputeTiles/Quad",
            "_VirtualLens_Root/Local/Compute/HighResolution/Downsample/Quad",
            "_VirtualLens_Root/Local/Compute/HighResolution/RealtimeCompute/Quad",
            "_VirtualLens_Root/Local/Writer/Normal/RealtimeWriter",
            "_VirtualLens_Root/Local/Writer/HighResolution/RealtimeWriter",
            "_VirtualLens_Root/Local/Writer/HighResolution/HighResolutionWriter",
        };

        #endregion

        #region Motion Generators

        private static void RegisterZoomMotion(
            MotionTemplateParameters parameters, ImplementationSettings settings, ArtifactsFolder folder)
        {
            var root = settings.Avatar;
            var clip = new AnimationClip();
            var values = settings.ZoomFovs();
            foreach (var groupPath in CaptureCameraPaths)
            {
                var group = HierarchyUtility.PathToObject(root, groupPath);
                if (group == null) { continue; }
                foreach (var camera in group.GetComponentsInChildren<Camera>(true))
                {
                    var cameraPath = HierarchyUtility.RelativePath(root, camera.gameObject);
                    AnimationClipEditor.AppendValue(clip, cameraPath, typeof(Camera), "field of view", values);
                }
            }
            foreach (var path in ComputeRendererPaths)
            {
                AnimationClipEditor.AppendValue(clip, path, typeof(MeshRenderer), "material._FieldOfView", values);
            }
            folder.CreateAsset(clip);
            // VirtualLens2/Core/Animations/Placeholders/Zoom.anim
            parameters.Add(AssetUtility.LoadAssetByGUID<AnimationClip>("4605d0f9cfdde2d4c80f18af6265c401"), clip);
        }

        private static void RegisterApertureMotion(
            MotionTemplateParameters parameters, ImplementationSettings settings, ArtifactsFolder folder)
        {
            var values = new[] { settings.ApertureMinParameter, settings.ApertureMaxParameter };
            var thresh = new[] { settings.ApertureMinParameter, settings.ApertureMinParameter };
            var flags = new[] { 0.0f, 1.0f };
            var clips = new[] { new AnimationClip(), new AnimationClip() };
            foreach (var path in ComputeRendererPaths)
            {
                for (var i = 0; i < 2; ++i)
                {
                    AnimationClipEditor.AppendValue(
                        clips[i], path, typeof(MeshRenderer), "material._LogFNumber", values[i]);
                    AnimationClipEditor.AppendValue(
                        clips[i], path, typeof(MeshRenderer), "material._BlurringThresh", thresh[i]);
                    AnimationClipEditor.AppendValue(
                        clips[i], path, typeof(MeshRenderer), "material._Blurring", flags[i]);
                }
            }
            folder.CreateAsset(clips[0]);
            folder.CreateAsset(clips[1]);
            
            // VirtualLens2/Core/Animations/Placeholders/ApertureMin.anim
            parameters.Add(AssetUtility.LoadAssetByGUID<AnimationClip>("5be4c8a7f6a92ab4cbb4ec8d8e7ef5db"), clips[0]);
            // VirtualLens2/Core/Animations/Placeholders/ApertureMax.anim
            parameters.Add(AssetUtility.LoadAssetByGUID<AnimationClip>("113089bf8eec90542ab08f080bfa460c"), clips[1]);
        }

        private static void RegisterDistanceMotion(
            MotionTemplateParameters parameters, ImplementationSettings settings, ArtifactsFolder folder)
        {
            var values = new[]
                { settings.ManualFocusingDistanceMinParameter, settings.ManualFocusingDistanceMaxParameter };
            var thresh = new[]
                { settings.ManualFocusingDistanceMinParameter, settings.ManualFocusingDistanceMinParameter };
            var clips = new[] { new AnimationClip(), new AnimationClip() };
            foreach (var path in ComputeRendererPaths)
            {
                for (var i = 0; i < 2; ++i)
                {
                    AnimationClipEditor.AppendValue(
                        clips[i], path, typeof(MeshRenderer), "material._LogFocusDistance", values[i]);
                    AnimationClipEditor.AppendValue(
                        clips[i], path, typeof(MeshRenderer), "material._FocusingThresh", thresh[i]);
                }
            }
            folder.CreateAsset(clips[0]);
            folder.CreateAsset(clips[1]);
            
            // VirtualLens2/Core/Animations/Placeholders/DistanceMin.anim
            parameters.Add(AssetUtility.LoadAssetByGUID<AnimationClip>("0252f150775f383449351e783fa0279f"), clips[0]);
            // VirtualLens2/Core/Animations/Placeholders/DistanceMax.anim
            parameters.Add(AssetUtility.LoadAssetByGUID<AnimationClip>("d6421ea187b66994197cbf16d653e9e1"), clips[1]);
        }

        private static void RegisterExposureMotion(
            MotionTemplateParameters parameters, ImplementationSettings settings, ArtifactsFolder folder)
        {
            var values = new[] { settings.ExposureMinParameter, settings.ExposureMaxParameter };
            var clips = new[] { new AnimationClip(), new AnimationClip() };
            foreach (var path in ComputeRendererPaths)
            {
                for (var i = 0; i < 2; ++i)
                {
                    AnimationClipEditor.AppendValue(
                        clips[i], path, typeof(MeshRenderer), "material._Exposure", values[i]);
                }
            }
            folder.CreateAsset(clips[0]);
            folder.CreateAsset(clips[1]);
            
            // VirtualLens2/Core/Animations/Placeholders/ExposureMin.anim
            parameters.Add(AssetUtility.LoadAssetByGUID<AnimationClip>("10d1b7ec22a81aa41bc9e77c18d78164"), clips[0]);
            // VirtualLens2/Core/Animations/Placeholders/ExposureMax.anim
            parameters.Add(AssetUtility.LoadAssetByGUID<AnimationClip>("860f373828cb7f64597c80252183779e"), clips[1]);
        }

        private static void RegisterDroneYawSpeedMotion(
            MotionTemplateParameters parameters, ImplementationSettings settings, ArtifactsFolder folder)
        {
            var path = "_VirtualLens_Root/Local/Transform/Accumulator/Controller";
            var speed = settings.DroneYawSpeedScale * 0.1f;
            var negative = Quaternion.Euler(0.0f, -speed, 0.0f);
            var positive = Quaternion.Euler(0.0f, speed, 0.0f);
            var negClip = new AnimationClip();
            AnimationClipEditor.AppendValue(negClip, path, typeof(Transform), "m_LocalRotation.x", negative.x);
            AnimationClipEditor.AppendValue(negClip, path, typeof(Transform), "m_LocalRotation.y", negative.y);
            AnimationClipEditor.AppendValue(negClip, path, typeof(Transform), "m_LocalRotation.z", negative.z);
            AnimationClipEditor.AppendValue(negClip, path, typeof(Transform), "m_LocalRotation.w", negative.w);
            var posClip = new AnimationClip();
            AnimationClipEditor.AppendValue(posClip, path, typeof(Transform), "m_LocalRotation.x", positive.x);
            AnimationClipEditor.AppendValue(posClip, path, typeof(Transform), "m_LocalRotation.y", positive.y);
            AnimationClipEditor.AppendValue(posClip, path, typeof(Transform), "m_LocalRotation.z", positive.z);
            AnimationClipEditor.AppendValue(posClip, path, typeof(Transform), "m_LocalRotation.w", positive.w);
            folder.CreateAsset(negClip);
            folder.CreateAsset(posClip);
            // VirtualLens2/Core/Animations/Placeholders/DroneYaw*.anim
            parameters.Add(AssetUtility.LoadAssetByGUID<AnimationClip>("b4050538ebd7d8e458e33ca45dbd3843"), posClip);
            parameters.Add(AssetUtility.LoadAssetByGUID<AnimationClip>("952ad7fdd2030264d9ade8b34e56ce8c"), negClip);
        }

        private static void RegisterFarPlaneMotion(
            MotionTemplateParameters parameters, ImplementationSettings settings, ArtifactsFolder folder)
        {
            var root = settings.Avatar;
            var clips = new[] { new AnimationClip(), new AnimationClip(), new AnimationClip() };
            var scale = 1.0f;
            foreach (var clip in clips)
            {
                foreach (var groupPath in CaptureCameraPaths)
                {
                    var group = HierarchyUtility.PathToObject(root, groupPath);
                    if (group == null) { continue; }
                    foreach (var camera in group.GetComponentsInChildren<Camera>(true))
                    {
                        var cameraPath = HierarchyUtility.RelativePath(root, camera.gameObject);
                        AnimationClipEditor.AppendValue(
                            clip, cameraPath, typeof(Camera), "far clip plane", scale * settings.ClippingFar);
                    }
                }
                foreach (var path in ComputeRendererPaths)
                {
                    AnimationClipEditor.AppendValue(
                        clip, path, typeof(MeshRenderer), "material._Far", scale * settings.ClippingFar);
                }
                folder.CreateAsset(clip);
                scale *= 10.0f;
            }
            // VirtualLens2/Core/Animations/Placeholders/FarPlane_[1,10,100]x.anim
            parameters.Add(AssetUtility.LoadAssetByGUID<AnimationClip>("218573be9fc8f3c4cab9db15b50f632e"), clips[0]);
            parameters.Add(AssetUtility.LoadAssetByGUID<AnimationClip>("0ef0ad9869d03e6488c695ae8debf683"), clips[1]);
            parameters.Add(AssetUtility.LoadAssetByGUID<AnimationClip>("b0f40d73030ae20429dd8544c42cdfbe"), clips[2]);
        }

        private static void RegisterAvatarScalingMotion(
            MotionTemplateParameters parameters, ImplementationSettings settings, ArtifactsFolder folder)
        {
            var root = settings.Avatar;
            var maxScale = Constants.MaxScaling;
            var negativeClip = new AnimationClip();
            var neutralClip = new AnimationClip();
            var positiveClip = new AnimationClip();
            var invNegativeClip = new AnimationClip();
            var invNeutralClip = new AnimationClip();
            var invPositiveClip = new AnimationClip();

            void SetPosition(AnimationClip clip, string path, Vector3 value)
            {
                AnimationClipEditor.AppendValue(clip, path, typeof(Transform), "m_LocalPosition.x", value.x);
                AnimationClipEditor.AppendValue(clip, path, typeof(Transform), "m_LocalPosition.y", value.y);
                AnimationClipEditor.AppendValue(clip, path, typeof(Transform), "m_LocalPosition.z", value.z);
            }

            void SetScale(AnimationClip clip, string path, float value)
            {
                AnimationClipEditor.AppendValue(clip, path, typeof(Transform), "m_LocalScale.x", value);
                AnimationClipEditor.AppendValue(clip, path, typeof(Transform), "m_LocalScale.y", value);
                AnimationClipEditor.AppendValue(clip, path, typeof(Transform), "m_LocalScale.z", value);
            }

            {
                // Mesh: Gizmo
                var path = "_VirtualLens_Root/Local/Transform/Gizmo";
                SetScale(negativeClip, path, 0.0f);
                SetScale(neutralClip, path, 1.0f);
                SetScale(positiveClip, path, maxScale);
            }
            {
                // Mesh: Controller (Sphere)
                var path = "_VirtualLens_Root/Local/Transform/Controller/Enabler/Rotation/Delta/Sphere";
                SetScale(negativeClip, path, 0.0f);
                SetScale(neutralClip, path, 1.0f);
                SetScale(positiveClip, path, maxScale);
            }
            {
                // Transform: Dead zone
                var path = "_VirtualLens_Root/Local/Transform/Controller/Enabler/Arm0/Arm1";
                var value = settings.DroneLinearDeadZoneSize;
                SetPosition(negativeClip, path, new Vector3(0.0f, 0.0f, 0.0f));
                SetPosition(neutralClip, path, new Vector3(value, 0.0f, 0.0f));
                SetPosition(positiveClip, path, new Vector3(value * maxScale, 0.0f, 0.0f));
            }
            {
                // Transform: Drone speed
                var path = "_VirtualLens_Root/Local/Transform/SpeedControl/Drone";
                var speed = settings.DroneLinearSpeedScale;
                SetScale(invNegativeClip, path, 0.0f);
                SetScale(invNeutralClip, path, speed);
                SetScale(invPositiveClip, path, maxScale * speed);
            }
            {
                // Camera: Near plane
                foreach (var groupPath in CaptureCameraPaths)
                {
                    var group = HierarchyUtility.PathToObject(root, groupPath);
                    if (group == null) { continue; }
                    foreach (var camera in group.GetComponentsInChildren<Camera>(true))
                    {
                        var cameraPath = HierarchyUtility.RelativePath(root, camera.gameObject);
                        AnimationClipEditor.AppendValue(
                            negativeClip, cameraPath, typeof(Camera), "near clip plane", settings.ClippingNear);
                        AnimationClipEditor.AppendValue(
                            neutralClip, cameraPath, typeof(Camera), "near clip plane", settings.ClippingNear);
                        AnimationClipEditor.AppendValue(
                            positiveClip, cameraPath, typeof(Camera), "near clip plane",
                            maxScale * settings.ClippingNear);
                    }
                }
                foreach (var path in ComputeRendererPaths)
                {
                    AnimationClipEditor.AppendValue(
                        negativeClip, path, typeof(MeshRenderer), "material._Near", settings.ClippingNear);
                    AnimationClipEditor.AppendValue(
                        neutralClip, path, typeof(MeshRenderer), "material._Near", settings.ClippingNear);
                    AnimationClipEditor.AppendValue(
                        positiveClip, path, typeof(MeshRenderer), "material._Near", maxScale * settings.ClippingNear);
                }
            }
            {
                // Camera: Touch screen
                var parent = MarkerDetector.DetectPreview(settings);
                var scale = parent.transform.lossyScale.y;
                var path = "_VirtualLens_Root/Local/Preview/Camera";
                var orthographicSize = scale * 0.5f * (16.0f / 9.0f);
                var nearClipPlane = scale * 0.02f;
                var farClipPlane = scale * 0.2f * settings.TouchScreenThickness;
                AnimationClipEditor.AppendValue(
                    negativeClip, path, typeof(Camera), "orthographic size", 0.0f);
                AnimationClipEditor.AppendValue(
                    neutralClip, path, typeof(Camera), "orthographic size", orthographicSize);
                AnimationClipEditor.AppendValue(
                    positiveClip, path, typeof(Camera), "orthographic size", maxScale * orthographicSize);
                AnimationClipEditor.AppendValue(
                    negativeClip, path, typeof(Camera), "near clip plane", 0.0f);
                AnimationClipEditor.AppendValue(
                    neutralClip, path, typeof(Camera), "near clip plane", nearClipPlane);
                AnimationClipEditor.AppendValue(
                    positiveClip, path, typeof(Camera), "near clip plane", maxScale * nearClipPlane);
                AnimationClipEditor.AppendValue(
                    negativeClip, path, typeof(Camera), "far clip plane", 0.0f);
                AnimationClipEditor.AppendValue(
                    neutralClip, path, typeof(Camera), "far clip plane", farClipPlane);
                AnimationClipEditor.AppendValue(
                    positiveClip, path, typeof(Camera), "far clip plane", maxScale * farClipPlane);
            }

            folder.CreateAsset(negativeClip);
            folder.CreateAsset(neutralClip);
            folder.CreateAsset(positiveClip);
            folder.CreateAsset(invNegativeClip);
            folder.CreateAsset(invNeutralClip);
            folder.CreateAsset(invPositiveClip);

            parameters.Add(
                AssetUtility.LoadAssetByGUID<AnimationClip>("517f8b3e9b5e9ba48b82a5fac6cb2f00"),
                neutralClip);
            parameters.Add(
                AssetUtility.LoadAssetByGUID<AnimationClip>("6c21e36a857210446b973ec421e67205"),
                negativeClip);
            parameters.Add(
                AssetUtility.LoadAssetByGUID<AnimationClip>("ff621afe1778d064fb295ab4e051dd0e"),
                positiveClip);
            parameters.Add(
                AssetUtility.LoadAssetByGUID<AnimationClip>("bf3b63f0b74c38643a62ee3c8aac1ab6"),
                invNeutralClip);
            parameters.Add(
                AssetUtility.LoadAssetByGUID<AnimationClip>("4914a1e1c43c25244af2ce714f57271e"),
                invNegativeClip);
            parameters.Add(
                AssetUtility.LoadAssetByGUID<AnimationClip>("f7be67ecd714d984297105a912e47c3a"),
                invPositiveClip);
        }

        private static void RegisterMaxBlurrinessMotion(
            MotionTemplateParameters parameters, ImplementationSettings settings, ArtifactsFolder folder)
        {
            var clip = new AnimationClip();
            foreach (var path in ComputeRendererPaths)
            {
                AnimationClipEditor.AppendValue(
                    clip, path, typeof(MeshRenderer), "material._MaxNumRings", settings.MaxBlurriness);
            }
            folder.CreateAsset(clip);
            // VirtualLens2/Core/Animations/Placeholders/MaxBlurriness.anim
            parameters.Add(AssetUtility.LoadAssetByGUID<AnimationClip>("3fa459b4565178345b1c4741517645ad"), clip);
        }

        private static void RegisterPreviewMaterialsMotion(
            MotionTemplateParameters parameters, ImplementationSettings settings, ArtifactsFolder folder)
        {
            // VirtualLens2/Materials/VirtualLensPreview.mat
            var placeholderMaterial = AssetUtility.LoadAssetByGUID<Material>("f9d9632b4c0a6f7439776c9bf3f64ad1");
            // VirtualLens2/Core/Materials/System/VirtualLensPreview.mat
            var actualMaterial = AssetUtility.LoadAssetByGUID<Material>("2bd4e12658720e549b0ccd10968d9ad9");

            var clip = new AnimationClip();
            foreach (var renderer in settings.Avatar.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (var i = 0; i < materials.Length; ++i)
                {
                    if (materials[i] != placeholderMaterial) { continue; }
                    var path = HierarchyUtility.RelativePath(settings.Avatar, renderer.gameObject);
                    var property = $"m_Materials.Array.data[{i}]";
                    AnimationClipEditor.AppendValue(clip, path, renderer.GetType(), property, actualMaterial);
                }
            }
            folder.CreateAsset(clip);
            // VirtualLens2/Core/Animations/Placeholders/ReplacePreviewMaterials.anim
            parameters.Add(AssetUtility.LoadAssetByGUID<AnimationClip>("c05e3ba36ad7e9a4d99677c51a5abdc1"), clip);
        }

        #endregion

        #region Parameter Drivers

        private static void RegisterDefaultParameters(AnimatorController controller, ImplementationSettings settings)
        {
            var layer = controller.layers.First(l => l.name == "VirtualLens2 Initialize");
            var state = layer.stateMachine.states.First(s => s.state.name == "Init").state;
            var driver = (VRCAvatarParameterDriver)state.behaviours[0];

            void RegisterValue(string name, float value)
            {
                driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
                {
                    name = ParameterPrefix + name, value = value
                });
            }

            var zoom = settings.FocalToValue(settings.FocalLengthDefault);
            var aperture = settings.ApertureEnabled
                ? settings.ApertureToValue(settings.ApertureFNumberDefault)
                : 0.0f;
            var distance = 0.0f;
            var exposure = settings.ExposureEnabled
                ? settings.ExposureToValue(settings.ExposureDefault)
                : 0.5f;

            RegisterValue("Control", 0);
            RegisterValue("Zoom", zoom);
            RegisterValue("Aperture", aperture);
            RegisterValue("Distance", distance);
            RegisterValue("Exposure", exposure);
            RegisterValue("X", 0.0f);
            RegisterValue("AFMode", (float)settings.DefaultAutoFocusMode);
            RegisterValue("TrackingSpeed", (float)settings.DefaultAutoFocusTrackingSpeed);
            RegisterValue("AFSpeed", (float)settings.DefaultFocusingSpeed);
            RegisterValue("Grid", (float)settings.DefaultGrid);
            RegisterValue("Information", settings.DefaultInformation ? 1.0f : 0.0f);
            RegisterValue("Level", settings.DefaultLevelMeter ? 1.0f : 0.0f);
            RegisterValue("Peaking", (float)settings.DefaultPeakingMode);
            RegisterValue("DepthEnabler", settings.DefaultDepthEnabler ? 1.0f : 0.0f);
            RegisterValue("PreviewHUD", 0.0f);
            RegisterValue("LocalPlayerMask", 1.0f);
            RegisterValue("RemotePlayerMask", 1.0f);
            RegisterValue("UIMask", 0.0f);
            
            RegisterValue("Version", Constants.Version);
            RegisterValue("Zoom Min", settings.FocalLengthMin);
            RegisterValue("Zoom Max", settings.FocalLengthMax);
            RegisterValue("Aperture Min", settings.ApertureFNumberMin);
            RegisterValue("Aperture Max", settings.ApertureFNumberMax);
            RegisterValue("Exposure Range", settings.ExposureRange);
            RegisterValue("Resolution X", settings.UseHighResolution ? 3840 : 1920);
            RegisterValue("Resolution Y", settings.UseHighResolution ? 2160 : 1080);
        }

        private static void RegisterQuickCalls(AnimatorController controller, ImplementationSettings settings)
        {
            var layer = controller.layers.First(l => l.name == "VirtualLens2 QuickCall");
            for (var i = 0; i < settings.QuickCalls.Count; ++i)
            {
                var item = settings.QuickCalls[i];
                var state = layer.stateMachine.states.First(s => s.state.name == i.ToString()).state;
                var driver = (VRCAvatarParameterDriver)state.behaviours[0];
                if (item.Focal != null)
                {
                    driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
                    {
                        name = ParameterPrefix + "Zoom",
                        value = settings.FocalToValue((float)item.Focal)
                    });
                }
                if (item.Aperture != null)
                {
                    driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
                    {
                        name = ParameterPrefix + "Aperture",
                        value = settings.ApertureToValue((float)item.Aperture)
                    });
                }
                if (item.Exposure != null)
                {
                    driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
                    {
                        name = ParameterPrefix + "Exposure",
                        value = settings.ExposureToValue((float)item.Exposure)
                    });
                }
            }
        }

        #endregion

        #region Utilities

        private static AV3EditorLib.WriteDefaultsOverrideMode SelectWriteDefaultsMode(AnimatorController controller)
        {
            bool hasWdOn = false;
            foreach (var layer in controller.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    if (state.state.name.Contains("(WD On)")) { continue; }
                    if (state.state.name.Contains("(WD Off)")) { continue; }
                    if (state.state.writeDefaultValues) { hasWdOn = true; }
                }
            }
            return hasWdOn
                ? AV3EditorLib.WriteDefaultsOverrideMode.ForceEnable
                : AV3EditorLib.WriteDefaultsOverrideMode.ForceDisable;
        }

        #endregion
    }
}
