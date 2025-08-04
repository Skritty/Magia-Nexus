using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public interface IPriority
{
    public byte Priority { get; }
}

/// <summary>
/// A modifier with priority tiers. Lower tiers will be ignored.
/// </summary>
[Serializable]
public class PrioritySolver<T> : Solver<T>, IPriority
{
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public byte Priority { get; set; }

    public System.Action AddModifier(T modifier, byte priority)
    {
        PrioritySolver<T> data = new PrioritySolver<T>();
        data._value = modifier;
        data.Priority = priority;
        Modifiers.Add(data);
        changed = true;
        return () => Modifiers.Remove(data);
    }

    public override void Solve()
    {
        if (Modifiers.Count == 0)
        {
            _value = default;
            return;
        }
        byte highestPriority = 0;
        foreach (IDataContainer<T> modifier in Modifiers)
        {
            if (modifier is not IPriority) continue;
            if ((modifier as IPriority).Priority > highestPriority) highestPriority = (modifier as PrioritySolver<T>).Priority;
        }
        List<IDataContainer<T>> priorityModifiers = new List<IDataContainer<T>>();
        foreach (IDataContainer<T> modifier in Modifiers)
        {
            if (modifier is not IPriority)
            {
                if (highestPriority == 0) priorityModifiers.Add(modifier);
                else continue;
            }
            else if ((modifier as IPriority).Priority == highestPriority)
            {
                priorityModifiers.Add(modifier);
            }
        }
        if (priorityModifiers.Count == 0) _value = default;
        _value = HandleSamePriorityModifiers(priorityModifiers);
    }

    protected virtual T HandleSamePriorityModifiers(List<IDataContainer<T>> modifiers)
    {
        return modifiers[0].Value;
    }

    public new PrioritySolver<T> Clone()
    {
        PrioritySolver<T> clone = (PrioritySolver<T>)base.Clone();
        clone.Priority = Priority;
        return clone;
    }
}
