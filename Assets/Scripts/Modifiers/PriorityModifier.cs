using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A modifier with priority tiers. Lower tiers will be ignored.
/// </summary>
public abstract class PriorityModifier<T> : Modifier<T>
{
    public byte priority;
    public void AddModifier(Modifier<T> modifier, byte priority = 0)
    {
        AddModifier(modifier);
        this.priority = priority;
    }

    public override void Solve()
    {
        byte highestPriority = 0;
        foreach (PriorityModifier<T> modifier in modifiers)
        {
            if (modifier == null) continue;
            if (modifier.priority > highestPriority) highestPriority = modifier.priority;
        }
        List<PriorityModifier<T>> priorityModifiers = new List<PriorityModifier<T>>();
        foreach (PriorityModifier<T> modifier in modifiers)
        {
            if (modifier == null) continue;
            if (modifier.priority == highestPriority)
            {
                priorityModifiers.Add(modifier);
            }
        }
        Value = HandleSamePriorityModifiers(priorityModifiers);
    }

    protected virtual T HandleSamePriorityModifiers(List<PriorityModifier<T>> modifiers)
    {
        foreach(PriorityModifier<T> modifier in modifiers)
        {
            return modifier.Value;
        }
        return default;
    }
}
