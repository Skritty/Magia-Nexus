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
    [SerializeReference, FoldoutGroup("@GetType()"), InfoBox("These are in effect until the end of damage calculation")]
    public List<PersistentEffect> temporaryEffects = new();

    public override void Activate()
    {
        if (Target.Stat<Stat_Life>().maxLife <= 0) return;
        foreach (PersistentEffect effect in temporaryEffects)
        {
            effect.OnGained();
        }
        if (!preventTriggers)
        {
            Owner.Stat<Stat_PlayerOwner>().Proxy(x => x.Trigger<Trigger_OnHit>(this, this));
            Target.Trigger<Trigger_WhenHit>(this, this);
        }

        Target.Stat<Stat_Life>().TakeDamage(this);

        foreach (PersistentEffect effect in temporaryEffects)
        {
            effect.OnLost();
        }
        if (ignoreFrames > 0)
            new PE_IgnoreEntity(ignoreFrames).Create(this);
    }

    public DamageInstance Clone()
    {
        DamageInstance clone = (DamageInstance)MemberwiseClone();
        clone.temporaryEffects = temporaryEffects;
        return clone;
    }
}