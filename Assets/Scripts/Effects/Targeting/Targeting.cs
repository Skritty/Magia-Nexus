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

public abstract class Targeting
{
    protected Entity owner;
    public abstract List<Entity> GetTargets(object source, Entity owner);
    public abstract List<Entity> GetTargets(object source, Trigger trigger, Entity owner);
    public virtual void OnDrawGizmos(Transform owner) { }
    public virtual T Clone<T>() where T : Effect
    {
        return (T)MemberwiseClone();
    }
}

public abstract class MultiTargeting : Targeting
{
    public TargetFilter entitiesAffected = TargetFilter.Enemies;
    public TargetSorting sortingMethod = TargetSorting.None;
    //public Vector2 targetingRange;
    public int numberOfTargets;

    public override List<Entity> GetTargets(object source, Entity owner)
    {
        this.owner = owner;
        List<Entity> targets = new List<Entity>();
        TargetFilter targetType;
        foreach (Entity entity in Entity.FindObjectsOfType<Entity>())
        {
            // Can it be targeted?
            if (entity.Stat<Stat_Untargetable>().untargetable) continue;
            if (owner.Stat<Stat_Ignored>().IsIgnored(source, entity)) continue;

            // Is it in the affected entities?
            if(entity.Stat<Stat_Team>().team != owner.Stat<Stat_Team>().team)
            {
                targetType = TargetFilter.Enemies;
            }
            else if(entity == owner || entity == owner.Stat<Stat_Proxy>().proxyOwner)
            {
                targetType = TargetFilter.Self;
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
        if (numberOfTargets > 0 && targets.Count > numberOfTargets)
            targets.RemoveRange(numberOfTargets, targets.Count - numberOfTargets);
        return targets;
    }
    public override List<Entity> GetTargets(object source, Trigger trigger, Entity owner)
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