using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrantEntityActions : Effect
{
    [SerializeReference]
    public List<Action> actions = new();
    [SerializeReference]
    public List<Action> repeatActions = new();
    public override void Activate()
    {
        foreach (Action action in actions)
        {
            Target.Stat<Stat_Actions>().AddAction(action);
        }
        foreach (Action action in repeatActions)
        {
            Target.Stat<Stat_Actions>().AddRepeatAction(action);
        }
    }
}