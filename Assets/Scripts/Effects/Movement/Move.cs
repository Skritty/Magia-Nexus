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
        Owner.GetMechanic<Stat_Movement>().movementSelector?.Create(this);
        new Trigger_MovementDirectionCalc(Target, Target);

        if (rotate)
        {
            Owner.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Owner.GetMechanic<Stat_Movement>().facingDir);
        }

        Owner.transform.position +=
            GetMultiplier(EffectTag.MovementSpeed) 
            * Mathf.Clamp(Owner.GetMechanic<Stat_Movement>().baseMovementSpeed, 0, float.MaxValue) 
            * Mathf.Clamp(Target.GetMechanic<Stat_Movement>().dirMovementSpeedMulti, 0, float.MaxValue) 
            * Time.fixedDeltaTime 
            * Owner.GetMechanic<Stat_Movement>().facingDir.normalized;
        if (obeyMapEdge)
        {
            Owner.transform.position = Vector3.ClampMagnitude(Owner.transform.position, 15f);// TODO: Change this to pathing
        }
    }
}
