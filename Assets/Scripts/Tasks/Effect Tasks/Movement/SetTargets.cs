using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Analytics;

public class SetTargets : EffectTask
{
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Targeting targeting;
    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        byte priority = 0;
        foreach(Entity t in targeting.FindTargets(target))
        {
            owner.GetStat<Stat_Targets>().AddModifier(new Modifier_Priority<Entity>(target, priority++)); // TODO: figure out how to override previous targets
        }
    }
}