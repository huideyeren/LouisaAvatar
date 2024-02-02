/*
 * KawaiiSittingSetup
 * 可愛い座りツールの簡易設定用ツール
 * 
 * Copyright(c) 2022 UniSakiStudio
 * Released under the MIT license
 * https://opensource.org/licenses/mit-license.php
 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Avatars.Components;

public class KawaiiSittingSetup : EditorWindow
{
    public VRCAvatarDescriptor SelectAvater;
    public SettingAnswer footHeightAnswer;
    public int FootHeightAnswerSelection;
    private string outputLogs = "";
    private string errorLogs = "";
    private RuntimeAnimatorController prevCheckAnimationController = null;
    private bool prevOtherAnimatorControllerIsExist = false;
    private Vector2 _scrollPosition = Vector2.zero;

    public enum SettingAnswer {
        Yes,
        No,
        Cancel,
        NotSelection,
    };

    public void DisplayDialogAndDebugMessage(string title, string message, bool isError = true, bool dialog = false)
    {
        if (isError)
        {
            if (errorLogs.Length > 0)
            {
                errorLogs += "\n";
            }
            errorLogs += message;
            Debug.LogError(message);
        }
        else
        {
            if (outputLogs.Length > 0)
            {
                outputLogs += "\n";
            }
            outputLogs += message;
            Debug.Log(message);
        }
        if (dialog)
        {
            EditorUtility.DisplayDialog(title, message, "OK");
        }
    }

    static public T FindAndLoadAsset<T>(string filter, bool filenameMatch = false) where T : Object
    {
        var assetGuids = AssetDatabase.FindAssets(filter);
        if (assetGuids.Length == 0)
        {
            return default(T);
        }
        var filename = filter.Split(" ".ToCharArray())[0];
        var assetPaths = assetGuids.Select(guid => AssetDatabase.GUIDToAssetPath(guid));
        var assetPath = assetPaths.FirstOrDefault((path) => {
            return System.IO.Path.GetFileNameWithoutExtension(path).Equals(filename);
        });
        if (assetPath == null && !filenameMatch)
        {
            assetPath = assetPaths.First();
        }

        return AssetDatabase.LoadAssetAtPath<T>(assetPath);
    }


    public void SetAll()
    {
        SetParameter();
        SetMenu();
        SetBaseLayer();
    }

    [MenuItem("GameObject/ゆにさきスタジオ/可愛い座りツール設定画面", false, 30)]
    static public void OpenWindow()
    {
        var window = GetWindow<KawaiiSittingSetup>("可愛い座りツール初期設定");

        if (Selection.activeGameObject)
        {
            var avater = Selection.activeGameObject.GetComponent<VRCAvatarDescriptor>();
            window.SelectAvater = avater;
        }
    }

    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        EditorGUILayout.LabelField("可愛い座りツール　初期設定", new GUIStyle() { fontStyle = FontStyle.Bold, fontSize = 20, });
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("設定項目", new GUIStyle() { fontStyle = FontStyle.Bold, });
        EditorGUILayout.BeginVertical(GUI.skin.box);

        // アバターの選択
        SelectAvater = EditorGUILayout.ObjectField("アバター", SelectAvater, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;

        if (SelectAvater == null)
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.HelpBox("アバターを選択してください", MessageType.Error);
            EditorGUILayout.EndScrollView();
            return;
        }

        // 利き手の選択
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("足の高さ調節機能");
        FootHeightAnswerSelection = GUILayout.SelectionGrid(FootHeightAnswerSelection, new string[] { "使用する", "不使用", }, 2, new GUIStyle(EditorStyles.radioButton), GUILayout.Width(130));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("足の高さ変更機能を使用すると、足の高さを自由に変更できて好きな高さに座ることができます。ただしExpressionParameter（最大256bit）のうち8bitを使用します（このツールはこの機能以外でも27bit使用します", new GUIStyle() { fontStyle = FontStyle.Italic, fontSize = 10, wordWrap = true, });

        var baseLayer = SelectAvater.baseAnimationLayers.First(layer => layer.type == VRCAvatarDescriptor.AnimLayerType.Base);
        var animatorController = baseLayer.animatorController;
        if (animatorController != prevCheckAnimationController)
        {
            if (animatorController == null)
            {
                prevOtherAnimatorControllerIsExist = false;
                prevCheckAnimationController = animatorController;
            }
            else
            {
                var officialAnimatorControllerNames = new List<string>()
                {
                    "KawaiiSitting_Locomotion",
                    "SleepTogether_Locomotion",
                    "SleepTogether_KawaiiSitting_Locomotion",
                    "VirtualLoveBoy_Locomotion",
                    "VirtualLoveBoy_KawaiiSitting_Locomotion",
                    "VirtualLoveGirl_Locomotion",
                    "VirtualLoveGirl_KawaiiSitting_Locomotion",
                    "SleepTogether_VirtualLoveBoy_Locomotion",
                    "SleepTogether_VirtualLoveBoy_KawaiiSitting_Locomotion",
                    "SleepTogether_VirtualLoveGirl_Locomotion",
                    "SleepTogether_VirtualLoveGirl_KawaiiSitting_Locomotion",
                };
                prevOtherAnimatorControllerIsExist = true;
                foreach (var name in officialAnimatorControllerNames)
                {
                    var officialAnimatorController = FindAndLoadAsset<AnimatorController>(name + " t:AnimatorController");
                    if (animatorController == officialAnimatorController)
                    {
                        prevOtherAnimatorControllerIsExist = false;
                        break;
                    }
                }
                prevCheckAnimationController = animatorController;
            }
        }
        if (prevOtherAnimatorControllerIsExist)
        {
            EditorGUILayout.HelpBox("アバターのBaseLayerに既に別のAnimatorControllerが設定されています", MessageType.Error);
            if (GUILayout.Button("[AutoFix]BaseLayerのAnimatorControllerを解除", GUILayout.Height(40)))
            {
                var index = SelectAvater.baseAnimationLayers.ToList().IndexOf(baseLayer);
                SelectAvater.baseAnimationLayers[index].animatorController = null;
                EditorUtility.SetDirty(SelectAvater);
            }
        }

        EditorGUILayout.EndVertical();

        // 初期設定開始ボタン
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        using (new EditorGUI.DisabledScope(SelectAvater == null))
        {
            EditorGUILayout.LabelField("1.初期設定", new GUIStyle() { fontStyle = FontStyle.Bold, });
            EditorGUILayout.LabelField("このボタンを押すと、ツールを使用するのに必要な設定を自動で行います");
            if (GUILayout.Button("初期設定を開始", GUILayout.Height(40)))
            {
                footHeightAnswer = FootHeightAnswerSelection == 0 ? SettingAnswer.Yes : SettingAnswer.No;

                errorLogs = "";

                SetAll();

                if (errorLogs.Length > 0)
                {
                    DisplayDialogAndDebugMessage("エラー", "エラーが発生しました。確認してください", true, true);
                }
                else
                {
                    DisplayDialogAndDebugMessage("設定完了", "設定完了しました", false, true);
                }
            }
        }

        // ログ表示
        if (errorLogs.Length > 0)
        {
            EditorGUILayout.HelpBox(errorLogs, MessageType.Error);
        }

        EditorGUILayout.LabelField("ログ出力", new GUIStyle() { fontStyle = FontStyle.Bold, });
        var wrapStyle = new GUIStyle(EditorStyles.textArea);
        wrapStyle.wordWrap = true;
        EditorGUILayout.TextArea(outputLogs, wrapStyle, GUILayout.ExpandHeight(true));

        EditorGUILayout.EndScrollView();
    }

    public void SetParameter()
    {
        // ExpressionsがCustomizeされてなかったらする
        if (!SelectAvater.customExpressions)
        {
            SelectAvater.customExpressions = true;
        }

        // ExpressionParametersを設定していなかったら用意したファイルをコピーして使う
        if (SelectAvater.expressionParameters == null)
        {
            // 可愛い座り用のパラメーターを検索
            var parameterAssetName = footHeightAnswer == SettingAnswer.Yes ? "KawaiiSitting_FootHeight_ExpressionParameters" : "KawaiiSitting_ExpressionParameters";
            var parametersAssetGuids = AssetDatabase.FindAssets(parameterAssetName);
            if (parametersAssetGuids.Length == 0)
            {
                DisplayDialogAndDebugMessage("エラー", string.Format("プロジェクトに{0}.assetがありません。正しくツールをインポートしてください", parameterAssetName));
                return;
            }
            var parametersPath = AssetDatabase.GUIDToAssetPath(parametersAssetGuids[0]);
            var toolDirectoryPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(parametersPath)) + "/UserGeneratedData";
            System.IO.Directory.CreateDirectory(toolDirectoryPath);
            var newParameterPath = AssetDatabase.GenerateUniqueAssetPath(toolDirectoryPath + "/" + SelectAvater.name + "_ExpressionParameters.asset");
            AssetDatabase.CopyAsset(parametersPath, newParameterPath);

            var parametersAsset = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(newParameterPath);
            SelectAvater.expressionParameters = parametersAsset;

            EditorUtility.SetDirty(SelectAvater);
            EditorUtility.SetDirty(SelectAvater.expressionParameters);
            AssetDatabase.SaveAssets();

            DisplayDialogAndDebugMessage("設定完了", "ツールに付属したパラメータファイルをコピーして設定しました", false);
            return;
        }

        var newParams = new List<(string name, VRCExpressionParameters.ValueType valueType, float defaultValue, bool saved)>();
        newParams.Add(("SitShallow", VRCExpressionParameters.ValueType.Float, 0, true));
        newParams.Add(("SitDeep", VRCExpressionParameters.ValueType.Float, 0, true));
        newParams.Add(("SitSleep", VRCExpressionParameters.ValueType.Float, 0, true));
        newParams.Add(("LocomotionLock", VRCExpressionParameters.ValueType.Bool, 0, true));
        newParams.Add(("HeadTrackingLock", VRCExpressionParameters.ValueType.Bool, 0, true));
        newParams.Add(("HandTrackingLock", VRCExpressionParameters.ValueType.Bool, 0, true));
        if (footHeightAnswer == SettingAnswer.Yes)
        {
            newParams.Add(("FootHeight", VRCExpressionParameters.ValueType.Float, 0, true));
        }
        var deleteParams = new List<(string name, VRCExpressionParameters.ValueType valueType)>();
        if (footHeightAnswer == SettingAnswer.No)
        {
            deleteParams.Add(("FootHeight", VRCExpressionParameters.ValueType.Float));
        }

        int prevParameterNum = SelectAvater.expressionParameters.parameters.Length;
        int deletedParametersNum = 0;
        SelectAvater.expressionParameters.parameters = SelectAvater.expressionParameters.parameters.Where((param) => {
            return deleteParams.Count(deleteParam => deleteParam.name.Equals(param.name) && deleteParam.valueType == param.valueType) == 0;
        }).ToArray();
        deletedParametersNum = prevParameterNum - SelectAvater.expressionParameters.parameters.Length;

        int appendedParametersNum = 0;
        foreach (var newParam in newParams) {
            if (ArrayUtility.Find(SelectAvater.expressionParameters.parameters, (param) => newParam.name == param.name) != null)
            {
                continue;
            }
            var parameter = new VRCExpressionParameters.Parameter();
            parameter.name = newParam.name;
            parameter.valueType = newParam.valueType;
            parameter.defaultValue = newParam.defaultValue;
            parameter.saved = newParam.saved;

            ArrayUtility.Add(ref SelectAvater.expressionParameters.parameters, parameter);
            appendedParametersNum++;
        }

        if (appendedParametersNum > 0 || deletedParametersNum > 0)
        {
            EditorUtility.SetDirty(SelectAvater);
            EditorUtility.SetDirty(SelectAvater.expressionParameters);
            AssetDatabase.SaveAssets();
        }

        if (appendedParametersNum > 0 && deletedParametersNum > 0)
        {
            DisplayDialogAndDebugMessage("設定完了", string.Format("アバターのパラメータファイルに{0}個のパラメータを追加し、{1}個のパラメータを削除しました", appendedParametersNum, deletedParametersNum), false);
        }
        else if (appendedParametersNum > 0)
        {
            DisplayDialogAndDebugMessage("設定完了", string.Format("アバターのパラメータファイルに{0}個のパラメータを追加しました", appendedParametersNum), false);
        }
        else if (deletedParametersNum > 0)
        {
            DisplayDialogAndDebugMessage("設定完了", string.Format("アバターのパラメータファイルに{0}個のパラメータを削除しました", deletedParametersNum), false);
        }
        else
        {
            DisplayDialogAndDebugMessage("設定不要", "アバターにはすでに必要なパラメータが設定されていたので、変更を行いませんでした", false);
        }

    }

    public void SetMenu()
    {
        // ExpressionsがCustomizeされてなかったらする
        if (!SelectAvater.customExpressions)
        {
            SelectAvater.customExpressions = true;
        }

        // 可愛い座り用のメニューを検索
        var menuAssetGuids = AssetDatabase.FindAssets("KawaiiSitting_ExpressionsMenu");
        if (menuAssetGuids.Length == 0)
        {
            DisplayDialogAndDebugMessage("エラー", "プロジェクトにKawaiiSitting_ExpressionsMenu.assetがありません。正しくツールをインポートしてください");
            return;
        }
        var menuPath = AssetDatabase.GUIDToAssetPath(menuAssetGuids[0]);
        var menuAsset = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(menuPath);

        var menuAssetWithFoot = FindAndLoadAsset<VRCExpressionsMenu>("KawaiiSitting_FootHeight_ExpressionsMenu", true);

        // ExpressionsMenuを設定していなかったら作る
        if (SelectAvater.expressionsMenu == null)
        {
            var newExpressionMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();

            var toolDirectoryPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(menuPath)) + "/UserGeneratedData";
            System.IO.Directory.CreateDirectory(toolDirectoryPath);
            var uniquePath = AssetDatabase.GenerateUniqueAssetPath(toolDirectoryPath + "/" + SelectAvater.name + "_ExpressionsMenu.asset");
            AssetDatabase.CreateAsset(newExpressionMenu, uniquePath);

            SelectAvater.expressionsMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(uniquePath);
        }

        var menuIcon = FindAndLoadAsset<Texture2D>("KawaiiSitting_Menu t:Texture", true);
        if (menuIcon == null)
        {
            DisplayDialogAndDebugMessage("エラー", "プロジェクトにSleepTogether_Menu.pngがありません。正しくツールをインポートしてください");
        }

        // すでに設定してあったら変更が必要な項目だけ差し替える
        var currectSubMenu = footHeightAnswer == SettingAnswer.Yes ? menuAssetWithFoot : menuAsset;
        var existMenuAsset = SelectAvater.expressionsMenu.controls.Find((menu) => menu.subMenu == menuAsset || menu.subMenu == menuAssetWithFoot);
        if (existMenuAsset != null)
        {
            bool noChange = true;

            if (existMenuAsset.icon == null)
            {
                existMenuAsset.icon = menuIcon;
                EditorUtility.SetDirty(SelectAvater);
                EditorUtility.SetDirty(SelectAvater.expressionsMenu);
                AssetDatabase.SaveAssets();
                noChange = false;
                DisplayDialogAndDebugMessage("設定完了", "アバターにメニューにアイコンを設定しました", false);
            }

            if (existMenuAsset.subMenu != currectSubMenu)
            {
                existMenuAsset.subMenu = currectSubMenu;
                EditorUtility.SetDirty(SelectAvater);
                EditorUtility.SetDirty(SelectAvater.expressionsMenu);
                AssetDatabase.SaveAssets();
                noChange = false;
                DisplayDialogAndDebugMessage("設定完了", "アバターにメニューを差し替えました", false);
            }

            if (noChange)
            {
                DisplayDialogAndDebugMessage("設定不要", "アバターにはすでに必要なメニューが設定されていたので、変更を行いませんでした", false);
            }
            return;
        }


        // メニューアイテムは7個が最大
        if (SelectAvater.expressionsMenu != null && SelectAvater.expressionsMenu.controls.Count >= 7)
        {
            DisplayDialogAndDebugMessage("設定不要", "アバターのメニューにはすでに7つコントロールがあったので追加できませんでした", false);
            return;
        }

        var newControl = new VRCExpressionsMenu.Control();
        newControl.name = "可愛い座り";
        newControl.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
        newControl.subMenu = currectSubMenu;
        newControl.icon = menuIcon;
        SelectAvater.expressionsMenu.controls.Add(newControl);

        EditorUtility.SetDirty(SelectAvater);
        EditorUtility.SetDirty(SelectAvater.expressionsMenu);
        AssetDatabase.SaveAssets();

        DisplayDialogAndDebugMessage("設定完了", "アバターにメニューを追加しました", false);
    }

    public void SetBaseLayer()
    {
        AnimatorController baseAnimatorController = null;
        if (SelectAvater.baseAnimationLayers != null)
        {
            var animLayerIndex = ArrayUtility.FindIndex(SelectAvater.baseAnimationLayers, (layer) => layer.type == VRCAvatarDescriptor.AnimLayerType.Base && !layer.isDefault && layer.animatorController != null);
            if (animLayerIndex != -1)
            {
                baseAnimatorController = SelectAvater.baseAnimationLayers[animLayerIndex].animatorController as AnimatorController;
            }
        }

        // Baserレイヤー設定済み
        var kawaiiSittingAnimatorController = FindAndLoadAsset<AnimatorController>("KawaiiSitting_Locomotion t:AnimatorController");
        if (baseAnimatorController != null)
        {
            var replace_settings = new[]
            {
                new { extension = "SleepTogether_Locomotion", replace = "SleepTogether_KawaiiSitting_Locomotion", message = "添い寝ツールが設定されていたため、併用できるAnimatorControllerをアバターに設定しました", },
                new { extension = "VirtualLoveBoy_Locomotion", replace = "VirtualLoveBoy_KawaiiSitting_Locomotion", message = "三点だいしゅきツール・男の子用が設定されていたため、併用できるAnimatorControllerをアバターに設定しました", },
                new { extension = "VirtualLoveGirl_Locomotion", replace = "VirtualLoveGirl_KawaiiSitting_Locomotion", message = "三点だいしゅきツール・女の子用が設定されていたため、併用できるAnimatorControllerをアバターに設定しました", },
                new { extension = "SleepTogether_VirtualLoveBoy_Locomotion", replace = "SleepTogether_VirtualLoveBoy_KawaiiSitting_Locomotion", message = "添い寝ツールと三点だいしゅきツール・男の子用が設定されていたため、併用できるAnimatorControllerをアバターに設定しました", },
                new { extension = "SleepTogether_VirtualLoveGirl_Locomotion", replace = "SleepTogether_VirtualLoveGirl_KawaiiSitting_Locomotion", message = "添い寝ツールと三点だいしゅきツール・女の子用が設定されていたため、併用できるAnimatorControllerをアバターに設定しました", },
            };

            foreach (var replace_setting in replace_settings)
            {
                var extensionAnimatorController = FindAndLoadAsset<AnimatorController>(string.Format("{0} t:AnimatorController", replace_setting.extension), true);
                var replaceAnimatorController = FindAndLoadAsset<AnimatorController>(string.Format("{0} t:AnimatorController", replace_setting.replace), true);
                if (baseAnimatorController == extensionAnimatorController && replaceAnimatorController != null)
                {
                    baseAnimatorController = replaceAnimatorController;
                    var animLayer = SelectAvater.baseAnimationLayers.First(layer => layer.type == VRCAvatarDescriptor.AnimLayerType.Base);
                    var animLayerIndex = ArrayUtility.IndexOf(SelectAvater.baseAnimationLayers, animLayer);
                    SelectAvater.baseAnimationLayers[animLayerIndex].animatorController = baseAnimatorController;
                    SelectAvater.baseAnimationLayers[animLayerIndex].isDefault = false;
                    SelectAvater.baseAnimationLayers[animLayerIndex].isEnabled = true;
                    EditorUtility.SetDirty(SelectAvater);
                    AssetDatabase.SaveAssets();

                    DisplayDialogAndDebugMessage("設定完了", replace_setting.message, false);
                    return;
                }

                if (baseAnimatorController == replaceAnimatorController)
                {
                    DisplayDialogAndDebugMessage("設定不要", "すでにBaseLayerに可愛い座りツール用のAnimatorControllerが設定されているので、変更しませんでした", false);
                    return;
                }
            }
            if (baseAnimatorController == kawaiiSittingAnimatorController)
            {
                DisplayDialogAndDebugMessage("設定不要", "すでにBaseLayerに可愛い座りツール用のAnimatorControllerが設定されているので、変更しませんでした", false);
                return;
            }

            DisplayDialogAndDebugMessage("エラー", "すでにBaseLayerにAnimatorControllerが設定されているので、変更しませんでした。手動で変更するか、一度BaseLayerに設定しているAnimatorControllerを外してから再度試してください");
            return;
        }


        {
            baseAnimatorController = kawaiiSittingAnimatorController;
            var animLayer = SelectAvater.baseAnimationLayers.First(layer => layer.type == VRCAvatarDescriptor.AnimLayerType.Base);
            var animLayerIndex = ArrayUtility.IndexOf(SelectAvater.baseAnimationLayers, animLayer);
            SelectAvater.baseAnimationLayers[animLayerIndex].animatorController = baseAnimatorController;
            SelectAvater.baseAnimationLayers[animLayerIndex].isDefault = false;
            SelectAvater.baseAnimationLayers[animLayerIndex].isEnabled = true;
            EditorUtility.SetDirty(SelectAvater);
            AssetDatabase.SaveAssets();

            DisplayDialogAndDebugMessage("設定完了", "アバターのBaseレイヤーに可愛い座りツール用の設定を追加しました", false);
        }
    }



    [MenuItem("GameObject/ゆにさきスタジオ/可愛い座りツール設定画面", true)]
    private static bool Validate()
    {
        if (!Selection.activeGameObject)
        {
            return false;
        }
        var avator = Selection.activeGameObject.GetComponent<VRCAvatarDescriptor>();
        return avator != null;
    }
}
