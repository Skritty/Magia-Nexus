using Sirenix.OdinInspector;
using UnityEngine;

public class Movement_Orbit<T> : MovementDirectionSelector<T>
{
    [FoldoutGroup("@GetType()")]
    public float orbitDistance;
    [FoldoutGroup("@GetType()")]
    public bool reverseDirection;

    public override void DoEffect(Entity Owner, Entity target, float multiplier, bool triggered)
    {
        SetMoveDir(target, multiplier);
    }

    private void SetMoveDir(Entity target, float multiplier)
    {
        float orbitDist = orbitDistance * target.Stat<Stat_AoESize>().Value;
        Vector3 dirToTarget = Vector3.zero;
        bool zero = false;
        if (target.Stat<Stat_MovementTarget>().Value != null)
        {
            dirToTarget = target.transform.position - target.Stat<Stat_MovementTarget>().Value.transform.position;
        }
        if (dirToTarget == Vector3.zero)
        {
            zero = true;
            dirToTarget = target.Stat<Stat_MovementTarget>().Value.GetMechanic<Mechanic_Movement>().facingDir;
            Debug.Log(dirToTarget);
        }
        //if (dirToTarget == Vector3.zero) dirToTarget = Target.transform.position + Target.transform.up * orbitDistance;
        Vector3 perpendicularVector = Vector3.Cross(dirToTarget, Vector3.forward);
        if (reverseDirection) perpendicularVector = -perpendicularVector;

        if (zero)
        {
            perpendicularVector = dirToTarget;
        }
        else if (dirToTarget.magnitude > orbitDist)
        {
            perpendicularVector = Vector3.Lerp(perpendicularVector, -dirToTarget, (dirToTarget.magnitude - orbitDist) / orbitDist);
        }
        else
        {
            perpendicularVector = Vector3.Lerp(dirToTarget, perpendicularVector, dirToTarget.magnitude / orbitDist);
        }
        
        target.GetMechanic<Mechanic_Movement>().facingDir = perpendicularVector;
        target.AddModifier<Stat_MovementSpeed>(new NumericalModifier(value: multiplier, step: CalculationStep.Multiplicative, temporary: true));
    }
}