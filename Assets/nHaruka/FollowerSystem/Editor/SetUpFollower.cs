﻿using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using VRC.Core;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using static SetUpFollower;
using static VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
using static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control;

public class SetUpFollower : EditorWindow
{

    private VRCAvatarDescriptor avatarDescriptor;
    private VRCAvatarDescriptor HumAvatar;
    private bool debugMode = false;
    private bool WriteDefault = true;
    private string convertedDataPath = "Assets/nHaruka/FollowerSystem/Animation/ConvertedAnimation/";
    private string controllerPath = "Assets/nHaruka/FollowerSystem/Animation/Humanoid/HumanoidLocomotion.asset";
    //private string convertedControllerPath = "Assets/nHaruka/FollowerSystem/Animation/ConvertedAnimation/LocomotionConverted.asset";
    private string avatarPath = "FollowerSystem/Follower/Avatar";
    private AnimatorController humLocomotionController;
    private bool useFX = false;
    private bool[] inheritLayers = null;
    private Vector2 scrollPosition;
    private AnimatorController origFX;
    private VRCExpressionParameters origExParams;
    private VRCExpressionsMenu origExMenu;


    // Start is called before the first frame update
    [MenuItem("nHaruka/FollowerSystem")]
    // Start is called before the first frame update
    private static void Init()
    {
        var window = GetWindowWithRect<SetUpFollower>(new Rect(0, 0, 400, 600));
        window.Show();
    }
    
    // Update is called once per frame
    private void OnGUI()
    {
        GUILayout.Space(10);
        avatarDescriptor =
            (VRCAvatarDescriptor)EditorGUILayout.ObjectField("MainAvatar", avatarDescriptor, typeof(VRCAvatarDescriptor), true);
        
        GUILayout.Space(10);
        HumAvatar =
            (VRCAvatarDescriptor)EditorGUILayout.ObjectField("FollowerAvatar", HumAvatar, typeof(VRCAvatarDescriptor), true);

        if (HumAvatar != null)
        {
            var FxAnimatorLayer =
                        HumAvatar.GetComponent<VRCAvatarDescriptor>().baseAnimationLayers.First(item => item.type == VRCAvatarDescriptor.AnimLayerType.FX && item.animatorController != null);

            origExParams = HumAvatar.GetComponent<VRCAvatarDescriptor>().expressionParameters;

            origExMenu = HumAvatar.GetComponent<VRCAvatarDescriptor>().expressionsMenu;

            origFX = (AnimatorController)FxAnimatorLayer.animatorController;

            if (origFX != null)
            {
                useFX = GUILayout.Toggle(useFX, "元のアバターのFXレイヤーを継承する");

                if (useFX)
                {
                    GUILayout.Label("継承するレイヤーを選択");
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                    if (inheritLayers == null || inheritLayers.Length != origFX.layers.Length)
                    {
                        inheritLayers = new bool[origFX.layers.Length];
                    }
                    for (int i = 0; i < inheritLayers.Length; i++)
                    {
                        inheritLayers[i] = GUILayout.Toggle(inheritLayers[i], origFX.layers[i].name);
                    }
                    GUILayout.EndScrollView();

                }

            }
        }
        GUILayout.Space(10);
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.wordWrap = true;
        style.fontStyle = FontStyle.Normal;
        WriteDefault = GUILayout.Toggle(WriteDefault, "WriteDefaults");
        EditorGUILayout.LabelField("※導入アバターのFXレイヤーがどちらで統一されているかによって選択してください。\n統一されていないと表情がおかしくなったり正しく機能しなかったりします。", style);
        GUILayout.Space(10);

        if (GUILayout.Button("Setup"))
        {
            try
            {
                setup();
                EditorUtility.DisplayDialog("Finished", "Finished!", "OK");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", "An error occurred. See console log.", "OK");
                Debug.LogError(e);
                remove();
            }
        }
        if (GUILayout.Button("Remove"))
        {

            try
            {
                remove();
                EditorUtility.DisplayDialog("Finished", "Finished!", "OK");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", "An error occurred. See console log.", "OK");
                Debug.LogError(e);
            }
        }

    }

