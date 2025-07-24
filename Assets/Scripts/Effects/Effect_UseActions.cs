using System.Collections.Generic;
using Sirenix.OdinInspector;

public class Effect_UseActions<T> : EffectTask<T>
{
    [FoldoutGroup("@GetType()")]
    public List<Action> actions = new List<Action>();

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        foreach (Action action in actions)
        {
            action.DoEffects(Owner);
        }
    }
}
