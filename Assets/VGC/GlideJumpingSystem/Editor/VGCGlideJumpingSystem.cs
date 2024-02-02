using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;

#if VRC_SDK_VRCSDK3
using VRC.SDK3.Avatars.Components;
#endif

// ver 0.0.5
// Copyright (c) 2022 TKSP

// 名前空間の定義
namespace VGC
{

    public sealed class VGCGlideJumpingSystem : EditorWindow
    {
        private string _csvFolderPath = "Assets/VGC/GlideJumpingSystem/Localize";
        private List<string> _texts = new List<string>();

        /// <summary>
        /// アバター格納用変数
        /// </summary>
        private GameObject _targetAvatar;
        private GameObject _prevAvater = null;
        private VRCAvatarDescriptor _descriptor = null;

        // 使用するアセットのパス
        private string _controllerPath = "Assets/VGC/GlideJumpingSystem/Controller/GlideJumpingFxLayer.controller";
        private string _exParamPath = "Assets/VGC/GlideJumpingSystem/Parameters/GlideJumpingSystemParameters.asset";
        private string _exMenuPath = "Assets/VGC/GlideJumpingSystem/Menu/GlideJumpingSystemMainMenu.asset";
        private string _prefabPath = "Assets/VGC/GlideJumpingSystem/Prefabs/VGCGlideJumpingSystem.prefab";

        // WriteDefault設定
        private bool _writeDefault = false;

        /// <summary>
        /// Windowメニュー作成
        /// </summary>
        [MenuItem("VGC/GlideJumpingSystem")]
        static void Open()
        {
            var window = GetWindow<VGCGlideJumpingSystem>("GlideJumpingSystem");
            // ウィンドウサイズの指定
            window.minSize = new Vector2(400, 500);
        }

        /// <summary>
        /// メニューのUI
        /// </summary>
        private void OnGUI()
        {
            if (!VGCLocalizeHelper.ShowEditLanguage(_csvFolderPath))
                return;

            _texts = VGCLocalizeHelper.GetLocalizeTexts(_csvFolderPath);

            GUILayout.Label(_texts[0]);

            // GameObject型のフィールドから値を取得
            _targetAvatar = EditorGUILayout.ObjectField(_texts[1], _targetAvatar, typeof(GameObject), true) as GameObject;

            // 変更があるか検知
            bool isChanged = _targetAvatar != _prevAvater;

            if (isChanged)
            {
                _prevAvater = _targetAvatar;

                if (_targetAvatar)
                {
                    // 更新
                    _descriptor = _targetAvatar.GetComponent<VRCAvatarDescriptor>();
                    if (_descriptor)
                    {
                        _writeDefault = LoadWriteDefault(_descriptor);
                    }
                }
                else
                {
                    _descriptor = null;
                }
            }

            if (_descriptor)
            {

                GUILayout.Space(10);
                _writeDefault = EditorGUILayout.Toggle(_texts[7], _writeDefault);
                //ヘルプボックスを表示 
                EditorGUILayout.HelpBox(_texts[8], MessageType.Info);
                GUILayout.Space(10);

                // targetAvaterがnullなら囲んだ領域を操作不可にする
                EditorGUI.BeginDisabledGroup(!_targetAvatar);

                // ボタン
                if (GUILayout.Button(_texts[2]))
                {
                    Setup();
                }

                GUILayout.Space(40);

                // ボタン
                if (GUILayout.Button(_texts[3]))
                {
                    DeleteGlideJumpingSystem(true);
                }

                // ここまで操作不可
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                GUILayout.Label("※"+_texts[4]);
            }
        }

        /// <summary>
        /// FXLayerから最初に見つかったStateのWriteDefaultValueを返す
        /// </summary>
        /// <returns>WriteDefaultValue</returns>
        private bool LoadWriteDefault(VRCAvatarDescriptor descriptor)
        {
            bool retVal = false;

            if (!descriptor)
                return false;

            // コントローラーを取得
            AnimatorController fxLayer = descriptor.GetController(VRCAvatarDescriptor.AnimLayerType.FX, out int index);

            if (!fxLayer)
            {
                return false;
            }

            // 最初に見つかったStateのValueを返す
            foreach (var layer in fxLayer.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    return state.state.writeDefaultValues;
                }
            }

