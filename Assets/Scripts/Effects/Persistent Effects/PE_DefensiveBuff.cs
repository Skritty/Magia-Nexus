using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_DefensiveBuff : PersistentEffect
{
    public DamageModifier modifiers;
    public override void OnGained()
    {
        target.Stat<Stat_Life>().damageModifers.Add(modifiers);
    }

    public override void OnLost()
    {
        target.Stat<Stat_Life>().damageModifers.Remove(modifiers);
    }
}