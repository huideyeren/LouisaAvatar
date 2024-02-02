using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace XWear.XWearPackage.Editor.Util.ExtraDrawer
{
    [Serializable]
    public class BodyPartGroupDrawer
    {
        public enum PartGroupType
        {
            Head,
            Torso,
            LeftArm,
            RightArm,
            LeftHand,
            RightHand,
            LeftLeg,
            RightLeg,
        }

        public PartGroupType groupType;
        private readonly GUIContent _iconContent;
        private readonly Vector2 _buttonOffsetRatio;

        [SerializeReference]
        public BodyPartGroupDetailDrawer detailDrawer;

        public BodyPartGroupDrawer(
            PartGroupType groupType,
            HumanBodyBones[] groupedHumanBodyBones,
            string buildInIconPath,
            Vector2 buttonOffsetRatio
        )
        {
            this.groupType = groupType;
            _iconContent = EditorGUIUtility.IconContent(buildInIconPath);
            _buttonOffsetRatio = buttonOffsetRatio;
            detailDrawer = new BodyPartGroupDetailDrawer()
            {
                partGroupType = groupType,
                groupedHumanBodyBones = groupedHumanBodyBones,
                zoomIconDrawer = new ZoomIconDrawer(groupType)
            };
        }

        public void DrawPart(Rect rect, Action<bool> onDetailShowStateChanged)
        {
            GUI.DrawTexture(rect, _iconContent.image);
            DrawSelectButton(rect, onDetailShowStateChanged);
        }

        private void DrawSelectButton(Rect rect, Action<bool> onDetailShowStateChanged)
        {
            const float size = 22.0f;

            var posX = rect.center.x - size / 2 + rect.size.x / 2 * _buttonOffsetRatio.x;
            // Rectは左上が原点なので引いて行って位置を出す
            var posY = rect.center.y - size / 2 - rect.size.y / 2 * _buttonOffsetRatio.y;
            var position = new Vector2(posX, posY);
            var buttonRect = new Rect(rect)
            {
                position = position,
                width = size,
                height = size,
            };

            var newDetailShowState = GUI.Toggle(
                buttonRect,
                detailDrawer.isShow,
                new GUIContent("○"),
                new GUIStyle(GUI.skin.button)
                {
                    stretchWidth = false,
                    stretchHeight = false,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = 20,
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0)
                }
            );

            if (newDetailShowState != detailDrawer.isShow)
            {
                onDetailShowStateChanged?.Invoke(newDetailShowState);
            }

            detailDrawer.isShow = newDetailShowState;
        }
    }
}
