using Sirenix.OdinInspector;
using UnityEngine;

public class Stat_MovementSpeed : NumericalSolver, IStat<float> { }
public class Stat_MovementTarget : PrioritySolver<Entity>, IStat<Entity> { }
public class Stat_MovementTargetingMethod : PrioritySolver<Targeting>, IStat<Targeting> { }
public class Stat_MovementSelector : PrioritySolver<MovementDirectionSelector>, IStat<MovementDirectionSelector> { }
//public class Stat_MovementDirection : NumericalSolver, IStat { } TODO: make vector3 solver
public class Mechanic_Movement : Mechanic
{
    [FoldoutGroup("Movement")]
    public Vector3 facingDir = Vector3.right;
    [FoldoutGroup("Movement")]
    public bool rotate = false;
    [FoldoutGroup("Movement")]
    public bool obeyMapEdge = false;

    public override void Tick()
    {
        if (Owner.GetStat<Stat_Stunned>().Value) return;
        Move();
    }

    public void Move()
    {
        if (Owner.GetStat<Stat_MovementTarget>().Value == null) return;

        Owner.GetStat<Stat_MovementSelector>().Value?.DoTask(Owner);
        Trigger_MovementDirectionCalc.Invoke(Owner, Owner);

        if (rotate)
        {
            Owner.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Owner.GetStat<Mechanic_Movement>().facingDir);
        }

        float movementSpeed = Owner.GetStat<Stat_MovementSpeed>().Value;
        Owner.transform.position +=
            movementSpeed
            * Time.fixedDeltaTime
            * facingDir.normalized;
        if (obeyMapEdge)
        {
            Owner.transform.position = Vector3.ClampMagnitude(Owner.transform.position, 15f);// TODO: Change this to pathing
        }
    }
}