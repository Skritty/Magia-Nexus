using Sirenix.OdinInspector;
using UnityEngine;

public class Movement_HomeToTarget : MovementDirectionSelector
{
    [FoldoutGroup("@GetType()")]
    public float homingRateDegreesPerSecond;

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        SetMoveDir(Target, multiplier);
    }

    private void SetMoveDir(Entity target, float multiplier)
    {
        Vector3 dirToTarget = Vector3.zero;
        if (target.Stat<Stat_MovementTarget>().Value != null)
        {
            dirToTarget = (target.Stat<Stat_MovementTarget>().Value.transform.position - target.transform.position).normalized;
        }
        else
        {
            dirToTarget = target.GetMechanic<Mechanic_Movement>().facingDir;
        }
        Quaternion angleChange = Quaternion.FromToRotation(target.GetMechanic<Mechanic_Movement>().facingDir, dirToTarget);
        bool negative = angleChange.eulerAngles.z > 180;
        dirToTarget =
            Quaternion.Euler(0, 0, (negative ? -1 : 1) *
            Mathf.Clamp(homingRateDegreesPerSecond * Time.fixedDeltaTime,
            0, negative ? -(angleChange.eulerAngles.z - 360) : angleChange.eulerAngles.z))
            * target.GetMechanic<Mechanic_Movement>().facingDir;
        target.GetMechanic<Mechanic_Movement>().facingDir = dirToTarget;
        target.AddModifier<float, Stat_MovementSpeed>(new Modifier_Numerical(multiplier, step: CalculationStep.Multiplicative, tickDuration: 1));
        target.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dirToTarget);
    }
}