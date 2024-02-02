using System;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualLens2.AV3EditorLib
{
    /// <summary>
    /// Utility functions for hierarchy related operations.
    /// </summary>
    public static class HierarchyUtility
    {
        /// <summary>
        /// Determines if one object is a descendant of another.
        /// </summary>
        /// <param name="root">The object that are expected to be an ancestor.</param>
        /// <param name="target">The object that are expected to be a descendant.</param>
        /// <returns><c>true</c> if <c>target</c> is a descendant of <c>root</c>, <c>false</c> if otherwise.</returns>
        public static bool IsDescendant(Transform root, Transform target)
        {
            if (root == null || target == null) { return false; }
            if (root == target) { return true; }
            return IsDescendant(root, target.parent);
        }

        /// <summary>
        /// Determines if one object is a descendant of another.
        /// </summary>
        /// <param name="root">The object that are expected to be an ancestor.</param>
        /// <param name="target">The object that are expected to be a descendant.</param>
        /// <returns><c>true</c> if <c>target</c> is a descendant of <c>root</c>, <c>false</c> if otherwise.</returns>
        public static bool IsDescendant(GameObject root, GameObject target)
        {
            if (root == null || target == null) { return false; }
            return IsDescendant(root.transform, target.transform);
        }


        /// <summary>
        /// Returns a relative path from one object to another.
        /// </summary>
        /// <param name="root">The source object.</param>
        /// <param name="target">The destination object.</param>
        /// <returns>The relative path from <c>root</c> to <c>target</c>.</returns>
        /// <exception cref="InvalidOperationException"><c>target</c> is not a descendant of <c>root</c>.</exception>
        public static string RelativePath(Transform root, Transform target)
        {
            if (root == null || target == null)
            {
                throw new InvalidOperationException("target is not a descendant of root");
            }
            if (root == target) { return ""; }
            var tokens = new List<string>();
            var cur = target;
            while (cur != null && cur != root)
            {
                tokens.Add(cur.name);
                cur = cur.parent;
            }
            if (cur == null)
            {
                throw new InvalidOperationException("target is not a descendant of root");
            }
            tokens.Reverse();
            return string.Join("/", tokens);
        }

        /// <summary>
        /// Returns a relative path from one object to another.
        /// </summary>
        /// <param name="root">The source object.</param>
        /// <param name="target">The destination object.</param>
        /// <returns>The relative path from <c>root</c> to <c>target</c>.</returns>
        /// <exception cref="InvalidOperationException"><c>target</c> is not a descendant of <c>root</c>.</exception>
        public static string RelativePath(GameObject root, GameObject target)
        {
            if (root == null || target == null)
            {
                throw new InvalidOperationException("target is not a descendant of root");
            }
            return RelativePath(root.transform, target.transform);
        }


        /// <summary>
        /// Gets an object from root object and relative path.
        /// </summary>
        /// <param name="root">The object used as an origin.</param>
        /// <param name="path">The path from <c>root</c> to the target object.</param>
        /// <returns>The object retrieved from parameters, or <c>null</c> if an object was not found.</returns>
        /// <exception cref="ArgumentNullException"><c>path</c> is <c>null</c>.</exception>
        public static Transform PathToObject(Transform root, string path)
        {
            if (path == null) { throw new ArgumentNullException(nameof(path)); }
            if (root == null) { return null; }
            var cur = root;
            foreach (var token in path.Split('/'))
            {
                cur = cur.Find(token);
                if (cur == null) { return null; }
            }
            return cur;
        }

        /// <summary>
        /// Gets an object from root object and relative path.
        /// </summary>
        /// <param name="root">The object used as an origin.</param>
        /// <param name="path">The path from <c>root</c> to the target object.</param>
        /// <returns>The object retrieved from parameters, or <c>null</c> if an object was not found.</returns>
        /// <exception cref="ArgumentNullException"><c>path</c> is <c>null</c>.</exception>
        public static GameObject PathToObject(GameObject root, string path)
        {
            if (path == null) { throw new ArgumentNullException(nameof(path)); }
            if (root == null) { return null; }
            return PathToObject(root.transform, path)?.gameObject;
        }
    }
}