using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// A modifier with priority tiers. Lower tiers will be ignored.
/// </summary>
public class PrioritySolver<T> : Stat<T>
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
        OnChange?.Invoke(Value);
        return () => Modifiers.Remove(data);
    }

    public override void Solve()
    {
        byte highestPriority = 0;
        foreach (PrioritySolver<T> modifier in Modifiers)
        {
            if (modifier == null) continue;
            if (modifier.Priority > highestPriority) highestPriority = modifier.Priority;
        }
        List<IDataContainer<T>> priorityModifiers = new List<IDataContainer<T>>();
        foreach (IDataContainer<T> modifier in Modifiers)
        {
            if (modifier is not PrioritySolver<T>)
            {
                if (highestPriority == 0) priorityModifiers.Add(modifier);
                else continue;
            }
            if ((modifier as PrioritySolver<T>).Priority == highestPriority)
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

    public override Stat Clone()
    {
        PrioritySolver<T> clone = (PrioritySolver<T>)base.Clone();
        clone.Priority = Priority;
        return clone;
    }
}
