using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XWear.XWearPackage.Editor.Common;
using XWear.XWearPackage.Editor.Util;

namespace XWear.XWearPackage.Editor
{
    [Serializable]
    public class TabContent
    {
        public string label;

        [SerializeReference]
        public EditorContent editorContent;
    }

    [Serializable]
    public class XWearPackageEditorWindow : EditorWindow
    {
        private const string EditorWindowSizeXPrefKey = "VROID_XWEAR_PACKAGE_WINDOW_SIZE_X";
        private const string EditorWindowSizeYPrefKey = "VROID_XWEAR_PACKAGE_WINDOW_SIZE_Y";

        [SerializeField]
        private List<TabContent> tabContents;

        [SerializeField]
        private int selectedTabIndex;

        private string[] _tabLabels;

        [SerializeField]
        private MessagesContainer.Language currentLanguage;

        [SerializeField]
        private List<string> logs = new List<string>();

        private Vector2 _logScroll;

        [SerializeField]
        private GUIStyle contentStyle;

        [MenuItem("VRoid/XWear Package Window")]
        private static void Init()
        {
            var w = EditorWindow.CreateWindow<XWearPackageEditorWindow>();
            w.Show();
            w.SetWindowSize();
        }

        private void SetWindowSize()
        {
            var currentPosition = position;
            var x = EditorPrefs.GetFloat(EditorWindowSizeXPrefKey, 570);
            var y = EditorPrefs.GetFloat(EditorWindowSizeYPrefKey, 620);
            currentPosition.size = new Vector2(x, y);
        }

        private void Awake()
        {
            tabContents = new List<TabContent>
            {
                new TabContent
                {
                    label = "Exporter",
                    editorContent = new PackageExporterContent(this)
                },
                new TabContent
                {
                    label = "Importer",
                    editorContent = new PackageImporterContent(this)
                }
            };

            _tabLabels = tabContents.Select(x => x.label).ToArray();
            currentLanguage = MessagesContainer.CurrentLanguage;

            contentStyle = new GUIStyle()
            {
                padding = new RectOffset()
                {
                    left = 6,
                    right = 6,
                    top = 10,
                    bottom = 10
                },
            };

            foreach (var tabContent in tabContents)
            {
                Undo.undoRedoPerformed += tabContent.editorContent.OnUndoRedo;
            }

            Undo.undoRedoPerformed += Repaint;
        }

        private void OnFocus()
        {
            tabContents[selectedTabIndex].editorContent.OnFocus();
        }

        private void OnHierarchyChange()
        {
            tabContents[selectedTabIndex].editorContent.OnHierarchyChange();
        }

        private void OnDestroy()
        {
            EditorPrefs.SetFloat(EditorWindowSizeXPrefKey, position.size.x);
            EditorPrefs.SetFloat(EditorWindowSizeYPrefKey, position.size.y);
        }

        public void ForceUpdate()
        {
            tabContents[selectedTabIndex].editorContent.OnForceUpdate();
        }

        public void AddLog(string log)
        {
            logs.Add($"{DateTime.Now:[hh:mm:ss]} {log}");
        }

        private void OnGUI()
        {
            var selectedLanguage = (MessagesContainer.Language)
                EditorGUILayout.EnumPopup(
                    MessagesContainer.EditorWindowMessages.LabelLanguage,
                    currentLanguage
                );

            if (selectedLanguage != currentLanguage)
            {
                MessagesContainer.SwitchCurrentLanguage(selectedLanguage);
                currentLanguage = selectedLanguage;
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                selectedTabIndex = CustomEditorUtil.DrawUpdatableToolBar(
                    selectedTabIndex,
                    _tabLabels,
                    onUpdate: newIndex =>
                    {
                        tabContents[newIndex].editorContent.OnSelect();
                    }
                );
            }

            EditorGUILayout.BeginVertical(contentStyle);
            tabContents[selectedTabIndex].editorContent.DrawGui();
            EditorGUILayout.EndVertical();
        }

        private void DrawLogs()
        {
            var richLabel = new GUIStyle(EditorStyles.label) { richText = true };
            using (_ = new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                if (GUILayout.Button("Clear"))
                {
                    logs.Clear();
                }

                using (var scroll = new EditorGUILayout.ScrollViewScope(_logScroll))
                {
                    foreach (var log in logs)
                    {
                        EditorGUILayout.LabelField(log, style: richLabel);
                    }

                    _logScroll = scroll.scrollPosition;
                }
            }
        }
    }
}
