using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class Stat_MovementSpeed : NumericalSolver, IStat<float> { }
public class Stat_Targets : PrioritySolver<Entity>, IStat<Entity> { }
public class Stat_MovementSelector : PrioritySolver<MovementTargetPositionSelector>, IStat<MovementTargetPositionSelector> { }
public class Mechanic_Movement : Mechanic
{
    [FoldoutGroup("Movement")]
    public NavMeshAgent agent;
    [FoldoutGroup("Movement")]
    public Vector3 movementTargetPosition;
    [FoldoutGroup("Movement")]
    public Vector3 facingDir;
    [FoldoutGroup("Movement")]
    public bool rotate = false;
    [FoldoutGroup("Movement")]
    public bool useNavmesh = true;
    private Vector3 previousTargetPosition;

    public override void Tick()
    {
        if (Owner.GetStat<Stat_Stunned>().Value) return;
        Move();
    }

    public void Move()
    {
        if (movementTargetPosition == Owner.transform.position)
        {
            if (useNavmesh)
            {
                agent.isStopped = true;
            }
        }
        Owner.GetStat<Stat_MovementSelector>().Value?.DoTask(Owner);
        Trigger_MovementDirectionCalc.Invoke(Owner, Owner);

        if (Owner.GetStat<Stat_MovementSpeed>().Value == 0) return;

        if (useNavmesh)
        {
            NavMesh.SamplePosition(movementTargetPosition, out NavMeshHit hit, 1000, 1);
            NavMeshPath path = new NavMeshPath();// TODO: Do NOT calculate the path every frame
            if (!NavMesh.CalculatePath(Owner.transform.position, hit.position, 1, path)) return;

            /*if (path.corners.Length > 1)
            {
                facingDir = (path.corners[1] - Owner.transform.position).normalized;
            }*/

            agent.speed = Owner.GetStat<Stat_MovementSpeed>().Value;
            agent.SetPath(path);
            agent.isStopped = false;
        }
        else
        {
            if (rotate) Owner.transform.localRotation = Quaternion.FromToRotation(Vector3.up, facingDir);
            facingDir = (movementTargetPosition - Owner.transform.position).normalized;
            Owner.transform.position += facingDir * Owner.GetStat<Stat_MovementSpeed>().Value * Time.fixedDeltaTime;
        }
    }
}