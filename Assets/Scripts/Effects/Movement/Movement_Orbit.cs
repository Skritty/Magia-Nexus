using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_Orbit : MovementDirectionSelector
{
    [FoldoutGroup("@GetType()")]
    public float orbitDistance;
    [FoldoutGroup("@GetType()")]
    public bool reverseDirection;

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        SetMoveDir(Target);
    }

    private void SetMoveDir(Entity Target)
    {
        float orbitDist = orbitDistance * Target.Stat<Stat_AoESize>().Value;
        Vector3 dirToTarget = Vector3.zero;
        bool zero = false;
        if (Target.Stat<Stat_MovementTarget>().Value != null)
        {
            dirToTarget = Target.transform.position - Target.Stat<Stat_MovementTarget>().Value.transform.position;
        }
        if (dirToTarget == Vector3.zero)
        {
            zero = true;
            dirToTarget = Target.Stat<Stat_MovementTarget>().Value.GetMechanic<Mechanic_Movement>().facingDir;
            Debug.Log(dirToTarget);
        }
        //if (dirToTarget == Vector3.zero) dirToTarget = Target.transform.position + Target.transform.up * orbitDistance;
        Vector3 perpendicularVector = Vector3.Cross(dirToTarget, Vector3.forward);
        if (reverseDirection) perpendicularVector = -perpendicularVector;

        if (zero)
        {
            perpendicularVector = dirToTarget;
        }
        else if (dirToTarget.magnitude > orbitDist)
        {
            perpendicularVector = Vector3.Lerp(perpendicularVector, -dirToTarget, (dirToTarget.magnitude - orbitDist) / orbitDist);
        }
        else
        {
            perpendicularVector = Vector3.Lerp(dirToTarget, perpendicularVector, dirToTarget.magnitude / orbitDist);
        }
        
        Target.GetMechanic<Mechanic_Movement>().facingDir = perpendicularVector;
        Target.Stat<Stat_MovementSpeed>().Modifiers[1].Value = effectMultiplier;
    }
}