using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Skritty.Tools.Utilities
{
    [System.Serializable]
    public class SerializedHashSet<T> : HashSet<T>, ISerializationCallbackReceiver
    {
        public List<T> values;

        public SerializedHashSet() { }

        public SerializedHashSet(IEnumerable<T> values)
        {
            AddRange(values);
        }

        //json-friendly data (values) has been deserialized. Prepare to pack it back into our HashSet
        public void OnAfterDeserialize()
        {
            Clear();

            /*//using Clear() while on the serialization thread screws up the m_bucket Array inside of HashSet.
            //Looking at the .NET source for HashSet, you can supposedly get around this by removing the entires one at a time.
            //It's a good bit slower but this callback only runs in editor.
            T[] cache = this.ToArray();
            for (int i = 0; i < cache.Length; i++)
            {
                this.Remove(cache[i]);
            }*/

            //this.EnsureCapacity(values.Count); //hoping this will help with the occasional 0 buckets issue
            if (values != null)
                AddRange(values);
        }

        //Rebuild values
        public void OnBeforeSerialize()
        {
            if (values == null)
                values = new List<T>();

            //Avoids directly touching the HashSet bc that gets wonky
            HashSet<T> tempHashFilter = new HashSet<T>(values);

            values.Clear();

            foreach (T tempHashVal in tempHashFilter)
            {
                values.Add(tempHashVal);
            }
        }        

        public void AddRange(IEnumerable<T> values)
        {
            foreach (T item in values)
            {
                this.Add(item);
            }
        }
    }

    public static class SerializedHashSetExtensions
    {
        public static SerializedHashSet<T> ToSerializedHashSet<T>(this IEnumerable<T> hash)
        {
            return new SerializedHashSet<T>(hash);
        }
    }
}
