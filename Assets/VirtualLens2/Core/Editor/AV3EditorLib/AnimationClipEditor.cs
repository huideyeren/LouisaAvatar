using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VirtualLens2.AV3EditorLib
{
    /// <summary>
    /// Utility functions for manipulating animation clips.
    /// </summary>
    public static class AnimationClipEditor
    {
        /// <summary>
        /// Returns a deep copy of an animation clip.
        /// </summary>
        /// <param name="clip">The animation clip to be cloned.</param>
        /// <returns>The deep copy of <c>clip</c>.</returns>
        public static AnimationClip Clone(AnimationClip clip)
        {
            if (clip == null) { return null; }
            return Object.Instantiate(clip);
        }


        /// <summary>
        /// Appends a curve with single point.
        /// </summary>
        /// <param name="clip">The animation clip to be modified.</param>
        /// <param name="path">The path of the object that is animated.</param>
        /// <param name="type">The type of the property to be animated.</param>
        /// <param name="prop">The name of the property to be animated.</param>
        /// <param name="value">The value of the property.</param>
        /// <exception cref="ArgumentNullException"><c>clip</c>, <c>path</c> or <c>prop</c> is <c>null</c>.</exception>
        public static void AppendValue(AnimationClip clip, string path, Type type, string prop, float value)
        {
            if (clip == null) { throw new ArgumentNullException(nameof(clip)); }
            if (path == null) { throw new ArgumentNullException(nameof(path)); }
            if (prop == null) { throw new ArgumentNullException(nameof(prop)); }

            Undo.RecordObject(clip, "Append animation curve");
            var binding = new EditorCurveBinding() {path = path, propertyName = prop, type = type};
            var curve = new AnimationCurve();
            curve.AddKey(0.0f, value);
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }

        /// <summary>
        /// Appends a curve with single point.
        /// </summary>
        /// <param name="clip">The animation clip to be modified.</param>
        /// <param name="path">The path of the object that is animated.</param>
        /// <param name="type">The type of the property to be animated.</param>
        /// <param name="prop">The name of the property to be animated.</param>
        /// <param name="value">The value of the property.</param>
        /// <exception cref="ArgumentNullException"><c>clip</c>, <c>path</c> or <c>prop</c> is <c>null</c>.</exception>
        public static void AppendValue(AnimationClip clip, string path, Type type, string prop, bool value)
        {
            AppendValue(clip, path, type, prop, value ? 1.0f : 0.0f);
        }

        /// <summary>
        /// Appends a curve with single point.
        /// </summary>
        /// <param name="clip">The animation clip to be modified.</param>
        /// <param name="path">The path of the object that is animated.</param>
        /// <param name="type">The type of the property to be animated.</param>
        /// <param name="prop">The name of the property to be animated.</param>
        /// <param name="value">The value of the property.</param>
        /// <exception cref="ArgumentNullException"><c>clip</c>, <c>path</c> or <c>prop</c> is <c>null</c>.</exception>
        public static void AppendValue(AnimationClip clip, string path, Type type, string prop, Object value)
        {
            if (clip == null) { throw new ArgumentNullException(nameof(clip)); }
            if (path == null) { throw new ArgumentNullException(nameof(path)); }
            if (prop == null) { throw new ArgumentNullException(nameof(prop)); }
            
            Undo.RecordObject(clip, "Append animation curve");
            var binding = new EditorCurveBinding() {path = path, propertyName = prop, type = type};
            var keyframes = new[] {new ObjectReferenceKeyframe() {time = 0.0f, value = value}};
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        }


        /// <summary>
        /// Appends a curve with sequence of values.
        /// </summary>
        /// <param name="clip">The animation clip to be modified.</param>
        /// <param name="path">The path of the object that is animated.</param>
        /// <param name="type">The type of the property to be animated.</param>
        /// <param name="prop">The name of the property to be animated.</param>
        /// <param name="value">The sequence of the property values.</param>
        /// <exception cref="ArgumentNullException"><c>clip</c>, <c>path</c>, <c>prop</c> or <c>values</c> is <c>null</c>.</exception>
        public static void AppendValue(AnimationClip clip, string path, Type type, string prop, IEnumerable<float> values)
        {
            if (clip == null) { throw new ArgumentNullException(nameof(clip)); }
            if (path == null) { throw new ArgumentNullException(nameof(path)); }
            if (prop == null) { throw new ArgumentNullException(nameof(prop)); }
            if (values == null) { throw new ArgumentNullException(nameof(values)); }

            Undo.RecordObject(clip, "Append animation curve");
            var binding = new EditorCurveBinding() {path = path, propertyName = prop, type = type};
            var curve = new AnimationCurve();
            var index = 0;
            foreach (var x in values)
            {
                curve.AddKey(index, x);
                ++index;
            }
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }


        /// <summary>
        /// Merges two animation clips.
        /// </summary>
        /// <param name="destination">The animation clip to be modified.</param>
        /// <param name="source">The animation clip to be merged with the <c>destination</c>.</param>
        /// <exception cref="ArgumentNullException"><c>destination</c> or <c>source</c> is <c>null</c>.</exception>
        public static void Merge(AnimationClip destination, AnimationClip source)
        {
            if (destination == null) { throw new ArgumentNullException(nameof(destination)); }
            if (source == null) { throw new ArgumentNullException(nameof(source)); }
            
            Undo.RecordObject(destination, "Merge animation clips");
            var instance = Object.Instantiate(source);
            foreach (var binding in AnimationUtility.GetCurveBindings(instance))
            {
                var curve = AnimationUtility.GetEditorCurve(instance, binding);
                AnimationUtility.SetEditorCurve(destination, binding, curve);
            }
            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(instance))
            {
                var curve = AnimationUtility.GetObjectReferenceCurve(instance, binding);
                AnimationUtility.SetObjectReferenceCurve(destination, binding, curve);
            }
        }


        private const string TemplatePrefix = "__AV3EL_TEMPLATE__/";

        /// <summary>
        /// Determines if an animation clip has templated bindings.
        /// </summary>
        /// <param name="clip">The animation clip to be determined.</param>
        /// <returns><c>true</c> if <c>clip</c> has templated bindings, or <c>false</c> if otherwise.</returns>
        public static bool IsTemplate(AnimationClip clip)
        {
            if (clip == null) { return false; }
            var curveBindings = AnimationUtility.GetCurveBindings(clip);
            var objectReferenceCurveBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            if (curveBindings.Any(binding => binding.path.StartsWith(TemplatePrefix))) { return true; }
            if (objectReferenceCurveBindings.Any(binding => binding.path.StartsWith(TemplatePrefix))) { return true; }
            return false;
        }

        /// <summary>
        /// Replaces templated bindings in an animation clip.
        /// </summary>
        /// <param name="clip">The animation clip to be modified.</param>
        /// <param name="root">The root object for determining paths in the hierarchy.</param>
        /// <param name="parameters">The set of name-object pairs representing parameter mappings.</param>
        /// <exception cref="ArgumentNullException"><c>clip</c>, <c>root</c> or <c>parameters</c> is <c>null</c>.</exception>
        public static void ProcessTemplate(AnimationClip clip, GameObject root, AnimationTemplateParameters parameters)
        {
            if (clip == null) { throw new ArgumentNullException(nameof(clip)); }
            if (root == null) { throw new ArgumentNullException(nameof(root)); }
            if (parameters == null) { throw new ArgumentNullException(nameof(parameters)); }
            
            var keys = new List<string>();
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                if (!binding.path.StartsWith(TemplatePrefix)) { continue; }
                keys.Add(binding.path.Substring(TemplatePrefix.Length));
            }
            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
            {
                if (!binding.path.StartsWith(TemplatePrefix)) { continue; }
                keys.Add(binding.path.Substring(TemplatePrefix.Length));
            }
            foreach (var key in keys)
            {
                foreach (var obj in parameters[key])
                {
                    if (!HierarchyUtility.IsDescendant(root, obj))
                    {
                        throw new InvalidOperationException($"Object {obj} is not a descendant of root.");
                    }
                }
            }
            if (keys.Count == 0) { return; }

            Undo.RecordObject(clip, "Process animation template");
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                if (!binding.path.StartsWith(TemplatePrefix)) { continue; }
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                var key = binding.path.Substring(TemplatePrefix.Length);
                foreach (var obj in parameters[key])
                {
                    var newBinding = new EditorCurveBinding()
                    {
                        path = HierarchyUtility.RelativePath(root, obj),
                        propertyName = binding.propertyName,
                        type = binding.type
                    };
                    AnimationUtility.SetEditorCurve(clip, newBinding, curve);
                }
                AnimationUtility.SetEditorCurve(clip, binding, null);
            }
            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
            {
                if (!binding.path.StartsWith(TemplatePrefix)) { continue; }
                var curve = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                var key = binding.path.Substring(TemplatePrefix.Length);
                foreach (var obj in parameters[key])
                {
                    var newBinding = new EditorCurveBinding()
                    {
                        path = HierarchyUtility.RelativePath(root, obj),
                        propertyName = binding.propertyName,
                        type = binding.type
                    };
                    AnimationUtility.SetObjectReferenceCurve(clip, newBinding, curve);
                }
                AnimationUtility.SetObjectReferenceCurve(clip, binding, null);
            }
        }
    }
}