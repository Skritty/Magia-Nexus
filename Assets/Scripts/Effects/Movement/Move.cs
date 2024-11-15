using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : Effect
{
    [FoldoutGroup("@GetType()")]
    public bool rotate;
    [FoldoutGroup("@GetType()")]
    public bool obeyMapEdge = false;

    public override void Activate()
    {
        Owner.Trigger<Trigger_MovementDirectionCalc>(Owner, this);
        if (rotate)
        {
            Owner.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Owner.Stat<Stat_Movement>().facingDir);
        }

        Owner.transform.position +=
            Owner.Stat<Stat_Movement>().facingDir
            * Owner.Stat<Stat_Movement>().baseMovementSpeed
            * Target.Stat<Stat_Movement>().dirMovementSpeedMulti
            * GetMultiplier(EffectTag.MovementSpeed) 
            * Time.fixedDeltaTime;
        Owner.transform.position = Vector3.ClampMagnitude(Owner.transform.position, 15f);// TODO: Change this to pathing
    }
}
