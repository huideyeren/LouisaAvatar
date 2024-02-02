/*  
 *  VRChatThumbnailer
 *  2020
 *  author: ureishi
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ureishi.VRChatThumbnailer.Script
{
    [DisallowMultipleComponent]
    public class VRChatThumbnailer : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private Texture texture;
        private RawImage rawImage;
        private RectTransform rectTransform;

        private Texture prevTexture;

        private void Start()
        {
            StartCoroutine(SetupThumbnailCanvas());
        }

        private void Update()
        {
            /// TODO:
            /// (できれば)Update()ではなくUndo, Redoイベントを拾う?.
            /// アスペクト比を保つかなど選択できるように.
            /// マウス操作で画像を拡大や移動させられるように.
            if (rectTransform && rawImage && texture != prevTexture)
            {
                prevTexture = texture;

                var r = rectTransform.rect.width / rectTransform.rect.height;
                var hr = texture.width > texture.height;

                rawImage.texture = texture;

                rawImage.uvRect = new Rect()
                {
                    x = hr ? 0.5f - (float)texture.height / texture.width / 2 * r : 0,
                    y = hr ? 0 : 0.5f - (float)texture.width / texture.height / 2 / r,
                    width = hr ? (float)texture.height / texture.width * r : 1,
                    height = hr ? 1 : (float)texture.width / texture.height / r
                };
            }
        }

        private void OnEnable()
        {
            if (rectTransform) rectTransform.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            if (rectTransform) rectTransform.gameObject.SetActive(false);
        }

        private IEnumerator SetupThumbnailCanvas()
        {
            yield return null;

            var vrcCam = GameObject.Find("VRCCam");
            if (!vrcCam) yield break;

            var thumbnailCanvas = new GameObject()
            {
                name = "ThumbnailCanvas"
            };
            thumbnailCanvas.transform.SetParent(transform);

            var canvas = thumbnailCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            var camera = vrcCam.GetComponent<Camera>();
            canvas.worldCamera = camera;
            canvas.planeDistance = camera.nearClipPlane + 0.01f;

            rawImage = thumbnailCanvas.AddComponent<RawImage>();

            rectTransform = thumbnailCanvas.GetComponent<RectTransform>();
        }

        #region EditorClass
#if UNITY_EDITOR
        [CustomEditor(typeof(VRChatThumbnailer))]
        public class VRChatThumbnailerEditor : Editor
        {
            private const float THUMBNAIL_WIDTH = 400;
            private const float THUMBNAIL_HEIGHT = 300;

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                var targetScript = target as VRChatThumbnailer;

                if (PrefabUtility.IsPartOfPrefabAsset(targetScript.gameObject))
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.Space();
                        {
                            GUI.enabled = false;
                            var _ = EditorGUILayout.ObjectField(
                                targetScript.texture,
                                typeof(Texture), false,
                                GUILayout.Width(THUMBNAIL_WIDTH),
                                GUILayout.Height(THUMBNAIL_HEIGHT)
                              ) as Texture;
                            GUI.enabled = true;
                        }
                        EditorGUILayout.Space();
                    }
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                targetScript.gameObject.tag = "EditorOnly";
                serializedObject.Update();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.Space();
                    {
                        EditorGUI.BeginChangeCheck();
                        var texture = EditorGUILayout.ObjectField(
                            targetScript.texture,
                            typeof(Texture), false,
                            GUILayout.Width(THUMBNAIL_WIDTH),
                            GUILayout.Height(THUMBNAIL_HEIGHT)
                             ) as Texture;
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (PrefabUtility.IsPartOfPrefabInstance(targetScript.gameObject))
                            {
                                PrefabUtility.UnpackPrefabInstance(targetScript.gameObject,
                                    PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                            }
                            Undo.RecordObject(target, "Changed Thumbnail Texture");
                            targetScript.texture = texture;
                        }
                    }
                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndHorizontal();

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
        #endregion EditorClass
    }
}