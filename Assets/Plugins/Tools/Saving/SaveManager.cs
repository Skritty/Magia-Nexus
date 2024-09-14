using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Skritty.Tools.Utilities;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Serialization;

namespace Skritty.Tools.Saving
{
    /// <summary>
    /// When applied to fields or properties, they will be saved and loaded when this.Save() and this.Load() are called. Netonsoft's <see cref="JsonProperty"/> can also be used if you'd like.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class SaveLoadAttribute : Attribute { }

    public enum SaveFolders 
    { 
        Debug = 1, 
        Inventory = 2, 
        Progress = 3, 
        Checkpoints = 4, 
        Settings = 5
    }

    public static class SaveManager
    {
        private static Action globalSave;
        private static string SaveFilePath(SaveFolders folder, string label = "") => Application.persistentDataPath + "/SaveData/" + folder.ToString() + "/" + (string.IsNullOrEmpty(label) ? "" : label + ".json");

        #region Extension Methods
        /// <summary>
        /// Saves any fields or properties marked with the <see cref = "SaveLoadAttribute">SaveLoad</see> attribute. Nested classes will also need to be marked!
        /// </summary>
        /// <param name="folder">Folder location of the save file</param>
        /// <param name="label">Save file name</param>
        public static void Save(this object self, SaveFolders folder, string label)
        {
            if(string.IsNullOrEmpty(label))
            {
                ConsoleLog.LogError($"Please enter a save label for object {self}!");
                return;
            }
            if(!Directory.Exists(SaveFilePath(folder)))
            {
                Directory.CreateDirectory(SaveFilePath(folder));
            }

            JsonSerializerSettings settings = new JsonSerializerSettings() { ContractResolver = new UseSaveLoadAttributeContractResolver() };
            string json = JsonConvert.SerializeObject(self, settings);
            File.WriteAllText(SaveFilePath(folder, label), json);

            ConsoleLog.Log("JSON: " + json);
        }

        /// <summary>
        /// Loads saved data into any fields or properties marked with the <see cref = "SaveLoadAttribute">SaveLoad</see> attribute. Nested classes will also need to be marked!
        /// </summary>
        /// <param name="folder">Folder location of the save file</param>
        /// <param name="label">Save file name</param>
        public static void Load<T>(this T self, SaveFolders folder, string label)
        {
            if(!File.Exists(SaveFilePath(folder, label)))
            {
                ConsoleLog.LogWarning($"No save found for {self} with label {label}! Failed to load!");
                return;
            }

            string json = File.ReadAllText(SaveFilePath(folder, label));
            JsonSerializerSettings settings = new JsonSerializerSettings() { ContractResolver = new UseSaveLoadAttributeContractResolver() };
            T container = JsonConvert.DeserializeObject<T>(json, settings);

            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.DeclaredOnly;
            List<MemberInfo> members = typeof(T).GetAllMembers(bindingFlags).Where(m => m.IsDefined(typeof(SaveLoadAttribute)) || m.IsDefined(typeof(JsonPropertyAttribute))).ToList();
            foreach(MemberInfo member in members)
            {
                member.SetValue(self, member.GetValue(container));
            }
        }

        /// <summary>
        /// Creates and returns a new instances of the desired class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Load<T>(SaveFolders folder, string label)
        {
            T instance = Activator.CreateInstance<T>();
            instance.Load(folder, label);
            return instance;
        }

        public static void SubscribeToGlobalSaves(this object self, SaveFolders folder, string label)
        {
            globalSave += () => Save(self, folder, label);
        }

        public static void UnsubscribeFromGlobalSaves(this object self, SaveFolders folder, string label)
        {
            globalSave -= () => Save(self, folder, label);
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Deletes the desired save data.
        /// </summary>
        /// <param name="folder">Folder location of the save file</param>
        /// <param name="label">Save file name</param>
        public static void ClearSaveData(SaveFolders folder, string label)
        {
            if(!File.Exists(SaveFilePath(folder, label)))
            {
                ConsoleLog.LogWarning($"No save data found to delete for label {label}");
                return;
            }
            else
            {
                File.Delete(SaveFilePath(folder, label));
                ConsoleLog.Log($"Save data {folder}/{label} has been deleted!");
            }
        }

        public static void GlobalSave()
        {
            globalSave?.Invoke();
        }
        #endregion

        protected class UseSaveLoadAttributeContractResolver : DefaultContractResolver
        {
            protected override List<MemberInfo> GetSerializableMembers(Type objectType)
            {
                BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.DeclaredOnly;
                return objectType.GetAllMembers(bindingFlags).Where(m => m.IsDefined(typeof(SaveLoadAttribute)) || m.IsDefined(typeof(JsonPropertyAttribute))).ToList();
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) => base.CreateProperties(type, MemberSerialization.Fields);
        }
    }
}
