using System;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "ViewableGameAsset/Personality")]
public class Personality : ViewableGameAsset
{
    public bool lockTarget, lockMovementTarget;
    [SerializeReference]
    public Targeting targeting;
    [SerializeReference]
    public Targeting movement;
    [SerializeReference]
    public MovementDirectionSelector movementSelector;
    [SerializeReference]
    public Trigger conditionalTrigger;

    public void Initialize(Entity entity)
    {
        SetPersonality(entity);
        conditionalTrigger?.SubscribeToTasks(entity, 0);
    }

    public void SetPersonality(Entity entity)
    {
        // TODO: lock target won't get cleared when changed - fix that (maybe add a set function?)
        entity.Stat<Stat_LockTarget>().Add(lockTarget);
        entity.Stat<Stat_LockTarget>().Add(lockMovementTarget);
        entity.Stat<Stat_TargetingMethod>().Add(targeting);
        entity.Stat<Stat_MovementTargetingMethod>().Add(movement);
        entity.Stat<Stat_MovementSelector>().Add(movementSelector);
    }
}