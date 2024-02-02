using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ExporterMessages = XWear.XWearPackage.Editor.Common.MessagesContainer.ExporterMessages;
using CommonMessages = XWear.XWearPackage.Editor.Common.MessagesContainer.CommonMessage;

namespace XWear.XWearPackage.Editor.Util.ExtraDrawer
{
    [Serializable]
    public class AvatarAdvancedDrawer
    {
        [SerializeField]
        private bool foldOut;

        private bool _addFlag;
        private readonly List<int> _deleteCache = new List<int>();

        private readonly Dictionary<int, GameObject> _updateCache =
            new Dictionary<int, GameObject>();

        private static GUIStyle InfoTextStyle => new(EditorStyles.label) { wordWrap = true };

        public void Draw(List<GameObject> exportChildren, Action onUpdate)
        {
            foldOut = EditorGUILayout.Foldout(
                foldOut,
                ExporterMessages.LabelAdvancedOptionsFoldOut
            );

            EditorGUI.indentLevel++;

            if (foldOut)
            {
                DrawInfo();
                EditorGUILayout.Space();
                DrawChildren(exportChildren, onUpdate);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawInfo()
        {
            EditorGUILayout.LabelField(ExporterMessages.TextAdvancedSettingsInfo, InfoTextStyle);
        }

        private void DrawChildren(List<GameObject> exportChildren, Action onUpdate)
        {
            EditorGUILayout.LabelField(ExporterMessages.LabelAvatarDressList);

            _addFlag = false;

            if (CustomEditorUtil.DrawIndentedButton(CommonMessages.AddToList))
            {
                _addFlag = true;
                exportChildren.Add(null);
            }

            if (exportChildren.Count == 0)
            {
                CustomEditorUtil.DrawInfo(ExporterMessages.MessageAvatarDressListIsEmpty);
            }

            for (var index = 0; index < exportChildren.Count; index++)
            {
                var child = exportChildren[index];
                EditorGUILayout.BeginHorizontal();

                var indexTmp = index;
                CustomEditorUtil.DrawUpdatableObjectField(
                    "",
                    child,
                    typeof(GameObject),
                    true,
                    newValue =>
                    {
                        _updateCache.Add(indexTmp, (GameObject)newValue);
                    }
                );

                if (GUILayout.Button(CommonMessages.DeleteFromList))
                {
                    _deleteCache.Add(indexTmp);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (_updateCache.Count > 0)
            {
                foreach (var kvp in _updateCache)
                {
                    exportChildren[kvp.Key] = kvp.Value;
                }
            }

            if (_deleteCache.Count > 0)
            {
                foreach (var deleteTarget in _deleteCache)
                {
                    exportChildren.RemoveAt(deleteTarget);
                }
            }

            if (_addFlag || _updateCache.Count > 0 || _deleteCache.Count > 0)
            {
                onUpdate?.Invoke();
            }

            if (_updateCache.Count > 0)
            {
                _updateCache.Clear();
            }

            if (_deleteCache.Count > 0)
            {
                _deleteCache.Clear();
            }
        }
    }
}
