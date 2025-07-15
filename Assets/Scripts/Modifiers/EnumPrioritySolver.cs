using System;
using System.Collections.Generic;

public class EnumPrioritySolver<T> : PrioritySolver<T> where T : struct, Enum
{
    protected override T HandleSamePriorityModifiers(List<Stat<T>> modifiers)
    {
        T value = default;
        foreach (Stat<T> modifier in modifiers)
        {
            modifier.Solve();
            return value = (T)Enum.ToObject(typeof(T), Convert.ToInt32(value) | Convert.ToInt32(modifier.Value));
        }
        return value;
    }
}