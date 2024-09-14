using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TE_GrantEntityActions : Effect
{
    [SerializeReference]
    public List<Action> actions = new();
    [SerializeReference]
    public List<Action> repeatActions = new();
    public override void Activate()
    {
        foreach (Action action in actions)
        {
            target.Stat<Stat_Actions>().AddAction(action);
        }
        foreach (Action action in repeatActions)
        {
            target.Stat<Stat_Actions>().AddRepeatAction(action);
        }
    }
}