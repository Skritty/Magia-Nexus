using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class Movement_DistanceFromMainTarget : MovementTargetPositionSelector
{
    [FoldoutGroup("@GetType()")]
    public float distanceFromTarget;
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
            if (useNavmesh)
            {
                float dist = 0;
                Vector3 pos = mainTarget.transform.position;
                NavMeshPath path = new NavMeshPath(); // TODO: Do NOT calculate the path every frame
                if(!NavMesh.CalculatePath(target.transform.position, mainTarget.transform.position, 1, path)) return target.transform.position;
                for (int i = path.corners.Length - 2; i >= 0; i--)
                {
                    Vector3 dirAlongPath = path.corners[i + 1] - path.corners[i];
                    if(dist + dirAlongPath.magnitude > distanceFromTarget)
                    {
                        // Desired distance is along path
                        pos += dirAlongPath.normalized * (distanceFromTarget - dist);
                        return pos;
                    }
                    else
                    {
                        pos += dirAlongPath;
                        dist += dirAlongPath.magnitude;
                    }
                }
                // Desired distance is beyond path
                return target.transform.position + (path.corners[0] - path.corners[1]).normalized * (distanceFromTarget - dist);
            }
            else
            {
                return mainTarget.transform.position + (target.transform.position - mainTarget.transform.position).normalized * distanceFromTarget;
            }
        }
    }
}