using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityCommon;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public System.Action cleanup;

    [SerializeReference]
    private ReferenceSerializableHashSet<IStatTag> stats = new(); // TODO: make a backing fancy type dictionary for runtime
    private List<(IModifier, int)> durationModifiers = new();
    [SerializeReference, HideReferenceObjectPicker, ListDrawerSettings(ShowFoldout = false, HideRemoveButton = true)]
    private List<Mechanic> mechanics = new();
    public T GetMechanic<T>() where T : Mechanic => IBoundInstances<Entity, T>.GetInstance(this);

    public T Stat<T>() where T : IStatTag
    {
        foreach (IStatTag stat in stats)
        {
            if (stat is T)
            {
                return (T)stat;
            }
        }
        T s = (T)Activator.CreateInstance(typeof(T));
        stats.Add(s);
        return s;
    }

    public bool HasStat<T>() where T : IStatTag
    {
        foreach (IStatTag stat in stats)
        {
            if (stat is T)
            {
                return true; 
            }
        }
        return false;
        
    }

    public void AddModifier(IModifier modifier)
    {
        // Get stat
        Stat stat;
        if (stats.TryGetValue(modifier.Tag, out IStatTag tag))
        {
            stat = tag as Stat;
        }
        else
        {
            stats.Add(modifier.Tag);
            stat = modifier.Tag as Stat;
        }

        AddModifier(modifier, stat);
    }

    private void AddModifier(IModifier modifier, Stat stat)
    {
        // For each stack being added
        for(int s = 0; s < modifier.StacksAdded; s++)
        {
            // Check if at max stacks
            if (modifier.MaxStacks > 0 && stat.ContainsModifier(modifier, out int count))
            {
                if (modifier.RefreshDuration)
                {
                    for (int i = 0; i < durationModifiers.Count; i++)
                    {
                        var stack = durationModifiers[i];
                        stack.Item2 = 0;
                        durationModifiers[i] = stack;
                    }
                }
                if (count == modifier.MaxStacks) return;
            }

            // Add new stack
            stat.AddModifier(modifier);
            if (modifier.TickDuration > 0)
            {
                durationModifiers.Add((modifier, modifier.TickDuration));
            }
        }
        switch (modifier.Alignment)
        {
            case Alignment.Buff:
                Trigger_BuffGained.Invoke(modifier, modifier, this);
                break;
            case Alignment.Debuff:
                Trigger_DebuffGained.Invoke(modifier, modifier, this);
                break;
        }
        Trigger_ModifierGained.Invoke(modifier, modifier, this);
    }

    public void RemoveModifier(IModifier modifier)
    {
        if (modifier == null) return;
        Stat stat;
        if (stats.TryGetValue(modifier.Tag, out IStatTag tag))
        {
            stat = tag as Stat;
        }
        else
        {
            stats.Add(modifier.Tag);
            stat = modifier.Tag as Stat;
        }
        RemoveModifier(modifier, stat);
    }

    private void RemoveModifier(IModifier modifier, Stat stat)
    {
        if (!stat.ContainsModifier(modifier, out _)) return;
        stat.RemoveModifier(modifier);
        foreach ((IModifier, int) mod in durationModifiers.ToArray())
        {
            if(mod.Item1 == modifier)
            {
                durationModifiers.Remove(mod);
                break;
            }
        }
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
            if(mod.Item1.Alignment == alignment)
            {
                modifier = durationModifiers[0].Item1;
                break;
            }
        }
        RemoveModifier(modifier);
    }

    public void AddModifier<STAT>(IDataContainer modifier) where STAT : IStatTag
    {
        Stat stat = null;
        foreach (IStatTag s in stats)
        {
            if(s.GetType() == typeof(STAT))
            {
                stat = (Stat)s;
            }
        }
        if(stat == null)
        {
            stat = default;
            stats.Add((IStatTag)stat);
        }
        if(modifier is IModifier)
        {
            AddModifier((IModifier)modifier, stat);
        }
        else
        {
            stat.AddModifier(modifier);
        }
    }

    public void AddModifier<STAT, T>(T value, int duration) where STAT : IStatTag
    {
        Stat stat = null;
        foreach (IStatTag s in stats)
        {
            if (s.GetType() == typeof(STAT))
            {
                stat = (Stat)s;
            }
        }
        if (stat == null)
        {
            stat = default;
            stats.Add((IStatTag)stat);
        }

        AddModifier(new DummyModifier<T>(value, tickDuration: duration), stat);
    }

    public void RemoveModifier<STAT>(IDataContainer modifier) where STAT : IStatTag
    {
        Stat stat = null;
        foreach (IStatTag s in stats)
        {
            if (s.GetType() == typeof(STAT))
            {
                stat = (Stat)s;
            }
        }
        if (stat == null) return;
        if (modifier is IModifier)
        {
            RemoveModifier((IModifier)modifier, stat);
        }
        else
        {
            stat.RemoveModifier(modifier);
        }
    }

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        foreach (Mechanic stat in mechanics)
        {
            stat.AddInstance(this);
        }
    }

    private void FixedUpdate()
    {
        TickStats();
        TickMechanics();
    }

    private void TickMechanics()
    {
        foreach (Mechanic stat in mechanics)
        {
            stat.Tick();
        }
    }

    private void TickStats()
    {
        for (int i = 0; i < durationModifiers.Count; i++)
        {
            var stack = durationModifiers[i];

            if(stack.Item2 == 1)
            {
                RemoveModifier(stack.Item1);
                durationModifiers.RemoveAt(i);
                i--;
                continue;
            }

            stack.Item2--;
            durationModifiers[i] = stack;
        }
    }

    private void OnDestroy()
    {
        foreach (Mechanic stat in mechanics)
        {
            stat.OnDestroy();
            stat.RemoveInstance(this);
        }

        cleanup?.Invoke();
    }

    private void OnValidate()
    {
        foreach (Mechanic stat in mechanics)
        {
            if (stat == null) continue;
            stat.Owner = this;
        }
    }
}

// Entity: a thing that can be interacted with
// - Stats
// - Mechanics

// Stat: A value linked to a reference class that can be modified and solved

// Mechanic: Functionality that ticks on the owner and can be referenced

// Effect: Modifiable container class with functionality that happens instantaneously

// Trigger: A callback with stored data