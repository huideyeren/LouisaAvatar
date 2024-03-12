using System;
using System.Text;
using UnityEditor;
using XWear.IO;
using XWear.XWearPackage.Editor.Validator;

namespace XWear.XWearPackage.Editor.Common
{
    public static class MessagesContainer
    {
        private const string CurrentLanguageEditorPrefKey = "VROID_XWEAR_PACKAGE_CURRENT_LANGUAGE";

        public static Language CurrentLanguage { get; private set; }

        static MessagesContainer()
        {
            var defaultLanguage =
                UnityEngine.Application.systemLanguage == UnityEngine.SystemLanguage.Japanese
                    ? Language.Ja
                    : Language.En;
            var langValue = EditorPrefs.GetInt(CurrentLanguageEditorPrefKey, (int)defaultLanguage);
            CurrentLanguage = (Language)langValue;
        }

        public static void SwitchCurrentLanguage(Language language)
        {
            CurrentLanguage = language;
        }

        public enum Language
        {
            Ja,
            En
        }

        public static class EditorWindowMessages
        {
            public static Message LabelLanguage = new Message(ja: "言語", en: "Languages");
        }

        public static class ValidatorMessages
        {
            public static Message GetValidatorMessage(ValidateResultType validateResultType)
            {
                switch (validateResultType)
                {
                    case ValidateResultType.Ok:
                        return MessageValidateOk;
                    case ValidateResultType.CommonErrorMissingRootGameObject:
                        return CommonError;
                    case ValidateResultType.CommonErrorNoHierarchy:
                        return MessageCommonErrorNoHierarchy;
                    case ValidateResultType.CommonErrorNoRenderer:
                        return MessageCommonErrorNoRenderer;
                    case ValidateResultType.SmrErrorNotFound:
                        return MessageSmrErrorNotFound;
                    case ValidateResultType.SmrErrorMissingRootBone:
                        return MessageSmrErrorMissingRootBone;
                    case ValidateResultType.SmrErrorNotContainsRootBone:
                        return MessageSmrErrorNotContainsRootBone;
                    case ValidateResultType.SmrErrorInactiveWeightedBones:
                        return MessageSmrErrorInactiveWeightedBones;
                    case ValidateResultType.SmrWarningContainsNulBone:
                        return MessageSmrWarningContainsNullBones;
                    case ValidateResultType.SmrWarningZeroWeightBone:
                        return MessageSmrWarningZeroWeightBone;

                    case ValidateResultType.AvatarErrorNotHumanoid:
                        return MessageAvatarErrorNotHumanoid;
                    case ValidateResultType.AvatarErrorNotFoundAnimator:
                        return MessageAvatarErrorNotFoundAnimator;
                    case ValidateResultType.AvatarErrorContainsSameNameHierarchy:
                        return MessageAvatarErrorContainsSameNameHierarchy;

                    case ValidateResultType.WearErrorContainsNullHumanoidMapComponent:
                        return MessageWearErrorContainsNullHumanoidMapComponent;
                    case ValidateResultType.WearErrorInvalidHumanoidComponent:
                        return MessageWearErrorInvalidHumanoidComponent;
                    case ValidateResultType.WearErrorNotFoundHumanoidComponent:
                        return MessageWearErrorNotFoundHumanoidComponent;

                    case ValidateResultType.WearErrorEmptyHumanoidMap:
                        return MessageWearErrorEmptyHumanoidMap;

                    case ValidateResultType.AccessoryWarningNotFoundAccessoryMap:
                        return MessageAccessoryWarningNotFoundAccessoryMap;
                    case ValidateResultType.AccessoryWarningMapBoneIsEmpty:
                        return MessageAccessoryWarningMapBoneIsEmpty;
                    case ValidateResultType.AccessoryIsNotSupported:
                        return AccessoryIsNotSupported;

                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(validateResultType),
                            validateResultType,
                            null
                        );
                }
            }

            private static readonly Message MessageValidateOk = new Message(ja: "正常です。", en: "OK");

            private static readonly Message CommonError = new Message(
                ja: "出力対象のGameObjectを指定してください。",
                en: "You must specify the GameObject to output."
            );

            private static readonly Message MessageCommonErrorNoHierarchy = new Message(
                ja: "出力対象に階層構造がありません。",
                en: "There is no hierarchical structure in the output target."
            );

            private static readonly Message MessageCommonErrorNoRenderer = new Message(
                ja: "出力対象にメッシュが含まれていません。",
                en: "Mesh is not included in the output target."
            );

