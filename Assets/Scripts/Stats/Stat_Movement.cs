using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Movement : GenericStat<Stat_Movement>
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
}