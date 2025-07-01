using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Stat_Movement : Mechanic<Stat_Movement>
{
    [FoldoutGroup("Movement")]
    public Entity movementTarget;
    [FoldoutGroup("Movement")]
    public float baseMovementSpeed;
    [FoldoutGroup("Movement"), SerializeReference]
    public MovementDirectionSelector movementSelector;
    [FoldoutGroup("Movement")]
    public Vector3 facingDir = Vector3.right;
    [FoldoutGroup("Movement")]
    public float dirMovementSpeedMulti = 1;
    [FoldoutGroup("Movement")]
    public bool rotate = false;
    [FoldoutGroup("Movement")]
    public bool obeyMapEdge = false;

    public override void Tick()
    {
        base.Tick();
        Move();
    }

    public void Move()
    {
        movementSelector?.Create(Owner);
        new Trigger_MovementDirectionCalc(Owner, Owner);

        if (rotate)
        {
            Owner.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Owner.GetMechanic<Stat_Movement>().facingDir);
        }

        Owner.transform.position +=
            Owner.GetMechanic<Stat_EffectModifiers>().CalculateModifier(EffectTag.MovementSpeed)
            * Mathf.Clamp(baseMovementSpeed, 0, float.MaxValue)
            * Mathf.Clamp(dirMovementSpeedMulti, 0, float.MaxValue)
            * Time.fixedDeltaTime
            * Owner.GetMechanic<Stat_Movement>().facingDir.normalized;
        if (obeyMapEdge)
        {
            Owner.transform.position = Vector3.ClampMagnitude(Owner.transform.position, 15f);// TODO: Change this to pathing
        }
    }
}