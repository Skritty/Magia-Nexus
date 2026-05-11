using Sirenix.OdinInspector;
using UnityEngine;

public class Movement_Orbit : MovementTargetPositionSelector
{
    [FoldoutGroup("@GetType()")]
    public float orbitDistance;
    [FoldoutGroup("@GetType()")]
    public bool reverseDirection;

    protected override Vector3 GetMovementTargetPosition(Entity owner, Entity target, float multiplier)
    {
        /*float orbitDist = orbitDistance * Stats.GetStat<Stat_AoESize>(target).Value;
        Vector3 dirToTarget = Vector3.zero;
        bool zero = false;
        if (Stats.GetStat<Stat_Targets>(target).Value != null)
        {
            dirToTarget = target.transform.position - Stats.GetStat<Stat_Targets>(target).Value.transform.position;
        }
        if (dirToTarget == Vector3.zero)
        {
            zero = true;
            dirToTarget = Stats.GetStat<Stat_Targets>(target).Value.GetStat<Mechanic_Movement>().facingDir;
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

        target.GetStat<Mechanic_Movement>().facingDir = perpendicularVector;
        target.GetStat<Stat_MovementSpeed>().AddModifier(new Modifier_Numerical(value: multiplier, step: CalculationStep.Multiplicative, tickDuration: 1));*/
        return target.transform.position;
    }
}