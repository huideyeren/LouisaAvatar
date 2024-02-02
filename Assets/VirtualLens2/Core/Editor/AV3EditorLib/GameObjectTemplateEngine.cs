using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VirtualLens2.AV3EditorLib
{
    /// <summary>
    /// Replaces references to placeholder game objects by actual objects.
    /// </summary>
    public static class GameObjectTemplateEngine
    {
        /// <summary>
        /// Replaces references to placeholder game objects by actual objects.
        /// </summary>
        /// <param name="root">The root of template tree.</param>
        /// <param name="parameters">Mappings from placeholder names to actual objects.</param>
        /// <exception cref="ArgumentNullException"><c>root</c> or <c>parameters</c> is <c>null</c>.</exception>
        public static void Apply(GameObject root, GameObjectTemplateParameters parameters)
        {
            const string prefix = "__AV3EL_TEMPLATE__";
            
            // Validate arguments
            if (root == null) { throw new ArgumentNullException();}
            if (parameters == null) { throw new ArgumentNullException(); }

            var templateRoot = root.transform.Find(prefix);
            if (templateRoot == null)
            {
                Debug.LogWarning("Target object has no template placeholders.");
                return;
            }

            // Enumerate placeholders
            var mapping = new Dictionary<Transform, GameObject>(
                new ReferenceEqualityComparer<Transform>());
            foreach (Transform child in templateRoot)
            {
                if (!parameters.ContainsKey(child.name))
                {
                    Debug.LogWarning($"{nameof(parameters)} does not contains value for '{child.name}'.");
                    continue;
                }
                mapping.Add(child, parameters[child.name]);
            }

            // Apply template
            void ApplyBindingsRecur(GameObject cur)
            {
                foreach (var comp in cur.GetComponents<Component>())
                {
                    Undo.RecordObject(comp, "Apply template");
                    var so = new SerializedObject(comp);
                    var prop = so.GetIterator();
                    while (prop.NextVisible(true))
                    {
                        if (prop.propertyType != SerializedPropertyType.ObjectReference) { continue; }
                        if (prop.objectReferenceValue is GameObject targetObject)
                        {
                            var key = targetObject.transform;
                            if (!mapping.ContainsKey(key)) { continue; }
                            prop.objectReferenceValue = mapping[key];
                        }
                        else if (prop.objectReferenceValue is Component targetComponent)
                        {
                            var key = targetComponent.transform;
                            if (!mapping.ContainsKey(key)) { continue; }
                            var type = prop.objectReferenceValue.GetType();
                            prop.objectReferenceValue = mapping[key].GetComponent(type);
                        }
                    }
                    so.ApplyModifiedProperties();
                }
                foreach (Transform child in cur.transform)
                {
                    if (cur == root && child.name == prefix) { continue; }
                    ApplyBindingsRecur(child.gameObject);
                }
            }

            ApplyBindingsRecur(root);
            
            // Remove placeholders
            if (PrefabUtility.IsPartOfAnyPrefab(templateRoot))
            {
                if (!templateRoot.gameObject.tag.Equals("EditorOnly"))
                {
                    Debug.LogWarning(
                        "Placeholder object in a prefab cannot be removed. They should be tagged as EditorOnly");
                }
            }
            else
            {
                Undo.DestroyObjectImmediate(templateRoot.gameObject);
            }
        }
    }
}