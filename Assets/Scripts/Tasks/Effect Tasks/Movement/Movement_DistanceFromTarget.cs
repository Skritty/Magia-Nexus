using Sirenix.OdinInspector;
using UnityEngine;

public class Movement_DistanceFromTarget : MovementDirectionSelector
{
    [FoldoutGroup("@GetType()")]
    public float distanceFromTarget;
    [FoldoutGroup("@GetType()")]
    public float threshold = 0.25f;

    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        SetMoveDir(target, multiplier);
    }

    private void SetMoveDir(Entity target, float multiplier)
    {
        if (target.Stat<Stat_MovementTarget>().Value == null)
        {
            target.AddModifier<Stat_MovementSpeed>(new NumericalModifier(value: 0, step: CalculationStep.Multiplicative, tickDuration: 1));
        }
        else
        {
            Vector3 dirFromTarget = target.transform.position - target.Stat<Stat_MovementTarget>().Value.transform.position;
            dirFromTarget = dirFromTarget.normalized * distanceFromTarget - dirFromTarget;
            if(dirFromTarget.magnitude <= threshold)
            {
                target.AddModifier<Stat_MovementSpeed>(new NumericalModifier(value: 0, step: CalculationStep.Multiplicative, tickDuration: 1));
            }
            else
            {
                target.GetMechanic<Mechanic_Movement>().facingDir = dirFromTarget.normalized;
                target.AddModifier<Stat_MovementSpeed>(new NumericalModifier(value: multiplier, step: CalculationStep.Multiplicative, tickDuration: 1));
            }
        }
    }
}