using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_DistanceFromTarget : PersistentEffect
{
    public float distanceFromTarget;
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
        if (target.Stat<Stat_Target>().target == null)
        {
            target.Stat<Stat_Movement>().facingDir = Vector3.zero;
        }
        else
        {
            Vector3 dirFromTarget = target.transform.position - target.Stat<Stat_Target>().target.transform.position;
            dirFromTarget = dirFromTarget - dirFromTarget.normalized * distanceFromTarget;
            target.Stat<Stat_Movement>().facingDir = dirFromTarget.normalized;
        }
    }
}