using System;
using UnityEngine;

namespace XWear.XWearPackage.Editor.Util.ExtraDrawer
{
    [Serializable]
    public class ZoomIconDrawer
    {
        private readonly BodyPartGroupDrawer.PartGroupType _partGroupType;

        [SerializeField]
        private bool isShowMore;

        public ZoomIconDrawer(BodyPartGroupDrawer.PartGroupType partGroupType)
        {
            _partGroupType = partGroupType;
        }

        public void DrawZoomIcon(int maxHeight, int maxWidth)
        {
            var iconContent = BodyPartGroupDetailGuiInfo.GetBodyPartGroupDetailIcon(_partGroupType);
            if (iconContent == null)
            {
                return;
            }

            var imageRect = GUILayoutUtility.GetRect(
                iconContent,
                GUIStyle.none,
                GUILayout.MaxWidth(maxWidth),
                GUILayout.MaxHeight(maxHeight)
            );

            var iconPositionOffsetRatio = BodyPartGroupDetailGuiInfo.GetIconPositionOffsetRatio(
                _partGroupType
            );

            var posX = imageRect.position.x * iconPositionOffsetRatio.x;
            var posY = imageRect.position.y * iconPositionOffsetRatio.y;
            imageRect.position = new Vector2(posX, posY);

            GUI.DrawTexture(imageRect, iconContent.image);

            DrawButton(imageRect, null);
        }

        private void DrawButton(Rect rect, Action<bool> onDetailShowStateChanged)
        {
            if (
                !ZoomIconGuiInfo.ZoomIconContents.TryGetValue(
                    _partGroupType,
                    out var zoomGuiContents
                )
            )
            {
                return;
            }

            const float size = 22.0f;

            foreach (var zoomIconContent in zoomGuiContents)
            {
                var offset = zoomIconContent.ButtonOffsetRatio;
                var posX = rect.center.x - size / 2 + rect.size.x / 2 * offset.x;
                // Rectは左上が原点なので引いて行って位置を出す
                var posY = rect.center.y - size / 2 - rect.size.y / 2 * offset.y;
                var position = new Vector2(posX, posY);
                var buttonRect = new Rect(rect)
                {
                    position = position,
                    width = size,
                    height = size,
                };

                var newDetailShowState = GUI.Toggle(
                    buttonRect,
                    isShowMore,
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

                if (newDetailShowState != isShowMore)
                {
                    onDetailShowStateChanged?.Invoke(newDetailShowState);
                }
            }
        }
    }
}
