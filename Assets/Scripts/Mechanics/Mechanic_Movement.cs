using Sirenix.OdinInspector;
using UnityEngine;

public class Stat_MovementSpeed : NumericalSolver, IStatTag<float> { }
public class Stat_MovementTarget : PrioritySolver<Entity>, IStatTag<float> { }
public class Stat_MovementSelector : PrioritySolver<MovementDirectionSelector<Entity>>, IStatTag<MovementDirectionSelector<Entity>> { }
//public class Stat_MovementDirection : NumericalSolver, IStatTag { } TODO: make vector3 solver
public class Mechanic_Movement : Mechanic<Mechanic_Movement>
{
    [FoldoutGroup("Movement")]
    public Vector3 facingDir = Vector3.right;
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
        Owner.Stat<Stat_MovementSelector>().Value?.DoTask(null, Owner);
        Trigger_MovementDirectionCalc.Invoke(Owner, Owner);

        if (rotate)
        {
            Owner.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Owner.GetMechanic<Mechanic_Movement>().facingDir);
        }

        Owner.transform.position +=
            Owner.Stat<Stat_MovementSpeed>().Value
            * Time.fixedDeltaTime
            * Owner.GetMechanic<Mechanic_Movement>().facingDir.normalized;
        if (obeyMapEdge)
        {
            Owner.transform.position = Vector3.ClampMagnitude(Owner.transform.position, 15f);// TODO: Change this to pathing
        }
    }
}