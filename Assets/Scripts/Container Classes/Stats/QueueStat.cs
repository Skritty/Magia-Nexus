using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class QueueStat<T> : IModifiable<T>, IEnumerable<T>
{
    [field: SerializeReference, PropertyOrder(1), FoldoutGroup("@GetType()"), ReadOnly]
    public List<IDataContainer<T>> Modifiers { get; set; } = new();
    public int Count => Modifiers.Count;

    public IEnumerator<T> GetEnumerator()
    {
        Queue<T> queue = new Queue<T>();
        foreach (IDataContainer<T> data in Modifiers)
        {
            queue.Enqueue(data.Value);
        }
        return queue.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enqueue(T value)
    {
        DataContainer<T> data = new DataContainer<T>(value);
        Modifiers.Add(data);
    }

    public T Dequeue()
    {
        if (Modifiers.Count == 0) return default; 
        IDataContainer<T> data = Modifiers[0];
        Modifiers.RemoveAt(0);
        return data.Value;
    }

    public bool TryAdd(IDataContainer modifier)
    {
        IDataContainer<T> cast = (IDataContainer<T>)modifier;
        if (cast == null) return false;
        Modifiers.Add(cast);
        return true;
    }
    public void Add(IDataContainer<T> modifier)
    {
        Modifiers.Add(modifier);
    }

    public void Remove(IDataContainer modifier)
    {
        Modifiers.Remove(modifier as IDataContainer<T>);
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