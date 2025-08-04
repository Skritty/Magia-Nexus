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
        if (target.Stat<Stat_MovementTarget>().Value == null)
        {
            target.AddModifier<float, Stat_MovementSpeed>(new Modifier_Numerical(value: 0, step: CalculationStep.Multiplicative, tickDuration: 1));
        }
        else
        {
            Vector3 dirFromTarget = target.transform.position - target.Stat<Stat_MovementTarget>().Value.transform.position;
            dirFromTarget = dirFromTarget.normalized * distanceFromTarget - dirFromTarget;
            if (dirFromTarget.magnitude <= threshold)
            {
                target.AddModifier<float, Stat_MovementSpeed>(new Modifier_Numerical(value: 0, step: CalculationStep.Multiplicative, tickDuration: 1));
            }
            else
            {
                target.GetMechanic<Mechanic_Movement>().facingDir = dirFromTarget.normalized;
                target.AddModifier<float, Stat_MovementSpeed>(new Modifier_Numerical(value: movementSpeedMultiplier * multiplier, step: CalculationStep.Multiplicative, tickDuration: 1));
            }
        }
    }
}