using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class SetMovementTarget : EffectTask
{
    [FoldoutGroup("@GetType()")]
    public EffectTargetSelector movementTarget = EffectTargetSelector.Target;

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        Owner.GetMechanic<Mechanic_Movement>().facingDir = Target.transform.position - Owner.transform.position;
        switch (movementTarget)
        {
            case EffectTargetSelector.Owner:
                Owner.Stat<Stat_MovementTarget>().Value = Owner;
                break;
            case EffectTargetSelector.Target:
                Owner.Stat<Stat_MovementTarget>().Value = Target; // TODO: this is bad
                break;
        }
    }
}