using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_StartingDelay : PersistentEffect
{
    public int tickDelay;
    public override void OnGained()
    {
        Target.GetMechanic<Stat_Actions>().startingTickDelay += tickDelay;
    }

    public override void OnLost()
    {
        Target.GetMechanic<Stat_Actions>().startingTickDelay -= tickDelay;
    }
}