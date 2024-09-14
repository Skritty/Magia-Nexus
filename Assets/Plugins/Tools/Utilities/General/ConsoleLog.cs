// Created by: Ryan King

using System.Runtime.CompilerServices;
using UnityEngine;
using System;
using System.IO;

namespace Skritty.Tools.Utilities
{
    public static class ConsoleLog
    {
        private static string logFilePath = Application.dataPath + "/Logs/" + 
            DateTime.Today.ToString("MMddyy") + ".log";

        #region ConsoleSettings
        private static ConsoleSettings _settings;
        public static ConsoleSettings Settings
        {
            get
            {
                if (_settings != null) return _settings;
                else
                {
                    _settings = new ConsoleSettings();
                    return _settings;
                }
            }
        }
        #endregion ConsoleSettings

        //-----------Public Functions-----------//

        #region Basic Logging
        /// <summary>
        /// Basic Debug.Log() wrapper that a string value as well as the file, line number, 
        /// and function that the call originated from.
        /// </summary>
        /// <param name="output">String value to be logged.</param>
        public static void Log(string output, bool toConsole = false, [CallerFilePath] string f = "", 
            [CallerLineNumber] int l = 0, [CallerMemberName] string m = "")
        {
            Debug.Log(GenerateDebugMsg(output, f, l, m));

            if(DebugConsole.Instance.consoleLogOutputting || toConsole)
            {
                AddToDebugConsole(output);
            }
        }

        /// <summary>
        /// Basic Debug.LogWarning() wrapper that outputs a string value as well as the file, 
        /// line number, and function that the call originated from.
        /// </summary>
        /// <param name="output">String value to be logged.</param>
        public static void LogWarning(string output, bool toConsole = false, [CallerFilePath] string f = "", 
            [CallerLineNumber] int l = 0, [CallerMemberName] string m = "")
        {
            Debug.LogWarning(GenerateDebugMsg(output, f, l, m));

            if(DebugConsole.Instance.consoleLogOutputting || toConsole)
            {
                AddToDebugConsole(output);
            }
        }

        /// <summary>
        /// Basic Debug.LogError() wrapper that outputs a string value as well as the file, 
        /// line number, and function that the call originated from.
        /// </summary>
        /// <param name="output">String value to be logged.</param>
        public static void LogError(string output, bool toConsole = false, [CallerFilePath] string f = "", 
            [CallerLineNumber] int l = 0, [CallerMemberName] string m = "")
        {
            Debug.LogError(GenerateDebugMsg(output, f, l, m));

            if(DebugConsole.Instance.consoleLogOutputting || toConsole)
            {
                AddToDebugConsole(output);
            }
        }
        #endregion Basic Logging

        public static void Assert(bool condition, string output, [CallerFilePath] string f = "",
            [CallerLineNumber] int l = 0, [CallerMemberName] string m = "")
        {
            if(!condition)
            {
                Debug.LogError(GenerateDebugMsg(output, f, l, m));
            }
        }

        public static void AssertWarn(bool condition, string output, [CallerFilePath] string f = "",
            [CallerLineNumber] int l = 0, [CallerMemberName] string m = "")
        {
            if(!condition)
            {
                Debug.LogWarning(GenerateDebugMsg(output, f, l, m));
            }
        }

        #region Log to config file
        /// <summary>
        /// Basic logging plus uses ConsoleSettings.txt to do advanced logging. Currently only supports file logging. If set to true then also saves logs to file at logFilePath.
        /// </summary>
        /// <param name="output">String value to be logged.</param>
        public static void LogWithConfig(string output, [CallerFilePath] string filePath = "", 
            [CallerLineNumber] int lineNum = 0, [CallerMemberName] string methodName = "")
        {
            string logValue = "File - " + GetFileNameFromPath(filePath) + " | Line - " + 
                lineNum + " | Method - " + methodName + " | " + output;

            //TODO: Ryan @Trevor: This makes a link but the link doesn't go to the
            //line it says it should. It goes to this line.
            //Debug.Log($"<a href=\"{f}\" line=\"{l}\">{f}:{l}</a>");

            Debug.Log("[" + DateTime.Now.ToShortDateString() + " " + 
                DateTime.Now.ToShortTimeString() + "] " + logValue);
            
            //Write to log file
            if (Settings.fileLog)
            {
                if (!File.Exists(logFilePath))
                {
                    File.Create(logFilePath);
                }
                File.AppendAllText(logFilePath, "\n" + "[" + DateTime.Now.ToShortDateString() + 
                    " " + DateTime.Now.ToShortTimeString() + "] " + logValue);
            }
        }

        #endregion Log to config file

        #region Log Variable
        //TODO: Ryan: Working on making a log that outputs a variable's name and value.
        //Currently doesn't output the correct variable name.
        public static void LogPlusVariable(object value, [CallerFilePath] string f = "", 
            [CallerLineNumber] int l = 0, [CallerMemberName] string m = "")
        {
            Debug.Log(GenerateDebugVariableMsg(value, f, l, m));
            //MemberInfoGetting.GetMemberName(() => value) + ": " + value.ToString());
        }

        #endregion Log Variable

        //-----------Private Functions-----------//

        private static string GetFileNameFromPath(string path)
        {
            return path.Substring(path.LastIndexOf('\\') + 1);
        }

        private static void AddToDebugConsole(string outputValue)
        {
            if(DebugConsole.Instance)
            {
                DebugConsole.Instance.AddOutputEntry(outputValue);
            }
            else
            {
                LogWarning("Add Debug Console prefab to managers.");
            }
        }

        #region Generators

        private static string GenerateDebugMsg(string output, string filePath, 
            int lineNum, string methodName)
        {
            return $"[File: {GetFileNameFromPath(filePath)} | Line: {lineNum} | Method: {methodName}]" +
                $"\n{output}";
        }

        //TODO: Ryan: Working on making a log that outputs a variable's name and value.
        //Currently doesn't output the correct variable name.
        private static string GenerateDebugVariableMsg(object value, string filePath, 
            int lineNum, string methodName)
        {
            return "File: " + GetFileNameFromPath(filePath) + " | Line: " + lineNum + 
                " | Method: " + methodName + " | Variable - " + nameof(value) + " - " + 
                value.ToString() + " | Value - " + value.ToString();
        }

        #endregion Generators
    }

}
