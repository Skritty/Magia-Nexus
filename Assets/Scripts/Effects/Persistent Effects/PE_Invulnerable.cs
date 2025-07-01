using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_Invulnerable : PersistentEffect
{
    public override void OnGained()
    {
        Target.Stat<Stat_Invulnerable>().Value = true;
    }

    public override void OnLost()
    {
        Target.Stat<Stat_Invulnerable>().Value = false;
    }
}