using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using UnityEditor;
using UnityEngine;
using XWear.IO;
using XWear.IO.Common;
using XWear.IO.XResource.Licensing;
using XWear.XWearPackage.Editor.Exporter;
using XWear.XWearPackage.Editor.Util;
using XWear.XWearPackage.Editor.Util.Export;
using XWear.XWearPackage.Editor.Validator.Error;
using Messages = XWear.XWearPackage.Editor.Common.MessagesContainer.ExporterMessages;
using CommonMessages = XWear.XWearPackage.Editor.Common.MessagesContainer.CommonMessage;
using Object = UnityEngine.Object;

namespace XWear.XWearPackage.Editor
{
    [Serializable]
    public class PackageExporterContent : EditorContent
    {
        [SerializeField]
        private GameObject exportTarget;

        [SerializeField]
        private List<GameObject> exportChildren = new();

        [SerializeField]
        private ExportContext.ExportType exportType;

        [SerializeField]
        private XResourceLicenseObject licenseObject;

        private ExportContext _exportContext;

        [SerializeField]
        private CustomEditorUtil.UpdatableEnumPopupDrawer exportTypeDrawer =
            new(EnumLabelCreator.ExportTypeEnumLabels);

        [SerializeReference]
        private AvatarExporterContent avatarExporterContent;

        [SerializeField]
        private WearExporterContent wearExporterContent;

        [SerializeField]
        private AccessoryExporterContent accessoryExporterContent;

        [SerializeReference]
        private ExporterContent currentExporterContent;

        [SerializeField]
        private Vector2 mainContentsScrollPosition;

        public PackageExporterContent(XWearPackageEditorWindow root)
            : base(root)
        {
            avatarExporterContent = new AvatarExporterContent();
            wearExporterContent = new WearExporterContent();
            accessoryExporterContent = new AccessoryExporterContent();
            currentExporterContent = avatarExporterContent;
        }

        private bool IsValidExport()
        {
            var validateResults = currentExporterContent.currentValidateResults;
            return validateResults.Count != 0
                && validateResults.All(x => x is not ValidateResultError);
        }

        public override void OnSelect()
        {
            base.OnSelect();
            RefreshContext();
        }

        public override void OnHierarchyChange()
        {
            base.OnHierarchyChange();
            RefreshContext();
        }

        public override void OnFocus()
        {
            base.OnFocus();
            RefreshContext();
        }

        public override void OnUndoRedo()
        {
            base.OnUndoRedo();
            RefreshContext();
        }

        public override void OnForceUpdate()
        {
            base.OnForceUpdate();
            RefreshContext();
        }

        private void RefreshContext()
        {
            _exportContext = new ExportContext()
            {
                exportType = exportType,
                exportRoot = exportTarget,
                exportChildren = exportChildren,
                licenseObject = licenseObject
            };

            RefreshExporterContent();
        }

        private void RefreshExporterContent()
        {
            switch (exportType)
            {
                case ExportContext.ExportType.Avatar:
                    currentExporterContent = avatarExporterContent;
                    break;
                case ExportContext.ExportType.Wear:
                    currentExporterContent = wearExporterContent;
                    break;
                case ExportContext.ExportType.Accessory:
                    currentExporterContent = accessoryExporterContent;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            currentExporterContent.UpdateCurrentRootGameObject(_exportContext);
        }

        public override void DrawGui()
        {
            DrawExportTargetField();
            // DrawLicenseFile();
            DrawRunExportButton();
            using (
                var scroll = new EditorGUILayout.ScrollViewScope(
                    mainContentsScrollPosition,
                    GUI.skin.box
                )
            )
            {
                DrawValidateResults();
                currentExporterContent.DrawGui(_exportContext, RefreshContext);
                mainContentsScrollPosition = scroll.scrollPosition;
            }
        }

        private void DrawExportTargetField()
        {
            exportType = exportTypeDrawer.DrawWithConvertEnum<ExportContext.ExportType>(
                Messages.LabelExportMode,
                onUpdate: newValue =>
                {
                    exportType = newValue;
                    RefreshContext();
                }
            );

            if (exportType == ExportContext.ExportType.Accessory)
            {
                return;
            }

            exportTarget = (GameObject)
                CustomEditorUtil.DrawUpdatableObjectField(
                    Messages.LabelExportTarget,
                    exportTarget,
                    typeof(GameObject),
                    true,
                    onUpdate: newValue =>
                    {
                        exportTarget = (GameObject)newValue;
                        RefreshContext();
                    }
                );
        }

        private void DrawRunExportButton()
        {
            EditorGUI.BeginDisabledGroup(!IsValidExport());

            if (GUILayout.Button(Messages.ButtonExport))
            {
                string ext;
                switch (_exportContext.exportType)
                {
                    case ExportContext.ExportType.Avatar:
                        ext = FileExtensions.XWearAvatar;
                        break;
                    case ExportContext.ExportType.Wear:
                    case ExportContext.ExportType.Accessory:
                        ext = FileExtensions.XWearDress;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var savePath = EditorUtility.SaveFilePanel(
                    Messages.SaveFileDialog,
                    "",
                    Messages.DefaultExportFileName,
                    ext
                );
                if (string.IsNullOrEmpty(savePath))
                {
                    return;
                }

                Export(savePath);

                EditorUtility.DisplayDialog(
                    Messages.DialogTitle,
                    Messages.DialogCompleteMessage,
                    CommonMessages.Ok
                );
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DrawValidateResults()
        {
            if (currentExporterContent == null)
            {
                return;
            }

            foreach (var validateResult in currentExporterContent.currentValidateResults)
            {
                ValidateResultInfoDrawer.DrawValidateInfo(validateResult);
            }
        }

        private void DrawLicenseFile()
        {
            licenseObject = (XResourceLicenseObject)
                CustomEditorUtil.DrawUpdatableObjectField(
                    Messages.LabelLicense,
                    licenseObject,
                    typeof(XResourceLicenseObject),
                    false,
                    onUpdate: _ =>
                    {
                        RefreshContext();
                    }
                );
        }

        private void Export(string savePath)
        {
            var copied = new List<GameObject>();
            try
            {
                XItemExportUtil.Export(savePath, _exportContext, exportTarget, out copied);
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog(
                    Messages.DialogTitle,
                    Messages.DialogFailedMessage,
                    "OK"
                );
                ExceptionDispatchInfo.Capture(e).Throw();
            }
            finally
            {
                foreach (var copiedGameObject in copied)
                {
                    Object.DestroyImmediate(copiedGameObject);
                }
#if UNITY_EDITOR
                IO.Editor.EditorAssetUtil.Cleanup();
#endif
            }
        }
    }
}
