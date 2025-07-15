using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_HomeToTarget : MovementDirectionSelector
{
    [FoldoutGroup("@GetType()")]
    public float homingRateDegreesPerSecond;

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        SetMoveDir(Target);
    }

    private void SetMoveDir(Entity Target)
    {
        Vector3 dirToTarget = Vector3.zero;
        if (Target.Stat<Stat_MovementTarget>().Value != null)
        {
            dirToTarget = (Target.Stat<Stat_MovementTarget>().Value.transform.position - Target.transform.position).normalized;
        }
        else
        {
            dirToTarget = Target.GetMechanic<Mechanic_Movement>().facingDir;
        }
        Quaternion angleChange = Quaternion.FromToRotation(Target.GetMechanic<Mechanic_Movement>().facingDir, dirToTarget);
        bool negative = angleChange.eulerAngles.z > 180;
        dirToTarget =
            Quaternion.Euler(0, 0, (negative ? -1 : 1) *
            Mathf.Clamp(homingRateDegreesPerSecond * Time.fixedDeltaTime,
            0, negative ? -(angleChange.eulerAngles.z - 360) : angleChange.eulerAngles.z))
            * Target.GetMechanic<Mechanic_Movement>().facingDir;
        Target.GetMechanic<Mechanic_Movement>().facingDir = dirToTarget;
        Target.Stat<Stat_MovementSpeed>().Modifiers[1].Value = effectMultiplier;
        Target.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dirToTarget);
    }
}