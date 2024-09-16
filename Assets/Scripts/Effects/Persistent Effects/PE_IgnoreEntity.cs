using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_IgnoreEntity : PersistentEffect
{
    public override void OnGained()
    {
        Owner.Stat<Stat_Targetable>().AddIgnored(Source, Target);
    }

    public override void OnLost()
    {
        Owner.Stat<Stat_Targetable>().RemoveIgnored(Source, Target);
    }
}