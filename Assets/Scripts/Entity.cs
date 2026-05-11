using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public System.Action cleanup;

    [SerializeReference]
    private List<IValueContainer> stats = new();
    [SerializeReference]
    private List<IValueContainer<Trigger>> defaultTriggers = new();
    [SerializeReference, HideReferenceObjectPicker, ListDrawerSettings(ShowFoldout = false, HideRemoveButton = true)]
    private List<Mechanic> mechanics = new();

    private void Awake()
    {
        foreach(IValueContainer stat in stats)
        {
            this.AddStat(stat);
        }
        foreach (Mechanic mechanic in mechanics)
        {
            mechanic.AddInstance(this);
        }
        foreach (IValueContainer<Trigger> trigger in defaultTriggers)
        {
            trigger.Value.SubscribeToTasks(this, this);
            this.GetStat<Stat_Triggers>().AddModifier(trigger);
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
        TickMechanics();
    }

    private void TickMechanics()
    {
        foreach (Mechanic stat in mechanics)
        {
            stat.Tick();
        }
    }

    private void OnDestroy()
    {
        foreach (Mechanic stat in mechanics)
        {
            stat.OnDestroy();
            stat.Remove(this);
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