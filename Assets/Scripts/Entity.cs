using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public System.Action cleanup;

    [SerializeReference]
    private List<IStat> stats = new(); // TODO: make a backing fancy type dictionary for runtime
    private List<(IModifier, int)> durationModifiers = new();
    [SerializeReference, HideReferenceObjectPicker, ListDrawerSettings(ShowFoldout = false, HideRemoveButton = true)]
    private List<Mechanic> mechanics = new();
    public T GetMechanic<T>() where T : Mechanic => IBoundInstances<Entity, T>.GetInstance(this, false);

    public T Stat<T>() where T : IStat
    {
        foreach (IStat stat in stats)
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

    public IStat Stat(IStat tag)
    {
        foreach (IStat s in stats)
        {
            if (s.GetType() == tag.GetType())
            {
                return s;
            }
        }
        IStat newStat = (IStat)Activator.CreateInstance(tag.GetType());
        stats.Add(newStat);
        return newStat;
    }

    public bool HasStat<T>() where T : IStat
    {
        foreach (IStat stat in stats)
        {
            if (stat is T)
            {
                return true; 
            }
        }
        return false;
        
    }

    public void TryAddModifier(IModifier modifier, int overrideTickDuration = -1)
    {
        AddModifier(modifier, Stat(modifier.Tag), overrideTickDuration);
    }

    public void AddModifier<T>(IModifier<T> modifier, int overrideTickDuration = -1) => AddModifier(modifier, Stat(modifier.Tag), overrideTickDuration);

    public void AddModifier<T, StatTag>(IDataContainer<T> modifier, int overrideTickDuration = -1) where StatTag : IStat<T>
    {
        if (modifier is IModifier<T>)
        {
            AddModifier((IModifier<T>)modifier, Stat<StatTag>(), overrideTickDuration);
        }
        else
        {
            Stat<StatTag>().Add(modifier);
        }
    }

    public void AddModifier<T, StatTag>(T value, int duration) where StatTag : class, IStat<T>
    {
        IStat<T> tag = Stat<StatTag>();
        AddModifier(new Modifier<T>(value, tag, tickDuration: duration), tag);
    }

    private void AddModifier(IModifier modifier, IStat stat, int overrideTickDuration = -1)
    {
        if (modifier == null || stat == null) return;
        // For each stack being added
        for(int s = 0; s < modifier.StacksAdded; s++)
        {
            // Check if at max stacks
            if (modifier.MaxStacks > 0 && stat.Contains(modifier, out int count))
            {
                if (modifier.RefreshDuration)
                {
                    for (int i = 0; i < durationModifiers.Count; i++)
                    {
                        var stack = durationModifiers[i];
                        stack.Item2 = modifier.TickDuration;
                        durationModifiers[i] = stack;
                    }
                }
                if (count >= modifier.MaxStacks) return;
            }

            // Add new stack
            if(!stat.TryAdd(modifier)) return;
            if(overrideTickDuration > 0)
            {
                durationModifiers.Add((modifier, overrideTickDuration));
            }
            else if (modifier.TickDuration > 0)
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
        foreach(IStat stat in stats)
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
            if(mod.Item1.Alignment == alignment)
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
    }

    private void Awake()
    {
        foreach (Mechanic mechanic in mechanics)
        {
            mechanic.AddInstance(this);
        }
        foreach (IDataContainer<Trigger> trigger in Stat<Stat_Triggers>().Modifiers)
        {
            trigger.Value.SubscribeToTasks(this, this);
            IModifier durationTrigger = trigger as IModifier;
            if (durationTrigger != null && durationTrigger.TickDuration > 0)
            {
                durationModifiers.Add((durationTrigger, durationTrigger.TickDuration));
            }
        }
    }

    private void Start()
    {
        foreach (Mechanic mechanic in mechanics)
        {
            mechanic.Initialize();
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