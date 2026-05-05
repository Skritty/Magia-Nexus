using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Skritty.Tools.Utilities
{
    [CreateAssetMenu(menuName = "Persistant Data Store", fileName = "Data Store")]
    public class PersistantDataStore : ScriptableObject
    {
        #region Static
        private static SerializedDictionary<string, PersistantDataStore> stores = new SerializedDictionary<string, PersistantDataStore>();
        private static void Add(string name, PersistantDataStore value)
        {
            if (!stores.TryAdd(name, value))
                stores[name] = value;
            else
                Debug.Log($"Added {name} to data store catalogue");
        }

        public static PersistantDataStore GetDataStore(string name)
        {
            if (PersistantDataStoreManager.Instance && !PersistantDataStoreManager.Instance.taken)
            {
                foreach (PersistantDataStore store in PersistantDataStoreManager.Instance.stores)
                {
                    stores.TryAdd(store.name, store);
                }
                PersistantDataStoreManager.Instance.taken = true;
            }

            PersistantDataStore value;
            if (stores.TryGetValue(name, out value))
                return value;
            value = CreateInstance<PersistantDataStore>();
            value.name = name;
            Add(name, value);
            return value;
        }
        #endregion

        #region Scriptable Object
        [SerializeField]
        private SerializedDictionary<string, object> data = new SerializedDictionary<string, object>();

        public T Get<T>(string key)
        {
            object value;
            if (data.TryGetValue(key, out value))
            {
                if (value == null) return default;
                return (T)value;
            }
            return default;
        }

        public void Set(string key, object value)
        {
            if (!data.TryAdd(key, value))
                data[key] = value;
        }

        public bool Has(string key)
        {
            return data.ContainsKey(key);
        }
        #endregion
    }
}