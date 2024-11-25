using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMovementTarget : Effect
{
    public MovementTarget movementTarget = MovementTarget.Target;
    public override void Activate()
    {
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