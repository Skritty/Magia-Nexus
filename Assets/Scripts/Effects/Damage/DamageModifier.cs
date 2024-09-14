using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class DamageModifier
{
    public SerializedDictionary<DamageType, DamageStats> damageTypeModifiers = new SerializedDictionary<DamageType, DamageStats>();
    [Serializable]
    public class DamageStats
    {
        public float multiplier = 1;
        public float increased = 1;
        public float flat;
    }

    public void Merge(DamageModifier defense)
    {
        foreach(KeyValuePair<DamageType, DamageStats> modifier in defense.damageTypeModifiers)
        {
            foreach (DamageType type in Enum.GetValues(typeof(DamageType)))
            {
                if (modifier.Key.HasFlag(type))
                {
                    if (!damageTypeModifiers.ContainsKey(type))
                    {
                        damageTypeModifiers.Add(type, new DamageStats());
                        damageTypeModifiers[type].multiplier = modifier.Value.multiplier;
                        damageTypeModifiers[type].increased = modifier.Value.increased;
                        damageTypeModifiers[type].flat = modifier.Value.flat;
                        continue;
                    }
                    else
                    {
                        damageTypeModifiers[type].multiplier *= modifier.Value.multiplier;
                        damageTypeModifiers[type].increased += modifier.Value.increased - 1;
                        damageTypeModifiers[type].flat += modifier.Value.flat;
                    }
                }
            }
        }
    }

    public void ApplyModifiers(DamageInstance damage)
    {
        foreach (DamageType type in Enum.GetValues(typeof(DamageType)))
        {
            if (!damageTypeModifiers.ContainsKey(type)) continue;
            if (!damage.damageTypes.HasFlag(type)) continue;
            if (!damage.skipFlatDamageReduction)
            {
                damage.damage += damageTypeModifiers[type].flat;
            }
            damage.damage *= damageTypeModifiers[type].increased;
            damage.damage *= damageTypeModifiers[type].multiplier;
        }
    }

    public DamageModifier Clone()
    {
        return (DamageModifier)MemberwiseClone();
    }
}

// 100 damage, 2x (10 flat, 0.5x multi)
// 50 damage -> 40 damage
// 20 damage -> 10 damage

// 100 damage, 1x (20flat, 0.25x multi)
// 25 damage -> 5 damage