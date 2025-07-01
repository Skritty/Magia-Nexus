using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using System;

public class Entity : MonoBehaviour
{
    [SerializeReference]
    public List<Stat> stats = new List<Stat>();
    public System.Action cleanup;
    [SerializeReference, HideReferenceObjectPicker, ListDrawerSettings(ShowFoldout = false, HideRemoveButton = true)]
    private List<Mechanic> baseStats = new List<Mechanic>();
    public T GetMechanic<T>() where T : Mechanic => IBoundInstances<Entity, T>.GetInstance(this);

    public T Stat<T>() where T: IStatTag
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

    public void AddModifier<T>(IModifier<T> modifier)
    {
        foreach (Stat stat in stats)
        {
            if (modifier.Tag != stat) continue;
            stat.AddModifier((Stat)modifier);
        }
    }

    public void RemoveModifier<T>(IModifier<T> modifier)
    {
        foreach (Stat stat in stats)
        {
            if (modifier.Tag != stat) continue;
            stat.AddModifier((Stat)modifier);
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
        HandleStats();
    }

    private void HandleStats()
    {
        foreach (Mechanic stat in baseStats)
        {
            stat.Tick();
        }
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