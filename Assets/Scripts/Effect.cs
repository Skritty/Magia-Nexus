using Sirenix.OdinInspector;
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
    private int _sourceID;
    [FoldoutGroup("@GetType()"), ShowInInspector, ReadOnly]
    private int SourceID
    {
        get
        {
            if (_sourceID == 0) _sourceID = GetUID();
            return _sourceID;
        }
    }
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

    protected virtual bool UsedInCalculations => false;

    /// <summary>
    /// Multiplies the output of the effect, multiplied through chains of effects
    /// </summary>
    [FoldoutGroup("@GetType()"), ShowIf("@UsedInCalculations")]
    public float effectMultiplier = 1;
    [FoldoutGroup("@GetType()"), ShowIf("@UsedInCalculations")]
    public bool inheritEffectTagsOnTrigger; // Overrides effectTags if triggered by another effect with that effect's effectTags
    [FoldoutGroup("@GetType()"), ShowIf("@UsedInCalculations")]
    public SerializedDictionary<EffectTag, float> effectTags = new SerializedDictionary<EffectTag, float>() { {EffectTag.None, 1f} };
    //[FoldoutGroup("@GetType()")]
    //public List<EffectTagContainer> effectTags2 = new List<EffectTagContainer> { new EffectTagContainer(EffectTag.None, 1f) };
    
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Targeting targetSelector = new Targeting_Self();

    public float GetMultiplier()
    {
        float multiplier = 0;// TODO: split up multipliers? might be too complicated
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
            {
                e.effectMultiplier *= triggeringEffect.effectMultiplier;
                if (inheritEffectTagsOnTrigger)
                {
                    e.effectTags.Clear();
                    e.effectTags.AddRange(triggeringEffect.effectTags);
                }
            }
                
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

    protected Effect Clone()
    {
        Effect effect = (Effect)MemberwiseClone();
        effect.effectTags = new();
        effect.effectTags.AddRange(effectTags);
        return effect;
    }
    
    public virtual void DoEffect()
    {
        Owner.Trigger<Trigger_OnActivateEffect>(this, this);
        Activate();
    }

    public abstract void Activate();

    public int GetUID()
    {
        int UID = (int)effectMultiplier + GetType().GetHashCode();
        foreach (KeyValuePair<EffectTag, float> tag in effectTags)
        {
            UID += (int)tag.Key + (int)tag.Value;
        }
        return UID;
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
}