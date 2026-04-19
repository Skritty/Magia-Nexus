using System.Collections.Generic;

public interface IModifier : IDataContainer
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

    public Alignment Alignment { get; }
    public int MaxStacks { get; }
    public int StacksAdded { get; }
    public bool PerPlayer { get; }
    public int TickDuration { get; }
    public bool RefreshDuration { get; }
}

public interface IModifier<T> : IModifier, IDataContainer<T>
{
    public IStat<T> StatTag { get; }

    /// <summary>
    /// Adds this modifier to StatTag's bound instance
    /// </summary>
    public void AddToStat(object boundObject)
    {
        Add(boundObject, boundObject.GetStat(StatTag));
    } 
    public void AddToStat<Stat>(object boundObject) where Stat : IStat<T>
    {
        Add(boundObject, boundObject.GetStat<Stat>());
    }

    private void Add(object boundObject, IStat<T> stat)
    {
        // For each stack being added
        for (int s = 0; s < StacksAdded; s++)
        {
            // Check if at max stacks
            if (MaxStacks > 0 && stat.Contains(this, out int count))
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
            stat.Modifiers.Add(this);
            if (TickDuration > 0)
            {
                // Remove it after the duration expires
                durationModifiers.Add((() => stat.Modifiers.Remove(this), TickDuration));
            }
        }
        switch (Alignment)
        {
            case Alignment.Buff:
                Trigger_BuffGained.Invoke(this, this, stat);
                break;
            case Alignment.Debuff:
                Trigger_DebuffGained.Invoke(this, this, stat);
                break;
        }
        Trigger_ModifierGained.Invoke(this, this, stat);
    }

    /// <summary>
    /// Removes this modifier from StatTag's bound instance
    /// </summary>
    public void RemoveFrom(object boundObject)
    {
        // TODO
        IStat<T> stat = boundObject.GetStat(StatTag);
        stat.Modifiers.Remove(this);
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