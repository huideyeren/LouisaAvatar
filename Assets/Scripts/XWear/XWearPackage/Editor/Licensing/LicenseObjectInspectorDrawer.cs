using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using XWear.IO.XResource.Licensing;
using Messages = XWear.XWearPackage.Editor.Common.MessagesContainer.LicenseMessages;

namespace XWear.XWearPackage.Editor.Licensing
{
    [CustomEditor(typeof(XResourceLicenseObject), true)]
    public class LicenseObjectInspectorDrawer : UnityEditor.Editor
    {
        private XResourceLicenseObject _target;

        private void OnEnable()
        {
            _target = (XResourceLicenseObject)target;
        }

        public override void OnInspectorGUI()
        {
            var license = _target.license;
            DrawAuthor(license);

            var licenseType = _target.license.licenseType;
            switch (licenseType)
            {
                case XResourceLicense.LicenseType.Text:
                    DrawTextLicense(license);
                    break;
                case XResourceLicense.LicenseType.File:
                    DrawFileLicense(license);
                    break;
                case XResourceLicense.LicenseType.Url:
                    DrawUrlLicense(license);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawAuthor(XResourceLicense license)
        {
            EditorGUILayout.LabelField(Messages.LabelAuthor, license.author);
        }

        [SerializeField]
        private Vector2 textLicenseScroll;

        private void DrawTextLicense(XResourceLicense license)
        {
            textLicenseScroll = EditorGUILayout.BeginScrollView(
                textLicenseScroll,
                GUILayout.MinHeight(800)
            );
            EditorGUILayout.TextArea(license.licenseText, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private void DrawUrlLicense(XResourceLicense license)
        {
            EditorGUILayout.LabelField(Messages.LabelLicenseUrl);
            if (EditorGUILayout.LinkButton(license.licenseText))
            {
                Application.OpenURL(license.licenseText);
            }
        }

        private void DrawFileLicense(XResourceLicense license)
        {
            EditorGUILayout.LabelField(Messages.LabelLicenseFile);
            EditorGUILayout.LabelField(license.licenseFileName);
            if (GUILayout.Button(Messages.ButtonOpenLicenseFile))
            {
                if (!string.IsNullOrEmpty(license.licenseFileName))
                {
                    var cachePath = Path.Combine(
                        Application.temporaryCachePath,
                        license.licenseFileName
                    );
                    File.WriteAllBytes(cachePath, license.licenseFileBinary);
                    Application.OpenURL(cachePath);
                }
            }
            EditorGUILayout.LinkButton(license.licenseText);
        }
    }
}
