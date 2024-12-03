using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DamageInstance : Effect
{
    [FoldoutGroup("@GetType()")]
    public int ignoreFrames;
    [FoldoutGroup("@GetType()")]
    public bool skipFlatDamageReduction;
    [FoldoutGroup("@GetType()")]
    public bool preventTriggers;
    
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<Effect> ownerEffects = new();
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<Effect> targetEffects = new();
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<Effect> onHitEffects = new();
    [SerializeReference, FoldoutGroup("@GetType()"), InfoBox("These are in effect until the end of damage calculation")]
    public List<TriggeredEffect> temporaryTriggeredEffects = new();

    public override void Activate()
    {
        if (Target.Stat<Stat_Life>().maxLife <= 0) return;
        foreach (TriggeredEffect effect in temporaryTriggeredEffects)
        {
            Owner.Stat<Stat_PersistentEffects>().AddOrRemoveSimilarEffect(effect, effect.stacks, Owner);
        }
        foreach (Effect effect in targetEffects)
        {
            effect.Create(Source, Owner, Target);
        }
        foreach (Effect effect in ownerEffects)
        {
            effect.Create(Source, Owner, Owner);
        }
        if (!preventTriggers)
        {
            foreach (Effect effect in onHitEffects)
            {
                effect.Create(this);
            }
            Owner.Stat<Stat_PlayerOwner>().Proxy(x => x.Trigger<Trigger_OnHit>(this, this));
            Target.Trigger<Trigger_WhenHit>(this, this);
        }

        Target.Stat<Stat_Life>().TakeDamage(this);

        if (ignoreFrames > 0)
            new PE_IgnoreEntity(ignoreFrames).Create(this);

        foreach (TriggeredEffect effect in temporaryTriggeredEffects)
        {
            Owner.Stat<Stat_PersistentEffects>().AddOrRemoveSimilarEffect(effect, -effect.stacks, Owner);
        }
    }

    public new DamageInstance Clone()
    {
        DamageInstance clone = (DamageInstance)base.Clone();
        clone.ownerEffects = ownerEffects;
        clone.targetEffects = targetEffects;
        clone.temporaryTriggeredEffects = temporaryTriggeredEffects;
        return clone;
    }
}