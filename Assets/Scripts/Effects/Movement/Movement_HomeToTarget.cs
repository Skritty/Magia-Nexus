using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_HomeToTarget : PersistentEffect
{
    public float homingRateDegreesPerSecond;

    public override void OnGained()
    {
        SetMoveDir();
    }

    public override void OnTick()
    {
        SetMoveDir();
    }

    private void SetMoveDir()
    {
        Vector3 dirToTarget = Vector3.zero;
        if (target.Stat<Stat_Target>().target != null)
        {
            dirToTarget = (target.Stat<Stat_Target>().target.transform.position - target.transform.position).normalized;
        }
        else
        {
            dirToTarget = target.Stat<Stat_Movement>().facingDir;
        }
        Quaternion angleChange = Quaternion.FromToRotation(target.Stat<Stat_Movement>().facingDir, dirToTarget);
        bool negative = angleChange.eulerAngles.z > 180;
        dirToTarget =
            Quaternion.Euler(0, 0, (negative ? -1 : 1) *
            Mathf.Clamp(homingRateDegreesPerSecond * Time.fixedDeltaTime,
            0, negative ? -(angleChange.eulerAngles.z - 360) : angleChange.eulerAngles.z))
            * target.Stat<Stat_Movement>().facingDir;
        target.Stat<Stat_Movement>().facingDir = dirToTarget;
        target.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dirToTarget);
    }
}