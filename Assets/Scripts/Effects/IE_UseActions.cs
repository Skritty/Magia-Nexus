using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IE_UseActions : Effect
{
    public List<Action> actions = new List<Action>();
    public override void Activate()
    {
        foreach(Action action in actions)
        {
            action.OnStart(owner);
            action.Tick(owner);
            action.OnEnd(owner);
        }
    }
}
