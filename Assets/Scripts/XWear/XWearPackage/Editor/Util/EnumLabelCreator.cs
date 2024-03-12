using System;
using System.IO;
using XWear.IO;
using XWear.XWearPackage.Editor.Common;

namespace XWear.XWearPackage.Editor.Util
{
    public static class EnumLabelCreator
    {
        private static Message[] EnumValueLabelCreator<T>(Func<T, Message> create)
            where T : Enum
        {
            var t = typeof(T);
            if (!t.IsEnum)
            {
                throw new InvalidDataException("Type must be Enum");
            }

            var values = (T[])Enum.GetValues(t);
            var result = new Message[values.Length];
            for (var index = 0; index < result.Length; index++)
            {
                result[index] = create.Invoke(values[index]);
            }

            return result;
        }

        public static Message[] ExportTypeEnumLabels
        {
            get
            {
                return EnumValueLabelCreator<ExportContext.ExportType>(create: enumValue =>
                {
                    switch (enumValue)
                    {
                        case ExportContext.ExportType.Avatar:
                            return MessagesContainer.ExporterMessages.LabelExportTypeAvatar;
                        case ExportContext.ExportType.Wear:
                            return MessagesContainer.ExporterMessages.LabelExportTypeWear;
                        case ExportContext.ExportType.Accessory:
                            return MessagesContainer.ExporterMessages.LabelExportTypeAccessory;
                        default:
                            throw new ArgumentOutOfRangeException(
                                nameof(enumValue),
                                enumValue,
                                null
                            );
                    }
                });
            }
        }

        public static Message[] ImportTypeEnumLabels
        {
            get
            {
                return EnumValueLabelCreator<ImportContext.ImportType>(create: enumValue =>
                {
                    switch (enumValue)
                    {
                        case ImportContext.ImportType.Avatar:
                            return MessagesContainer.ImporterMessages.LabelExportTypeAvatar;
                        case ImportContext.ImportType.Wear:
                            return MessagesContainer.ImporterMessages.LabelExportTypeWear;
                        case ImportContext.ImportType.Accessory:
                            return MessagesContainer.ImporterMessages.LabelExportTypeAccessory;
                        default:
                            throw new ArgumentOutOfRangeException(
                                nameof(enumValue),
                                enumValue,
                                null
                            );
                    }
                });
            }
        }
    }
}