    void setup()
    {
        
        try
        {
            remove();
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }

        var mainAnimator = avatarDescriptor.GetComponent<Animator>();

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/nHaruka/FollowerSystem/FollowerSystem.prefab");
        var SystemRoot = GameObject.Instantiate<GameObject>(prefab);
        SystemRoot.name = "FollowerSystem";
        SystemRoot.transform.parent = avatarDescriptor.transform;
        SystemRoot.transform.position = avatarDescriptor.transform.position;
        SystemRoot.transform.rotation = avatarDescriptor.transform.rotation;

        var FollowerRoot = SystemRoot.transform.Find("Follower");

        HumAvatar.transform.parent = FollowerRoot;
        HumAvatar.transform.position = FollowerRoot.transform.position;
        HumAvatar.transform.rotation = FollowerRoot.transform.rotation;

        HumAvatar.name = "Avatar";

        humLocomotionController = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        AnimationClip[] animationClips = humLocomotionController.animationClips;
        AnimationClip[] convertedClips = new AnimationClip[animationClips.Length];

        try
        {
            foreach (var anim in animationClips)
            {
                var convertedAnimationClip = ConvertHumanoidToGeneric(anim, HumAvatar.gameObject);
                convertedClips[Array.IndexOf(animationClips, anim)] = convertedAnimationClip;
                if (!Directory.Exists(convertedDataPath + avatarDescriptor.name))
                {
                    Directory.CreateDirectory(convertedDataPath + avatarDescriptor.name);
                }
                AssetDatabase.DeleteAsset(convertedDataPath + avatarDescriptor.name + "/" + convertedAnimationClip.name + ".anim");
                AssetDatabase.CreateAsset(convertedClips[Array.IndexOf(animationClips, anim)], convertedDataPath + avatarDescriptor.name + "/" + convertedAnimationClip.name + ".anim");
            }
        }
        catch(Exception e) 
        {
            EditorUtility.DisplayDialog("Error", "AvatarConversion Faild", "OK");
            Debug.LogException(e);
            remove();
            return;
        }

        var convertedControllerPath = convertedDataPath + avatarDescriptor.name + "/LocomotionConverted.asset";

        if (File.Exists(convertedControllerPath))
        {
            AssetDatabase.DeleteAsset(convertedControllerPath);
        }

        AssetDatabase.CopyAsset(controllerPath, convertedControllerPath);
        

        var convertedController = AssetDatabase.LoadAssetAtPath<AnimatorController>(convertedControllerPath);

        foreach(var layer in convertedController.layers)
        {
            foreach (var state in layer.stateMachine.states)
            {
                foreach (var clip in convertedClips)
                {
                    if (clip != null && state.state.motion.name == "Blend Tree")
                    {
                        BlendTree blendTree = (BlendTree)state.state.motion;
                        var children = blendTree.children;
                        for (int n = 0; n < children.Length; n++)
                        {
                            if (children[n].motion.name == clip.name)
                            {
                                children[n].motion = clip;
                            }
                        }
                        blendTree.children = children;
                    }
                    if (clip != null && state.state.motion.name == clip.name)
                    {
                        state.state.motion = clip;
                    }
                    
                }
            }
        }

        var followerAnimator = HumAvatar.GetComponent<Animator>();

        var AvatarAnchorCons = SystemRoot.transform.Find("Base/AvatarAnchor").GetComponent<ParentConstraint>();
        AvatarAnchorCons.SetSource(0, new ConstraintSource { weight = 1, sourceTransform = avatarDescriptor.transform });

        var RemotePositionRootAnchorCons = SystemRoot.transform.Find("RemotePositionRoot/Anchor").GetComponent<PositionConstraint>();
        RemotePositionRootAnchorCons.SetSource(0, new ConstraintSource { weight = 1, sourceTransform = avatarDescriptor.transform });



        var IKEO = FollowerRoot.transform.Find("AimIK").GetComponent<RootMotion.FinalIK.IKExecutionOrder>();
        IKEO.animator = avatarDescriptor.GetComponent<Animator>();
        var AimIKHead = FollowerRoot.transform.Find("AimIK/Head").GetComponent<RootMotion.FinalIK.AimIK>();
        AimIKHead.solver.transform = followerAnimator.GetBoneTransform(HumanBodyBones.Head);
        AimIKHead.solver.bones[0].transform = followerAnimator.GetBoneTransform(HumanBodyBones.Spine);
        AimIKHead.solver.bones[0].weight = 0.3f;
        AimIKHead.solver.bones[1].transform = followerAnimator.GetBoneTransform(HumanBodyBones.Chest);
        AimIKHead.solver.bones[1].weight = 0.6f;
        AimIKHead.solver.bones[2].transform = followerAnimator.GetBoneTransform(HumanBodyBones.Head);
        AimIKHead.solver.bones[2].weight = 1f;
        AimIKHead.solver.target = SystemRoot.transform.Find("Base/AvatarAnchor/Head");

        var AimIKLeftEye = FollowerRoot.transform.Find("AimIK/LeftEye").GetComponent<RootMotion.FinalIK.AimIK>();
        AimIKLeftEye.solver.transform = followerAnimator.GetBoneTransform(HumanBodyBones.LeftEye);
        AimIKLeftEye.solver.bones[0].transform = followerAnimator.GetBoneTransform(HumanBodyBones.LeftEye);
        AimIKLeftEye.solver.target = SystemRoot.transform.Find("Base/AvatarAnchor/Head");

        var AimIKRightEye = FollowerRoot.transform.Find("AimIK/RightEye").GetComponent<RootMotion.FinalIK.AimIK>();
        AimIKRightEye.solver.transform = followerAnimator.GetBoneTransform(HumanBodyBones.RightEye);
        AimIKRightEye.solver.bones[0].transform = followerAnimator.GetBoneTransform(HumanBodyBones.RightEye);
        AimIKRightEye.solver.target = SystemRoot.transform.Find("Base/AvatarAnchor/Head");

        var Aimpoint2 = new GameObject("AimPoint2");
        Aimpoint2.transform.parent = followerAnimator.GetBoneTransform(HumanBodyBones.Hips);
        Aimpoint2.transform.localPosition = new Vector3(0, 0.5f, 1f);

        var AvatarAnchorHeadPos = SystemRoot.transform.Find("Base/AvatarAnchor/Head").GetComponent<PositionConstraint>();
        AvatarAnchorHeadPos.SetSource(0, new ConstraintSource { weight = 1, sourceTransform = Aimpoint2.transform });
        AvatarAnchorHeadPos.enabled = false;

        var AvatarAnchorHeadPare = SystemRoot.transform.Find("Base/AvatarAnchor/Head").GetComponent<ParentConstraint>();
        AvatarAnchorHeadPare.SetSource(0, new ConstraintSource { weight = 1, sourceTransform = mainAnimator.GetBoneTransform(HumanBodyBones.Head) });

        var EyelimitR = followerAnimator.GetBoneTransform(HumanBodyBones.RightEye).gameObject.GetComponent<RootMotion.FinalIK.RotationLimitAngle>();
        if(EyelimitR == null)
        {
            EyelimitR = followerAnimator.GetBoneTransform(HumanBodyBones.RightEye).gameObject.AddComponent<RootMotion.FinalIK.RotationLimitAngle>();
        }
        EyelimitR.limit = 6f;
        EyelimitR.twistLimit = 180f;
        EyelimitR.axis = new Vector3(0.45f, -0.12f, 1f);

        var EyelimitL = followerAnimator.GetBoneTransform(HumanBodyBones.LeftEye).gameObject.GetComponent<RootMotion.FinalIK.RotationLimitAngle>();
        if (EyelimitL == null)
        {
            EyelimitL = followerAnimator.GetBoneTransform(HumanBodyBones.LeftEye).gameObject.AddComponent<RootMotion.FinalIK.RotationLimitAngle>();
        }
        EyelimitL.limit = 6f;
        EyelimitL.twistLimit = 180f;
        EyelimitL.axis = new Vector3(-0.45f, -0.12f, 1f);



        AssetDatabase.DeleteAsset("Assets/nHaruka/FollowerSystem/Animation/MainAnimator/" + avatarDescriptor.name + "_Main_Copy.asset");
        AssetDatabase.CopyAsset("Assets/nHaruka/FollowerSystem/Animation/MainAnimator/Main.controller", "Assets/nHaruka/FollowerSystem/Animation/MainAnimator/"+ avatarDescriptor.name + "_Main_Copy.controller");

        var AddAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/nHaruka/FollowerSystem/Animation/MainAnimator/"+ avatarDescriptor.name + "_Main_Copy.controller");

        AnimatorController FxAnimator = null;

        try
        {
            var FxAnimatorLayer =
                    avatarDescriptor.baseAnimationLayers.First(item => item.type == VRCAvatarDescriptor.AnimLayerType.FX && item.animatorController != null);

            FxAnimator = (AnimatorController)FxAnimatorLayer.animatorController;
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("Error", "No FxAnimator Found", "OK");
            Debug.LogError(ex.Message);
            remove();
            return;
        }


        EditorUtility.SetDirty(FxAnimator);

        FxAnimator.parameters = FxAnimator.parameters.Union(AddAnimatorController.parameters).ToArray();
        foreach (var layer in AddAnimatorController.layers)
        {
            FxAnimator.AddLayer(layer);
            if(layer.name == "FS_MirrorDetection")
            {
                FxAnimator.AddLayer(convertedController.layers[0]);
            }
        }

        var convertedFXPath = convertedDataPath + avatarDescriptor.name + "/FXConverted.asset";
        if (File.Exists(convertedFXPath))
        {
            AssetDatabase.DeleteAsset(convertedFXPath);
        }
        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(origFX), convertedFXPath);
        var convertedFX = AssetDatabase.LoadAssetAtPath<AnimatorController>(convertedFXPath);

