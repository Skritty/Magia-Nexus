using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_HomeToTarget : Effect
{
    [FoldoutGroup("@GetType()")]
    public float homingRateDegreesPerSecond;

    public override void Activate()
    {
        SetMoveDir();
    }

    private void SetMoveDir()
    {
        Vector3 dirToTarget = Vector3.zero;
        if (Target.Stat<Stat_Movement>().movementTarget != null)
        {
            dirToTarget = (Target.Stat<Stat_Movement>().movementTarget.transform.position - Target.transform.position).normalized;
        }
        else
        {
            dirToTarget = Target.Stat<Stat_Movement>().facingDir;
        }
        Quaternion angleChange = Quaternion.FromToRotation(Target.Stat<Stat_Movement>().facingDir, dirToTarget);
        bool negative = angleChange.eulerAngles.z > 180;
        dirToTarget =
            Quaternion.Euler(0, 0, (negative ? -1 : 1) *
            Mathf.Clamp(homingRateDegreesPerSecond * Time.fixedDeltaTime,
            0, negative ? -(angleChange.eulerAngles.z - 360) : angleChange.eulerAngles.z))
            * Target.Stat<Stat_Movement>().facingDir;
        Target.Stat<Stat_Movement>().facingDir = dirToTarget;
        Target.Stat<Stat_Movement>().dirMovementSpeedMulti = effectMultiplier;
        Target.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dirToTarget);
    }
}