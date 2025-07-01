using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berserk : Effect
{
    public override void Activate()
    {
        if (Target.GetMechanic<Stat_Actions>().actions.Count == 0) return;
        int randomActionIndex = Random.Range(0, Target.GetMechanic<Stat_Actions>().actions.Count);
        Action action = Target.GetMechanic<Stat_Actions>().actionsOverride[randomActionIndex];
        if(action == null) action = Target.GetMechanic<Stat_Actions>().actions[randomActionIndex];
        Target.GetMechanic<Stat_Actions>().AddAction(action);
    }
}
