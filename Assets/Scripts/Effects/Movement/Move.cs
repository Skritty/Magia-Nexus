using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : Effect
{
    [FoldoutGroup("@GetType()")]
    public bool rotate;

    public override void Activate()
    {
        if (rotate)
        {
            Owner.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Owner.Stat<Stat_Movement>().facingDir);
        }

        Owner.transform.position +=
            Owner.Stat<Stat_Movement>().facingDir
            * Owner.Stat<Stat_Movement>().baseMovementSpeed
            * GetMultiplier(EffectTag.MovementSpeed) 
            * Time.fixedDeltaTime;
    }
}
