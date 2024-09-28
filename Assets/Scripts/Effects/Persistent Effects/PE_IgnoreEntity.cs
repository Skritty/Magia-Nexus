using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_IgnoreEntity : PersistentEffect
{
    public PE_IgnoreEntity() { }
    public PE_IgnoreEntity(int tick)
    {
        tickDuration = tick;
        refreshDuration = false;
        maxStacks = 99;
    }

    public override void OnGained()
    {
        Target.Stat<Stat_Targetable>().AddIgnored(Owner, Source);
    }

    public override void OnLost()
    {
        Target.Stat<Stat_Targetable>().RemoveIgnored(Owner, Source);
    }
}