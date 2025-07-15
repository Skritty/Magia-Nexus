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

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        SetMoveDir(Target);
    }

    private void SetMoveDir(Entity Target)
    {
        if (Target.Stat<Stat_MovementTarget>().Value == null)
        {
            Target.Stat<Stat_MovementSpeed>().Modifiers[1].Value = 0;
        }
        else
        {
            Vector3 dirFromTarget = Target.transform.position - Target.Stat<Stat_MovementTarget>().Value.transform.position;
            dirFromTarget = dirFromTarget.normalized * distanceFromTarget - dirFromTarget;
            if(dirFromTarget.magnitude <= threshold)
            {
                Target.Stat<Stat_MovementSpeed>().Modifiers[1].Value = 0;
            }
            else
            {
                Target.GetMechanic<Mechanic_Movement>().facingDir = dirFromTarget.normalized;
                Target.Stat<Stat_MovementSpeed>().Modifiers[1].Value = effectMultiplier;
            }
        }
    }
}