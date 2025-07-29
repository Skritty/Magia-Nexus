using System.Collections;
using System.Collections.Generic;

public class QueueStat<T> : Stat<T>, IEnumerable<T>
{
    public int Count => Modifiers.Count;

    public bool Contains(T value)
    {
        return Modifiers.Exists(x => x.Value.Equals(value));
    }

    public void Enqueue(T value)
    {
        AddModifier(value);
    }

    public T Dequeue()
    {
        if (Count == 0) return default;
        T value = Modifiers[0].Value;
        Modifiers.RemoveAt(0);
        return value;
    }

    public void Clear()
    {
        Modifiers.Clear();
        changed = true;
        OnChange?.Invoke(Value);
    }

    public void AddModifiers(ICollection<T> values)
    {
        foreach (T value in values)
        {
            AddModifier(value);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        List<T> array = new();
        foreach (IDataContainer<T> data in Modifiers)
        {
            array.Add(data.Value);
        }
        return array.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}