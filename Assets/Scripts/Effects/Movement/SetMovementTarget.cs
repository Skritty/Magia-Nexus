using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class SetMovementTarget : Effect
{
    [FoldoutGroup("@GetType()")]
    public EffectTargetSelector movementTarget = EffectTargetSelector.Target;
    public override void Activate()
    {
        Owner.GetMechanic<Stat_Movement>().facingDir = Target.transform.position - Owner.transform.position;
        switch (movementTarget)
        {
            case EffectTargetSelector.Owner:
                Owner.GetMechanic<Stat_Movement>().movementTarget = Owner;
                break;
            case EffectTargetSelector.Target:
                Owner.GetMechanic<Stat_Movement>().movementTarget = Target;
                break;
        }
    }
}