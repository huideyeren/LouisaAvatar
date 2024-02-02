using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using XWear.IO.XResource.Licensing;
using Messages = XWear.XWearPackage.Editor.Common.MessagesContainer.LicenseMessages;

namespace XWear.XWearPackage.Editor
{
    public class LicenseObjectGenerator : EditorWindow
    {
        [SerializeField]
        private XResourceLicense.LicenseType licenseType;

        [SerializeField]
        private string author;

        [SerializeField]
        private string textLicense;

        [SerializeField]
        private string licenseUrl;

        [SerializeField]
        private string licenseFilePath;

        [SerializeField]
        private DefaultAsset saveDirAsset;

        [SerializeField]
        private string saveDir;

        private const string DefaultSaveDir = "Assets";

        /*
        [MenuItem("VRoid/License Generator")]
        */
        private static void Init()
        {
            var w = CreateWindow<LicenseObjectGenerator>();
            w.Show();
            w.saveDir = DefaultSaveDir;
            w.saveDirAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DefaultSaveDir);
        }

        private void OnGUI()
        {
            var newSaveDirAsset = (DefaultAsset)
                EditorGUILayout.ObjectField(
                    Messages.LabelSaveDir,
                    saveDirAsset,
                    typeof(DefaultAsset),
                    true
                );

            if (!ReferenceEquals(saveDirAsset, newSaveDirAsset))
            {
                saveDirAsset = newSaveDirAsset;
                saveDir = AssetDatabase.GetAssetPath(saveDirAsset);
            }

            DrawAuthor();
            DrawLicenseType();

            switch (licenseType)
            {
                case XResourceLicense.LicenseType.Text:
                    DrawTextLicense();
                    break;
                case XResourceLicense.LicenseType.File:
                    DrawFileLicense();
                    break;
                case XResourceLicense.LicenseType.Url:
                    DrawUrlLicense();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (GUILayout.Button("Generate"))
            {
                OnClickGenerate();
            }
        }

        private void DrawAuthor()
        {
            author = EditorGUILayout.TextField(Messages.LabelAuthor, author);
        }

        private void DrawLicenseType()
        {
            licenseType = (XResourceLicense.LicenseType)
                EditorGUILayout.EnumPopup(Messages.LabelLicenseType, licenseType);
        }

        [SerializeField]
        private Vector2 textLicenseScroll;

        private void DrawTextLicense()
        {
            EditorGUILayout.LabelField(Messages.LabelLicenseText);

            textLicenseScroll = EditorGUILayout.BeginScrollView(
                textLicenseScroll,
                GUILayout.MinHeight(240)
            );
            textLicense = EditorGUILayout.TextArea(textLicense, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private void DrawUrlLicense()
        {
            EditorGUILayout.LabelField(Messages.LabelLicenseUrl);
            licenseUrl = EditorGUILayout.TextField(licenseUrl);
        }

        private void DrawFileLicense()
        {
            EditorGUILayout.LabelField(Messages.LabelLicenseFile);
            if (!string.IsNullOrEmpty(licenseFilePath))
            {
                if (EditorGUILayout.LinkButton(licenseFilePath))
                {
                    Application.OpenURL(licenseFilePath);
                }
            }

            if (GUILayout.Button(Messages.ButtonSelectLicenseFile))
            {
                var newFilePath = EditorUtility.OpenFilePanel(
                    Messages.ButtonSelectLicenseFile,
                    "",
                    "pdf"
                );
                if (!string.IsNullOrEmpty(newFilePath))
                {
                    licenseFilePath = newFilePath;
                }
            }

            EditorGUILayout.HelpBox(Messages.InfoLicenseFileExtension, MessageType.Info);
        }

        private void OnClickGenerate()
        {
            var newObject = CreateInstance<XResourceLicenseObject>();
            var newLicense = GenerateLicense();
            newObject.license = newLicense;
            var assetPath = Path.Combine(saveDir, "License.asset");
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            AssetDatabase.CreateAsset(newObject, assetPath);

            var resultAsset = AssetDatabase.LoadAssetAtPath<XResourceLicenseObject>(assetPath);
            EditorUtility.DisplayDialog(
                nameof(LicenseObjectGenerator),
                Messages.DialogMessageCompleteGenerate,
                "OK"
            );
            EditorGUIUtility.PingObject(resultAsset);
        }

        private XResourceLicense GenerateLicense()
        {
            var newLicense = new XResourceLicense();
            newLicense.author = author;
            newLicense.licenseType = licenseType;

            switch (licenseType)
            {
                case XResourceLicense.LicenseType.Text:
                    newLicense.licenseText = textLicense;
                    break;
                case XResourceLicense.LicenseType.File:
                    if (string.IsNullOrEmpty(licenseFilePath))
                    {
                        throw new Exception("License File Path is null or empty");
                    }

                    newLicense.licenseFileName = Path.GetFileName(licenseFilePath);
                    newLicense.licenseFileBinary = File.ReadAllBytes(licenseFilePath);
                    break;
                case XResourceLicense.LicenseType.Url:
                    newLicense.licenseText = licenseUrl;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return newLicense;
        }
    }
}
