using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_GrantEntityActions<T> : EffectTask<T>
{
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<Action> actions = new();

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        foreach (Action action in actions)
        {
            Target.GetMechanic<Mechanic_Actions>().AddAction(action);
        }
    }
}