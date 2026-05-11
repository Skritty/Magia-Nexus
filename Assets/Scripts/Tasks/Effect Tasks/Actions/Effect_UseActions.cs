using System.Collections.Generic;
using Sirenix.OdinInspector;

public class Effect_UseActions : EffectTask
{
    [FoldoutGroup("@GetType()")]
    public List<Skill> actions = new List<Skill>();

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        foreach (Skill action in actions)
        {
            action.DoEffects(Owner);
        }
    }
}
