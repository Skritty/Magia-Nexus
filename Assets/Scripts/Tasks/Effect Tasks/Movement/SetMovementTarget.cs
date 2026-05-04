using Sirenix.OdinInspector;
using UnityEngine.Analytics;

public class SetMovementTarget : EffectTask
{
    [FoldoutGroup("@GetType()")]
    public EffectTargetSelector movementTarget = EffectTargetSelector.Target;

    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        owner.GetStat<Mechanic_Movement>().facingDir = target.transform.position - owner.transform.position;
        switch (movementTarget)
        {
            case EffectTargetSelector.Owner:
                owner.GetStat<Stat_MovementTarget>().AddModifier(new Modifier<Entity>(value: owner, tickDuration: 1));
                break;
            case EffectTargetSelector.Target:
                owner.GetStat<Stat_MovementTarget>().AddModifier(new Modifier<Entity>(value: target, tickDuration: 1));
                break;
        }
    }
}