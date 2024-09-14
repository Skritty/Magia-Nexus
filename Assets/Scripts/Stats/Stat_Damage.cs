using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Damage : GenericStat<Stat_Damage>
{
    [FoldoutGroup("Damage")]
    public List<DamageModifier> damageModifers = new List<DamageModifier>();

    public DamageInstance CalculateDamage(DamageInstance damage)
    {
        owner?.Trigger<Trigger_OnDamageCalculation>(damage);
        DamageModifier offense = new DamageModifier();
        foreach (DamageModifier d in damageModifers)
        {
            offense.Merge(d);
        }
        offense.ApplyModifiers(damage);
        return damage;
    }
}