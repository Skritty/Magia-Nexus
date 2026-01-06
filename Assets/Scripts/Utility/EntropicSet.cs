using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntropicSet<T>
{
    private HashSet<T>[] entropyLevels;
    //private List<T>[] entropyLevelsList;
    private Dictionary<T, int> itemEntropyLevels = new();

    public int Count => itemEntropyLevels.Count;
    public T[] AsArray => itemEntropyLevels.Select(x => x.Key).ToArray();

    public EntropicSet(int maxEntropyLevel)
    {
        entropyLevels = new HashSet<T>[maxEntropyLevel];
        for(int i = 0; i < maxEntropyLevel; i++)
        {
            entropyLevels[i] = new HashSet<T>();
        }
    }

    /// <summary>
    /// Entropy must be > 0
    /// </summary>
    /// <param name="entropyLevel">Entropy must be > 0</param>
    public void Add(int entropyLevel, T item)
    {
        if (entropyLevel < 1) return;
        entropyLevel--;
        
        entropyLevels[entropyLevel].Add(item);
        itemEntropyLevels.Add(item, entropyLevel);
    }

    public void Update(int entropyLevel, T item)
    {
        if (!itemEntropyLevels.ContainsKey(item)) return;
        if (entropyLevel < 1) return;
        entropyLevel--;

        entropyLevels[itemEntropyLevels[item]].Remove(item);
        entropyLevels[entropyLevel].Add(item);
        itemEntropyLevels[item] = entropyLevel;
    }

    public void Remove(T item)
    {
        if (!itemEntropyLevels.ContainsKey(item)) return;
        entropyLevels[itemEntropyLevels[item]].Remove(item);
        itemEntropyLevels.Remove(item);
    }

    public bool Contains(T item) => itemEntropyLevels.ContainsKey(item);

    public HashSet<T> GetSetAtEntropyLevel(int entropyLevel)
    {
        return entropyLevels[entropyLevel];
    }

    public T GetRandomAtEntropyLevel(int entropyLevel)
    {
        T[] items = entropyLevels[entropyLevel].ToArray();
        return items[Random.Range(0,items.Length)];
    }

    public T GetRandomAtLowestEntropy()
    {
        foreach(HashSet<T> entropyLevel in entropyLevels)
        {
            if (entropyLevel.Count == 0) continue;
            return entropyLevel.ToArray()[Random.Range(0, entropyLevel.Count)];
        }
        return default;
    }
}
