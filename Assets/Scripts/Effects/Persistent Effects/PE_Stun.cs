using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_Stun : PersistentEffect
{
    public override void OnGained()
    {
        Target.Stat<Stat_Actions>().stunned = true;
    }

    public override void OnLost()
    {
        Target.Stat<Stat_Actions>().stunned = false;
    }
}