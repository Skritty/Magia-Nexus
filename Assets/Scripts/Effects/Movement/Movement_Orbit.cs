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

    public override void Activate()
    {
        SetMoveDir();
    }

    private void SetMoveDir()
    {
        float orbitDist = orbitDistance * Target.Stat<Stat_EffectModifiers>().CalculateModifier(EffectTag.AoESize);
        Vector3 dirToTarget = Vector3.zero;
        bool zero = false;
        if (Target.Stat<Stat_Movement>().movementTarget != null)
        {
            dirToTarget = Target.transform.position - Target.Stat<Stat_Movement>().movementTarget.transform.position;
        }
        if (dirToTarget == Vector3.zero)
        {
            zero = true;
            dirToTarget = Target.Stat<Stat_Movement>().movementTarget.Stat<Stat_Movement>().facingDir;
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
        
        Target.Stat<Stat_Movement>().facingDir = perpendicularVector;
        Target.Stat<Stat_Movement>().dirMovementSpeedMulti = effectMultiplier;
    }
}