/*using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


[Serializable]
public abstract class Effect
{
    /// <summary>
    /// Ability that applied this effect (to prevent duplicates)
    /// </summary>
    protected int _sourceID;
    [FoldoutGroup("@GetType()")]//, ShowInInspector, ReadOnly]
    protected int SourceID
    {
        get
        {
            if (_sourceID == 0) _sourceID = GetUID();
            return _sourceID;
        }
    }
    public Effect Source => _source;
    protected Effect _source;

    /// <summary>
    /// Entity that applied the effect
    /// </summary>
    public Entity Owner
    {
        get => _owner;
        set => _owner = value;
    }
    protected Entity _owner;

    /// <summary>
    /// Entity that is recieving the effect
    /// </summary>
    public Entity Target => _target;
    protected Entity _target;

    /// <summary>
    /// Multiplies the output of the effect, multiplied through chains of effects
    /// </summary>
    [FoldoutGroup("@GetType()")]
    public float effectMultiplier = 1;

    [FoldoutGroup("@GetType()")]
    public int ignoreFrames;

    [SerializeReference, FoldoutGroup("@GetType()")]
    public Targeting targetSelector = new Targeting_Self();

    public float GetMultiplier(EffectTag tags)
    {
        return Owner.GetMechanic<Stat_PlayerOwner>().playerEntity.GetMechanic<Stat_EffectModifiers>().CalculateModifier(tags);
    }

    /// <summary>
    /// Set the effect info without creating a clone
    /// </summary>
    public void SetInfo(Effect source, Entity owner, Entity target)
    {
        _source = source;
        _owner = owner;
        _target = target;
    }

    /// <summary>
    /// Activates a clone of the effect to targets returned by the targetSelector
    /// </summary>
    public void Create(Entity owner, float multiplier = 1)
    {
        foreach (Entity target in targetSelector.GetTargets(this, owner))
        {
            Effect e = Clone();
            e._source = this;
            e._owner = owner;
            e._target = target;
            e.effectMultiplier *= multiplier;
            e.DoEffect();
        }
    }

    /// <summary>
    /// Activates a clone of the effect to targets returned by the targetSelector
    /// </summary>
    public void Create(Entity owner, Entity proxy, float multiplier = 1)
    {
        foreach (Entity target in targetSelector.GetTargets(this, owner, proxy))
        {
            Effect e = Clone();
            e._source = this;
            e._owner = owner;
            e._target = target;
            e.effectMultiplier *= multiplier;
            e.DoEffect();
        }
    }

    /// <summary>
    /// Activates a clone of the effect to targets returned by the targetSelector, which is given the trigger
    /// </summary>
    public void CreateFromTrigger(Entity owner, Effect effect, Entity proxy = null)
    {
        foreach (Entity target in targetSelector.GetTargets(this, effect, owner, proxy))
        {
            Effect e = Clone();
            e._source = this;
            e._owner = owner;
            e._target = target;
            e.effectMultiplier *= effect.effectMultiplier;

            e.DoEffect();
        }
    }

    /// <summary>
    /// Activates a clone of the effect with manually input source, owner, and target
    /// </summary>
    public void Create(Effect source, Entity owner, Entity target, float multiplier = 1)
    {
        Effect e = Clone();
        e._source = source;
        e._owner = owner;
        e._target = target;
        e.effectMultiplier *= multiplier;
        e.DoEffect();
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
        e.effectMultiplier *= inherit.effectMultiplier;
        e.DoEffect();
    }

    public virtual Effect Clone()
    {
        Effect clone = (Effect)MemberwiseClone();
        clone.targetSelector = clone.targetSelector.Clone();
        return clone;
    }
    
    public void DoEffect(float multiplier, Entity target)
    {
        if (ignoreFrames > 0)
            new PE_IgnoreEntity(this, ignoreFrames);
        Activate();
    }

    public abstract void Activate();

    public int GetUID()
    {
        return GetHashCode();
    }
}

[Serializable]
public struct EffectTagContainer
{
    public EffectTag tag;
    public float value;

    public EffectTagContainer(EffectTag tag, float value)
    {
        this.tag = tag;
        this.value = value;
    }
}*/