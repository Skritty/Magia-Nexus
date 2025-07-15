using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A modifier with priority tiers. Lower tiers will be ignored.
/// </summary>
public abstract class PrioritySolver<T> : Stat<T>
{
    public byte Priority { get; set; }
    public void AddModifier(IModifier modifier, byte priority = 0)
    {
        AddModifier(modifier);
        this.Priority = priority;
    }

    public override void Solve()
    {
        byte highestPriority = 0;
        foreach (PrioritySolver<T> modifier in Modifiers)
        {
            if (modifier == null) continue;
            if (modifier.Priority > highestPriority) highestPriority = modifier.Priority;
        }
        List<Stat<T>> priorityModifiers = new List<Stat<T>>();
        foreach (PrioritySolver<T> modifier in Modifiers)
        {
            if (modifier == null) continue;
            if (modifier.Priority == highestPriority)
            {
                priorityModifiers.Add(modifier);
            }
        }
        if (priorityModifiers.Count == 0) Value = default;
        Value = HandleSamePriorityModifiers(priorityModifiers);
    }

    protected virtual T HandleSamePriorityModifiers(List<Stat<T>> modifiers)
    {
        return modifiers[0].Value;
    }
}
