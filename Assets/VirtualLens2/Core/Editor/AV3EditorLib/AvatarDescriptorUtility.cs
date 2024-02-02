using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace VirtualLens2.AV3EditorLib
{
    /// <summary>
    /// Utility functions for manipulating <c>VRCAvatarDescriptor</c>.
    /// </summary>
    public class AvatarDescriptorUtility
    {
        /// <summary>
        /// Fixes broken playable layer types.
        /// </summary>
        /// <param name="descriptor">The avatar descriptor to be fixed.</param>
        /// <exception cref="ArgumentNullException"><c>descriptor</c> is null.</exception>
        public static void FixPlayableLayerTypes(VRCAvatarDescriptor descriptor)
        {
            if (descriptor == null) { throw new ArgumentNullException(nameof(descriptor)); }
            
            // Check isHuman
            var animator = descriptor.GetComponent<Animator>();
            if (!animator || !animator.avatar || !animator.avatar.isHuman) { return; }
            // Check number of FX layers
            var numFXLayers = descriptor.baseAnimationLayers.Count(
                layer => layer.type == VRCAvatarDescriptor.AnimLayerType.FX);
            if (numFXLayers == 1) { return; }
            // Fix layer types
            // https://feedback.vrchat.com/sdk-bug-reports/p/sdk202009250008-switching-a-rig-from-generic-to-the-humanoid-rig-type-causes-dup
            Undo.RecordObject(descriptor, "Fix playable layer types");
            descriptor.baseAnimationLayers[3].type = VRCAvatarDescriptor.AnimLayerType.Action;
            descriptor.baseAnimationLayers[4].type = VRCAvatarDescriptor.AnimLayerType.FX;
        }
        
        
        /// <summary>
        /// Gets or creates an animator controller for specific layer type.
        /// </summary>
        /// <param name="descriptor">The avatar descriptor contains playable layers.</param>
        /// <param name="type">The requested type of playable layer.</param>
        /// <returns>The animator controller for specified layer type.</returns>
        /// <exception cref="ArgumentNullException"><c>descriptor</c> is null.</exception>
        public static AnimatorController GetPlayableLayer(
            VRCAvatarDescriptor descriptor, VRCAvatarDescriptor.AnimLayerType type)
        {
            if (descriptor == null) { throw new ArgumentNullException(nameof(descriptor)); }
            foreach (var layer in descriptor.baseAnimationLayers)
            {
                if (layer.type != type) { continue; }
                if (layer.animatorController != null) { return layer.animatorController as AnimatorController; }
            }
            return null;
        }


        /// <summary>
        /// Gets or creates an animator controller for specific layer type.
        /// </summary>
        /// <param name="descriptor">The avatar descriptor contains playable layers.</param>
        /// <param name="type">The requested type of playable layer.</param>
        /// <param name="folder">The artifacts folder helper for saving created assets.</param>
        /// <returns>The animator controller for specified layer type.</returns>
        /// <exception cref="ArgumentNullException"><c>descriptor</c> or <c>folder</c> is null.</exception>
        public static AnimatorController GetOrCreatePlayableLayer(
            VRCAvatarDescriptor descriptor, VRCAvatarDescriptor.AnimLayerType type, ArtifactsFolder folder)
        {
            if (descriptor == null) { throw new ArgumentNullException(nameof(descriptor)); }
            if (folder == null) { throw new ArgumentNullException(nameof(folder)); }
            foreach (var layer in descriptor.baseAnimationLayers)
            {
                if (layer.type != type) { continue; }
                if (layer.animatorController != null) { return layer.animatorController as AnimatorController; }
            }

            Undo.RecordObject(descriptor, "Create playable layer");
            descriptor.customizeAnimationLayers = true;
            for (var i = 0; i < descriptor.baseAnimationLayers.Length; ++i)
            {
                var layer = descriptor.baseAnimationLayers[i];
                if (layer.type != type) { continue; }
                var path = folder.GenerateAssetPath<AnimatorController>();
                var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
                Undo.RegisterCreatedObjectUndo(controller, "Create animator controller");
                descriptor.baseAnimationLayers[i].isDefault = false;
                descriptor.baseAnimationLayers[i].animatorController = controller;
                return controller;
            }
            return null;
        }
        

        /// <summary>
        /// Gets or creates an expressions menu for an avatar.
        /// </summary>
        /// <param name="descriptor">The avatar descriptor contains an expressions menu.</param>
        /// <param name="folder">The artifacts folder helper for saving created assets.</param>
        /// <returns>The expressions menu for the avatar.</returns>
        /// <exception cref="ArgumentNullException"><c>descriptor</c> or <c>folder</c> is null.</exception>
        public static VRCExpressionsMenu GetOrCreateExpressionsMenu(
            VRCAvatarDescriptor descriptor, ArtifactsFolder folder)
        {
            if (descriptor == null) { throw new ArgumentNullException(nameof(descriptor)); }
            if (folder == null) { throw new ArgumentNullException(nameof(folder)); }
            if (descriptor.customExpressions && descriptor.expressionsMenu != null)
            {
                return descriptor.expressionsMenu;
            }
            
            Undo.RecordObject(descriptor, "Create expressions menu");
            descriptor.customExpressions = true;
            if (descriptor.expressionsMenu != null) { return descriptor.expressionsMenu; }
            var obj = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            folder.CreateAsset(obj);
            descriptor.expressionsMenu = obj;
            return obj;
        }
        

        /// <summary>
        /// Gets or creates expression parameters for an avatar.
        /// </summary>
        /// <param name="descriptor">The avatar descriptor contains expression parameters.</param>
        /// <param name="folder">The artifacts folder helper for saving created assets.</param>
        /// <returns>The expression parameters for the avatar.</returns>
        /// <exception cref="ArgumentNullException"><c>descriptor</c> or <c>folder</c> is null.</exception>
        public static VRCExpressionParameters GetOrCreateExpressionParameters(
            VRCAvatarDescriptor descriptor, ArtifactsFolder folder)
        {
            if (descriptor == null) { throw new ArgumentNullException(nameof(descriptor)); }
            if (folder == null) { throw new ArgumentNullException(nameof(folder)); }
            if (descriptor.customExpressions && descriptor.expressionParameters != null)
            {
                return descriptor.expressionParameters;
            }
            
            Undo.RecordObject(descriptor, "Create expression parameters");
            descriptor.customExpressions = true;
            if (descriptor.expressionParameters != null) { return descriptor.expressionParameters; }
            var obj = ScriptableObject.CreateInstance<VRCExpressionParameters>();
            folder.CreateAsset(obj);
            descriptor.expressionParameters = obj;
            return obj;
        }
    }
}