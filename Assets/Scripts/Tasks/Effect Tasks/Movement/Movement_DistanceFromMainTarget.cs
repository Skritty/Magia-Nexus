using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class Movement_DistanceFromMainTarget : MovementTargetPositionSelector
{
    [FoldoutGroup("@GetType()")]
    public Vector2 distanceRangeFromTarget;
    [FoldoutGroup("@GetType()")]
    public bool useNavmesh = true;

    protected override Vector3 GetMovementTargetPosition(Entity owner, Entity target, float multiplier)
    {
        Entity mainTarget = owner.GetStat<Stat_Targets>().Value;

        if (mainTarget == null)
        {
            return target.transform.position;
        }
        else
        {
            float dist = 0;
            if (useNavmesh)
            {
                NavMeshPath path = new NavMeshPath();
                NavMesh.CalculatePath(target.transform.position, mainTarget.transform.position, 0, path);
                for (int i = 1; i < path.corners.Length; i++) dist += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
            else
            {
                Vector3 dirFromTarget = mainTarget.transform.position - target.transform.position;
                dist = dirFromTarget.magnitude;
            }

            if (dist >= distanceRangeFromTarget.x || dist <= distanceRangeFromTarget.x)
            {
                return target.transform.position;
            }
            else
            {
                return mainTarget.transform.position;
            }
        }
    }
}