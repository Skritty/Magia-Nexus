using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    None = 1,
    Lowest = 2,
    Highest = 4,
    Random = 8
}

[Serializable]
public abstract class Targeting
{
    protected Entity owner;
    public string name, description;
    public abstract List<Entity> GetTargets(Effect source, Entity owner);
    public abstract List<Entity> GetTargets(Effect source, Trigger trigger, Entity owner);
    public virtual void OnDrawGizmos(Transform owner) { }
    public virtual T Clone<T>() where T : Effect
    {
        return (T)MemberwiseClone();
    }
}

[Serializable]
public abstract class MultiTargeting : Targeting
{
    public TargetFilter entitiesAffected = TargetFilter.Enemies;
    public TargetSorting sortingMethod = TargetSorting.None;
    //public Vector2 targetingRange;
    public int numberOfTargets = -1;

    public override List<Entity> GetTargets(Effect source, Entity owner)
    {
        if(owner == null)
        {
            Debug.LogWarning("Owner of Targeting is null!");
            return new List<Entity>();
        }
        this.owner = owner;
        List<Entity> targets = new List<Entity>();
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

        if(sortingMethod != TargetSorting.None)
            targets.Sort(SortTargets);

        int actualNumberOfTargets = numberOfTargets + owner.Stat<Stat_Targeting>().numberOfTargets;
        if (numberOfTargets >= 0 && targets.Count > actualNumberOfTargets)
            targets.RemoveRange(actualNumberOfTargets, targets.Count - actualNumberOfTargets);
        return targets;
    }
    public override List<Entity> GetTargets(Effect source, Trigger trigger, Entity owner)
    {
        return GetTargets(source, owner);
    }
    protected virtual bool IsValidTarget(Entity target) => true;
    protected virtual int SortTargets(Entity e1, Entity e2) 
    {  
        if(sortingMethod == TargetSorting.Random) return UnityEngine.Random.Range(-1, 2);
        return 0; 
    }
}