            private static readonly Message MessageSmrErrorNotFound = new Message(
                ja: "出力対象にSkinnedMeshRendererが含まれていません。",
                en: "SkinnedMeshRenderer is not included in the output target."
            );

            private static readonly Message MessageSmrErrorMissingRootBone = new Message(
                ja: "SkinnedMeshRendererにRootBoneがアサインされていないものがあります。",
                en: "Some RootBone is not assigned to SkinnedMeshRenderer."
            );

            private static readonly Message MessageSmrErrorNotContainsRootBone = new Message(
                ja: "出力対象のGameObject以下にSkinnedMeshRendererのRootBoneが含まれていません。",
                en: "The RootBone of SkinnedMeshRenderer is not included under the output target GameObject."
            );

            private static readonly Message MessageSmrErrorInactiveWeightedBones = new Message(
                ja: "ウェイトが塗られている非アクティブなボーンが存在します。",
                en: "There are inactive bones with weights painted on them."
            );

            private static readonly Message MessageSmrWarningContainsNullBones = new Message(
                ja: "SkinnedMeshRenderer.bonesにNull値が含まれています。\n"
                    + "出力時にメッシュデータからそのインデックスは削除されます。\n"
                    + "元のオブジェクトは変更されません。",
                en: "SkinnedMeshRenderer.bones contains a null value.\n"
                    + "The index will be removed from the mesh data at output.\n"
                    + "The original object is not modified."
            );

            private static readonly Message MessageSmrWarningZeroWeightBone = new Message(
                ja: "出力対象にウェイトが塗られていないボーンが含まれています。\n"
                    + "出力時にメッシュデータから該当インデックスが削除され、ヒエラルキー上から削除されます。\n"
                    + "元のオブジェクトは変更されません。",
                en: "Unweighted bones are included in the output. \n"
                    + "The corresponding index is removed from the mesh data at output and deleted from the hierarchy.\n"
                    + " The original object remains unchanged."
            );

            private static readonly Message MessageAvatarErrorNotHumanoid = new Message(
                ja: "アバターとして出力する場合、出力対象はHumanoidである必要があります。",
                en: "When outputting as avatar, the output target must be a Humanoid."
            );

            private static readonly Message MessageAvatarErrorNotFoundAnimator = new Message(
                ja: "アバターとして出力する場合、RootになるGameObjectにAnimatorがアタッチされている必要があります。",
                en: "When outputting as avatar, the Animator must be attached to the GameObject that will be the Root."
            );

            private static readonly Message MessageAvatarErrorContainsSameNameHierarchy =
                new Message(
                    ja: "階層構造上に同名オブジェクトが含まれています。",
                    en: "The hierarchical structure contains objects with the same name."
                );

            private static readonly Message MessageWearErrorContainsNullHumanoidMapComponent =
                new Message(
                    ja: "HumanoidMapComponent内にNullなボーンが含まれます。",
                    en: "Null bones are included in the HumanoidMapComponent."
                );

            private static readonly Message MessageWearErrorInvalidHumanoidComponent = new Message(
                ja: "出力対象のGameObject以下に含まれないHumanoidComponentのボーンが存在します。",
                en: "There are bones of HumanoidComponent that are not included under the output target GameObject."
            );

            private static readonly Message MessageWearErrorNotFoundHumanoidComponent = new Message(
                ja: $"衣装として出力する場合、{nameof(HumanoidMapComponent)}がアタッチされている必要があります。\n"
                    + $"ウィンドウから自動設定を実行するか、手動で設定をおこなってください。",
                en: $"To output as a costume, {nameof(HumanoidMapComponent)} must be attached.\n"
                    + $" You can either run the automatic configuration from the window or configure it manually."
            );

            private static readonly Message MessageWearErrorEmptyHumanoidMap = new Message(
                ja: $"{nameof(HumanoidMapComponent)}に登録されているボーンがありません\n"
                    + $"ウィンドウから自動設定を実行するか、手動で設定をおこなってください。",
                en: $"There are no bones registered in the {nameof(HumanoidMapComponent)}.\n"
                    + $" Please execute automatic configuration from the window or configure manually."
            );

            private static readonly Message MessageAccessoryWarningNotFoundAccessoryMap =
                new Message(
                    ja: $"アクセサリとして出力する場合、{nameof(AccessoryMapComponent)}をアタッチすることができます。\n"
                        + $"VRoid Studioにインポートした際にアタッチ先の推奨ボーンを指定することが可能です。",
                    en: ""
                );

