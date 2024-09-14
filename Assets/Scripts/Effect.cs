using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public abstract class Effect
{
    [SerializeReference]
    public Targeting targetSelector = new Targeting_Self();
    /// <summary>
    /// Ability that applied this effect (to prevent duplicates)
    /// </summary>
    [HideInInspector]
    public object source;
    [BoxGroup("Effect", showLabel: false), PropertyOrder(-3)]
    public int sourceID => source == null ? 0 : source.GetHashCode();

    /// <summary>
    /// Entity that applied the effect
    /// </summary>
    [BoxGroup("Effect", showLabel: false), PropertyOrder(-2)]
    public Entity owner;

    /// <summary>
    /// Entity that is recieving the effect
    /// </summary>
    [BoxGroup("Effect", showLabel: false), PropertyOrder(-1)]
    public Entity target;

    /// <summary>
    /// Multiplies the output of the effect
    /// </summary>
    [HideInInspector]
    public float effectMultiplier = 1;

    public void Create(object source, Entity owner)
    {
        foreach (Entity target in targetSelector.GetTargets(source, owner))
        {
            Effect e = Clone<Effect>();
            e.source = source;
            e.owner = owner;
            e.target = target;
            e.Activate();
        }
    }
    public void Create(object source, Entity owner, Trigger trigger)
    {
        foreach (Entity target in targetSelector.GetTargets(source, trigger, owner))
        {
            Effect e = Clone<Effect>();
            e.source = source;
            e.owner = owner;
            e.target = target;
            e.Activate(trigger);
        }
    }
    public void Create(Effect inherit)
    {
        Effect e = Clone<Effect>();
        e.source = inherit.source;
        e.owner = inherit.owner;
        e.target = inherit.target;
        e.Activate();
    }
    public virtual T Clone<T>() where T : Effect
    {
        return (T)MemberwiseClone();
    }
    
    public abstract void Activate();
    public virtual void Activate(Trigger trigger) 
    {
        Activate();
    }
}