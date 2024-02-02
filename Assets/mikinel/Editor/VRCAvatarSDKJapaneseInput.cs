using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.Elements;
using Task = System.Threading.Tasks.Task;

//2023-12-05
//VRChatSDK - Base 3.4.1
//VRChatSDK - Avatar 3.4.1

/// <summary>
/// VRCAvatarSDK to allow input of avatar names in Japanese
/// </summary>
public class VRCAvatarSDKJapaneseInput : EditorWindow
{
    private static VRCSdkControlPanel window;
    
    [MenuItem("VRChat SDK/Show Control Panel", false, 600)]
    public static void CustomShowControlPanel()
    {
        var sdkControlPanel = typeof(VRCSdkControlPanel);
        
        var showControlPanelMethod = sdkControlPanel.GetMethod("ShowControlPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        showControlPanelMethod.Invoke(null, null);
        
        window = (VRCSdkControlPanel)GetWindow(typeof(VRCSdkControlPanel));
        
        var tabButtons = typeof(VRCSdkControlPanel).GetField("_tabButtons", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var buttonArray = (Button[])tabButtons.GetValue(window);

        var builderButton = buttonArray[1];
        
        builderButton.clickable.clicked += OnInvokeRenderTabs;
        
        if(VRCSettings.ActiveWindowPanel == 1)
            OnInvokeRenderTabs();
    }

    private static void OnInvokeRenderTabs()
    {
        var builderPanel = typeof(VRCSdkControlPanel).GetField("_builderPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var builderVisualElement = (VisualElement)builderPanel.GetValue(window);

        builderVisualElement.schedule.Execute(async x =>
        {
            await Task.Delay(10);
            
            builderPanel = typeof(VRCSdkControlPanel).GetField("_builderPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
           
            builderVisualElement = (VisualElement)builderPanel.GetValue(window);
            
            var infoBlock = builderVisualElement.Q("content-info-block");

            var nameVrcTextField = infoBlock.Q<VRCTextField>("content-name");
            var descriptionVrcTextField = infoBlock.Q<VRCTextField>("content-description");
            var imguiContainer = new IMGUIContainer(() =>
            {
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField(" Name", GUILayout.Width(135));
                
                var newName = EditorGUILayout.TextField(nameVrcTextField.text, GUILayout.Width(337));
                if (newName != nameVrcTextField.text)
                {
                    nameVrcTextField.value = newName;
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField(" Description", GUILayout.Width(135));
                
                var newDescription = EditorGUILayout.TextField(descriptionVrcTextField.text, GUILayout.Width(337));
                if (newDescription != descriptionVrcTextField.text)
                {
                    descriptionVrcTextField.value = newDescription;
                }
                
                EditorGUILayout.EndHorizontal();
            });

            var vrcTextFieldParent = nameVrcTextField.parent;
            vrcTextFieldParent.Insert(0, imguiContainer);
            nameVrcTextField.style.display = DisplayStyle.None;
            descriptionVrcTextField.style.display = DisplayStyle.None;
        });
    }
}
