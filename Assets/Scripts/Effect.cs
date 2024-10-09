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
    public Effect Source => _source;
    private Effect _source;

    /// <summary>
    /// Entity that applied the effect
    /// </summary>
    public Entity Owner
    {
        get => _owner;
        set => _owner = value;
    }
    private Entity _owner;

    /// <summary>
    /// Entity that is recieving the effect
    /// </summary>
    public Entity Target => _target;
    private Entity _target;

    /// <summary>
    /// Multiplies the output of the effect, multiplied through chains of effects
    /// </summary>
    [FoldoutGroup("@GetType()")]
    public float effectMultiplier = 1;
    [FoldoutGroup("@GetType()")]
    public SerializedDictionary<EffectTag, float> effectTags = new SerializedDictionary<EffectTag, float>() { {EffectTag.None, 1f} };
    
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Targeting targetSelector = new Targeting_Self();

    public float GetMultiplier()
    {
        float multiplier = 0;
        foreach (KeyValuePair<EffectTag, float> tag in effectTags)
        {
            multiplier += Owner.Stat<Stat_PlayerOwner>().playerEntity.Stat<Stat_EffectModifiers>().CalculateModifier(tag.Key);
        }
        return multiplier;
    }

    public float GetMultiplier(EffectTag tags)
    {
        return Owner.Stat<Stat_PlayerOwner>().playerEntity.Stat<Stat_EffectModifiers>().CalculateModifier(tags);
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
    public void Create(Entity owner)
    {
        foreach (Entity target in targetSelector.GetTargets(this, owner))
        {
            Effect e = Clone();
            e._source = this;
            e._owner = owner;
            e._target = target;
            e.DoEffect();
        }
    }

    /// <summary>
    /// Activates a clone of the effect to targets returned by the targetSelector, which is given the trigger
    /// </summary>
    public void Create(Entity owner, Trigger trigger, Effect triggeringEffect)
    {
        foreach (Entity target in targetSelector.GetTargets(this, trigger, owner))
        {
            Effect e = Clone();
            e._source = this;
            e._owner = owner;
            e._target = target;
            if(triggeringEffect != null)
                e.effectMultiplier *= triggeringEffect.effectMultiplier;
            e.DoEffect();
        }
    }

    /// <summary>
    /// Activates a clone of the effect with manually input source, owner, and target
    /// </summary>
    public void Create(Effect source, Entity owner, Entity target)
    {
        Effect e = Clone();
        e._source = source;
        e._owner = owner;
        e._target = target;
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

    private Effect Clone()
    {
        return (Effect)MemberwiseClone();
    }
    
    public virtual void DoEffect()
    {
        Owner.Trigger<Trigger_OnActivateEffect>(this, this);
        Activate();
    }

    public abstract void Activate();
}