            return retVal;
        }

        private void Setup()
        {
            if (!_descriptor)
            {
                EditorUtility.DisplayDialog("ERROR", _texts[4], "OK");
                return;
            }

            // 初期化して再生成
            DeleteGlideJumpingSystem(false);

            // ギミック生成
            var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath);
            var systemObject = Instantiate(sourcePrefab, _targetAvatar.transform);
            systemObject.name = sourcePrefab.name;

            // コントローラ生成
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(_controllerPath);
            WriteDefaultUtil.WriteDefaultChangeAll(controller, _writeDefault);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // FXLayerに追加
            AnimatorController fxLayer = _descriptor.GetController(VRCAvatarDescriptor.AnimLayerType.FX, out int index);

            if (fxLayer)
            {
                // アバターに設定されているfxLayerにsourceのレイヤーとパラメータを追加
                VGCUtility.MargeAnimatorController(controller, fxLayer);
            }
            else
            {
                // FxLayerが設定されていないので新規作成して追加
                string controllerPath = AssetDatabase.GetAssetPath(controller);
                VGCLib.CopyAndAttachAnimationLayer(_descriptor, VRCAvatarDescriptor.AnimLayerType.FX, controllerPath);
            }

            // パラメータの追加
            VGCLib.SetupParam(_descriptor, _exParamPath);

            // メニューの追加
            VGCLib.SetupMenu(_descriptor, _exMenuPath);

            EditorUtility.DisplayDialog("SUCCESS", _texts[5], "OK");

            Repaint();

        }

        private void DeleteGlideJumpingSystem(bool showCompleateMessage)
        {
            if (!_targetAvatar)
                return;

            if (!_descriptor)
            {
                EditorUtility.DisplayDialog("ERROR", _texts[4], "OK");
                return;
            }

            var systemObject = _targetAvatar.transform.Find("VGCGlideJumpingSystem");
            if(systemObject)
            {
                DestroyImmediate(systemObject.gameObject);
            }

            // FXLayer取得
            AnimatorController fxLayer = _descriptor.GetController(VRCAvatarDescriptor.AnimLayerType.FX, out int index);

            if (fxLayer)
            {
                // レイヤー削除
                VGCLib.DeleteAnimationLayer(ref fxLayer, "VGCDashSpeed");
                VGCLib.DeleteAnimationLayer(ref fxLayer, "VGCColliderDash");
                VGCLib.DeleteAnimationLayer(ref fxLayer, "VGCColliderJump");
                VGCLib.DeleteAnimationLayer(ref fxLayer, "VGCGlideHeight");
                VGCLib.DeleteAnimationLayer(ref fxLayer, "VGCGlide");

                // FxLayerのパラメータ削除
                VGCLib.DeleteAnimationParameter(ref fxLayer, "VGCColliderDash");
                VGCLib.DeleteAnimationParameter(ref fxLayer, "VGCColliderJump");
                VGCLib.DeleteAnimationParameter(ref fxLayer, "VGCDashSpeed");
                VGCLib.DeleteAnimationParameter(ref fxLayer, "VGCGlideHeight");
                VGCLib.DeleteAnimationParameter(ref fxLayer, "VGCGlideSpeed");
            }

            // exパラメータ削除
            VGCLib.DeleateExpressionParameters(ref _descriptor, "VGCColliderDash");
            VGCLib.DeleateExpressionParameters(ref _descriptor, "VGCColliderJump");
            VGCLib.DeleateExpressionParameters(ref _descriptor, "VGCDashSpeed");
            VGCLib.DeleateExpressionParameters(ref _descriptor, "VGCGlideHeight");
            VGCLib.DeleateExpressionParameters(ref _descriptor, "VGCGlideSpeed");

            // exメニュー削除
            VGCLib.DeleteExpressionsMenu(ref _descriptor, "GlideJumpingSystem");

            if (showCompleateMessage)
            {
                EditorUtility.DisplayDialog("SUCCESS", _texts[6], "OK");
            }

            Repaint();
        }
    }
}