        EditorUtility.SetDirty(convertedFX);

        var paramList = new Dictionary<string, AnimatorControllerParameterType>();
        if (useFX)
        {
            for (int i = 0; i < convertedFX.layers.Length; i++)
            {
                if (inheritLayers[i] == true)
                {
                    foreach (var anytransition in convertedFX.layers[i].stateMachine.anyStateTransitions)
                    {
                        for (int k = 0; k < anytransition.conditions.Length; k++)
                        {
                            var paramType = convertedFX.parameters.FirstOrDefault(val => val.name == anytransition.conditions[k].parameter).type;
                            if (!paramList.ContainsKey(anytransition.conditions[k].parameter))
                            {
                                
                                paramList.Add(anytransition.conditions[k].parameter, paramType);
                            }
                        }
                    }
                    for(int p = 0; p < convertedFX.layers[i].stateMachine.states.Length; p++)
                    {
                        if (convertedFX.layers[i].stateMachine.states[p].state.cycleOffsetParameter != "")
                        {
                            var paramType = convertedFX.parameters.FirstOrDefault(val => val.name == convertedFX.layers[i].stateMachine.states[p].state.cycleOffsetParameter).type;
                            if (!paramList.ContainsKey(convertedFX.layers[i].stateMachine.states[p].state.cycleOffsetParameter))
                            {
                                paramList.Add(convertedFX.layers[i].stateMachine.states[p].state.cycleOffsetParameter, paramType);
                            }
                        }
                        if (convertedFX.layers[i].stateMachine.states[p].state.mirrorParameter != "")
                        {
                            var paramType = convertedFX.parameters.FirstOrDefault(val => val.name == convertedFX.layers[i].stateMachine.states[p].state.mirrorParameter).type;
                            if (!paramList.ContainsKey(convertedFX.layers[i].stateMachine.states[p].state.mirrorParameter))
                            {
                                paramList.Add(convertedFX.layers[i].stateMachine.states[p].state.mirrorParameter, paramType);
                            }
                        }
                        if (convertedFX.layers[i].stateMachine.states[p].state.speedParameter != "")
                        {
                            var paramType = convertedFX.parameters.FirstOrDefault(val => val.name == convertedFX.layers[i].stateMachine.states[p].state.speedParameter).type;
                            if (!paramList.ContainsKey(convertedFX.layers[i].stateMachine.states[p].state.speedParameter))
                            {
                                paramList.Add(convertedFX.layers[i].stateMachine.states[p].state.speedParameter, paramType);
                            }
                        }
                        if (convertedFX.layers[i].stateMachine.states[p].state.timeParameter != "")
                        {
                            
                            var paramType = convertedFX.parameters.FirstOrDefault(val => val.name == convertedFX.layers[i].stateMachine.states[p].state.timeParameter).type;
                            if (!paramList.ContainsKey(convertedFX.layers[i].stateMachine.states[p].state.timeParameter))
                            {
                                paramList.Add(convertedFX.layers[i].stateMachine.states[p].state.timeParameter, paramType);
                                //Debug.Log(state.state.timeParameter);
                            }
                        }

                        foreach (var transition in convertedFX.layers[i].stateMachine.states[p].state.transitions)
                        {
                            for (int k = 0; k < transition.conditions.Length; k++)
                            {
                                var paramType = convertedFX.parameters.FirstOrDefault(val => val.name == transition.conditions[k].parameter).type;
                                if (!paramList.ContainsKey(transition.conditions[k].parameter))
                                {
                                    paramList.Add(transition.conditions[k].parameter, paramType);
                                }
                            }
                        }
                    }
                    var convertedLayer = ConvertFX(convertedFX.layers[i]);
                    convertedLayer.name = "FS_" + convertedFX.layers[i].name;
                    if (!Array.Exists(FxAnimator.layers,(x => x.name == convertedLayer.name)))
                    {
                        FxAnimator.AddLayer(convertedLayer);
                    }
                    
                }
            }

            FxAnimator.parameters.ToList().ForEach(key => paramList.Remove(key.name));
            foreach (var param in paramList)
            {
                FxAnimator.AddParameter(param.Key, param.Value);
            }
            SerializableList<string> serializableList = new SerializableList<string> { list = paramList.Keys.ToList() };
            string json = JsonUtility.ToJson(serializableList);

            File.WriteAllText(convertedDataPath + avatarDescriptor.name + "/ParamList.json", json);

        }

