using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_DistanceFromTarget : MovementDirectionSelector
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
        if (Target.GetMechanic<Stat_Movement>().movementTarget == null)
        {
            Target.GetMechanic<Stat_Movement>().dirMovementSpeedMulti = 0;
        }
        else
        {
            Vector3 dirFromTarget = Target.transform.position - Target.GetMechanic<Stat_Movement>().movementTarget.transform.position;
            dirFromTarget = dirFromTarget.normalized * distanceFromTarget - dirFromTarget;
            if(dirFromTarget.magnitude <= threshold)
            {
                Target.GetMechanic<Stat_Movement>().dirMovementSpeedMulti = 0;
            }
            else
            {
                Target.GetMechanic<Stat_Movement>().facingDir = dirFromTarget.normalized;
                Target.GetMechanic<Stat_Movement>().dirMovementSpeedMulti = effectMultiplier;
            }
        }
    }
}