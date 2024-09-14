using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializedDictionary<Key, Value> : Dictionary<Key, Value>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<KeyValue> serializedPairs;

    [System.Serializable]
    private struct KeyValue
    {
        public Key key;
        public Value value;
    }

    public SerializedDictionary() { }
    public SerializedDictionary(Dictionary<Key, Value> original)
    {
        foreach (KeyValuePair<Key, Value> pair in original)
        {
            Add(pair.Key, pair.Value);
        }
    }
    public SerializedDictionary(SerializedDictionary<Key, Value> original)
    {
        foreach (KeyValuePair<Key, Value> pair in original)
        {
            Add(pair.Key, pair.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        Clear();
        if (serializedPairs != null)
        {
            foreach (KeyValue pair in serializedPairs)
            {
                Add(pair.key, pair.value);
            }
        }
    }

    public void OnBeforeSerialize()
    {
        if (serializedPairs == null)
            serializedPairs = new List<KeyValue>();
        serializedPairs.Clear();
        foreach (KeyValuePair<Key, Value> pair in this)
        {
            serializedPairs.Add(new KeyValue() { key = pair.Key, value = pair.Value });
        }
    }
}