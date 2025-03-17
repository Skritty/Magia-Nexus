using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class UseActions : Effect
{
    [FoldoutGroup("@GetType()")]
    public List<Action> actions = new List<Action>();
    public override void Activate()
    {
        foreach(Action action in actions)
        {
            foreach (Effect effect in action.effects)
            {
                effect.Create(Owner, effectMultiplier);
            }
        }
    }
}
