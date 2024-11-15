using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_DistanceFromTarget : Effect
{
    [FoldoutGroup("@GetType()")]
    public float distanceFromTarget;
    [FoldoutGroup("@GetType()")]
    public float threshold = 0.25f;

    public override void Activate()
    {
        SetMoveDir();
    }

    private void SetMoveDir()
    {
        if (Target.Stat<Stat_Movement>().movementTarget == null)
        {
            Target.Stat<Stat_Movement>().facingDir = Vector3.zero;
        }
        else
        {
            Vector3 dirFromTarget = Target.transform.position - Target.Stat<Stat_Movement>().movementTarget.transform.position;
            dirFromTarget = dirFromTarget.normalized * distanceFromTarget - dirFromTarget;
            if(dirFromTarget.magnitude <= threshold)
            {
                Target.Stat<Stat_Movement>().facingDir = Vector3.zero;
            }
            else
            {
                Target.Stat<Stat_Movement>().facingDir = dirFromTarget.normalized;
                Target.Stat<Stat_Movement>().dirMovementSpeedMulti = effectMultiplier;
            }
        }
    }
}