using System;
using UnityEngine;

namespace XWear.XWearPackage.Editor.Util
{
    public static class TypedLogger
    {
        private const string LogPrefix = "XWear Package";

        public enum LogType
        {
            Normal,
            Warning,
            Error
        }

        public static void Logging(
            string logMessage,
            string logLabel,
            LogType logType = LogType.Normal,
            Action<string> onLogging = null
        )
        {
            var label = $"[{LogPrefix}:{logLabel}]";
            var colorPrefix = "";
            var colorSuffix = "";
            switch (logType)
            {
                case LogType.Warning:
                    colorPrefix = "<color=\"yellow\">";
                    colorSuffix = "</color>";
                    break;
                case LogType.Error:
                    colorPrefix = "<color=\"red\">";
                    colorSuffix = "</color>";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }

            var log = $"{colorPrefix}{label}{colorSuffix} {logMessage}";
            Debug.Log(log);
            onLogging?.Invoke(log);
        }
    }
}
