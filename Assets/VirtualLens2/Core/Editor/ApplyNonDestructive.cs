#if WITH_NDMF

using System;
using System.Collections.Immutable;
using System.Linq;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VirtualLens2.AV3EditorLib;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

[assembly: ExportsPlugin(typeof(VirtualLens2.ApplyNonDestructive))]

namespace VirtualLens2
{
    public class ApplyNonDestructive : Plugin<ApplyNonDestructive>
    {
        public override string QualifiedName => "dev.logilabo.virtuallens2.apply-non-destructive";
        public override string DisplayName => "VirtualLens2";

        protected override void Configure()
        {
            InPhase(BuildPhase.Generating).Run("Apply VirtualLens2", ctx =>
            {
                var avatar = ctx.AvatarRootObject;

                // Find VirtualLens object
                var components = avatar
                    .GetComponentsInChildren<VirtualLensSettings>(true)
                    .Where(c => !c.gameObject.CompareTag("EditorOnly") && c.buildMode == BuildMode.NonDestructive)
                    .ToArray();
                if (components.Length == 0) { return; }
                if (components.Length > 1)
                {
                    EditorUtility.DisplayDialog(
                        "VirtualLens2",
                        "Failed to apply VirtualLens2.\n" +
                        "The avatar can contain up to one VirtualLens object.",
                        "OK");
                    throw new InvalidOperationException("Multiple VirtualLens Settings are found");
                }
                var component = components[0];

                // TODO Run migration?

                // Validate settings
                var messages = SettingsValidator.Validate(component);
                if (messages.Count(m => m.Type == MessageType.Error) > 0)
                {
                    EditorUtility.DisplayDialog(
                        "VirtualLens2",
                        "Failed to apply VirtualLens2.\n" +
                        "Please check validation report and fix problems.",
                        "OK");
                    Selection.activeObject = component;
                    throw new InvalidOperationException("Invalid VirtualLens configuration");
                }

                // Create FX layer if not exists
                CreateEmptyFXLayer(ctx);

                // TODO Fix Playable Layers?

                // Prepare abstract settings object
                var settings = new ImplementationSettings(component);

                // Generate marker objects
                GenerateMarkers(settings);

                // Apply VirtualLens2
                Applier.Apply(settings);

                // Purge VirtualLens Settings
                Object.DestroyImmediate(component);

                // Remove EditorOnly Objects
                void RemoveEditorOnlyRecur(GameObject obj, bool flag)
                {
                    if (obj.CompareTag("EditorOnly")) { flag = true; }
                    foreach (Transform tf in obj.transform)
                    {
                        RemoveEditorOnlyRecur(tf.gameObject, flag);
                    }
                    if (flag) { Object.DestroyImmediate(obj); }
                }

                var root = HierarchyUtility.PathToObject(settings.Avatar, "_VirtualLens_Root");
                RemoveEditorOnlyRecur(root, false);
            });
        }

        private static void CreateEmptyFXLayer(BuildContext ctx)
        {
            var descriptor = ctx.AvatarDescriptor;
            for (var i = 0; i < descriptor.baseAnimationLayers.Length; ++i)
            {
                var layer = descriptor.baseAnimationLayers[i];
                if (layer.type != VRCAvatarDescriptor.AnimLayerType.FX) { continue; }
                if (layer.animatorController != null) { break; }
                var asset = new AnimatorController { name = "FX" };
                AssetDatabase.AddObjectToAsset(asset, ctx.AssetContainer);
                asset.AddLayer("Base Layer");
                descriptor.baseAnimationLayers[i].isDefault = false;
                descriptor.baseAnimationLayers[i].animatorController = asset;
                break;
            }
        }

        private static void GenerateMarkers(ImplementationSettings settings)
        {
            if (settings.ScreenTouchers.All(obj => obj == null))
            {
                var markers = MarkerGenerator.GenerateScreenToucher(settings.Avatar);
                foreach (var obj in markers)
                {
                    PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely,
                        InteractionMode.AutomatedAction);
                }
                settings.ScreenTouchers = markers.ToImmutableList();
            }
            if (!settings.DroneController)
            {
                settings.DroneController = MarkerGenerator.GenerateDroneController(settings.Avatar);
                PrefabUtility.UnpackPrefabInstance(settings.DroneController, PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
            }
            if (!settings.RepositionOrigin)
            {
                settings.RepositionOrigin = MarkerGenerator.GenerateRepositionOrigin(settings.Avatar);
                PrefabUtility.UnpackPrefabInstance(settings.RepositionOrigin, PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
            }
            if (!settings.SelfieMarkerLeft || !settings.SelfieMarkerRight)
            {
                var pair = MarkerGenerator.GenerateEyeMarkers(
                    settings.Avatar,
                    !settings.SelfieMarkerLeft,
                    !settings.SelfieMarkerRight);
                foreach (var obj in pair)
                {
                    if (obj)
                    {
                        PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely,
                            InteractionMode.AutomatedAction);
                    }
                }
                if (!settings.SelfieMarkerLeft) { settings.SelfieMarkerLeft = pair[0]; }
                if (!settings.SelfieMarkerRight) { settings.SelfieMarkerRight = pair[1]; }
            }
        }
    }
}

#endif