using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_OverrideActions : PersistentEffect
{
    public SerializedDictionary<Action, Action> actionOverrides;
    public override void OnGained()
    {
        Owner.Stat<Stat_Actions>().OverrideActions(actionOverrides);
    }

    public override void OnLost()
    {
        Owner.Stat<Stat_Actions>().OverrideActions(actionOverrides, true);
    }
}
