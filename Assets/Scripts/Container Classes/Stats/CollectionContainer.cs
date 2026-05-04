using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CollectionContainer<T> : IModifiable<T>, IEnumerable<T>
{
    [field: SerializeReference, PropertyOrder(1), FoldoutGroup("@GetType()")]
    public List<IValueContainer<T>> Modifiers { get; set; } = new();
    public int Count => Modifiers.Count;
    public virtual List<T> ToList
    {
        get
        {
            List<T> list = new List<T>();
            foreach (IValueContainer<T> data in Modifiers)
            {
                list.Add(data.Value);
            }
            return list;
        }
    }
    public virtual void AddModifier(IValueContainer<T> modifier) => modifier.AddTo(this);
    public virtual void RemoveModifier(IValueContainer<T> modifier) => modifier.RemoveFrom(this);

    public virtual bool Contains(IValueContainer modifier, out int count)
    {
        count = 0;
        foreach (IValueContainer m in Modifiers)
        {
            if (m.Equals(modifier)) count++;
        }
        if (count > 0)
        {
            return true;
        }
        return false;
    }

    public virtual bool Contains(T value, out int count)
    {
        count = 0;
        foreach (IValueContainer<T> m in Modifiers)
        {
            if (m.Value.Equals(value)) count++;
        }
        if (count > 0)
        {
            return true;
        }
        return false;
    }

    public virtual void Clear()
    {
        Modifiers.Clear();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return ToList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
