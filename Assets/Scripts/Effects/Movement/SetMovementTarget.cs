using Sirenix.OdinInspector;

public class SetMovementTarget : EffectTask
{
    [FoldoutGroup("@GetType()")]
    public EffectTargetSelector movementTarget = EffectTargetSelector.Target;

    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        owner.GetMechanic<Mechanic_Movement>().facingDir = target.transform.position - owner.transform.position;
        switch (movementTarget)
        {
            case EffectTargetSelector.Owner:
                owner.AddModifier<Stat_MovementTarget, Entity>(owner, 1);
                break;
            case EffectTargetSelector.Target:
                owner.AddModifier<Stat_MovementTarget, Entity>(target, 1);
                break;
        }
    }
}