using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_OffensiveBuff : PersistentEffect
{
    public DamageModifier modifiers;
    public override void OnGained()
    {
        Target.Stat<Stat_Damage>().damageModifers.Add(modifiers);
    }

    public override void OnLost()
    {
        Target.Stat<Stat_Damage>().damageModifers.Remove(modifiers);
    }
}