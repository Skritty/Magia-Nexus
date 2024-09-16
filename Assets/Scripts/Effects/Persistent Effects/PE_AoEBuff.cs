using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_AoEBuff : PersistentEffect
{
    public float multiplier = 1;
    public override void OnGained()
    {
        Target.Stat<Stat_EffectModifiers>().aoeMultiplier += multiplier;
    }

    public override void OnLost()
    {
        Target.Stat<Stat_EffectModifiers>().aoeMultiplier -= multiplier;
    }
}
