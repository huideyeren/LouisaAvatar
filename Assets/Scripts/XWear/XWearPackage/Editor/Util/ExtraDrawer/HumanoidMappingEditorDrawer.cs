using System;
using UnityEditor;
using UnityEngine;
using XWear.IO;

namespace XWear.XWearPackage.Editor.Util.ExtraDrawer
{
    [Serializable]
    public class HumanoidMappingEditorDrawer
    {
        [SerializeReference]
        private BodyPartGroupDetailDrawer currentDetailDrawer = new();

        public void DrawBody(HumanoidMapComponent humanoidMapComponent)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();

            var silhouetteContent = BodyPartGroupGuiInfo.BodySilhouetteContent;
            EditorGUILayout.Space();
            if (currentDetailDrawer == null)
            {
                GUILayout.FlexibleSpace();
            }

            var maxWidth = (int)(silhouetteContent.image.width * 0.9f);
            var maxHeight = (int)(silhouetteContent.image.height * 0.9f);
            var imageRect = GUILayoutUtility.GetRect(
                silhouetteContent,
                GUIStyle.none,
                GUILayout.MaxWidth(maxWidth),
                GUILayout.MaxHeight(maxHeight)
            );

            for (var index = 0; index < BodyPartGroupGuiInfo.BodyPartContents.Length; index++)
            {
                var bodyPartGroup = BodyPartGroupGuiInfo.BodyPartContents[index];
                var targetIndex = index;
                bodyPartGroup.DrawPart(
                    imageRect,
                    onDetailShowStateChanged: (newState) =>
                    {
                        OnDetailShowStateChanged(targetIndex, newState);
                    }
                );
            }

            if (currentDetailDrawer == null)
            {
                GUILayout.FlexibleSpace();
            }

            DrawDetail(maxHeight, maxWidth, humanoidMapComponent);

            if (currentDetailDrawer == null)
            {
                EditorGUILayout.Space();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void OnDetailShowStateChanged(int index, bool newState)
        {
            currentDetailDrawer = !newState
                ? null
                : BodyPartGroupGuiInfo.BodyPartContents[index].detailDrawer;

            for (var i = 0; i < BodyPartGroupGuiInfo.BodyPartContents.Length; i++)
            {
                var bodyPartContent = BodyPartGroupGuiInfo.BodyPartContents[i];
                if (i != index)
                {
                    bodyPartContent.detailDrawer.isShow = false;
                }
            }
        }

        private void DrawDetail(
            int maxHeight,
            int maxWidth,
            HumanoidMapComponent humanoidMapComponent
        )
        {
            if (currentDetailDrawer == null)
            {
                return;
            }

            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
            currentDetailDrawer.Draw(maxHeight, maxWidth, humanoidMapComponent);
            GUILayout.EndVertical();
        }
    }
}
