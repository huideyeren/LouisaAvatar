using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VirtualLens2
{
    internal static class HierarchyUtil
    {
        private static Transform FindTransform(Transform root, string name)
        {
            if (root.name == name) { return root; }
            for (var i = 0; i < root.childCount; ++i)
            {
                var ret = FindTransform(root.GetChild(i), name);
                if (ret != null) { return ret; }
            }
            return null;
        }

        public static GameObject FindGameObject(GameObject root, string name)
        {
            var tf = FindTransform(root.transform, name);
            if (!tf) { return null; }
            return tf.gameObject;
        }


        public static GameObject FindPrefabInstance(GameObject root, string guid)
        {
            if (PrefabUtility.IsAnyPrefabInstanceRoot(root))
            {
                var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root);
                if (AssetDatabase.AssetPathToGUID(path) == guid)
                {
                    return root;
                }
            }
            foreach (Transform child in root.transform)
            {
                var ret = FindPrefabInstance(child.gameObject, guid);
                if (ret != null) { return ret; }
            }
            return null;
        }

        public static IList<GameObject> FindPrefabInstances(GameObject root, string guid)
        {
            var instances = new List<GameObject>();
            Recur(root);
            return instances;

            void Recur(GameObject obj)
            {
                if (PrefabUtility.IsAnyPrefabInstanceRoot(obj))
                {
                    var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
                    if (AssetDatabase.AssetPathToGUID(path) == guid)
                    {
                        instances.Add(obj);
                    }
                }
                foreach (Transform child in obj.transform) { Recur(child.gameObject); }
            }
        }


        public static GameObject PathToObject(GameObject root, string path)
        {
            var tokens = path.Split(new[] { '/' });
            var cur = root.transform;
            foreach (var token in tokens)
            {
                cur = cur.transform.Find(token);
                if (cur == null) { return null; }
            }
            return cur.gameObject;
        }

        public static Transform PathToTransform(Transform root, string path)
        {
            var tokens = path.Split('/');
            var cur = root.transform;
            foreach (var token in tokens)
            {
                cur = cur.transform.Find(token);
                if (cur == null) { return null; }
            }
            return cur;
        }
    }
}
