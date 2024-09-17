using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_IgnoreEntity : PersistentEffect
{
    public override void OnGained()
    {
        Target.Stat<Stat_Targetable>().AddIgnored(Source);
    }

    public override void OnLost()
    {
        Target.Stat<Stat_Targetable>().RemoveIgnored(Source);
    }
}