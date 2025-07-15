using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityCommon;

public class Entity : MonoBehaviour
{
    public System.Action cleanup;

    [SerializeReference]
    private ReferenceSerializableHashSet<IStatTag> stats = new();
    [SerializeReference, HideReferenceObjectPicker, ListDrawerSettings(ShowFoldout = false, HideRemoveButton = true)]
    private List<Mechanic> baseStats = new();
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
        return default;
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
        if (!stats.Contains(modifier.Tag)) stats.Add(modifier.Tag);
        (modifier.Tag as Stat).AddModifier(modifier);
        // TODO: precalculate modifiers
    }

    public void RemoveModifier(IModifier modifier)
    {
        foreach (Stat stat in stats)
        {
            if (modifier.Tag != stat) continue;
            stat.AddModifier(modifier);
        }
    }

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        foreach (Mechanic stat in baseStats)
        {
            stat.AddInstance(this);
        }
    }

    private void FixedUpdate()
    {
        TickMechanics();
        TickStats();
    }

    private void TickMechanics()
    {
        foreach (Mechanic stat in baseStats)
        {
            stat.Tick();
        }
    }

    private void TickStats()
    {

    }

    private void OnDestroy()
    {
        foreach (Mechanic stat in baseStats)
        {
            stat.OnDestroy();
            stat.RemoveInstance(this);
        }

        cleanup?.Invoke();
    }

    private void OnValidate()
    {
        foreach (Mechanic stat in baseStats)
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