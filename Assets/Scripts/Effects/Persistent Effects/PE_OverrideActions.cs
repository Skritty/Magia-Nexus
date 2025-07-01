using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class PE_OverrideActions : PersistentEffect
{
    [FoldoutGroup("@GetType()")]
    public SerializedDictionary<Action, Action> actionOverrides;
    public override void OnGained()
    {
        Target.GetMechanic<Stat_Actions>().OverrideActions(actionOverrides);
    }

    public override void OnLost()
    {
        Target.GetMechanic<Stat_Actions>().OverrideActions(actionOverrides, true);
    }
}
