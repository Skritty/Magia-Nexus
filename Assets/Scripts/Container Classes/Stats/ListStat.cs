using System.Collections.Generic;
using System.Collections;

public class ListStat<T> : Stat<T>, IEnumerable<T>
{
    public int Count => Modifiers.Count;
    public T this[int i]
    {
        get
        {
            if (Modifiers.Count >= i) return default;
            return Modifiers[i].Value;
        }
    }

    public T[] ToArray
    {
        get
        {
            T[] array = new T[Modifiers.Count];
            for (int i = 0; i < Modifiers.Count; i++)
            {
                array[i] = Modifiers[i].Value;
            }
            return array;
        }
    }

    public bool Contains(T value)
    {
        return Modifiers.Exists(x => x.Value.Equals(value));
    }

    public void Remove(T value)
    {
        foreach (IDataContainer<T> data in ToArray)
        {
            if (data is DataContainer && value.Equals(data.Value))
            {
                Modifiers.Remove(data);
                return;
            }
        }
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