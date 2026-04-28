using System;
using System.Collections.Generic;

public class EnumPrioritySolver<T> : PrioritySolver<T> where T : struct, Enum
{
    protected override T HandleSamePriorityModifiers(List<IValueContainer<T>> modifiers)
    {
        T value = default;
        foreach (IValueContainer<T> modifier in modifiers)
        {
            modifier.BoundObject = BoundObject;
            return value = (T)Enum.ToObject(typeof(T), Convert.ToInt32(value) | Convert.ToInt32(modifier.Value));
        }
        return value;
    }
}