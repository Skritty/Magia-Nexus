using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
public class DictionaryStat<Key, Value> : IModifiable<Value>, IEnumerable<Value>
{
    [field: SerializeReference]
    public InheritModifiers<Value> ModifierInheritMethod { get; set; } = new NoInherit<Value>();
    [field: SerializeReference, PropertyOrder(1), FoldoutGroup("@GetType()"), ReadOnly]
    private List<IDataContainer<Value>> _modifiers = new();
    public List<IDataContainer<Value>> Modifiers
    {
        get
        {
            List<IDataContainer<Value>> modifiers = ModifierInheritMethod.InheritedModifiers();
            return modifiers == null ? _modifiers : modifiers;
        }
        set
        {
            _modifiers = value;
        }
    }
    public Dictionary<Key, IDataContainer<Value>> ModifierDictionary { get; set; } = new();
    public int Count => Modifiers.Count;

    public IEnumerator<Value> GetEnumerator()
    {
        return ToList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Value this[Key key]
    {
        get
        {
            if (ModifierDictionary.Count == 0 || !ModifierDictionary.ContainsKey(key)) return default;
            return ModifierDictionary[key].Value;
        }
        set
        {
            ModifierDictionary.Remove(key);
            Add(key, value);
        }
    }

    public List<Value> ToList
    {
        get
        {
            List<Value> list = new List<Value>();
            foreach (IDataContainer<Value> data in Modifiers)
            {
                list.Add(data.Value);
            }
            return list;
        }
    }

    public bool TryAdd(IDataContainer modifier)
    {
        IDataContainer<Value> cast = (IDataContainer<Value>)modifier;
        if (cast == null) return false;
        Modifiers.Add(cast);
        return true;
    }

    public System.Action Add(Value value)
    {
        DataContainer<Value> data = new DataContainer<Value>(value);
        Modifiers.Add(data);
        return () => Modifiers.Remove(data);
    }

    public System.Action Add(Key key, Value value)
    {
        DataContainer<Value> data = new DataContainer<Value>(value);
        Modifiers.Add(data);
        ModifierDictionary.Add(key, data);
        return () =>
        {
            Modifiers.Remove(data);
            ModifierDictionary.Remove(key);
        };
    }

    public System.Action AddRange(IEnumerable<Value> values)
    {
        System.Action cleanup = null;
        foreach (Value value in values)
        {
            cleanup += Add(value);
        }
        return cleanup;
    }

    public void Add(IDataContainer<Value> modifier)
    {
        Modifiers.Add(modifier);
    }

    public void Remove(Value value)
    {
        foreach (IDataContainer<Value> m in Modifiers.ToArray())
        {
            if (m.Value.Equals(value)) Modifiers.Remove(m);
        }
    }

    public void Remove(IDataContainer modifier)
    {
        Modifiers.Remove(modifier as IDataContainer<Value>);
    }

    public void Clear()
    {
        Modifiers.Clear();
    }

    public bool Contains(Value modifier)
    {
        foreach (IDataContainer m in Modifiers)
        {
            if (m.Equals(modifier)) return true;
        }
        return false;
    }

    public bool Contains(IDataContainer modifier, out int count)
    {
        count = 0;
        foreach (IDataContainer m in Modifiers)
        {
            if (m.Equals(modifier)) count++;
        }
        if (count > 0)
        {
            return true;
        }
        return false;
    }

    public IModifiable Clone(bool preserveModifiers)
    {
        IModifiable<Value> clone = (IModifiable<Value>)MemberwiseClone();
        if (preserveModifiers) clone.Modifiers = new List<IDataContainer<Value>>(Modifiers);
        return clone;
    }
}