using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrantEntityActions : Effect
{
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<Action> actions = new();
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<Action> repeatActions = new();
    public override void Activate()
    {
        foreach (Action action in actions)
        {
            Target.GetMechanic<Stat_Actions>().AddAction(action);
        }
        foreach (Action action in repeatActions)
        {
            Target.GetMechanic<Stat_Actions>().AddRepeatAction(action);
        }
    }
}