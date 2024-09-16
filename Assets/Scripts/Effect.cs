using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public abstract class Effect
{
    /// <summary>
    /// Ability that applied this effect (to prevent duplicates)
    /// </summary>
    public object Source => _source;
    private object _source;

    /// <summary>
    /// Entity that applied the effect
    /// </summary>
    public Entity Owner => _owner;
    private Entity _owner;

    /// <summary>
    /// Entity that is recieving the effect
    /// </summary>
    public Entity Target => _target;
    private Entity _target;

    /// <summary>
    /// Multiplies the output of the effect, multiplied through chains of effects
    /// </summary>
    [HideInInspector]
    public float EffectMultiplier = 1;

    [SerializeReference]
    public Targeting targetSelector = new Targeting_Self();

    /// <summary>
    /// Set the effect info without creating a clone
    /// </summary>
    public void SetInfo(object source, Entity owner, Entity target)
    {
        _source = source;
        _owner = owner;
        _target = target;
    }

    /// <summary>
    /// Activates a clone of the effect to targets returned by the targetSelector
    /// </summary>
    public void Create(object source, Entity owner)
    {
        foreach (Entity target in targetSelector.GetTargets(source, owner))
        {
            Effect e = Clone();
            e._source = source;
            e._owner = owner;
            e._target = target;
            e.Activate();
        }
    }

    /// <summary>
    /// Activates a clone of the effect with manually input source, owner, and target
    /// </summary>
    public void Create(object source, Entity owner, Entity target)
    {
        Effect e = Clone();
        e._source = source;
        e._owner = owner;
        e._target = target;
        e.Activate();
    }

    /// <summary>
    /// Activates a clone of the effect to targets returned by the targetSelector, which is given the trigger
    /// </summary>
    public void Create(object source, Entity owner, Trigger trigger)
    {
        foreach (Entity target in targetSelector.GetTargets(source, trigger, owner))
        {
            Effect e = Clone();
            e._source = source;
            e._owner = owner;
            e._target = target;
            e.Activate(trigger);
        }
    }

    /// <summary>
    /// Inherits the source, ownership, and target of an effect before activating
    /// </summary>
    public void Create(Effect inherit)
    {
        Effect e = Clone();
        e._source = inherit.Source;
        e._owner = inherit.Owner;
        e._target = inherit.Target;
        e.Activate();
    }

    private Effect Clone()
    {
        return (Effect)MemberwiseClone();
    }
    
    public abstract void Activate();
    public virtual void Activate(Trigger trigger) 
    {
        Activate();
    }
}