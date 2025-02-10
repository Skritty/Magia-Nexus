using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class SetMovementTarget : Effect
{
    [FoldoutGroup("@GetType()")]
    public MovementTarget movementTarget = MovementTarget.Target;
    public override void Activate()
    {
        Owner.Stat<Stat_Movement>().facingDir = Target.transform.position - Owner.transform.position;
        switch (movementTarget)
        {
            case MovementTarget.Owner:
                Owner.Stat<Stat_Movement>().movementTarget = Owner;
                break;
            case MovementTarget.Target:
                Owner.Stat<Stat_Movement>().movementTarget = Target;
                break;
        }
    }
}

public enum MovementTarget { None, Owner, Target }