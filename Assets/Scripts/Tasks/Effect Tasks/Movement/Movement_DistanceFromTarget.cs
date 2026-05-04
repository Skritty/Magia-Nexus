using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class Movement_DistanceFromTarget : MovementDirectionSelector
{
    [FoldoutGroup("@GetType()")]
    public float movementSpeedMultiplier = 1;
    [FoldoutGroup("@GetType()")]
    public float distanceFromTarget;
    [FoldoutGroup("@GetType()")]
    public float threshold = 0.25f;

    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        Targeting targeting = owner.GetStat<Stat_MovementTargetingMethod>().Value;
        if (targeting != null)
            foreach (Entity entity in owner.GetStat<Stat_MovementTargetingMethod>().Value.Solve(owner))
            {
                // TODO: don't do this every frame
                owner.GetStat<Stat_MovementTarget>().AddModifier(new Modifier<Entity>(value: entity, tickDuration: 1));
                break;
            }

        if (target.GetStat<Stat_MovementTarget>().Value == null)
        {
            target.GetStat<Stat_MovementSpeed>().AddModifier(new Modifier_Numerical(value: 0, step: CalculationStep.Multiplicative, tickDuration: 1));
        }
        else
        {
            Vector3 dirFromTarget = target.transform.position - Stats.GetStat<Stat_MovementTarget>(target).Value.transform.position;
            dirFromTarget = dirFromTarget.normalized * distanceFromTarget - dirFromTarget;
            if (dirFromTarget.magnitude <= threshold)
            {
                target.GetStat<Stat_MovementSpeed>().AddModifier(new Modifier_Numerical(value: 0, step: CalculationStep.Multiplicative, tickDuration: 1));
            }
            else
            {
                target.GetStat<Mechanic_Movement>().facingDir = dirFromTarget.normalized;
                target.GetStat<Stat_MovementSpeed>().AddModifier(new Modifier_Numerical(value: movementSpeedMultiplier * multiplier, step: CalculationStep.Multiplicative, tickDuration: 1));
            }
        }
    }
}