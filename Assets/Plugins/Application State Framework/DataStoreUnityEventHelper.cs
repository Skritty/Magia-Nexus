using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Skritty.Tools.Utilities
{
    public class DataStoreUnityEventHelper : MonoBehaviour
    {
        [SerializeField]
        private string valueName;
        [SerializeField]
        private PersistantDataStore store;

        public void Set(Object value)
        {
            store.Set(valueName, value);
        }

        public void SetVariableName(string name)
        {
            valueName = name;
        }

        public void SetDataStore(PersistantDataStore store)
        {
            this.store = store;
        }

        public void SetString(string text)
        {
            store.Set(valueName, text);
        }

        public void SetInt(int amt)
        {
            store.Set(valueName, amt);
        }

        public void IncrementInt(int amt)
        {
            int original = store.Get<int>(valueName);
            original += amt;
            store.Set(valueName, original);
        }

        public void SetFloat(float amt)
        {
            store.Set(valueName, amt);
        }

        public void IncrementFloat(float amt)
        {
            float original = store.Get<float>(valueName);
            original += amt;
            store.Set(valueName, original);
        }
    }
}