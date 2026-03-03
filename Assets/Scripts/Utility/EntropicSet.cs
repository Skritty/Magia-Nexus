using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class EntropicSet<T>
{
    private List<HashSet<T>> entropyLevels = new();
    private Dictionary<T, int> itemEntropyLevels = new();

    public int Count => itemEntropyLevels.Count;
    public T[] AsArray => itemEntropyLevels.Select(x => x.Key).ToArray();

    public void Add(int entropyLevel, T item)
    {
        if (entropyLevel < 0) return;
        if(entropyLevels.Count <= entropyLevel)
        {
            for(int i = entropyLevels.Count; i < entropyLevel + 1; i++)
            {
                entropyLevels.Add(new HashSet<T>());
            }
        }
        entropyLevels[entropyLevel].Add(item);
        itemEntropyLevels.Add(item, entropyLevel);
    }

    public void Update(int entropyLevel, T item)
    {
        if (!itemEntropyLevels.ContainsKey(item)) return;
        if (entropyLevel < 0) return;
        if (entropyLevels.Count <= entropyLevel)
        {
            for (int i = entropyLevels.Count; i < entropyLevel + 1; i++)
            {
                entropyLevels.Add(new HashSet<T>());
            }
        }
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

    public void Clear()
    {
        entropyLevels.Clear();
        itemEntropyLevels.Clear();
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
