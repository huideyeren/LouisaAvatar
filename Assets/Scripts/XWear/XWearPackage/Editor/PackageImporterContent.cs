using System;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using XWear.IO.Common;
using XWear.XWearPackage.Editor.Util;
using XWear.XWearPackage.Editor.Util.Import;
using CommonMessages = XWear.XWearPackage.Editor.Common.MessagesContainer.CommonMessage;
using FileUtil = XWear.XWearPackage.Editor.Util.FileUtil;
using Messages = XWear.XWearPackage.Editor.Common.MessagesContainer.ImporterMessages;

namespace XWear.XWearPackage.Editor
{
    [Serializable]
    internal class PackageImporterContent : EditorContent
    {
        [Serializable]
        internal class ImportFolderCaches
        {
            private static string CacheSavePath =>
                Path.Combine(
                    FileUtil.GetValidXWearPackagePreferenceFolder(),
                    "ImportFolderCaches.json"
                );

            public string[] paths = { Application.dataPath };

            public string[] PopupPaths =>
                paths
                    .Select(
                        x => UnityEditor.FileUtil.GetProjectRelativePath(x).Replace('/', '\u2215')
                    )
                    .ToArray();

            private ImportFolderCaches() { }

            private static string DefaultCachesJson => JsonUtility.ToJson(new ImportFolderCaches());

            public static ImportFolderCaches LoadExportFolderCaches()
            {
                ImportFolderCaches result;
                if (File.Exists(CacheSavePath))
                {
                    var json = File.ReadAllText(CacheSavePath);
                    result = JsonUtility.FromJson<ImportFolderCaches>(json);
                }
                else
                {
                    result = new ImportFolderCaches();
                    File.WriteAllText(CacheSavePath, DefaultCachesJson);
                }

                var resultPaths = result.paths.ToList();
                foreach (var p in result.paths)
                {
                    if (!Directory.Exists(p))
                    {
                        resultPaths.Remove(p);
                    }
                }

                result.paths = resultPaths.ToArray();
                return result;
            }

            public void AddNewPath(string newPath)
            {
                if (paths.Contains(newPath))
                {
                    var index = paths.ToList().IndexOf(newPath);
                    SortCaches(index);
                    return;
                }

                var newPaths = paths.ToList();
                newPaths.Insert(0, newPath);
                paths = newPaths.ToArray();

                SaveCaches();
            }

            /// <summary>
            /// selectedPathIndexの要素が先頭になるようにpathsをソートしてEditorPrefsに書き直す
            /// </summary>
            /// <param name="selectedPathIndex"></param>
            public void SortCaches(int selectedPathIndex)
            {
                if (selectedPathIndex == 0)
                {
                    return;
                }

                var newPaths = paths.ToList();
                var selected = newPaths[selectedPathIndex];
                newPaths.Remove(selected);
                newPaths.Insert(0, selected);
                paths = newPaths.ToArray();
                SaveCaches();
            }

            private void SaveCaches()
            {
                File.WriteAllText(CacheSavePath, JsonUtility.ToJson(this));
            }
        }

        [SerializeField]
        private bool applySaveMaterial = true;

        [FormerlySerializedAs("exportFolderCaches")]
        public ImportFolderCaches importFolderCaches;

        [SerializeField]
        private int currentSelectedFolderCacheIndex;

        public PackageImporterContent(XWearPackageEditorWindow root)
            : base(root)
        {
            importFolderCaches = ImportFolderCaches.LoadExportFolderCaches();
        }

        public override async void DrawGui()
        {
            applySaveMaterial = EditorGUILayout.Toggle(
                Messages.LabelApplySavedMaterial,
                applySaveMaterial
            );

            DrawImportFolder();

            if (GUILayout.Button(Messages.LabelImportButton))
            {
                try
                {
                    var loadPath = ShowLoadFileSelectDialog();
                    if (string.IsNullOrEmpty(loadPath))
                    {
                        return;
                    }

                    var saveFolder = importFolderCaches.paths[currentSelectedFolderCacheIndex];
                    if (!Directory.Exists(saveFolder))
                    {
                        Directory.CreateDirectory(saveFolder);
                    }

                    importFolderCaches.SortCaches(currentSelectedFolderCacheIndex);
                    currentSelectedFolderCacheIndex = 0;

                    var notFoundShaders = await AvatarImportUtil.CheckShaderIsExist(
                        loadPath,
                        applySaveMaterial
                    );
                    if (notFoundShaders.Length > 0)
                    {
                        if (!ShowShaderNotFoundDialog(notFoundShaders))
                        {
                            return;
                        }
                    }

                    await AvatarImportUtil.RunImport(saveFolder, loadPath, applySaveMaterial);

                    EditorUtility.DisplayDialog(
                        Messages.FileDialogTitleImport,
                        Messages.DialogCompleteMessage,
                        "OK"
                    );
                }
                catch (Exception e)
                {
                    EditorUtility.DisplayDialog(
                        Messages.FileDialogTitleImport,
                        Messages.DialogFailedMessage,
                        "OK"
                    );
                    ExceptionDispatchInfo.Capture(e).Throw();
                }
                finally
                {
                    IO.Editor.EditorAssetUtil.Cleanup();
                }
            }
        }

        private void DrawImportFolder()
        {
            EditorGUILayout.BeginHorizontal();
            {
                currentSelectedFolderCacheIndex = EditorGUILayout.Popup(
                    Messages.LabelSaveFolder,
                    currentSelectedFolderCacheIndex,
                    importFolderCaches.PopupPaths,
                    GUILayout.ExpandWidth(true)
                );

                if (GUILayout.Button("...", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                {
                    var selectedFolder = ShowSaveFolderSelectDialog();
                    if (!string.IsNullOrEmpty(selectedFolder))
                    {
                        if (!selectedFolder.Contains(Application.dataPath))
                        {
                            EditorUtility.DisplayDialog(
                                Messages.FileDialogTitleImport,
                                Messages.DialogExportFolderIsInvalid,
                                "OK"
                            );
                        }
                        else
                        {
                            importFolderCaches.AddNewPath(selectedFolder);
                            currentSelectedFolderCacheIndex = 0;
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private string ShowSaveFolderSelectDialog()
        {
            var folder = EditorUtility.OpenFolderPanel(
                Messages.FileDialogTitleSelectSaveFolder,
                Application.dataPath,
                ""
            );
            return folder;
        }

        private string ShowLoadFileSelectDialog()
        {
            var loadFile = EditorUtility.OpenFilePanel(
                Messages.FileDialogTitleImport,
                "",
                FileExtensions.XWearAvatar
            );
            return loadFile;
        }

        private bool ShowShaderNotFoundDialog(string[] notFoundShaders)
        {
            if (
                EditorUtility.DisplayDialog(
                    Messages.DialogShaderNotFoundTitle,
                    Messages.DialogShaderNotFoundMessage(notFoundShaders),
                    ok: Messages.DialogShaderNotFoundContinue,
                    cancel: CommonMessages.Cancel
                )
            )
            {
                return true;
            }

            return false;
        }
    }
}
