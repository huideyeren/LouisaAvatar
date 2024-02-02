using System;
using UnityEditor;
using UnityEngine;
using VirtualLens2.AV3EditorLib;

namespace VirtualLens2
{
    internal static class SettingsPrefabMigrator
    {
        public static bool IsInstanceOfDefaultPrefab(GameObject obj)
        {
            bool isFirst = true;
            while (PrefabUtility.IsAnyPrefabInstanceRoot(obj))
            {
                var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
                if (!isFirst) { PrefabUtility.UnloadPrefabContents(obj); }
                isFirst = false;
                var guid = AssetDatabase.AssetPathToGUID(path);
                // VirtualLens2/Prefabs/DefaultSettings.prefab
                if (guid == "3c8a8e9b6ad5718489e23501fa495408") { return true; }
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (PrefabUtility.GetPrefabAssetType(prefab) != PrefabAssetType.Variant) { return false; }
                obj = PrefabUtility.LoadPrefabContents(path);
            }
            if (!isFirst) { PrefabUtility.UnloadPrefabContents(obj); }
            return false;
        }

        public static bool IsVariantOfDefaultPrefab(GameObject obj)
        {
            if (PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.Variant) { return false; }
            obj = PrefabUtility.LoadPrefabContents(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj));
            var ret = IsInstanceOfDefaultPrefab(obj);
            PrefabUtility.UnloadPrefabContents(obj);
            return ret;
        }

        public static void ReplaceByDefaultPrefabInstance(GameObject obj)
        {
            if (IsInstanceOfDefaultPrefab(obj)) { return; }
            
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Replace VirtualLens Settings");
            var undoGroupIndex = Undo.GetCurrentGroup();

            // VirtualLens2/Prefabs/DefaultSettings.prefab
            var prefab = AssetUtility.LoadAssetByGUID<GameObject>("3c8a8e9b6ad5718489e23501fa495408");
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                throw new ApplicationException(
                    "Failed to instantiate DefaultSettings prefab.\n" +
                    "Please reimport VirtualLens2 package.");
            }
            instance.transform.parent = obj.transform.parent;
            instance.transform.SetSiblingIndex(obj.transform.GetSiblingIndex());

            var oldObject = obj.GetComponent<VirtualLensSettings>();
            var newObject = instance.GetComponent<VirtualLensSettings>();
            EditorUtility.CopySerializedIfDifferent(oldObject, newObject);

            var name = obj.name;
            Undo.DestroyObjectImmediate(obj);

            instance.name = name;
            Undo.RegisterCreatedObjectUndo(instance, "Instantiate DefaultSettings");
            
            Undo.CollapseUndoOperations(undoGroupIndex);
        }
    }

}