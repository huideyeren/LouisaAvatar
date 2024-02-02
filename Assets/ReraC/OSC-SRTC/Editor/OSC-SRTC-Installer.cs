using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

public class OSC_SRTC_Installer : EditorWindow

{

    [MenuItem("ReraC/Install OSC-SRTC")]
    public static void ShowWindow()
    {
        var window = GetWindow<OSC_SRTC_Installer>("OSC-SRTC Installer");
        window.maxSize = new Vector2(500, 280);
        window.minSize = new Vector2(500, 280);
    }
    
    
    private VRCAvatarDescriptor avatar;


    private readonly string ParamPath = "Assets/ReraC/OSC-SRTC/SRTC Params.asset";
    private readonly string MenuPath = "Assets/ReraC/OSC-SRTC/SRTC Menu.asset";

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUI.skin.label.fontSize = 20;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Install OSC-SRTC");
        GUI.skin.label.fontSize = 12;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("0.8A");
        GUILayout.Space(10);

        GUI.skin.label.alignment = TextAnchor.MiddleLeft;


        var nAvatar = (VRCAvatarDescriptor)EditorGUILayout.ObjectField("Avatar", avatar, typeof(VRCAvatarDescriptor), true);

        GUILayout.Space(10);
        if (nAvatar != avatar)
        {
            avatar = nAvatar;
        }


        if(avatar !=null)
        {
            var srcMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(MenuPath);
            var srcParams = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(ParamPath);

            var dstMenu = avatar.expressionsMenu;
            var dstParams = avatar.expressionParameters;

            EditorGUILayout.LabelField("- Check destination Menu / Parameters");
            EditorGUILayout.ObjectField("Menu", dstMenu, typeof(VRCExpressionsMenu), true);
            EditorGUILayout.ObjectField("Expressions", dstParams, typeof(VRCExpressionParameters), true);
            GUILayout.Space(20);


            if (!dstMenu)
            {
                EditorGUILayout.HelpBox("Not found Expressions Menu. check first.", MessageType.Error);
                return;
            }
            
            if (!dstParams)
            {
                EditorGUILayout.HelpBox("Not found Expressions Parameters. check first.", MessageType.Error);
                return;
            }

            if (dstMenu.controls.Count >= 8)
            {
                EditorGUILayout.HelpBox("Too many Controls in Expressions Menu. check first.", MessageType.Error);
            }
            else if (GUILayout.Button("Install"))
            {
                MergeExpressionParameters(srcParams, dstParams);
                AddExpressionsMenu(srcMenu, dstMenu);
            }
            if(GUILayout.Button("Uninstall"))
            {
                DelExpressionParameters(srcParams, dstParams);
                DelExpressionsMenu(dstMenu);
            }

        }

        GUILayout.Space(15);
    }

    public void AddExpressionsMenu(VRCExpressionsMenu src, VRCExpressionsMenu dst)
    {
        VRCExpressionsMenu.Control NewExControl = new VRCExpressionsMenu.Control();
        NewExControl.name = "OSC-SRTC";
        NewExControl.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
        NewExControl.subMenu = src;

        dst.controls.Add(NewExControl);

        EditorUtility.SetDirty(dst);
        AssetDatabase.SaveAssets();
    }

    public void DelExpressionsMenu(VRCExpressionsMenu dst)
    {
        foreach(var i in dst.controls.ToArray())
        {
            if(i.name=="OSC-SRTC")
            {
                dst.controls.Remove(i);
            }
        }

        EditorUtility.SetDirty(dst);
        AssetDatabase.SaveAssets();
    }

    public void MergeExpressionParameters(VRCExpressionParameters src, VRCExpressionParameters dst)
    {
        if (!src || !dst || src == dst)
            return;
        List<VRCExpressionParameters.Parameter> MergedParams = new List<VRCExpressionParameters.Parameter>();

        foreach (var i in dst.parameters)
        {
            if (!MergedParams.Contains(i))
            {
                if (i.name != "")
                {
                    MergedParams.Add(i);

                }
            }
        }
        foreach (var i in src.parameters)
        {
            if (!MergedParams.Contains(i))
            {
                bool flag = false;
                foreach (var j in MergedParams.ToArray())
                {
                    if (i.name == j.name)
                        flag = true;
                }

                if (i.name != "" && !flag)
                {
                    MergedParams.Add(i);
                }
            }
        }

        dst.parameters = MergedParams.ToArray();
        EditorUtility.SetDirty(dst);
        AssetDatabase.SaveAssets();
    }

    public void DelExpressionParameters(VRCExpressionParameters src, VRCExpressionParameters dst)
    {
        if (!src || !dst)
            return;
        List<VRCExpressionParameters.Parameter> MergedParams = new List<VRCExpressionParameters.Parameter>();

        foreach (var i in dst.parameters)
        {
            if (!MergedParams.Contains(i))
            {
                if (i.name != "")
                {
                    MergedParams.Add(i);

                }
            }
        }

        foreach (var i in src.parameters)
        {
                foreach (var j in MergedParams.ToArray())
                {
                    if (i.name == j.name)
                    {
                        Debug.Log("del " + j.name);
                    MergedParams.Remove(j);
                    }
                }
        }

        dst.parameters = MergedParams.ToArray();
        EditorUtility.SetDirty(dst);
        AssetDatabase.SaveAssets();
    }
}
