using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseActions : Effect
{
    public List<Action> actions = new List<Action>();
    public override void Activate()
    {
        foreach(Action action in actions)
        {
            action.OnStart(Owner);
            action.Tick(Owner);
            action.OnEnd(Owner);
        }
    }
}
