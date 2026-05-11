using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class Stat_MovementSpeed : NumericalSolver, IStat<float> { }
public class Stat_Targets : PrioritySolver<Entity>, IStat<Entity> { }
public class Stat_MovementSelector : PrioritySolver<MovementTargetPositionSelector>, IStat<MovementTargetPositionSelector> { }
public class Mechanic_Movement : Mechanic
{
    [FoldoutGroup("Movement")]
    public Vector3 movementTargetPosition;
    [FoldoutGroup("Movement")]
    public Vector3 facingDir;
    [FoldoutGroup("Movement")]
    public bool rotate = false;
    [FoldoutGroup("Movement")]
    public bool useNavmesh = true;

    public override void Tick()
    {
        if (Owner.GetStat<Stat_Stunned>().Value) return;
        Move();
    }

    public void Move()
    {
        Owner.GetStat<Stat_MovementSelector>().Value?.DoTask(Owner);
        Trigger_MovementDirectionCalc.Invoke(Owner, Owner);

        if (Owner.GetStat<Stat_MovementSpeed>().Value == 0) return;

        if (useNavmesh)
        {
            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(Owner.transform.position, movementTargetPosition, 0, path);

            if (path.corners.Length > 1)
            {
                facingDir = (path.corners[1] - Owner.transform.position).normalized;
            }
        }
        else
        {
            facingDir = (movementTargetPosition - Owner.transform.position).normalized;
        }

        if (rotate) Owner.transform.localRotation = Quaternion.FromToRotation(Vector3.up, facingDir);

        Owner.transform.position += facingDir * Owner.GetStat<Stat_MovementSpeed>().Value * Time.fixedDeltaTime;
    }
}