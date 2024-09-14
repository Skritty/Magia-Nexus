using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_Untargetable : PersistentEffect
{
    public override void OnGained()
    {
        target.Stat<Stat_Untargetable>().untargetable = true;
    }

    public override void OnLost()
    {
        target.Stat<Stat_Untargetable>().untargetable = false;
    }
}
