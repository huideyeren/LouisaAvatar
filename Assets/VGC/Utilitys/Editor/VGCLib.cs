using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.Animations;

#if VRC_SDK_VRCSDK3
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
#endif

// ver 0.4.0
// Copyright (c) 2021 TKSP

// 名前空間の定義
namespace VGC
{
    public static class VGCLib
    {

        // trueで英語化(旧バージョンとの互換性のため残す)
        public static bool isEn = false;

        public enum Language
        {
            Japanese = 0,
            English,
            Korean,
        };

        // ここが増えると選択肢が増えるので注意
        private static string[] _languageCSVList = {
                "jp.csv",
                "en.csv",
                "kr.csv",
        };

        // メニュー設定
        private static string[] _languageArray = { "日本語", "English", "한국어" };

        // 現在選択中の言語(念のためpublicにしているが、できれば使用しない)
        public static Language language = Language.Japanese;

        // 新規作成アセットのフォルダパス
        static string _savePath = "Assets/VGC/CreateAssets";

        /// <summary>
        /// VGCリソース作成フォルダ名の取得
        /// </summary>
        /// <returns>VGCリソース作成フォルダ名</returns>
        public static string GetSavePath()
        {
            return _savePath;
        }

        /// <summary>
        /// 保存フォルダの作成
        /// </summary>
        public static void CreateAssetsFolder()
        {
            // フォルダが無ければ作成
            if (!Directory.Exists(_savePath))
            {
                AssetDatabase.CreateFolder("Assets/VGC", "CreateAssets");
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 保存先フォルダ内にフォルダを作成する(CreateAssetsフォルダ直下のみ対応)
        /// </summary>
        /// <param name="name">フォルダ名</param>
        public static void CreateFolder(string name)
        {
            CreateAssetsFolder();
            // フォルダが無ければ作成
            if (!Directory.Exists(_savePath + "/" + name))
            {
                AssetDatabase.CreateFolder(_savePath, name);
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 保存先フォルダ内にフォルダ階層を作成する
        /// </summary>
        /// <param name="folderPath">フォルダパス</param>
        public static void CreateFolderRecursive(string folderPath)
        {
            string[] arr = folderPath.Split('/');
            string path = "";
            string parentPath = _savePath;

            // savePathフォルダ作成
            CreateAssetsFolder();
            foreach (var arrPath in arr)
            {
                // フォルダが無ければ作成
                if (!Directory.Exists(parentPath + "/" + arrPath))
                {
                    AssetDatabase.CreateFolder(parentPath, arrPath);
                    AssetDatabase.Refresh();
                }
                path += "/";
                parentPath += "/" + arrPath;
            }
        }

        /// <summary>
        /// ローカライズテキストの取得
        /// </summary>
        /// <param name="csvFolderPath">csv格納フォルダのパス</param>
        /// <returns>読み込んだ文字列の配列</returns>
        public static List<string> GetLocalizeTexts(string csvFolderPath)
        {
            List<string> retTexts = new List<string>();

            string csvPath = csvFolderPath + "/" + _languageCSVList[(int)language];

            var csvFile = AssetDatabase.LoadAssetAtPath<TextAsset>(csvPath);

            if (!csvFile)
            {
                csvPath = csvFolderPath + "/" + _languageCSVList[(int)Language.English];
                csvFile = AssetDatabase.LoadAssetAtPath<TextAsset>(csvPath);
            }

            if (csvFile)
            {
                StringReader reader = new StringReader(csvFile.text);

                while (reader.Peek() > -1)
                {
                    // 一行読み込み
                    var text = reader.ReadLine();
                    if (text.Length == 0)
                        break;
                    text = text.Replace(@"\n", Environment.NewLine);
                    retTexts.Add(text);
                }
            }
            return retTexts;
        }

        /// <summary>
        /// テキストを取得する
        /// </summary>
        /// <param name="jp">日本語</param>
        /// <param name="en">英語</param>
        /// <param name="kr">中国語</param>
        /// <returns></returns>
        public static string convertLanguage(string jp, string en, string kr)
        {
            switch (language)
            {
                case Language.Japanese:
                    return jp;
                case Language.English:
                    return en;
                case Language.Korean:
                    return kr;
                default:
                    return en;
            }
        }

        /// <summary>
        /// 言語選択ウィンドウ表示
        /// </summary>
        /// <param name="csvFolderPath">csv格納フォルダのパス</param>
        /// <returns>対応している言語が選択されているか</returns>
        public static bool ShowEditLanguage(string csvFolderPath)
        {
            // 対応している言語のリスト
            List<string> handleLanguages = new List<string>();
            List<bool> isSupportList = new List<bool>();
            for (int i = 0; i < _languageCSVList.Length; i++)
            {
                string csvPath = csvFolderPath + "/" + _languageCSVList[i];
                var csvFile = AssetDatabase.LoadAssetAtPath<TextAsset>(csvPath);

                // 存在確認
                isSupportList.Add(csvFile);
                if (csvFile)
                {
                    handleLanguages.Add(_languageArray[i]);
                }
                else
                {
                    handleLanguages.Add("Languages not supported by this tool");
                }
            }

            language = (Language)Enum.ToObject(typeof(Language), EditorGUILayout.Popup("Language", (int)language, handleLanguages.ToArray()));

            // サポートしていない言語が選択されている
            if (!isSupportList[(int)language])
            {
                GUILayout.Label("※Unsupported language! Please select other language.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// AnimatorControllerを新規作成か初期化
        /// </summary>
        /// <param name="avatarDescriptor">アバター</param>
        /// <param name="fileName">ファイル名(.controller無し)</param>
        /// <returns>AnimatorController</returns>
        public static AnimatorController CreateOrResetAnimatorController(VRCAvatarDescriptor avatarDescriptor, string fileName)
        {
            if (!avatarDescriptor)
                return null;

            // フォルダの作成
            CreateAssetsFolder();
            CreateFolder(avatarDescriptor.gameObject.name);

            fileName += ".controller";

            // 保存先のパスを作成
            var folderPath = _savePath + "/" + avatarDescriptor.gameObject.name;
            var filePath = folderPath + "/" + fileName;

            // コントローラーを新規作成して上書き保存
            var controller = new AnimatorController();
            controller = SaveAsset<AnimatorController>(controller, filePath);
            return controller;
        }

        /// <summary>
        /// AnimationClipを新規作成か初期化
        /// </summary>
        /// <param name="avatarDescriptor">アバター</param>
        /// <param name="fileName">ファイル名(.anim無し)</param>
        /// <returns>AnimationClip</returns>
        public static AnimationClip CreateOrResetAnimationClip(VRCAvatarDescriptor avatarDescriptor, string fileName)
        {
            if (!avatarDescriptor)
                return null;

            // フォルダの作成
            CreateAssetsFolder();
            CreateFolder(avatarDescriptor.gameObject.name);

            fileName += ".anim";

            // 保存先のパスを作成
            var folderPath = _savePath + "/" + avatarDescriptor.gameObject.name;
            var filePath = folderPath + "/" + fileName;

            // Animationを新規作成して上書き保存
            var clip = new AnimationClip();
            clip = SaveAsset<AnimationClip>(clip, filePath);
            return clip;
        }

        /// <summary>
        /// pathのコントローラーをコピーして対象アバターの指定type先にアタッチする
        /// </summary>
        /// <param name="avatarDescriptor">対象のアバターを指定</param>
        /// <param name="type">レイヤータイプ</param>
        /// <param name="path">ソースコントローラーのパス</param>
        /// <param name="fileName">保存ファイル名を指定(しない場合はデフォルト)</param>
        public static void CopyAndAttachAnimationLayer(VRCAvatarDescriptor avatarDescriptor, VRCAvatarDescriptor.AnimLayerType type, string path, string fileName = "")
        {
            if (!avatarDescriptor)
                return;

            // フォルダの作成
            CreateAssetsFolder();
            CreateFolder(avatarDescriptor.gameObject.name);

            // カスタムレイヤーの有効化
            avatarDescriptor.customizeAnimationLayers = true;

            // コントローラーの場所を取得
            avatarDescriptor.GetController(type, out int index);

            // ファイル名の指定が無い場合ファイル名を作成
            if (fileName == "")
            {
                string layerName = Enum.GetName(typeof(VRCAvatarDescriptor.AnimLayerType), type);
                fileName = avatarDescriptor.gameObject.name + layerName + "Controller.controller";
            }

            // 保存先のパスを作成
            var folderPath = _savePath + "/" + avatarDescriptor.gameObject.name;
            var filePath = folderPath + "/" + fileName;

            // 既に存在するのであればそれを使用する
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(filePath);

            if (!controller)
            {
                // 元ファイルのコピーを作成し、保存
                AssetDatabase.CopyAsset(path, filePath);
                controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(filePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            // アバターにアタッチ
            var customLayer = avatarDescriptor.baseAnimationLayers[index];
            customLayer.isDefault = false;
            customLayer.animatorController = controller;
            avatarDescriptor.baseAnimationLayers[index] = customLayer;
        }

        /// <summary>
        /// パスに指定したVRCExpressionParametersの追加を行う
        /// </summary>
        /// <param name="avatarDescriptor">アバターのVRCAvatarDescriptorを指定</param>
        /// <param name="path">リソースのパス</param>
        public static void SetupParam(VRCAvatarDescriptor avatarDescriptor, string path)
        {
            var sourceParam = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(path);

            // Expressionsのカスタマイズを有効化
            avatarDescriptor.customExpressions = true;

            // パラメータが無ければ空のパラメータを作成してアタッチ
            if (!avatarDescriptor.expressionParameters)
            {
                avatarDescriptor.expressionParameters = CreateVGCParameters(avatarDescriptor);
            }

            // パラメータの追加処理
            addExpressionParameters(avatarDescriptor.expressionParameters, sourceParam);
        }

        /// <summary>
        /// パスに指定したVRCExpressionMenuの追加を行う
        /// </summary>
        /// <param name="avatarDescriptor">アバターのVRCAvatarDescriptorを指定</param>
        /// <param name="path">リソースのパス</param>
        /// <param name="overwrite">上書きするならtrue</param>
        public static void SetupMenu(VRCAvatarDescriptor avatarDescriptor, string path, bool overwrite = false)
        {
            var sourceMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(path);

            // Expressionsのカスタマイズを有効化
            avatarDescriptor.customExpressions = true;

            // メニューが無ければ空のメニューを作成してアタッチ
            if (!avatarDescriptor.expressionsMenu)
            {
                avatarDescriptor.expressionsMenu = CreateVGCMenu(avatarDescriptor);
            }

            // メニューの項目追加処理
            addExpressionMenu(avatarDescriptor.expressionsMenu, sourceMenu, overwrite);
        }

        /// <summary>
        /// アバター用のVRCExpressionParametersを新規作成し、返す
        /// </summary>
        /// <param name="avatarDescriptor">対象のVRCAvatarDescriptor</param>
        /// <returns>作成したVRCExpressionParameters</returns>
        public static VRCExpressionParameters CreateVGCParameters(VRCAvatarDescriptor avatarDescriptor, string fileName = "")
        {
            if (!avatarDescriptor)
                return null;

            // 保存先パスの作成
            var objName = avatarDescriptor.gameObject.name;
            var folderPath = _savePath + "/" + objName;
            var path = folderPath + "/VGC_" + objName + "Parameters.asset";

            if (fileName != "")
            {
                path = folderPath + "/VGC_" + fileName + "Param.asset";
            }

            // 既に保存されていればそれを返す
            var vgcParam = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(path);
            if (!vgcParam)
            {
                // フォルダの作成
                CreateAssetsFolder();
                CreateFolder(avatarDescriptor.gameObject.name);

                // ScriptableObjectの作成
                vgcParam = ScriptableObject.CreateInstance<VRCExpressionParameters>();
                // 初期化
                vgcParam.parameters = new VRCExpressionParameters.Parameter[0];
                // 作成して保存
                AssetDatabase.CreateAsset(vgcParam, path);
                AssetDatabase.SaveAssets();
            }
            return vgcParam;
        }

        /// <summary>
        /// アバター用のVRCExpressionsMenuを新規作成し、返す
        /// </summary>
        /// <param name="avatarDescriptor">対象のVRCAvatarDescriptor</param>
        /// <returns>作成したVRCExpressionsMenu</returns>
        public static VRCExpressionsMenu CreateVGCMenu(VRCAvatarDescriptor avatarDescriptor, string fileName = "")
        {
            if (!avatarDescriptor)
                return null;

            // 保存先パスの作成
            var objName = avatarDescriptor.gameObject.name;
            var folderPath = _savePath + "/" + objName;
            var path = folderPath + "/VGC_" + objName + "Menu.asset";

            if (fileName != "")
            {
                path = folderPath + "/VGC_" + fileName + "Menu.asset";
            }

            // 既に保存されていればそれを返す
            var vgcMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(path);
            if (!vgcMenu)
            {
                // フォルダの作成
                CreateAssetsFolder();
                CreateFolder(avatarDescriptor.gameObject.name);
                // ScriptableObjectの作成
                vgcMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                AssetDatabase.CreateAsset(vgcMenu, path);
                AssetDatabase.SaveAssets();
            }
            return vgcMenu;
        }

        /// <summary>
        /// 指定タイプのAnimatorControllerを取得するVRCAvatarDescriptorの拡張メソッド
        /// </summary>
        /// <param name="descriptor">VRCAvatarDescriptorを指定</param>
        /// <param name="type">タイプ指定</param>
        /// <param name="index">配列の何番目にあるか返す</param>
        /// <returns>
        /// 指定タイプのAnimatorControllerを返す
        /// 存在しなければnull
        /// </returns>
        public static AnimatorController GetController(this VRCAvatarDescriptor descriptor, VRCAvatarDescriptor.AnimLayerType type, out int index)
        {
            var layers = descriptor.baseAnimationLayers;
            index = -1;
            if (!descriptor.customizeAnimationLayers || layers.Length == 0)
                return null;

            // レイヤーを検索
            for (int i = 0; i < layers.Length; i++)
            {
                // タイプが一致の場合
                if (layers[i].type == type)
                {
                    // AnimatorControllerがnullでも変数が見つかると格納場所を取得できる
                    index = i;

                    // AnimatorControllerがあれば返す
                    if (layers[i].animatorController)
                    {
                        return layers[i].animatorController as AnimatorController;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// VRCExpressionParametersにパラメータを追加する
        /// </summary>
        /// <param name="target">対象のパラメータ</param>
        /// <param name="addSource">追加するパラメータ</param>
        public static void addExpressionParameters(VRCExpressionParameters target, VRCExpressionParameters addSource)
        {
            if (!target || !addSource)
                return;

            if (target == addSource)
            {
                // 完全一致
                return;
            }
            // 一致した追加パラメータを保存しておく
            List<VRCExpressionParameters.Parameter> matchParams = new List<VRCExpressionParameters.Parameter>();

            if (target.parameters.Length != 0)
            {
                for (int i = 0; i < addSource.parameters.Length; i++)
                {
                    for (int j = 0; j < target.parameters.Length; j++)
                    {
                        if (addSource.parameters[i].name == target.parameters[j].name)
                        {
                            if (!matchParams.Contains(addSource.parameters[i]))
                                matchParams.Add(addSource.parameters[i]);

                            break;
                        }
                    }
                }
            }

            // 追加する全てがターゲットに含まれる場合何もしない
            if (matchParams.Count == addSource.parameters.Length)
            {
                Debug.Log("既にパラメータが追加されています。");
                return;
            }

            // 設定するパラメータ配列の作成
            int newParamLength = target.parameters.Length + addSource.parameters.Length - matchParams.Count;
            VRCExpressionParameters.Parameter[] newParams = new VRCExpressionParameters.Parameter[newParamLength];

            for (int i = 0; i < target.parameters.Length; i++)
            {
                newParams[i] = target.parameters[i];
            }

            int index = target.parameters.Length;


            for (int i = 0; i < addSource.parameters.Length; i++)
            {
                // 一致した項目になければ追加
                if (!matchParams.Contains(addSource.parameters[i]))
                {
                    newParams[index] = addSource.parameters[i];
                    index++;
                }
            }

            // 空欄は削除
            int count = 0;
            for (int i = 0; i < newParams.Length; i++)
            {
                if (newParams[i].name == "")
                {
                    count++;
                }
            }

            if (count != 0)
            {
                VRCExpressionParameters.Parameter[] addParams = new VRCExpressionParameters.Parameter[newParams.Length - count];
                count = 0;
                for (int i = 0; i < newParams.Length; i++)
                {
                    if (newParams[i].name != "")
                    {
                        addParams[count] = newParams[i];
                        count++;
                    }
                }
                newParams = addParams;
            }

            // パラメータの上書き
            target.parameters = newParams;
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();

        }

        /// <summary>
        /// メニューが追加できるか確認する
        /// </summary>
        /// <param name="target">追加する対象のメニュー</param>
        /// <param name="addSource">追加するメニュー</param>
        /// <returns>追加可能か追加済みならtrue</returns>
        public static bool CheckAddMenuEnabled(VRCExpressionsMenu target, VRCExpressionsMenu addSource)
        {

            if (!addSource)
                return false;

            if (!target)
                return true;

            // 名称が一致した追加項目を保存しておく
            List<VRCExpressionsMenu.Control> matchParams = new List<VRCExpressionsMenu.Control>();
            for (int i = 0; i < addSource.controls.Count; i++)
            {
                for (int j = 0; j < target.controls.Count; j++)
                {
                    if (addSource.controls[i].name == target.controls[j].name)
                    {
                        if (!matchParams.Contains(addSource.controls[i]))
                            matchParams.Add(addSource.controls[i]);

                        break;
                    }
                }
            }

            if (matchParams.Count == addSource.controls.Count)
            {
                // 既に追加されている
                return true;
            }

            if (target.controls.Count + addSource.controls.Count - matchParams.Count > VRCExpressionsMenu.MAX_CONTROLS)
            {
                //EditorUtility.DisplayDialog("ERROR", "メニューの空き項目が足りないため、拡張できません。", "OK");
                return false;
            }

            return true;
        }

        /// <summary>
        /// VRCExpressionsMenuに項目を追加する
        /// </summary>
        /// <param name="target">対象のメニュー</param>
        /// <param name="addSource">追加するメニュー</param>
        /// <param name="overwrite">上書きするならtrue</param>
        public static void addExpressionMenu(VRCExpressionsMenu target, VRCExpressionsMenu addSource, bool overwrite)
        {

            if (!target || !addSource)
                return;

            if (target == addSource)
            {
                // 完全一致
                return;
            }

            // 名称が一致した追加項目を保存しておく
            List<VRCExpressionsMenu.Control> matchParams = new List<VRCExpressionsMenu.Control>();
            for (int i = 0; i < addSource.controls.Count; i++)
            {
                for (int j = 0; j < target.controls.Count; j++)
                {
                    if (addSource.controls[i].name == target.controls[j].name)
                    {
                        if (!matchParams.Contains(addSource.controls[i]))
                        {
                            if (overwrite)
                            {
                                target.controls[j] = addSource.controls[i];
                            }
                            matchParams.Add(addSource.controls[i]);
                        }
                        break;
                    }
                }
            }

            if (matchParams.Count == addSource.controls.Count)
            {
                if (!overwrite)
                {
                    Debug.Log("既にメニューが設定されています。");
                }
                else
                {
                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssets();
                }
                return;
            }

            if (target.controls.Count + addSource.controls.Count - matchParams.Count > VRCExpressionsMenu.MAX_CONTROLS)
            {
                //Debug.Log(target.controls.Count + "," + addSource.controls.Count + "," + matchParams.Count);
                EditorUtility.DisplayDialog("ERROR", "メニューの空き項目が足りないため、拡張できませんでした。", "OK");
                return;
            }

            // 追加
            for (int i = 0; i < addSource.controls.Count; i++)
            {
                if (!target.controls.Contains(addSource.controls[i]))
                    target.controls.Add(addSource.controls[i]);
            }

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }


        /// <summary>
        /// ControllerのLayersに追加するControllerのLayersと一致するものが存在するか判定する
        /// </summary>
        /// <param name="avatarDescriptor">VRCAvatarDescriptorを指定</param>
        /// <param name="type">アバターの判定対象レイヤータイプを指定</param>
        /// <param name="path">判定したいAnimatorControllerのパス</param>
        /// <param name="enabled">含まれていたらtrueを返す</param>
        /// <param name="enabledText">表示する文字列を返す</param>
        public static void CheckAddAnimationLayer(VRCAvatarDescriptor avatarDescriptor, VRCAvatarDescriptor.AnimLayerType type, string path, out List<bool> enabled, out List<string> enabledText)
        {
            enabled = new List<bool>();
            enabledText = new List<string>();

            if (!avatarDescriptor)
                return;

            // カスタムレイヤーの有効化
            avatarDescriptor.customizeAnimationLayers = true;

            // コントローラーの場所を取得
            var target = avatarDescriptor.GetController(type, out int index);

            var source = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);

            string layerName = Enum.GetName(typeof(VRCAvatarDescriptor.AnimLayerType), type);

            if (!target)
            {
                for (int i = 0; i < source.layers.Length; i++)
                {
                    enabled.Add(false);
                    enabledText.Add(layerName + "コントローラーに" + source.layers[i].name + "のLayerが追加されている");
                }
            }
            else
            {
                for (int i = 0; i < source.layers.Length; i++)
                {
                    bool enabledName = false;
                    for (int j = 0; j < target.layers.Length; j++)
                    {
                        if (source.layers[i].name == target.layers[j].name)
                        {
                            enabledName = true;
                            break;
                        }
                    }
                    enabled.Add(enabledName);
                    enabledText.Add(layerName + "コントローラーに" + source.layers[i].name + "のLayerが追加されている");
                }
            }
        }

        /// <summary>
        /// ControllerのParametersに追加するControllerのParametersと一致するものが存在するか判定する
        /// </summary>
        /// <param name="avatarDescriptor">VRCAvatarDescriptorを指定</param>
        /// <param name="type">アバターの判定対象レイヤータイプを指定</param>
        /// <param name="path">判定したいAnimatorControllerのパス</param>
        /// <param name="enabled">含まれていたらtrueを返す</param>
        /// <param name="enabledText">表示する文字列を返す</param>
        public static void CheckAddAnimationParam(VRCAvatarDescriptor avatarDescriptor, VRCAvatarDescriptor.AnimLayerType type, string path, out List<bool> enabled, out List<string> enabledText)
        {
            enabled = new List<bool>();
            enabledText = new List<string>();

            if (!avatarDescriptor)
                return;

            // カスタムレイヤーの有効化
            avatarDescriptor.customizeAnimationLayers = true;

            // コントローラーの場所を取得
            var target = avatarDescriptor.GetController(type, out int index);

            var source = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);

            string layerName = Enum.GetName(typeof(VRCAvatarDescriptor.AnimLayerType), type);

            if (!target)
            {
                for (int i = 0; i < source.parameters.Length; i++)
                {
                    enabled.Add(false);
                    enabledText.Add(layerName + "コントローラーに" + source.parameters[i].name + "のParameterが追加されている");
                }
            }
            else
            {
                for (int i = 0; i < source.parameters.Length; i++)
                {
                    bool enabledName = false;
                    for (int j = 0; j < target.parameters.Length; j++)
                    {
                        if (source.parameters[i].name == target.parameters[j].name)
                        {
                            enabledName = true;
                            break;
                        }
                    }
                    enabled.Add(enabledName);
                    enabledText.Add(layerName + "コントローラーに" + source.parameters[i].name + "のParameterが追加されている");
                }
            }
        }

        /// <summary>
        /// ExpressionParametersに追加するExpressionParametersと一致するものが存在するか判定する
        /// </summary>
        /// <param name="avatarDescriptor">VRCAvatarDescriptorを指定</param>
        /// <param name="path">判定したいExpressionParametersのパス</param>
        /// <param name="enabled">含まれていたらtrueを返す</param>
        /// <param name="enabledText">表示する文字列を返す</param>
        public static void CheckAddExpressionParam(VRCAvatarDescriptor avatarDescriptor, string path, out List<bool> enabled, out List<string> enabledText)
        {
            enabled = new List<bool>();
            enabledText = new List<string>();

            if (!avatarDescriptor)
                return;

            var sourceParam = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(path);
            // Expressionsのカスタマイズを有効化
            avatarDescriptor.customExpressions = true;

            // パラメータが無ければ全部false
            if (!avatarDescriptor.expressionParameters)
            {
                for (int i = 0; i < sourceParam.parameters.Length; i++)
                {
                    enabled.Add(false);
                    enabledText.Add("ExpressionParametersに" + sourceParam.parameters[i].name + "が追加されている");
                }
            }
            else
            {
                var targetParam = avatarDescriptor.expressionParameters;

                for (int i = 0; i < sourceParam.parameters.Length; i++)
                {
                    bool enabledName = false;
                    for (int j = 0; j < targetParam.parameters.Length; j++)
                    {
                        if (sourceParam.parameters[i].name == targetParam.parameters[j].name)
                        {
                            enabledName = true;
                            break;
                        }
                    }
                    enabled.Add(enabledName);
                    enabledText.Add("ExpressionParametersに" + sourceParam.parameters[i].name + "が追加されている");
                }
            }
        }

        /// <summary>
        /// ExpressionsMenuに追加するExpressionsMenuと一致するものが存在するか判定する
        /// </summary>
        /// <param name="avatarDescriptor">VRCAvatarDescriptorを指定</param>
        /// <param name="path">判定したいExpressionsMenuのパス</param>
        /// <param name="enabled">含まれていたらtrueを返す</param>
        /// <param name="enabledText">表示する文字列を返す</param>
        public static void CheckAddExpressionMenu(VRCAvatarDescriptor avatarDescriptor, string path, out List<bool> enabled, out List<string> enabledText, bool overwrite = false)
        {
            enabled = new List<bool>();
            enabledText = new List<string>();

            if (!avatarDescriptor)
                return;

            var sourceMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(path);
            // Expressionsのカスタマイズを有効化
            avatarDescriptor.customExpressions = true;

            // メニューが無ければ全部false
            if (!avatarDescriptor.expressionsMenu)
            {
                for (int i = 0; i < sourceMenu.controls.Count; i++)
                {
                    enabled.Add(false);
                    enabledText.Add("ExpressionMenuに" + sourceMenu.controls[i].name + "が追加されている");
                }
            }
            else
            {
                var targetMenu = avatarDescriptor.expressionsMenu;

                for (int i = 0; i < sourceMenu.controls.Count; i++)
                {
                    bool enabledName = false;
                    for (int j = 0; j < targetMenu.controls.Count; j++)
                    {
                        if (sourceMenu.controls[i].name == targetMenu.controls[j].name)
                        {

                            if (overwrite)
                            {
                                switch (targetMenu.controls[j].type)
                                {
                                    case VRCExpressionsMenu.Control.ControlType.Button:
                                        {
                                            if (sourceMenu.controls[i].parameter == targetMenu.controls[j].parameter)
                                                enabledName = true;
                                        }
                                        break;
                                    case VRCExpressionsMenu.Control.ControlType.Toggle:
                                        {
                                            if (sourceMenu.controls[i].parameter == targetMenu.controls[j].parameter && sourceMenu.controls[i].value == targetMenu.controls[j].value)
                                                enabledName = true;
                                        }
                                        break;
                                    case VRCExpressionsMenu.Control.ControlType.SubMenu:
                                        {
                                            if (sourceMenu.controls[i].subMenu == targetMenu.controls[j].subMenu)
                                                enabledName = true;
                                        }
                                        break;
                                    // TODO: 必要に応じて追加
                                    default:
                                        enabledName = true;
                                        break;
                                }
                            }
                            else
                            {
                                enabledName = true;
                            }
                            break;
                        }
                    }
                    enabled.Add(enabledName);
                    if (overwrite)
                    {
                        enabledText.Add("ExpressionMenuの" + sourceMenu.controls[i].name + "が更新されている");
                    }
                    else
                    {
                        enabledText.Add("ExpressionMenuに" + sourceMenu.controls[i].name + "が追加されている");
                    }
                }
            }
        }

        /// <summary>
        /// VGCMenuの作成とコントロールの登録
        /// </summary>
        /// <param name="avatarDescriptor">アバターを指定</param>
        /// <param name="source">追加するVRCExpressionsMenu.Control</param>
        public static void AddVGCMenu(VRCAvatarDescriptor avatarDescriptor, VRCExpressionsMenu.Control source)
        {
            if (!avatarDescriptor)
                return;

            // 追加するメニュー
            var vgcMainMenu = CreateVGCMenu(avatarDescriptor, "Main");
            var vgcSubMenu = CreateVGCMenu(avatarDescriptor, "Sub");

            List<VRCExpressionsMenu> subMenus = new List<VRCExpressionsMenu>();

            // サブメニュー内のサブメニュー
            for (int i = 1; i < VRCExpressionsMenu.MAX_CONTROLS + 1; i++)
            {
                subMenus.Add(CreateVGCMenu(avatarDescriptor, "Menu" + i));
            }

            // 追加項目があるか
            foreach (var menu in subMenus)
            {
                foreach (var control in menu.controls)
                {

                    if (control.name == source.name)
                    {
                        Debug.Log("既にメニューに追加されています");
                        return;
                    }
                }
            }

            // 最後のサブメニューを確認する
            if (subMenus[VRCExpressionsMenu.MAX_CONTROLS - 1].controls.Count + 1 > VRCExpressionsMenu.MAX_CONTROLS)
            {
                EditorUtility.DisplayDialog("ERROR", "メニューの空き項目が足りないため、拡張できませんでした。", "OK");
                return;
            }

            // どのサブメニューに追加するか計算
            VRCExpressionsMenu targetMenu = null;
            int addMenuIdx = 1;
            foreach (var menu in subMenus)
            {
                if (menu.controls.Count + 1 > VRCExpressionsMenu.MAX_CONTROLS)
                {
                    addMenuIdx++;
                    continue;
                }
                else
                {
                    targetMenu = menu;
                    break;
                }
            }

            // 大丈夫だと思うが念のためチェック
            if (!targetMenu)
            {
                EditorUtility.DisplayDialog("ERROR", "メニューの空き項目が足りないため、拡張できませんでした。", "OK");
                return;
            }

            // MainMenuにVGCのサブメニューがあるか確認
            bool hasVGCMenu = false;
            foreach (var control in vgcMainMenu.controls)
            {
                if (control.name == "VGC")
                {
                    hasVGCMenu = true;
                    break;
                }
            }

            // 無ければ追加
            if (!hasVGCMenu)
            {
                var mainControll = new VRCExpressionsMenu.Control();
                mainControll.name = "VGC";
                mainControll.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
                mainControll.subMenu = vgcSubMenu;
                vgcMainMenu.controls.Add(mainControll);
                EditorUtility.SetDirty(vgcMainMenu);

            }

            // サブメニューの追加
            bool hasAddMenu = false;
            foreach (var control in vgcSubMenu.controls)
            {
                if (control.name == "Menu" + addMenuIdx)
                {
                    hasAddMenu = true;
                    break;
                }
            }

            // 無ければ追加
            if (!hasAddMenu)
            {
                var mainControll = new VRCExpressionsMenu.Control();
                mainControll.name = "Menu" + addMenuIdx;
                mainControll.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
                mainControll.subMenu = targetMenu;
                vgcSubMenu.controls.Add(mainControll);
                EditorUtility.SetDirty(vgcSubMenu);
            }

            targetMenu.controls.Add(source);
            EditorUtility.SetDirty(targetMenu);
            AssetDatabase.SaveAssets();

            // メニューの項目追加処理
            addExpressionMenu(avatarDescriptor.expressionsMenu, vgcMainMenu, false);

        }


        /// <summary>
        /// アバターの子のアニメーションパスを取得する
        /// </summary>
        /// <param name="avatarDescriptor">アバターを指定</param>
        /// <param name="targetObject">対象のオブジェクト</param>
        /// <returns></returns>
        public static string getRelativePath(VRCAvatarDescriptor avatarDescriptor, GameObject targetObject)
        {
            string objectAnimPath = "";
            if (targetObject.transform.parent == avatarDescriptor.transform)
            {
                objectAnimPath = targetObject.name;
            }
            else
            {
                List<Transform> parents = new List<Transform>();
                Transform parent = targetObject.transform.parent;
                while (true)
                {
                    parents.Add(parent);
                    parent = parent.parent;
                    // ターゲットアバターまで来たらぬける
                    if (parent == avatarDescriptor.transform)
                    {
                        break;
                    }
                    // parentがnullなら無効
                    else if (!parent)
                    {
                        Debug.LogError("アバターの子じゃありません！親子関係を確認してください。");
                        return "";
                    }
                }

                // 反転
                parents.Reverse();

                for (int i = 0; i < parents.Count; i++)
                {
                    if (i == 0)
                    {
                        objectAnimPath = parents[i].name;
                    }
                    else
                    {
                        objectAnimPath += "/" + parents[i].name;
                    }
                }

                // 自身を追加
                objectAnimPath += "/" + targetObject.name;
            }

            return objectAnimPath;
        }

        /// <summary>
        /// アセットを上書きで保存する。無い場合は新規作成。
        /// </summary>
        /// <typeparam name="T">型指定</typeparam>
        /// <param name="obj">保存するオブジェクト</param>
        /// <param name="filePath">保存先パス</param>
        /// <returns>保存したオブジェクト</returns>
        public static T SaveAsset<T>(UnityEngine.Object obj, string filePath) where T : UnityEngine.Object
        {
            // アセットデータベースを検索して既存のファイルがあれば取得する
            var existAsset = AssetDatabase.LoadMainAssetAtPath(filePath);
            if (existAsset != null)
            {

                // 既存のアセットに上書き
                EditorUtility.CopySerialized(obj, existAsset);
                AssetDatabase.SaveAssets();
                return existAsset as T;
            }
            else
            {
                // 新しくアセット生成
                AssetDatabase.CreateAsset(obj, filePath);
                AssetDatabase.Refresh();
                return obj as T;
            }
        }

        /// <summary>
        /// アバター直下にロックギミックを生成する
        /// </summary>
        /// <param name="avatarDescriptor">アバターを指定</param>
        /// <returns>ロックギミック</returns>
        public static Transform AddLockGimmick(VRCAvatarDescriptor avatarDescriptor)
        {
            if (avatarDescriptor)
            {
                // LockGimmickの作成
                var prefabPath = "Assets/VGC/VGCResources/Prefabs/VGCLockGimmick.prefab";
                var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (sourcePrefab)
                {
                    var targetAvatar = avatarDescriptor.gameObject;
                    // 既にあるなら追加しない
                    var gimmick = targetAvatar.transform.Find("VGCLockGimmick");
                    if (!gimmick)
                    {
                        // アバター直下に生成
                        var obj = UnityEngine.Object.Instantiate(sourcePrefab, targetAvatar.transform);
                        obj.name = sourcePrefab.name;
                        gimmick = obj.transform;
                    }
                    return gimmick;
                }
            }
            return null;
        }

        /// <summary>
        /// ロックギミックに配置されていれば指定のオブジェクトのTransformを取得する
        /// </summary>
        /// <param name="avatarDescriptor"></param>
        /// <param name="objName"></param>
        /// <returns></returns>
        public static Transform getLockObjectTransform(VRCAvatarDescriptor avatarDescriptor, string objName)
        {
            if (avatarDescriptor)
            {
                var gimmick = avatarDescriptor.transform.Find("VGCLockGimmick");
                if (gimmick)
                {
                    return gimmick.Find(objName);
                }
            }
            return null;
        }

        /// <summary>
        /// ロックギミックの親をセットアップする
        /// </summary>
        /// <param name="avatarDescriptor">アバターを指定</param>
        /// <param name="sourceRoot">生成したロックギミック</param>
        /// <param name="positionEnabled">座標固定するならtrue</param>
        /// <param name="rotationEnabled">回転固定するならtrue</param>
        public static void SetupLockGimmickRoot(VRCAvatarDescriptor avatarDescriptor, GameObject sourceRoot, bool positionEnabled = true, bool rotationEnabled = true)
        {
            // ワールドポイント取得
            var world = AddLockGimmick(avatarDescriptor).Find("WorldPoint");

            // 回転制御
            if (rotationEnabled)
            {
                var rotConstraint = sourceRoot.AddComponent<RotationConstraint>();
                var source = new ConstraintSource();
                source.sourceTransform = world;
                source.weight = -1;
                rotConstraint.AddSource(source);
                rotConstraint.weight = 1;
                rotConstraint.constraintActive = true;
            }

            // 位置制御
            if (positionEnabled)
            {
                var posConstraint = sourceRoot.AddComponent<PositionConstraint>();
                var source = new ConstraintSource();
                source.sourceTransform = world;
                source.weight = -1;
                posConstraint.AddSource(source);
                posConstraint.weight = 0.5f;
                posConstraint.constraintActive = true;
            }
        }

        /// <summary>
        /// Texture2Dのリサイズ
        /// </summary>
        /// <param name="texture">元のテクスチャ</param>
        /// <param name="width">リサイズ後の横幅</param>
        /// <param name="height">リサイズ後の高さ</param>
        /// <returns>リサイズ後のテクスチャ</returns>
        public static Texture2D GetResized(this Texture2D texture, int width, int height)
        {
            // リサイズ後のサイズを持つRenderTextureを一時的に作成して書き込む
            var rt = RenderTexture.GetTemporary(width, height);
            Graphics.Blit(texture, rt);

            // 設定しているRenderTextureを避難
            var tempRenderTexture = RenderTexture.active;
            // リサイズ後のサイズを持つTexture2Dを作成してRenderTextureから書き込む
            RenderTexture.active = rt;
            var ret = new Texture2D(width, height);
            ret.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            ret.Apply();
            // 元に戻す
            RenderTexture.active = tempRenderTexture;
            // 削除
            RenderTexture.ReleaseTemporary(rt);
            return ret;
        }

        /// <summary>
        /// コントロール名が一致するExpressionParametersの要素を削除する
        /// </summary>
        /// <param name="descriptor">対象のアバター</param>
        /// <param name="paramName">削除するパラメーター名</param>
        public static void DeleteExpressionParameters(ref VRCAvatarDescriptor descriptor, string paramName)
        {
            DeleateExpressionParameters(ref descriptor, paramName);
        }

        /// <summary>
        /// コントロール名が一致するExpressionParametersの要素を削除する(スペルミスのためDuplicate)
        /// </summary>
        /// <param name="descriptor">対象のアバター</param>
        /// <param name="paramName">削除するパラメーター名</param>
        public static void DeleateExpressionParameters(ref VRCAvatarDescriptor descriptor, string paramName)
        {
            if (descriptor.expressionParameters)
            {
                DeleteExpressionParameters(ref descriptor.expressionParameters, paramName);
            }
        }

        /// <summary>
        /// コントロール名が一致するExpressionParametersの要素を削除する
        /// </summary>
        /// <param name="param">対象のパラメータ</param>
        /// <param name="paramName">削除するパラメーター名</param>
        public static void DeleteExpressionParameters(ref VRCExpressionParameters param, string paramName)
        {
            var source = param.parameters;

            if (source.Length == 0)
                return;

            var newArray = new VRCExpressionParameters.Parameter[source.Length - 1];

            //param.parameters
            int index = 0;
            foreach (var parameter in source)
            {
                if (parameter.name == paramName)
                {
                    break;
                }
                index++;
            }

            // 見つからなかった
            if (index == source.Length)
            {
                return;
            }

            // 一致したindexの削除
            Array.Copy(source, 0, newArray, 0, index);
            Array.Copy(source, index + 1, newArray, index, source.Length - index - 1);
            param.parameters = newArray;
            EditorUtility.SetDirty(param);
        }

        /// <summary>
        /// コントロール名が一致するExpressionMenuの要素を削除する
        /// </summary>
        /// <param name="descriptor">対象のアバター</param>
        /// <param name="menuName">削除するコントロール名</param>
        public static void DeleteExpressionsMenu(ref VRCAvatarDescriptor descriptor, string menuName)
        {
            DeleateExpressionsMenu(ref descriptor, menuName);
        }

        /// <summary>
        /// コントロール名が一致するExpressionMenuの要素を削除する(スペルミスのためDuplicate)
        /// </summary>
        /// <param name="descriptor">対象のアバター</param>
        /// <param name="menuName">削除するコントロール名</param>
        public static void DeleateExpressionsMenu(ref VRCAvatarDescriptor descriptor, string menuName)
        {
            if (descriptor.expressionsMenu)
            {
                DeleteExpressionsMenu(ref descriptor.expressionsMenu, menuName);
            }
        }

        /// <summary>
        /// コントロール名が一致するExpressionMenuの要素を削除する
        /// </summary>
        /// <param name="menu">対象のメニュー</param>
        /// <param name="menuName">削除するコントロール名</param>
        public static void DeleteExpressionsMenu(ref VRCExpressionsMenu menu, string menuName)
        {
            for (int i = menu.controls.Count - 1; i >= 0; i--)
            {
                if (menu.controls[i].name == menuName)
                {
                    menu.controls.Remove(menu.controls[i]);
                    EditorUtility.SetDirty(menu);
                }
            }

        }

        /// <summary>
        /// アニメーションコントローラーから指定の名称のレイヤーを削除する
        /// </summary>
        /// <param name="controller">コントローラー</param>
        /// <param name="layerName">削除するレイヤー名</param>
        public static void DeleteAnimationLayer(ref AnimatorController controller, string layerName)
        {
            DeleateAnimationLayer(ref controller, layerName);
        }

        /// <summary>
        /// アニメーションコントローラーから指定の名称のレイヤーを削除する(スペルミスのためDuplicate)
        /// </summary>
        /// <param name="controller">コントローラー</param>
        /// <param name="layerName">削除するレイヤー名</param>
        public static void DeleateAnimationLayer(ref AnimatorController controller, string layerName)
        {
            int index = 0;
            foreach (var layer in controller.layers)
            {
                if (layer.name == layerName)
                {
                    break;
                }
                index++;
            }

            // 見つからなかった
            if (index == controller.layers.Length)
            {
                return;
            }

            controller.RemoveLayer(index);
        }

        /// <summary>
        /// アニメーションコントローラーから指定の名称のパラメーターを削除する
        /// </summary>
        /// <param name="controller">コントローラー</param>
        /// <param name="paramName">削除するパラメーター名</param>
        public static void DeleteAnimationParameter(ref AnimatorController controller, string paramName)
        {
            DeleateAnimationParameter(ref controller, paramName);
        }

        /// <summary>
        /// アニメーションコントローラーから指定の名称のパラメーターを削除する(スペルミスのためDuplicate)
        /// </summary>
        /// <param name="controller">コントローラー</param>
        /// <param name="paramName">削除するパラメーター名</param>
        public static void DeleateAnimationParameter(ref AnimatorController controller, string paramName)
        {
            int index = 0;
            foreach (var param in controller.parameters)
            {
                if (param.name == paramName)
                {
                    break;
                }
                index++;
            }

            // 見つからなかった
            if (index == controller.parameters.Length)
            {
                return;
            }

            controller.RemoveParameter(index);
        }

        /// <summary>
        /// Projectのアセットを選択する
        /// </summary>
        /// <param name="path">パス</param>
        public static void SelectAsset(string path)
        {
            var targetAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            Selection.activeObject = targetAsset;
            EditorGUIUtility.PingObject(targetAsset);
        }

        /// <summary>
        /// アセットを選択する
        /// </summary>
        /// <param name="path">パス</param>
        public static void SelectObject(UnityEngine.Object obj)
        {
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }


        // 古い順に追加
        private static string[] _defaultActionLayerPaths = {
            "Assets/VRCSDK/Examples3/Animation/Controllers/vrc_AvatarV3ActionLayer.controller", // VRCSDK3-AVATAR-2022.07.26.21.45_Public
            "Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/Animation/Controllers/vrc_AvatarV3ActionLayer.controller" // VCC
        };

        // デフォルトレイヤーGUID
        static string _defaultBaseLayerGUID = "4e4e1a372a526074884b7311d6fc686b";
        static string _defaultHandLayerGUID = "404d228aeae421f4590305bc4cdaba16";
        static string _defaultActionLayerGUID = "3e479eeb9db24704a828bffb15406520";
        static string _defaultSittingLayerGUID = "1268460c14f873240981bf15aa88b21a";
        static string _defaultTPoseLayerGUID = "00121b5812372b74f9012473856d8acf";
        static string _defaultIKPoseLayerGUID = "a9b90a833b3486e4b82834c9d1f7c4ee";

        /// <summary>
        /// デフォルトレイヤーパス取得
        /// </summary>
        public static string GetDefaultLayerPath(VRCAvatarDescriptor.AnimLayerType type)
        {
            string guid = "";
            switch (type)
            {
                case VRCAvatarDescriptor.AnimLayerType.Action:
                    guid = _defaultActionLayerGUID;
                    Debug.Log("action");
                    break;
                case VRCAvatarDescriptor.AnimLayerType.Base:
                    guid = _defaultBaseLayerGUID;
                    break;
                case VRCAvatarDescriptor.AnimLayerType.FX:
                    guid = _defaultHandLayerGUID;
                    break;
                case VRCAvatarDescriptor.AnimLayerType.Gesture:
                    guid = _defaultHandLayerGUID;
                    break;
                case VRCAvatarDescriptor.AnimLayerType.Sitting:
                    guid = _defaultSittingLayerGUID;
                    break;
                case VRCAvatarDescriptor.AnimLayerType.TPose:
                    guid = _defaultTPoseLayerGUID;
                    break;
                case VRCAvatarDescriptor.AnimLayerType.IKPose:
                    guid = _defaultIKPoseLayerGUID;
                    break;
            }
            return AssetDatabase.GUIDToAssetPath(guid);
        }

        /// <summary>
        /// デフォルトレイヤー取得
        /// </summary>
        public static AnimatorController GetDefaultLayer(VRCAvatarDescriptor.AnimLayerType type)
        {
            string path = GetDefaultLayerPath(type);

            if (path == "")
                return null;

            return AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        }

    }
}