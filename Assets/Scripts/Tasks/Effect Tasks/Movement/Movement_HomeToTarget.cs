using Sirenix.OdinInspector;
using UnityEngine;

public class Movement_HomeToTarget : MovementTargetPositionSelector
{
    [FoldoutGroup("@GetType()")]
    public float homingRateDegreesPerSecond;

    protected override Vector3 GetMovementTargetPosition(Entity owner, Entity target, float multiplier)
    {
        /*Vector3 dirToTarget = Vector3.zero;
        if (Stats.GetStat<Stat_Targets>(target).Value != null)
        {
            dirToTarget = (Stats.GetStat<Stat_Targets>(target).Value.transform.position - target.transform.position).normalized;
        }
        else
        {
            dirToTarget = target.GetStat<Mechanic_Movement>().facingDir;
        }
        Quaternion angleChange = Quaternion.FromToRotation(target.GetStat<Mechanic_Movement>().facingDir, dirToTarget);
        bool negative = angleChange.eulerAngles.z > 180;
        dirToTarget =
            Quaternion.Euler(0, 0, (negative ? -1 : 1) *
            Mathf.Clamp(homingRateDegreesPerSecond * Time.fixedDeltaTime,
            0, negative ? -(angleChange.eulerAngles.z - 360) : angleChange.eulerAngles.z))
            * target.GetStat<Mechanic_Movement>().facingDir;
        target.GetStat<Mechanic_Movement>().facingDir = dirToTarget;
        target.GetStat<Stat_MovementSpeed>().AddModifier(new Modifier_Numerical(multiplier, step: CalculationStep.Multiplicative, tickDuration: 1));
        target.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dirToTarget);*/
        Entity mainTarget = owner.GetStat<Stat_Targets>().Value;
        return mainTarget != null ? mainTarget.transform.position : target.transform.position;
    }
}