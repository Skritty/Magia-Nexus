using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ListStat<T> : IModifiable<T>, IEnumerable<T>
{
    [field: SerializeReference]
    public InheritModifiers<T> ModifierInheritMethod { get; set; } = new NoInherit<T>();
    [field: SerializeReference, PropertyOrder(1), FoldoutGroup("@GetType()")]
    private List<IDataContainer<T>> _modifiers = new();
    public List<IDataContainer<T>> Modifiers
    {
        get
        {
            List<IDataContainer<T>> modifiers = ModifierInheritMethod.InheritedModifiers();
            return modifiers == null ? _modifiers : modifiers;
        }
        set
        {
            _modifiers = value;
        }
    }
    public int Count => Modifiers.Count;

    public IEnumerator<T> GetEnumerator()
    {
        return ToList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public T this[int index]
    {
        get
        {
            if (Modifiers.Count == 0) return default;
            return Modifiers[index].Value;
        }
    }

    public List<T> ToList
    {
        get
        {
            List<T> list = new List<T>();
            foreach (IDataContainer<T> data in Modifiers)
            {
                list.Add(data.Value);
            }
            return list;
        }
    }

    public bool TryAdd(IDataContainer modifier)
    {
        IDataContainer<T> cast = (IDataContainer<T>)modifier;
        if (cast == null) return false;
        Modifiers.Add(cast);
        return true;
    }

    public System.Action Add(T value)
    {
        DataContainer<T> data = new DataContainer<T>(value);
        Modifiers.Add(data);
        return () => Modifiers.Remove(data);
    }

    public System.Action AddRange(IEnumerable<T> values)
    {
        System.Action cleanup = null;
        foreach(T value in values)
        {
            cleanup += Add(value);
        }
        return cleanup;
    }

    public void Add(IDataContainer<T> modifier)
    {
        Modifiers.Add(modifier);
    }

    public void Remove(T value)
    {
        foreach (IDataContainer<T> m in Modifiers.ToArray())
        {
            if(m.Value.Equals(value)) Modifiers.Remove(m);
        }
    }

    public void Remove(IDataContainer modifier)
    {
        Modifiers.Remove(modifier as IDataContainer<T>);
    }

    public void Clear()
    {
        Modifiers.Clear();
    }

    public bool Contains(T modifier)
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
        IModifiable<T> clone = (IModifiable<T>)MemberwiseClone();
        if(preserveModifiers) clone.Modifiers = new List<IDataContainer<T>>(Modifiers);
        return clone;
    }
}