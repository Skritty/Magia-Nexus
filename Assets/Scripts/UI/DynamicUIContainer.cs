using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Skritty.Tools.Utilities
{ 
    public class DynamicUIContainer : UIBehaviour
    {
        [Header("Auto-Assign")]
        [SerializeField]
        private AutoAssign[] autoAssigns = new AutoAssign[0];
        [System.Serializable]
        private class AutoAssign
        {
            [Sirenix.OdinInspector.ShowInInspector]
            public System.Type assignToSingleton;
            [SerializeField, Sirenix.OdinInspector.ReadOnly]
            public string type;
            [SerializeField]
            public string valueName;
        }

        [Header("References")]
        [SerializeField, Sirenix.OdinInspector.ShowInInspector]
        private SerializedDictionary<string, Object> references = new SerializedDictionary<string, Object>();
        public Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();

        public T Get<T>(string key) where T : Object
        {
            try
            {
                return (T)references[key];
            }
            catch
            {
                return null;
            }
        }

        public T GetData<T>(string key)
        {
            return (T)data[key];
        }

        protected override void Start()
        {
            base.Start();
            AutoSetSelfToSingleton();
        }

        public void AutoSetSelfToSingleton()
        {
            foreach(AutoAssign a in autoAssigns)
            {
                try
                {
                    System.Type t = System.Type.GetType(a.type);
                    object instance = t.GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Public).GetValue(null, null);
                    System.Reflection.PropertyInfo p = t.GetProperty(a.valueName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                    System.Reflection.FieldInfo f = t.GetField(a.valueName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                    p?.SetValue(instance, this);
                    f?.SetValue(instance, this);
                }
                catch (System.Exception err)
                {
                    Debug.LogWarning(err);
                }
            }
        }

        protected override void OnValidate()
        {
            foreach (AutoAssign a in autoAssigns)
                if(a.assignToSingleton != null)
                    a.type = a.assignToSingleton.AssemblyQualifiedName;
        }
    }
}