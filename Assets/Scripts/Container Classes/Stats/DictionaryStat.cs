using System.Collections.Generic;

public class DictionaryStat<Key, Value> : CollectionContainer<(Key key, Value value)>
{
    private Dictionary<Key, Value> dictionary = new();
    public Value this[Key key]
    {
        get
        {
            if (Count == 0 || !dictionary.ContainsKey(key)) return default;
            return dictionary[key];
        }
        set
        {
            if(dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else Add(key, value);
        }
    }

    public System.Action Add(Key key, Value value)
    {
        var modifier = new ValueContainer<(Key key, Value value)>((key, value));
        Add(modifier);
        return () => Remove(modifier);
    }

    public override void Add(IValueContainer<(Key key, Value value)> modifier)
    {
        modifier.AddTo(this);
        dictionary.Add(modifier.Value.key, modifier.Value.value);
    }

    public void Remove(Key key)
    {
        foreach (IValueContainer<(Key key, Value value)> keyValuePair in Modifiers.ToArray())
        {
            if (keyValuePair.Value.key.Equals(key))
            {
                Remove(keyValuePair);
            }
        }
    }

    public override void Remove(IValueContainer<(Key key, Value value)> modifier)
    {
        modifier.RemoveFrom(this);
        dictionary.Remove(modifier.Value.key);
    }
}