        if (File.Exists(convertedDataPath + avatarDescriptor.name +  "/FollowerSystem_Params.asset"))
        {
            AssetDatabase.DeleteAsset(convertedDataPath + avatarDescriptor.name + "/FollowerSystem_Params.asset");
        }
        AssetDatabase.CopyAsset("Assets/nHaruka/FollowerSystem/FollowerSystem_Params.asset", convertedDataPath + avatarDescriptor.name + "/FollowerSystem_Params.asset");
        var AddExpParam = AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>(convertedDataPath + avatarDescriptor.name + "/FollowerSystem_Params.asset");

        avatarDescriptor.expressionParameters.parameters = avatarDescriptor.expressionParameters.parameters.Union(AddExpParam.parameters).ToArray();

        if (File.Exists(convertedDataPath + avatarDescriptor.name + "/FollowerSystem_Menu.asset"))
        {
            AssetDatabase.DeleteAsset(convertedDataPath + avatarDescriptor.name + "/FollowerSystem_Menu.asset");
        }
        AssetDatabase.CopyAsset("Assets/nHaruka/FollowerSystem/FollowerSystem_Menu.asset", convertedDataPath + avatarDescriptor.name + "/FollowerSystem_Menu.asset");
        var AddSubMenu = AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(convertedDataPath + avatarDescriptor.name + "/FollowerSystem_Menu.asset");


        if (useFX)
        {

            var list = avatarDescriptor.expressionParameters.parameters.ToList();
            for (int i = 0; i < origExParams.parameters.Length; i++)
            {
                if (paramList.ContainsKey(origExParams.parameters[i].name))
                {
                    list.Add(origExParams.parameters[i]);
                }
            }
            avatarDescriptor.expressionParameters.parameters = list.ToArray();


            var FollowerAvatarMenu = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control();
            FollowerAvatarMenu.name = "FollowerAvatarMenu";
            FollowerAvatarMenu.type = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu;
            FollowerAvatarMenu.subMenu = ConvertMenu(origExMenu);

            AddSubMenu.controls.Add(FollowerAvatarMenu);

        }

