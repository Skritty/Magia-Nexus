using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_DefensiveBuff : PersistentEffect
{
    public DamageModifier modifiers;
    public override void OnGained()
    {
        Target.Stat<Stat_Life>().damageModifers.Add(modifiers);
    }

    public override void OnLost()
    {
        Target.Stat<Stat_Life>().damageModifers.Remove(modifiers);
    }
}