            private static readonly Message MessageAccessoryWarningMapBoneIsEmpty = new Message(
                ja: $"{nameof(AccessoryMapComponent)}に登録されているボーンがありません",
                en: ""
            );

            private static readonly Message AccessoryIsNotSupported =
                new() { ja = $"アクセサリは現在非対応です", en = "Accessories are currently not supported." };

            public static Message GetFixLabel(FixFunctionType fixFunctionType)
            {
                switch (fixFunctionType)
                {
                    case FixFunctionType.Debug:
                        return LabelFix;
                    case FixFunctionType.AutoAssignHumanoidMapComponent:
                        return LabelFixAutoAssignHumanoidMapComponent;
                    case FixFunctionType.AddAccessoryMapComponent:
                        return LabelFixAddAccessoryMapComponent;
                    case FixFunctionType.RemoveNullBoneFromHumanoidMapComponent:
                        return LabelFixRemoveNullBoneFromHumanoidMapComponent;
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(fixFunctionType),
                            fixFunctionType,
                            null
                        );
                }
            }

            public static readonly Message LabelCheckValidateTarget = new Message(
                ja: "対象を確認",
                en: "Check the target"
            );

            private static readonly Message LabelFix = new Message(ja: $"自動で修正", en: "Auto fix");

            private static readonly Message LabelFixAutoAssignHumanoidMapComponent = new Message(
                ja: "自動でセットアップする",
                en: "Auto fix"
            );

            private static readonly Message LabelFixAddAccessoryMapComponent = new Message(
                ja: "AccessoryMapComponentをアサインする",
                en: "Auto fix"
            );

