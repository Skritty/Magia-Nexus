using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tally<T>
{
    Dictionary<T, int> tally = new Dictionary<T, int>();

    public void Add(T thing)
    {
        if(tally.TryAdd(thing, 1))
            tally[thing] += 1;
    }

    public void Remove(T thing)
    {
        if (tally.ContainsKey(thing))
        {
            tally[thing] -= 1;
            if (tally[thing] <= 0) tally.Remove(thing);
        }
    }

    public List<T> GetHighest(out int amount)
    {
        amount = int.MinValue;
        List<T> highest = new List<T>();
        foreach (KeyValuePair<T, int> tal in tally)
        {
            if(tal.Value > amount) amount = tal.Value;
        }
        foreach (KeyValuePair<T, int> tal in tally)
        {
            if (tal.Value == amount) highest.Add(tal.Key);
        }
        return highest;
    }

    public List<T> GetLowest(out int amount)
    {
        amount = int.MaxValue;
        List<T> highest = new List<T>();
        foreach (KeyValuePair<T, int> tal in tally)
        {
            if (tal.Value < amount) amount = tal.Value;
        }
        foreach (KeyValuePair<T, int> tal in tally)
        {
            if (tal.Value == amount) highest.Add(tal.Key);
        }
        return highest;
    }

    public List<T> GetMatchingAmounts(int amount)
    {
        List<T> highest = new List<T>();
        foreach (KeyValuePair<T, int> tal in tally)
        {
            if (tal.Value == amount) highest.Add(tal.Key);
        }
        return highest;
    }
}
