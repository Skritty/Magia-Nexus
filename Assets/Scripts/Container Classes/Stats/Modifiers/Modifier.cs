using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class Modifier : IValueContainer
{
    public static List<(System.Action removeAction, int tick)> durationModifiers = new(); // Worry about memory leaks
    public static void Tick()
    {
        for (int i = 0; i < durationModifiers.Count; i++)
        {
            var stack = durationModifiers[i];

            if (stack.tick == 1)
            {
                stack.removeAction?.Invoke();
                durationModifiers.RemoveAt(i);
                i--;
                continue;
            }

            stack.tick--;
            durationModifiers[i] = stack;
        }
    }

    public static void ClearDurationModifiers()
    {
        durationModifiers.Clear();
    }

    public Alignment Alignment { get; protected set; }
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public int MaxStacks { get; protected set; }
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public int StacksAdded { get; protected set; } = 1;
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public bool PerPlayer { get; protected set; }
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public int TickDuration { get; protected set; }
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public bool RefreshDuration { get; protected set; }
    public virtual IModifiable Tag { get; protected set; }

    public virtual bool IsDefaultValue() => true;
    public bool TryGet<T>(out T data)
    {
        IValueContainer<T> container = (IValueContainer<T>)this;
        if (container == null) data = default;
        else data = container.Value;
        return container != null;
    }

    public abstract void AddToStatTag(object boundObject);
    public abstract void RemoveFromStatTag(object boundObject);
}

[Serializable]
public class Modifier<T> : Modifier, IValueContainer<T>
{
    [field: SerializeReference, FoldoutGroup("@GetType()")]
    public IModifiable<T> StatTag { get; set; }
    [field: SerializeReference, FoldoutGroup("@GetType()")]
    public virtual T Value { get; set; }
    public List<IValueContainer<T>> Modifiers => null;
    

    public Modifier() { }

    public Modifier(T value = default, IModifiable<T> tag = default, Alignment alignment = Alignment.Neutral,
        int maxStacks = 0, int stacksAdded = 1, bool perPlayer = false,
        int tickDuration = 0, bool refreshDuration = false)
    {
        Value = value;
        StatTag = tag;
        Alignment = alignment;
        MaxStacks = maxStacks;
        StacksAdded = stacksAdded;
        PerPlayer = perPlayer;
        TickDuration = tickDuration;
        RefreshDuration = refreshDuration;
    }

    public override bool IsDefaultValue() => Value.Equals(default(T));

    /// <summary>
    /// Adds this modifier to StatTag's bound instance
    /// </summary>
    public override void AddToStatTag(object boundObject)
    {
        boundObject.GetStat(StatTag).Add(this);
    }

    public void AddTo(IModifiable<T> modifiable)
    {
        // For each stack being added
        for (int s = 0; s < StacksAdded; s++)
        {
            // Check if at max stacks
            if (MaxStacks > 0 && modifiable.Contains(this, out int count))
            {
                if (RefreshDuration)
                {
                    for (int i = 0; i < durationModifiers.Count; i++)
                    {
                        var stack = durationModifiers[i];
                        stack.Item2 = TickDuration;
                        durationModifiers[i] = stack;
                    }
                }
                if (count >= MaxStacks) return;
            }

            // Add new stack
            modifiable.Modifiers.Add(this);
            if (TickDuration > 0)
            {
                // Remove it after the duration expires
                durationModifiers.Add((() => modifiable.Remove(this), TickDuration));
            }
        }
        switch (Alignment)
        {
            case Alignment.Buff:
                Trigger_BuffGained.Invoke(this, this, modifiable);
                break;
            case Alignment.Debuff:
                Trigger_DebuffGained.Invoke(this, this, modifiable);
                break;
        }
        Trigger_ModifierGained.Invoke(this, this, modifiable);
    }

    /// <summary>
    /// Removes this modifier from StatTag's bound instance
    /// </summary>
    public override void RemoveFromStatTag(object boundObject)
    {
        RemoveFrom(boundObject.GetStat(StatTag));
    }

    public void RemoveFrom(IModifiable<T> modifiable)
    {
        // TODO
        modifiable.Modifiers.Remove(this);
    }

    /*public void RemoveModifier(IModifier modifier)
    {
        if (modifier == null) return;
        foreach (IStat stat in stats)
        {
            RemoveModifier(modifier, stat);
        }
    }

    private void RemoveModifier(IModifier modifier, IStat stat)
    {
        if (!stat.Contains(modifier, out _)) return;
        stat.Remove(modifier);
        switch (modifier.Alignment)
        {
            case Alignment.Buff:
                Trigger_BuffLost.Invoke(modifier, modifier, this);
                break;
            case Alignment.Debuff:
                Trigger_DebuffLost.Invoke(modifier, modifier, this);
                break;
        }
        Trigger_ModifierLost.Invoke(modifier, modifier, this);
    }

    public void RemoveOldestDurationModifier(Alignment alignment)
    {
        if (durationModifiers.Count == 0) return;
        IModifier modifier = null;
        foreach ((IModifier, int) mod in durationModifiers)
        {
            if (mod.Item1.Alignment == alignment)
            {
                modifier = durationModifiers[0].Item1;
                break;
            }
        }
        RemoveModifier(modifier);
    }

    public void RemoveModifier<StatTag>(IDataContainer modifier) where StatTag : IStat
    {
        if (modifier is IModifier)
        {
            RemoveModifier((IModifier)modifier, Stat<StatTag>());
        }
        else
        {
            Stat<StatTag>().Remove(modifier);
        }
    }*/
}