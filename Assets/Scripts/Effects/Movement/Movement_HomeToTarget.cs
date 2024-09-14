using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_HomeToTarget : PersistentEffect
{
    public float homingRateDegreesPerSecond;

    public override void Activate()
    {
        SetMoveDir();
    }

    public override void OnAction()
    {
        SetMoveDir();
    }

    private void SetMoveDir()
    {
        target = owner.Stat<Stat_Target>().target;
        Vector3 dirToTarget = Vector3.zero;
        if (target != null)
        {
            dirToTarget = (target.transform.position - owner.transform.position).normalized;
        }
        else
        {
            dirToTarget = owner.Stat<Stat_Movement>().facingDir;
        }
        Quaternion angleChange = Quaternion.FromToRotation(owner.Stat<Stat_Movement>().facingDir, dirToTarget);
        bool negative = angleChange.eulerAngles.z > 180;
        dirToTarget =
            Quaternion.Euler(0, 0, (negative ? -1 : 1) *
            Mathf.Clamp(homingRateDegreesPerSecond * Time.fixedDeltaTime,
            0, negative ? -(angleChange.eulerAngles.z - 360) : angleChange.eulerAngles.z))
            * owner.Stat<Stat_Movement>().facingDir;
        owner.Stat<Stat_Movement>().facingDir = dirToTarget;
        owner.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dirToTarget);
    }
}