            private static readonly Message LabelFixRemoveNullBoneFromHumanoidMapComponent =
                new Message(ja: "自動で取り除く", en: "Auto fix");
        }

        public static class CommonMessage
        {
            public static Message Ok = new Message(ja: "OK", en: "OK");
            public static Message Cancel = new Message(ja: "キャンセル", en: "Cancel");
            public static Message AddToList = new Message(ja: "リストに追加", en: "Add to list");

            public static Message DeleteFromList = new Message(
                ja: "リストから削除",
                en: "Remove from list"
            );
        }

        public static class ExporterMessages
        {
            public static Message LabelExporterTitle = new Message(
                ja: "エクスポーターウィンドウ",
                en: "Exporter window"
            );

            public static Message LabelExportTarget = new Message(
                ja: "エクスポート対象",
                en: "Export target"
            );

            public static Message LabelExportMode = new Message(ja: "エクスポートモード", en: "Export mode");

            public static Message ButtonExport = new Message(ja: "エクスポートを実行", en: "Run export");

            public static Message DialogTitle = new Message(
                ja: "VRoid XWear Exporter",
                en: "VRoid XWear Exporter"
            );

            public static Message DialogCompleteMessage = new Message(
                ja: "出力が正常に完了しました",
                en: "Export completed successfully"
            );

            public static Message DialogFailedMessage = new Message(
                ja: "出力に失敗しました",
                en: "Export failed"
            );

            public static Message SaveFileDialog = new Message(
                ja: "保存先を選択",
                en: "Select destination"
            );

            public static readonly string DefaultExportFileName = "export";
            public static Message LabelExportTypeAvatar = new Message(ja: "アバター", en: "Avatar");
            public static Message LabelExportTypeWear = new Message(ja: "衣装", en: "Wear");

            public static Message LabelExportTypeAccessory = new Message(
                ja: "アクセサリー",
                en: "Accessory"
            );

            public static Message LabelAdvancedOptionsFoldOut = new Message(
                ja: "高度な設定",
                en: "Advanced Settings"
            );

            public static Message LabelAvatarDressList = new Message(
                ja: "同梱する衣装の一覧",
                en: "Include outfits list"
            );

            public static Message MessageAvatarDressListIsEmpty = new Message(
                ja: "登録されている衣装のオブジェクトがありません",
                en: "There are no objects in the"
            );

            public static Message LabelLicense = new(ja: "ライセンス", en: "License");

            public static Message TextAdvancedSettingsInfo =
                new(
                    ja: "衣装オブジェクトを追加するとアバターとまとめてひとつのXAvatarとしてエクスポートすることができます。\n"
                        + "アバターのオブジェクト内に含まれている衣装は追加しなくてもまとめてエクスポートされます。",
                    en: "When outfit objects are added, they can be exported together with the avatar as a single XAvatar.\n"
                        + "Outfits included in the avatar object will be exported together even if they are not added."
                );
        }

        public static class ImporterMessages
        {
            public static Message LabelImportButton = new Message(ja: "読み込む", en: "Import");

            public static Message LabelApplySavedMaterial = new Message(
                ja: "元のShaderを読み込む",
                en: "Load the original Shader"
            );

            public static Message DialogShaderNotFoundTitle = new Message(
                ja: "Shaderが見つかりませんでした",
                en: "Shader not found"
            );

            public static string DialogShaderNotFoundMessage(string[] notFoundShaders)
            {
                var sb = new StringBuilder();
                sb.AppendLine(DialogShaderNotFoundMessage1);
                sb.AppendLine(DialogShaderNotFoundMessage2);
                sb.AppendLine("");
                for (int i = 0; i < notFoundShaders.Length; i++)
                {
                    if (i == 3)
                    {
                        sb.AppendLine("...");
                        break;
                    }

                    sb.AppendLine(notFoundShaders[i]);
                    if (i != notFoundShaders.Length - 1)
                    {
                        sb.AppendLine("");
                    }
                }

                return sb.ToString();
            }

            private static readonly Message DialogShaderNotFoundMessage1 = new Message(
                ja: "以下のShaderがプロジェクト内に含まれていないため、正常にマテリアルのインポートをすることができません。",
                en: "The following Shader is not included in the project and cannot import materials properly."
            );

            private static readonly Message DialogShaderNotFoundMessage2 = new Message(
                ja: "続行しますか？",
                en: "Continue?"
            );

            public static Message DialogShaderNotFoundContinue = new Message(ja: "続行する", en: "Yes");

            public static Message FileDialogTitleImport = new Message(
                ja: "VRoid XAvatar インポート",
                en: "VRoid XAvatar Import"
            );

            public static Message FileDialogTitleSelectSaveFolder = new Message(
                ja: "XAvatarの保存先を選択",
                en: "Select where you want to save your XAvatar"
            );

            public static Message DialogCompleteMessage = new Message(
                ja: "インポートが正常に完了しました",
                en: "Import successfully completed"
            );

            public static Message DialogFailedMessage = new Message(
                ja: "インポートに失敗しました",
                en: "Import failed"
            );

            public static Message LabelExportTypeAvatar = new Message(ja: "アバター", en: "Avatar");
            public static Message LabelExportTypeWear = new Message(ja: "衣装", en: "Wear");

            public static Message LabelExportTypeAccessory = new Message(
                ja: "アクセサリー",
                en: "Accessory"
            );

            public static Message DialogExportFolderIsInvalid =
                new(
                    ja: "出力先フォルダはプロジェクトフォルダである必要があります",
                    en: "Output destination folder must be a project folder"
                );

            public static Message LabelSaveFolder = new Message(ja: "保存先", en: "Save to");
        }

        public static class LicenseMessages
        {
            public static Message LabelLicenseType = new(ja: "ライセンスタイプ", en: "Author");
            public static Message ButtonSelectLicenseFile = new(ja: "ライセンスファイルを選択", en: "");
            public static Message InfoLicenseFileExtension = new(ja: "PDFファイルを選択できます", en: "");
            public static Message LabelSaveDir = new(ja: "保存先", en: "");

            public static Message DialogMessageCompleteGenerate =
                new(ja: "ライセンスオブジェクトを生成しました", en: "");

            public static Message LabelAuthor = new(ja: "作者", en: "Author");
            public static Message LabelLicenseText = new(ja: "ライセンス文書", en: "");
            public static Message LabelLicenseUrl = new(ja: "ライセンスUrl", en: "");
            public static Message LabelLicenseFile = new(ja: "ライセンスファイル", en: "");
            public static Message ButtonOpenLicenseFile = new(ja: "ファイルを開く", en: "");
        }
    }

    [Serializable]
    public struct Message
    {
        public string ja;
        public string en;

        public Message(string ja, string en)
        {
            this.ja = ja;
            this.en = en;
        }

        public override string ToString()
        {
            var result = "";
            switch (MessagesContainer.CurrentLanguage)
            {
                case MessagesContainer.Language.Ja:
                    result = ja;
                    break;
                case MessagesContainer.Language.En:
                    result = en;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // フォールバックを考慮
            return string.IsNullOrEmpty(result) ? ja : result;
        }

        public static implicit operator string(Message m)
        {
            return m.ToString();
        }
    }
}
