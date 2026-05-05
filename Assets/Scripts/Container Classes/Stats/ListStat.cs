using System.Collections.Generic;

public class ListStat<T> : CollectionContainer<T>
{
    public T this[int index]
    {
        get
        {
            if (Modifiers.Count == 0) return default;
            return Modifiers[index].Value;
        }
    }

    public System.Action Add(T value)
    {
        ValueContainer<T> modifier = new ValueContainer<T>(value);
        AddModifier(modifier);
        return () => RemoveModifier(modifier);
    }

    public void Remove(T value)
    {
        foreach (IValueContainer<T> modifier in Modifiers.ToArray())
        {
            if (modifier.Value.Equals(value)) RemoveModifier(modifier);
        }
    }

    public System.Action AddRange(params T[] values) => AddRange(values);
    public System.Action AddRange(IEnumerable<T> values)
    {
        System.Action cleanup = null;
        foreach(T value in values)
        {
            cleanup += Add(value);
        }
        return cleanup;
    }

    public bool Contains(T value)
    {
        foreach (IValueContainer<T> modifier in Modifiers)
        {
            if (modifier.Value.Equals(value)) return true;
        }
        return false;
    }

    public int IndexOf(T value)
    {
        int i = 0;
        foreach (IValueContainer<T> modifier in Modifiers)
        {
            if (modifier.Value.Equals(value)) return i;
            i++;
        }
        return -1;
    }
}