        if (avatarDescriptor.expressionsMenu.controls.Count != 8)
        {
            var newMenu = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control();
            newMenu.name = "FollowerSystem";
            newMenu.type = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu;
            newMenu.subMenu = AddSubMenu;

            avatarDescriptor.expressionsMenu.controls.Add(newMenu);

        }
        else
        {
            EditorUtility.DisplayDialog("Error", "ExpressionMenu is Max!", "OK");
            return;
        }




        foreach (var layer in FxAnimator.layers)
        {
            if (layer.name.StartsWith("FS_"))
            {
                foreach (var state in layer.stateMachine.states)
                {
                    if (WriteDefault == true)
                    {
                        state.state.writeDefaultValues = true;
                    }
                    else
                    {
                        state.state.writeDefaultValues = false;
                    }
                }
            }
        }
        

        DestroyImmediate(HumAvatar.GetComponent<Animator>());
        DestroyImmediate(HumAvatar.GetComponent<PipelineManager>());
        DestroyImmediate(HumAvatar.GetComponent<VRCAvatarDescriptor>());

        AssetDatabase.SaveAssets();

    }

    VRCExpressionsMenu ConvertMenu(VRCExpressionsMenu Menu)
    {
        if (File.Exists(convertedDataPath + avatarDescriptor.name + "/" + Menu.name + ".asset"))
        {
            AssetDatabase.DeleteAsset(convertedDataPath + avatarDescriptor.name + "/" + Menu.name + ".asset");
        }
        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(Menu), convertedDataPath + avatarDescriptor.name + "/"+ Menu.name +".asset");
        var newMenu = AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(convertedDataPath + avatarDescriptor.name + "/" + Menu.name + ".asset");
        EditorUtility.SetDirty(newMenu);
        var rmIndex = new List<int>();

        for (int i = 0; i < newMenu.controls.Count; i++)
        {
            if (newMenu.controls[i].type == VRCExpressionsMenu.Control.ControlType.SubMenu)
            {
                newMenu.controls[i].subMenu = ConvertMenu(newMenu.controls[i].subMenu);
                if (newMenu.controls[i].subMenu == null)
                {
                    Debug.Log(newMenu.controls[i].name);
                    rmIndex.Add(i);
                    //newMenu.controls.RemoveAt(i);
                }
            }
            else
            {
                if (!Array.Exists(avatarDescriptor.expressionParameters.parameters, (x => x.name == newMenu.controls[i].parameter.name)))
                {

                    if (newMenu.controls[i].subParameters == null ||newMenu.controls[i].subParameters.Length == 0)
                    {
                        Debug.Log(newMenu.controls[i].name);
                        rmIndex.Add(i);
                    }
                    else
                    {
                        for (int k = 0; k < newMenu.controls[i].subParameters.Length; k++)
                        {
                            if (!Array.Exists(avatarDescriptor.expressionParameters.parameters, (x => x.name == newMenu.controls[i].subParameters[k].name)))
                            {
                                Debug.Log(newMenu.controls[i].name);
                                rmIndex.Add(i);
                                //newMenu.controls.RemoveAt(i);                        }

                            }
                        }
                    }
                }

            }
        }
        for (int i = 0; i < rmIndex.Count; i++)
        {
            newMenu.controls.RemoveAt(rmIndex[rmIndex.Count - 1 - i]);
        }
        if (newMenu.controls.Count == 0)
        {
            return null;
        }
        return newMenu;
    }


    void remove()
    {
        try
        {
            DestroyImmediate(GameObject.Find("FollowerSystem").gameObject);
        }
        catch
        {
            Debug.LogWarning("FollowerSystem is Not Found in hierarchy");
        }
        try
        {
            avatarDescriptor.expressionsMenu.controls.RemoveAll(item => item.name == "FollowerSystem");
            avatarDescriptor.expressionParameters.parameters =
            avatarDescriptor.expressionParameters.parameters.Where(item => !item.name.Contains("FS_")).ToArray();
        }
        catch
        {
            Debug.LogWarning("FollowerSystem is Not Found in expressionsMenu or Parameters");
        }
        try
        {
            var FxAnimatorLayer =
                avatarDescriptor.baseAnimationLayers.First(item => item.type == VRCAvatarDescriptor.AnimLayerType.FX && item.animatorController != null);
            var FxAnimator = (AnimatorController)FxAnimatorLayer.animatorController;

            EditorUtility.SetDirty(FxAnimator);

            FxAnimator.layers = FxAnimator.layers.Where(item => !item.name.Contains("FS_")).ToArray();
            FxAnimator.parameters = FxAnimator.parameters.Where(item => !item.name.Contains("FS_")).ToArray();

            if (File.Exists(convertedDataPath + avatarDescriptor.name + "/ParamList.json"))
            {
                // ファイルから読み込み
                string json = File.ReadAllText(convertedDataPath + avatarDescriptor.name + "/ParamList.json");
                SerializableList<string> serializableList = JsonUtility.FromJson<SerializableList<string>>(json);
                for (int i = 0; i < serializableList.list.Count; i++)
                {
                    FxAnimator.parameters = FxAnimator.parameters.Where(item => !item.name.Contains(serializableList.list[i])).ToArray();
                    avatarDescriptor.expressionParameters.parameters = avatarDescriptor.expressionParameters.parameters.Where(item => !item.name.Contains(serializableList.list[i])).ToArray();
                }
            }
        }
        catch
        {
            Debug.LogWarning("Something wrong in FX layer");
        }

        AssetDatabase.SaveAssets();
    }

    AnimationClip ConvertHumanoidToGeneric(AnimationClip animationClip, GameObject HumanoidAvatar)
    {
        AnimationClip result = new AnimationClip();
        result.name = animationClip.name;
        

        var HumAnimator = HumanoidAvatar.GetComponent<Animator>();
        if (HumanoidAvatar ==null )
        {
            Debug.LogError("Animator is not found");
            return null;
        }

        if(!HumAnimator.avatar.isHuman)
        { 
            Debug.LogError("Avatar is not Vaild"); 
            return null;
        }

        var FrameCount = animationClip.length * animationClip.frameRate;
        var CurveRotX = new AnimationCurve[HumanTrait.BoneCount];
        var CurveRotY = new AnimationCurve[HumanTrait.BoneCount];
        var CurveRotZ = new AnimationCurve[HumanTrait.BoneCount];
        var CurveRotW = new AnimationCurve[HumanTrait.BoneCount];
        var CurveScaX = new AnimationCurve[HumanTrait.BoneCount];
        var CurveScaY = new AnimationCurve[HumanTrait.BoneCount];
        var CurveScaZ = new AnimationCurve[HumanTrait.BoneCount];


        var RootCurveRotX = new AnimationCurve();
        var RootCurveRotY = new AnimationCurve();
        var RootCurveRotZ = new AnimationCurve();
        var RootCurveRotW = new AnimationCurve();
        var RootCurveScaX = new AnimationCurve();
        var RootCurveScaY = new AnimationCurve();
        var RootCurveScaZ = new AnimationCurve();
        var RootCurvePosX = new AnimationCurve();
        var RootCurvePosY = new AnimationCurve();
        var RootCurvePosZ = new AnimationCurve();
        for (int i = 0; i <= FrameCount; i++)
        { 
            animationClip.SampleAnimation(HumanoidAvatar, i * (1 / animationClip.frameRate));
            //Debug.Log(i * (1 / animationClip.frameRate));
            //Debug.Log("a:"+HumAnimator.GetBoneTransform(HumanBodyBones.Spine).localRotation.y);
            var BoneTransforms = new Transform[HumanTrait.BoneCount];
            for (int k = 0; k < HumanTrait.BoneCount; k++)
            {
                BoneTransforms[k] = HumAnimator.GetBoneTransform(((HumanBodyBones[])Enum.GetValues(typeof(HumanBodyBones)))[k]);

                
                if (i == 0 )
                {
                    //BoneMapping.Add(((HumanBodyBones[])Enum.GetValues(typeof(HumanBodyBones)))[k], BoneTransforms[k]);

                    CurveRotX[k] = new AnimationCurve();
                    CurveRotY[k] = new AnimationCurve();
                    CurveRotZ[k] = new AnimationCurve();
                    CurveRotW[k] = new AnimationCurve();
                    CurveScaX[k] = new AnimationCurve();
                    CurveScaY[k] = new AnimationCurve();
                    CurveScaZ[k] = new AnimationCurve();
                }
                //Debug.Log(((HumanBodyBones[])Enum.GetValues(typeof(HumanBodyBones)))[k]);
                

                if (BoneTransforms[k] != null)
                {
                    if (i == 0)
                    {
                        CurveRotX[k].AddKey(new Keyframe(i / animationClip.frameRate, BoneTransforms[k].localRotation.x));
                        CurveRotY[k].AddKey(new Keyframe(i / animationClip.frameRate, BoneTransforms[k].localRotation.y));
                        CurveRotZ[k].AddKey(new Keyframe(i / animationClip.frameRate, BoneTransforms[k].localRotation.z));
                        CurveRotW[k].AddKey(new Keyframe(i / animationClip.frameRate, BoneTransforms[k].localRotation.w));
                        CurveScaX[k].AddKey(new Keyframe(i / animationClip.frameRate, BoneTransforms[k].localScale.x));
                        CurveScaY[k].AddKey(new Keyframe(i / animationClip.frameRate, BoneTransforms[k].localScale.y));
                        CurveScaZ[k].AddKey(new Keyframe(i / animationClip.frameRate, BoneTransforms[k].localScale.z));
                    }
                    else
                    {
                        if (CurveRotX[k].keys.Last().value != BoneTransforms[k].localRotation.x)
                        {
                            CurveRotX[k].AddKey(new Keyframe(i / animationClip.frameRate, BoneTransforms[k].localRotation.x));
                        }
                        if (CurveRotY[k].keys.Last().value != BoneTransforms[k].localRotation.y)
                        {
                            CurveRotY[k].AddKey(new Keyframe(i / animationClip.frameRate, BoneTransforms[k].localRotation.y));
                        }
                        if (CurveRotZ[k].keys.Last().value != BoneTransforms[k].localRotation.z)
                        {
                            CurveRotZ[k].AddKey(new Keyframe(i / animationClip.frameRate, BoneTransforms[k].localRotation.z));
                        }
                        if (CurveRotW[k].keys.Last().value != BoneTransforms[k].localRotation.w)
                        {
                            CurveRotW[k].AddKey(new Keyframe(i / animationClip.frameRate, BoneTransforms[k].localRotation.w));
                        }
                        if (CurveScaX[k].keys.Last().value != BoneTransforms[k].localScale.x)
                        {
                            CurveScaX[k].AddKey(new Keyframe(i / animationClip.frameRate, BoneTransforms[k].localScale.x));
                        }
                        if (CurveScaY[k].keys.Last().value != BoneTransforms[k].localScale.y)
                        {
                            CurveScaY[k].AddKey(new Keyframe(i / animationClip.frameRate, BoneTransforms[k].localScale.y));
                        }
                        if (CurveScaZ[k].keys.Last().value != BoneTransforms[k].localScale.z)
                        {
                            CurveScaZ[k].AddKey(new Keyframe(i / animationClip.frameRate, BoneTransforms[k].localScale.z));
                        }
                    }
                    result.SetCurve(GetPath(avatarDescriptor.transform, BoneTransforms[k]), typeof(Transform), "localRotation.x", CurveRotX[k]);
                    result.SetCurve(GetPath(avatarDescriptor.transform, BoneTransforms[k]), typeof(Transform), "localRotation.y", CurveRotY[k]);
                    result.SetCurve(GetPath(avatarDescriptor.transform, BoneTransforms[k]), typeof(Transform), "localRotation.z", CurveRotZ[k]);
                    result.SetCurve(GetPath(avatarDescriptor.transform, BoneTransforms[k]), typeof(Transform), "localRotation.w", CurveRotW[k]);
                    result.SetCurve(GetPath(avatarDescriptor.transform, BoneTransforms[k]), typeof(Transform), "localScale.x", CurveScaX[k]);
                    result.SetCurve(GetPath(avatarDescriptor.transform, BoneTransforms[k]), typeof(Transform), "localScale.y", CurveScaY[k]);
                    result.SetCurve(GetPath(avatarDescriptor.transform, BoneTransforms[k]), typeof(Transform), "localScale.z", CurveScaZ[k]);

                }
            }

            if (i == 0)
            {
                RootCurveRotX.AddKey(new Keyframe(i / animationClip.frameRate, HumanoidAvatar.transform.localRotation.x));
                RootCurveRotY.AddKey(new Keyframe(i / animationClip.frameRate, HumanoidAvatar.transform.localRotation.y));
                RootCurveRotZ.AddKey(new Keyframe(i / animationClip.frameRate, HumanoidAvatar.transform.localRotation.z));
                RootCurveRotW.AddKey(new Keyframe(i / animationClip.frameRate, HumanoidAvatar.transform.localRotation.w));
                RootCurveScaX.AddKey(new Keyframe(i / animationClip.frameRate, HumanoidAvatar.transform.localScale.x));
                RootCurveScaY.AddKey(new Keyframe(i / animationClip.frameRate, HumanoidAvatar.transform.localScale.y));
                RootCurveScaZ.AddKey(new Keyframe(i / animationClip.frameRate, HumanoidAvatar.transform.localScale.z));
                RootCurvePosX.AddKey(new Keyframe(i / animationClip.frameRate, HumanoidAvatar.transform.localPosition.x));
                RootCurvePosY.AddKey(new Keyframe(i / animationClip.frameRate, HumanoidAvatar.transform.localPosition.y));
                RootCurvePosZ.AddKey(new Keyframe(i / animationClip.frameRate, HumanoidAvatar.transform.localPosition.z));
            }
            else
            {
                if (RootCurveRotX.keys.Last().value != HumanoidAvatar.transform.localRotation.x)
                {
                    RootCurveRotX.AddKey(new Keyframe(i / animationClip.frameRate, HumanoidAvatar.transform.localRotation.x));
                }
                if (RootCurveRotY.keys.Last().value != HumanoidAvatar.transform.localRotation.y)
                {
                    RootCurveRotY.AddKey(new Keyframe(i / animationClip.frameRate, HumanoidAvatar.transform.localRotation.y));
                }
                if (RootCurveRotZ.keys.Last().value != HumanoidAvatar.transform.localRotation.z)
                {
                    RootCurveRotZ.AddKey(new Keyframe(i / animationClip.frameRate, HumanoidAvatar.transform.localRotation.z));
                }
                if (RootCurveRotW.keys.Last().value != HumanoidAvatar.transform.localRotation.w)
                {
                    RootCurveRotW.AddKey(new Keyframe(i / animationClip.frameRate, HumanoidAvatar.transform.localRotation.w));
                }
                if (RootCurveScaX.keys.Last().value != HumanoidAvatar.transform.localScale.x)
                {
                    RootCurveScaX.AddKey(new Keyframe(i / animationClip.frameRate, HumanoidAvatar.transform.localScale.x));
                }
                if (RootCurveScaY.keys.Last().value != HumanoidAvatar.transform.localScale.y)
                {
                    RootCurveScaY.AddKey(new Keyframe(i / animationClip.frameRate, HumanoidAvatar.transform.localScale.y));
                }
                if (RootCurveScaZ.keys.Last().value != HumanoidAvatar.transform.localScale.z)
                {
                    RootCurveScaZ.AddKey(new Keyframe(i / animationClip.frameRate, HumanoidAvatar.transform.localScale.z));
                }
            }
            result.SetCurve(avatarPath, typeof(Transform), "localPosition.x", RootCurvePosX);
            result.SetCurve(avatarPath, typeof(Transform), "localPosition.y", RootCurvePosY);
            result.SetCurve(avatarPath, typeof(Transform), "localPosition.z", RootCurvePosZ);
            result.SetCurve(avatarPath, typeof(Transform), "localRotation.x", RootCurveRotX);
            result.SetCurve(avatarPath, typeof(Transform), "localRotation.y", RootCurveRotY);
            result.SetCurve(avatarPath, typeof(Transform), "localRotation.z", RootCurveRotZ);
            result.SetCurve(avatarPath, typeof(Transform), "localRotation.w", RootCurveRotW);
            result.SetCurve(avatarPath, typeof(Transform), "localScale.x", RootCurveScaX);
            result.SetCurve(avatarPath, typeof(Transform), "localScale.y", RootCurveScaY);
            result.SetCurve(avatarPath, typeof(Transform), "localScale.z", RootCurveScaZ);


        }

        var Setting = AnimationUtility.GetAnimationClipSettings(animationClip);
        Setting.loopTime = true;
        Setting.loopBlend = true;
        AnimationUtility.SetAnimationClipSettings(result, Setting);

        return result;
    }

    AnimatorControllerLayer ConvertFX(AnimatorControllerLayer origLayer )
    {
        AnimatorControllerLayer convertedLayer = origLayer;
        convertedLayer.name = origLayer.name;
        Debug.Log(convertedLayer.name);

        foreach(var state in convertedLayer.stateMachine.states)
        {
            if (state.state.motion != null && state.state.motion.GetType() == typeof(BlendTree))
            {
                state.state.motion = ConvertBlendtree((BlendTree)state.state.motion);
            }
            else if (state.state.motion != null)
            {
                AnimationClip clip = state.state.motion as AnimationClip;
                var bindings = AnimationUtility.GetCurveBindings(clip);
                var newbindings = AnimationUtility.GetCurveBindings(clip);
                var curves = new AnimationCurve[bindings.Length];
                for (int i = 0; i < bindings.Length; i++)
                {
                    newbindings[i].path = GetPath(avatarDescriptor.transform, HumAvatar.transform) + "/" + bindings[i].path;
                    curves[i] = AnimationUtility.GetEditorCurve(clip, bindings[i]);
                }
                if (!Directory.Exists(convertedDataPath + avatarDescriptor.name))
                {
                    Directory.CreateDirectory(convertedDataPath + avatarDescriptor.name);
                }
                var convertedClip = new AnimationClip();
                AnimationUtility.SetEditorCurves(convertedClip, newbindings, curves);
                AssetDatabase.DeleteAsset(convertedDataPath + avatarDescriptor.name + "/" + clip.name + ".anim");
                AssetDatabase.CreateAsset(convertedClip, convertedDataPath + avatarDescriptor.name + "/" + clip.name + ".anim");

                state.state.motion = convertedClip;
            }
        }

        return convertedLayer;
    }

    BlendTree ConvertBlendtree(BlendTree orig)
    {
        BlendTree blendTree = orig;
        var children = blendTree.children;
        for (int n = 0; n < children.Length; n++)
        {
            if (children[n].motion != null && children[n].motion.GetType() == typeof(BlendTree))
            {
                children[n].motion = ConvertBlendtree((BlendTree)children[n].motion);
            }
            else if (children[n].motion != null)
            {
                AnimationClip clip = (AnimationClip)children[n].motion;
                var clipname = clip.name;
                var bindings = AnimationUtility.GetCurveBindings(clip);
                var newbindings = AnimationUtility.GetCurveBindings(clip);
                var curves = new AnimationCurve[bindings.Length];
                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!newbindings[i].path.StartsWith("FollowerSystem"))
                    {
                        newbindings[i].path = GetPath(avatarDescriptor.transform, HumAvatar.transform) + "/" + bindings[i].path;
                        curves[i] = AnimationUtility.GetEditorCurve(clip, bindings[i]);
                    }
                }
                if (!Directory.Exists(convertedDataPath + avatarDescriptor.name))
                {
                    Directory.CreateDirectory(convertedDataPath + avatarDescriptor.name);
                }
                var convertedClip = new AnimationClip();
                AnimationUtility.SetEditorCurves(convertedClip, newbindings, curves);
                AssetDatabase.DeleteAsset(convertedDataPath + avatarDescriptor.name + "/" + clipname + ".anim");
                AssetDatabase.CreateAsset(convertedClip, convertedDataPath + avatarDescriptor.name + "/" + clipname + ".anim");

                children[n].motion = convertedClip;
            }

        }
        blendTree.children = children;

        return blendTree;
    }


    private static string GetPath(Transform root, Transform self)
    {

        string path = self.gameObject.name;
        Transform parent = self.parent;

        while (root != parent)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }

    [System.Serializable]
    public class SerializableList<T>
    {
        public List<T> list;
    }
}