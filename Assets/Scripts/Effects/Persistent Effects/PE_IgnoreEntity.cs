using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_IgnoreEntity : PersistentEffect
{
    public override void OnGained()
    {
        owner.Stat<Stat_Ignored>().AddIgnored(source, target);
    }

    public override void OnLost()
    {
        owner.Stat<Stat_Ignored>().RemoveIgnored(source, target);
    }
}