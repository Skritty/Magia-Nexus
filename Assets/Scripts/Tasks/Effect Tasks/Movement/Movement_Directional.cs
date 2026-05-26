using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class Movement_Directional : MovementTargetPositionSelector
{
    [FoldoutGroup("@GetType()")]
    public Vector3 localDirection;

    protected override Vector3 GetMovementTargetPosition(Entity owner, Entity target, float multiplier)
    {
        return target.transform.rotation * localDirection.normalized;
    }
}
