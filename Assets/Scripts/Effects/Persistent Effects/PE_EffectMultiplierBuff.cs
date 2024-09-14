using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_EffectMuliplierBuff : PersistentEffect
{
    public float multiplier = 1;
    public override void OnGained()
    {
        target.Stat<Stat_Effect>().effectMultiplier += multiplier;
    }

    public override void OnLost()
    {
        target.Stat<Stat_Effect>().effectMultiplier -= multiplier;
    }
}
