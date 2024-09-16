using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DamageInstance : Effect
{
    public float damage;
    public int ignoreFrames;
    public bool skipFlatDamageReduction;
    public bool preventTriggers;
    public DamageType damageTypes;
    [SerializeReference, InfoBox("These are in effect until the end of damage calculation")]
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
            Owner.Trigger<Trigger_OnHit>(this);
            Target.Trigger<Trigger_WhenHit>(this);
        }
        damage *= Owner.Stat<Stat_EffectModifiers>().effectMultiplier;
        Target.Stat<Stat_Life>().TakeDamage(this);
        foreach (PersistentEffect effect in temporaryEffects)
        {
            effect.OnLost();
        }
        PE_IgnoreEntity ignore = new PE_IgnoreEntity();
        ignore.tickDuration = ignoreFrames;
        ignore.Create(this);
    }

    public DamageInstance Clone()
    {
        DamageInstance clone = (DamageInstance)MemberwiseClone();
        clone.temporaryEffects = temporaryEffects;
        return clone;
    }
}

// 50% phys reduction, 0% fire reduction, -10 phy, -5 fire
// Type 1: 200 damage phys/fire = 0.5 * 200 = 100 - 15 = 85 damage taken
// Type 2: 100 phys, 100 fire = 0.5 * 100 + 1 * 100 = 50 - 10 + 100 - 5 = 135 damage taken
// Type 2: 150 phys, 50 fire = 0.5 * 150 + 1 * 50 = 75 - 10 + 50 - 5 = 110 damage taken