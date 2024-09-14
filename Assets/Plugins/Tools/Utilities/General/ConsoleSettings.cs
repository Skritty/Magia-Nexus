// Created by: Ryan King

using UnityEngine;

namespace Skritty.Tools.Utilities
{
    [System.Serializable]
    public class ConsoleSettings
    {
        public bool fileLog;
        public bool databaseLog;
        public bool inGameLog;
        public string[] devs;

        public ConsoleSettings(bool useDefaultValues = false)
        {
            if (useDefaultValues)
            {
                fileLog = true;
                databaseLog = false;
                inGameLog = false;
                devs = new string[0];
                return;
            }

            ConsoleSettings d = DeserializeSettings();

            if (d == null)
            {
                d = new ConsoleSettings(true);
            }

            fileLog = d.fileLog;
            databaseLog = d.databaseLog;
            inGameLog = d.inGameLog;
            devs = d.devs;
        }

        public ConsoleSettings DeserializeSettings()
        {
            string fileText;
            try
            {
                fileText = Resources.Load<TextAsset>("ConsoleSettings").text;
            }
            catch
            {
                UnityEngine.Debug.Log("ConsoleSettings.txt file not found! Using default values.");
                return null;
            }

            return JsonUtility.FromJson<ConsoleSettings>(fileText);
        }
    }
}
