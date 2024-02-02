using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using XWear.XWearPackage.Editor.Common;

namespace XWear.XWearPackage.Editor.Util
{
    public static class CustomEditorUtil
    {
        /// <summary>
        /// enumとラベルを結びつけてDrawするクラス
        /// </summary>
        [Serializable]
        public class UpdatableEnumPopupDrawer
        {
            [SerializeField]
            private int selectedIndex;

            [SerializeField]
            private Message[] valueLabels;

            public UpdatableEnumPopupDrawer(Message[] valueLabels)
            {
                this.valueLabels = valueLabels;
            }

            public T DrawWithConvertEnum<T>(string label, Action<T> onUpdate = null)
                where T : Enum
            {
                var newSelectedIndex = EditorGUILayout.Popup(
                    label,
                    selectedIndex,
                    valueLabels.Select(x => (string)x).ToArray()
                );
                var t = typeof(T);
                if (!t.IsEnum)
                {
                    throw new InvalidDataException("Type must be Enum");
                }

                var newEnumValue = (T)Enum.ToObject(t, newSelectedIndex);

                if (newSelectedIndex != selectedIndex)
                {
                    selectedIndex = newSelectedIndex;
                    onUpdate?.Invoke(newEnumValue);
                }

                return newEnumValue;
            }
        }

        public static bool DrawUpdatableToggle(
            string label,
            bool value,
            Action<bool> onUpdate = null
        )
        {
            var newValue = EditorGUILayout.Toggle(label, value);
            if (newValue != value)
            {
                onUpdate?.Invoke(newValue);
            }

            return newValue;
        }

        public static UnityEngine.Object DrawUpdatableObjectField(
            string label,
            UnityEngine.Object obj,
            Type objType,
            bool allowSceneObjects,
            Action<UnityEngine.Object> onUpdate = null
        )
        {
            var newValue = EditorGUILayout.ObjectField(label, obj, objType, allowSceneObjects);
            if (!ReferenceEquals(newValue, obj))
            {
                onUpdate?.Invoke(newValue);
            }

            return newValue;
        }

        public static int DrawUpdatableToolBar(
            int index,
            string[] labels,
            Action<int> onUpdate = null
        )
        {
            var newIndex = GUILayout.Toolbar(
                index,
                labels,
                new GUIStyle(EditorStyles.toolbarButton)
            );
            if (newIndex != index)
            {
                onUpdate?.Invoke(newIndex);
            }

            return newIndex;
        }

        private static readonly GUIContent IndentedButtonGUIContent = new GUIContent();

        public static bool DrawIndentedButton(string label)
        {
            var currentRect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            IndentedButtonGUIContent.text = label;
            var clicked = GUI.Button(position: currentRect, IndentedButtonGUIContent);
            IndentedButtonGUIContent.text = "";
            return clicked;
        }

        private static GUIContent _indoIconContent;

        private static GUIContent InfoIcon
        {
            get
            {
                if (_indoIconContent == null)
                {
                    _indoIconContent = EditorGUIUtility.IconContent("console.infoicon");
                }

                return _indoIconContent;
            }
        }

        private static readonly GUIStyle InfoIconStyle = new GUIStyle()
        {
            stretchWidth = false,
            stretchHeight = false,
            alignment = TextAnchor.UpperLeft,
            imagePosition = ImagePosition.ImageOnly,
            clipping = TextClipping.Clip,
            wordWrap = true,
            padding = new RectOffset() { },
        };

        private static readonly GUIStyle InfoMessageStyle = new GUIStyle(GUI.skin.label)
        {
            wordWrap = true,
            padding = new RectOffset() { bottom = 10 },
        };

        private static readonly GUIStyle BoxStyle = new GUIStyle(new GUIStyle(EditorStyles.helpBox))
        {
            padding = new RectOffset()
            {
                left = 6,
                right = 6,
                top = 10,
                bottom = 10
            },
        };

        public static void DrawInfo(string message)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(15 * EditorGUI.indentLevel);
                EditorGUILayout.BeginVertical(new GUIStyle(BoxStyle));
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField(InfoIcon, InfoIconStyle);
                        EditorGUILayout.LabelField(message, InfoMessageStyle);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
