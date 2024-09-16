using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_EffectMuliplierBuff : PersistentEffect
{
    public float multiplier = 1;
    public override void OnGained()
    {
        Target.Stat<Stat_EffectModifiers>().effectMultiplier += multiplier;
    }

    public override void OnLost()
    {
        Target.Stat<Stat_EffectModifiers>().effectMultiplier -= multiplier;
    }
}
