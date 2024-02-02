using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XWear.XWearPackage.Editor.Common;
using XWear.XWearPackage.Editor.Util.ExtraDrawer;
using XWear.XWearPackage.Editor.Validator;
using XWear.XWearPackage.Editor.Validator.Error;
using XWear.XWearPackage.Editor.Validator.Warning;

namespace XWear.XWearPackage.Editor.Util
{
    public static class ValidateResultInfoDrawer
    {
        private static GUIContent _waningIconContent;
        private static GUIContent _errorIconContent;

        private static GUIContent WarningIcon
        {
            get
            {
                if (_waningIconContent == null)
                {
                    _waningIconContent = EditorGUIUtility.IconContent("console.warnicon");
                }

                return _waningIconContent;
            }
        }

        private static GUIContent ErrorIcon
        {
            get
            {
                if (_errorIconContent == null)
                {
                    _errorIconContent = EditorGUIUtility.IconContent("console.erroricon");
                }

                return _errorIconContent;
            }
        }

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

        private static readonly GUIStyle MessageStyle = new GUIStyle(GUI.skin.label)
        {
            wordWrap = true,
            padding = new RectOffset() { bottom = 10 },
        };

        private static readonly GUIStyle IconStyle = new GUIStyle()
        {
            stretchWidth = false,
            stretchHeight = false,
            alignment = TextAnchor.UpperLeft,
            imagePosition = ImagePosition.ImageOnly,
            clipping = TextClipping.Clip,
            wordWrap = true,
            padding = new RectOffset() { },
        };

        private static readonly GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button)
        {
            stretchWidth = false,
            wordWrap = true,
        };

        public static void DrawValidateInfo(ValidateResult validateResult)
        {
            if (validateResult.ValidateResultType == ValidateResultType.Ok)
            {
                return;
            }

            var messageType = MessageType.None;
            var message = MessagesContainer.ValidatorMessages.GetValidatorMessage(
                validateResult.ValidateResultType
            );

            if (validateResult is ValidateResultError error)
            {
                messageType = MessageType.Error;
            }
            else if (validateResult is ValidateResultWarning warning)
            {
                messageType = MessageType.Warning;
            }

            EditorGUILayout.BeginVertical(BoxStyle);
            {
                DrawInfo(message, messageType);
                DrawActionButtons(validateResult);
            }
            EditorGUILayout.EndVertical();
        }

        private static void DrawInfo(string message, MessageType messageType = MessageType.None)
        {
            GUIContent icon;
            switch (messageType)
            {
                case MessageType.Warning:
                    icon = WarningIcon;
                    break;
                case MessageType.Error:
                    icon = ErrorIcon;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label(icon, IconStyle);
                EditorGUILayout.LabelField(message, MessageStyle);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawActionButtons(ValidateResult validateResult)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (validateResult is IValidateResultSelectableSource selectable)
                {
                    if (
                        GUILayout.Button(
                            MessagesContainer.ValidatorMessages.LabelCheckValidateTarget,
                            ButtonStyle
                        )
                    )
                    {
                        Selection.activeObject = selectable.Source;
                        EditorGUIUtility.PingObject(Selection.activeObject);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (validateResult is IValidateResultFixable fixable)
            {
                var simpleFixActions = fixable.FixActions.OfType<SimpleFixFunction>();
                EditorGUILayout.BeginHorizontal();
                {
                    foreach (var simpleFixAction in simpleFixActions)
                    {
                        if (
                            GUILayout.Button(
                                MessagesContainer.ValidatorMessages.GetFixLabel(
                                    simpleFixAction.FixFunctionType
                                ),
                                ButtonStyle
                            )
                        )
                        {
                            if (fixable.FixActions != null)
                            {
                                var results = simpleFixAction.Function?.Invoke();
                                if (results != null)
                                {
                                    foreach (var result in results)
                                    {
                                        EditorUtility.SetDirty(result);
                                        AssetDatabase.SaveAssets();
                                    }
                                }
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
