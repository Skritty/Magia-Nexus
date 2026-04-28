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
        //Stats.GetStat<Stat_LockTarget>(entity).Add(lockTarget);
        //Stats.GetStat<Stat_LockTarget>(entity).Add(lockMovementTarget);
        Stats.GetStat<Stat_TargetingMethod>(entity).Add(targeting);
        Stats.GetStat<Stat_MovementTargetingMethod>(entity).Add(movement);
        Stats.GetStat<Stat_MovementSelector>(entity).Add(movementSelector);
    }
}