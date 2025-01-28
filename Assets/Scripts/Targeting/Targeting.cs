using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[Flags]
public enum TargetFilter
{
    Self = 1,
    Owner = 2,
    Allies = 4,
    Enemies = 8,
}

public enum TargetSorting
{
    Unsorted = 1,
    Lowest = 2,
    Highest = 4
}

public enum EffectTargetingSelector { Owner, Target }

[Serializable]
public abstract class Targeting
{
    protected Entity Owner;
    [ShowInInspector]
    protected Entity primaryTarget;
    public bool lockTarget;
    
    public abstract List<Entity> GetTargets(Effect source, Entity owner);
    public abstract List<Entity> GetTargets(Effect source, Trigger trigger, Entity owner);
    public virtual void OnDrawGizmos(Transform owner) { }
    public virtual T Clone<T>() where T : Targeting
    {
        return (T)MemberwiseClone();
    }
}

[Serializable]
public abstract class MultiTargeting : Targeting
{
    public TargetFilter entitiesAffected = TargetFilter.Enemies;
    public TargetSorting sortingMethod = TargetSorting.Unsorted;
    //public Vector2 targetingRange;
    public int numberOfTargets = -1;
    public Vector3 offset;
    public VFX vfx;
    protected List<Entity> targets;
    protected Vector3  GetCenter(Entity owner)
    {
        return owner.transform.position + Quaternion.FromToRotation(Vector3.up, owner.Stat<Stat_Movement>().facingDir) * offset;
    }

    public override List<Entity> GetTargets(Effect source, Entity owner)
    {
        if(owner == null)
        {
            Debug.LogWarning($"Owner of Targeting is null for {source.Owner}->{source.Target}!");
            return new List<Entity>();
        }
        if(lockTarget && !(primaryTarget == null || !primaryTarget.gameObject.activeSelf))
        {
            return targets;
        }
        this.Owner = owner;
        targets = new List<Entity>();
        TargetFilter targetType;
        foreach (Entity entity in Entity.FindObjectsOfType<Entity>())
        {
            // Can it be targeted?
            if (!entity.Stat<Stat_Targetable>().IsTargetable(owner, source)) continue;

            // Is it in the affected entities?
            if(entity.Stat<Stat_Team>().team != owner.Stat<Stat_Team>().team)
            {
                targetType = TargetFilter.Enemies;
            }
            else if(entity == owner)
            {
                targetType = TargetFilter.Self;
            }
            else if (entity == owner.Stat<Stat_PlayerOwner>().playerCharacter)
            {
                targetType = TargetFilter.Owner;
            }
            else
            {
                targetType = TargetFilter.Allies;
            }
            if (!entitiesAffected.HasFlag(targetType)) continue;
            if (!IsValidTarget(entity)) continue;

            // Is it within the targeting range?
            //float distance = Vector3.Distance(entity.transform.position, owner.transform.position);
            //if (distance < targetingRange.x * owner.Stat<Stat_Effect>().aoeMultiplier) continue;
            //if (distance > targetingRange.y * owner.Stat<Stat_Effect>().aoeMultiplier) continue;

            targets.Add(entity);
        }

        if(sortingMethod != TargetSorting.Unsorted)
            targets.Sort(SortTargets);

        if(numberOfTargets >= 0)
        {
            int actualNumberOfTargets = numberOfTargets + owner.Stat<Stat_Targeting>().additionalTargets;
            if (numberOfTargets >= 0 && targets.Count > actualNumberOfTargets)
                targets.RemoveRange(actualNumberOfTargets, targets.Count - actualNumberOfTargets);
        }

        if(targets.Count > 0)
            primaryTarget = targets[0];
        return targets;
    }
    public override List<Entity> GetTargets(Effect source, Trigger trigger, Entity owner)
    {
        GetTargets(source, owner);
        /*if (conditionalOnTrigger && trigger.effect != null && targets.Contains(trigger.effect.Owner))
        {
            targets = conditionalTarget.GetTargets(source, owner);
        }*/
        return targets;
    }
    protected virtual bool IsValidTarget(Entity target) => true;
    protected virtual int SortTargets(Entity e1, Entity e2) 
    {  
        if(sortingMethod == TargetSorting.Unsorted) return UnityEngine.Random.Range(-1, 2);
        return 0; 
    }
}