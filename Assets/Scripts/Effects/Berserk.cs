using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berserk : Effect
{
    public override void Activate()
    {
        if (Target.Stat<Stat_Actions>().actions.Count == 0) return;
        int randomActionIndex = Random.Range(0, Target.Stat<Stat_Actions>().actions.Count);
        Action action = Target.Stat<Stat_Actions>().actionsOverride[randomActionIndex];
        if(action == null) action = Target.Stat<Stat_Actions>().actions[randomActionIndex];
        Target.Stat<Stat_Actions>().AddAction(action);
    }
}
