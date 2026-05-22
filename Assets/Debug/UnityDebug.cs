using UnityEngine;
using System;

namespace AW.UnityResources
{

    public static class UnityDebug
    {
     
        private static bool _enableLog = true;
        private static bool _enableWarning = true;
        private static bool _enableError = true;
        private static bool _enableAssert = true;

        private static bool _includeTimestamp = true;

        private static Color _logColor = Color.green;
        private static Color _warningColor = Color.yellow;
        private static Color _errorColor = Color.red;
        private static Color _assertColor = Color.orange;

        private enum LogType
        {
            Log,
            Warning,
            Error,
            Assert
        }
      
        public static void Log(object caller, string message, Color? displayColor = null)
        {
            if (!_enableLog) return;
            Debug.Log(GetMessageContent(LogType.Log, caller, message, displayColor));
        }

        public static void LogWarning(object caller, string message, Color? displayColor = null)
        {
            if (!_enableWarning) return;
            Debug.LogWarning(GetMessageContent(LogType.Warning, caller, message, displayColor));
        }

        public static void LogError(object caller, string message, Color? displayColor = null)
        {
            if (!_enableError) return;
            Debug.LogError(GetMessageContent(LogType.Error, caller, message, displayColor));
        }

        public static void LogAssert(object caller, bool condition, string message, Color? displayColor = null)
        {
            if (!_enableAssert) return;
            Debug.Assert(condition, GetMessageContent(LogType.Assert, caller, message, displayColor));
        }

        private static string GetMessageContent(LogType logType, object caller, string message, Color? displayColor = null)
        {
            string messageContent = $"{GetFormattedLogType(logType)} {GetFormattedCaller(caller, displayColor)} {message}";

            if (_includeTimestamp)
            {
                string displayTime = DateTime.Now.ToString("HH:mm:ss");

                string timestampText = GetBoxedColorText(GetColorHexFromLogType(logType), displayTime);
                messageContent = timestampText + " " + messageContent;
            }

            return messageContent;
        }
        

        private static string GetFormattedLogType(LogType LogType)
            => GetBoxedColorText(GetColorHexFromLogType(LogType), LogType.ToString());
    

        private static string GetColorHexFromLogType(LogType LogType)
        {
            Color targetColor = LogType switch
            {
                LogType.Log => _logColor,
                LogType.Warning => _warningColor,
                LogType.Error => _errorColor,
                LogType.Assert => _assertColor,

                _ => Color.white
            };

            return  ColorUtility.ToHtmlStringRGBA(targetColor);

        }

        private static string GetFormattedCaller(object caller, Color? displayColor)
        {
            string name = caller is Type type ? type.Name : caller.GetType().Name;
            string callerText = $"[{name}]";

            if (displayColor != null)
            {
                string colorHex = ColorUtility.ToHtmlStringRGBA(displayColor.Value);
                callerText = GetBoxedColorText(colorHex, name);
            }

            return callerText;
        }

        private static string GetBoxedColorText(string colorHex, string boxedText)
            => $"<color=#{colorHex}>[{boxedText}]</color>";
        
    }

}
