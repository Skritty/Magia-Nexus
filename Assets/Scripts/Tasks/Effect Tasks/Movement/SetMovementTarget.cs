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
                owner.AddModifier<Entity, Stat_MovementTarget>(owner, 1);
                break;
            case EffectTargetSelector.Target:
                owner.AddModifier<Entity, Stat_MovementTarget>(target, 1);
                break;
        }
    }
}