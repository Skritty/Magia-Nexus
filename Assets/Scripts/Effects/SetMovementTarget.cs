using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMovementTarget : Effect
{
    public override void Activate()
    {
        Owner.Stat<Stat_Movement>().movementTarget = Target;
    }
}
