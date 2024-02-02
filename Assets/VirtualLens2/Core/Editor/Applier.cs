using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using VirtualLens2.AV3EditorLib;
using VRC.SDK3.Avatars.Components;

namespace VirtualLens2
{
    internal static class Applier
    {
        public static void Remove(
            GameObject avatar, GameObject cameraRoot, GameObject nonPreviewRoot,
            [CanBeNull] string artifactsFolder)
        {
            AnimatorControllerGenerator.Clear(avatar);
            ObjectGenerator.Clear(avatar, cameraRoot, nonPreviewRoot);
            ExpressionMenuGenerator.Clear(avatar);
            ExpressionParametersGenerator.Clear(avatar);
            if (!string.IsNullOrEmpty(artifactsFolder))
            {
                var folder = new ArtifactsFolder(artifactsFolder);
                folder.Clear();
            }
            if (avatar != null) EditorUtility.SetDirty(avatar);
            AssetDatabase.SaveAssets();
        }

        public static void Apply(ImplementationSettings settings)
        {
            const string progressBarTitle = "Applying VirtualLens2...";
            try
            {
                EditorUtility.DisplayProgressBar(progressBarTitle, "Loading settings", 0.1f);
                var folder = new ArtifactsFolder(settings.ArtifactsFolder);
                if (settings.ClearArtifactsFolder) folder.Clear();

                // Force enable custom animation layers and expressions
                var avatar = settings.Avatar.GetComponent<VRCAvatarDescriptor>();
                avatar.customizeAnimationLayers = true;
                avatar.customExpressions = true;

                EditorUtility.DisplayProgressBar(progressBarTitle, "Generating game objects", 0.2f);
                var objectSet = ObjectGenerator.Generate(settings, folder);

                EditorUtility.DisplayProgressBar(progressBarTitle, "Generating expression parameters", 0.4f);
                ExpressionParametersGenerator.Generate(settings, objectSet, folder);

                EditorUtility.DisplayProgressBar(progressBarTitle, "Generating expressions menu", 0.5f);
                ExpressionMenuGenerator.Generate(settings, objectSet, folder);

                EditorUtility.DisplayProgressBar(progressBarTitle, "Generating animations", 0.6f);
                AnimatorControllerGenerator.Generate(settings, folder, objectSet);
                settings.CameraRoot.SetActive(false);

                if (settings.BuildMode == BuildMode.Destructive)
                {
                    EditorUtility.DisplayProgressBar(progressBarTitle, "Saving generated assets", 0.9f);
                    EditorUtility.SetDirty(avatar);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    // Workaround: prefabs placed in build-time tasks should be unpacked
                    if (objectSet.VirtualLensRoot && PrefabUtility.IsAnyPrefabInstanceRoot(objectSet.VirtualLensRoot))
                    {
                        PrefabUtility.UnpackPrefabInstance(objectSet.VirtualLensRoot, PrefabUnpackMode.Completely,
                            InteractionMode.AutomatedAction);
                    }
                    if (objectSet.SelfieDetectorMarkers &&
                        PrefabUtility.IsAnyPrefabInstanceRoot(objectSet.SelfieDetectorMarkers))
                    {
                        PrefabUtility.UnpackPrefabInstance(objectSet.SelfieDetectorMarkers, PrefabUnpackMode.Completely,
                            InteractionMode.AutomatedAction);
                    }
                }
            }
            catch (Exception)
            {
                EditorUtility.DisplayDialog(
                    "VirtualLens2",
                    "Failed to implement VirtualLens2 unexpectedly.\n" +
                    "Please report the issue with console logs to the developer.",
                    "OK");
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
