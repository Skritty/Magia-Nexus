using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_AoEBuff : PersistentEffect
{
    public float multiplier = 1;
    public override void OnGained()
    {
        target.Stat<Stat_Effect>().aoeMultiplier += multiplier;
    }

    public override void OnLost()
    {
        target.Stat<Stat_Effect>().aoeMultiplier -= multiplier;
    }
}
