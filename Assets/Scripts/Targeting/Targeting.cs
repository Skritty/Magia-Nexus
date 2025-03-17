using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
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
    Unsorted = 1,
    Lowest = 2,
    Highest = 4
}

public enum EffectTargetingSelector { Owner, Target }

[Serializable]
public abstract class Targeting
{
    protected Entity Owner;
    protected Entity proxy;
    [ShowInInspector]
    protected Entity primaryTarget;
    public bool lockTarget;
    
    public abstract List<Entity> GetTargets(Effect source, Entity owner, Entity proxy = null);
    public abstract List<Entity> GetTargets(Effect source, Trigger trigger, Entity owner);
    public virtual void OnDrawGizmos(Transform owner) { }
    public virtual Targeting Clone()
    {
        return (Targeting)MemberwiseClone();
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
    protected Vector3 GetCenter()
    {
        if(proxy != null)
        {
            return proxy.transform.position + Quaternion.FromToRotation(Vector3.up, Owner.Stat<Stat_Movement>().facingDir) * offset; // Direction still determined by owner
        }
        else
        {
            return Owner.transform.position + Quaternion.FromToRotation(Vector3.up, Owner.Stat<Stat_Movement>().facingDir) * offset;
        }
        
    }

    public override List<Entity> GetTargets(Effect source, Entity owner, Entity proxy = null)
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
        this.proxy = proxy;
        targets = new List<Entity>();
        TargetFilter targetType;
        bool firstTarget = true;
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
            if (!IsValidTarget(entity, firstTarget)) continue;
            if (firstTarget) firstTarget = false;

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
        {
            DoFX(source, targets);
            primaryTarget = targets[0];
        }
            
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
    protected virtual void DoFX(Effect source, List<Entity> targets)
    {
        foreach(Entity target in targets)
        {
            VFX_Damage damage = vfx.PlayVFX<VFX_Damage>(target.transform, offset, Vector3.up, true);
            damage.ApplyDamage(source as DamageInstance);
        }
    }
    protected virtual bool IsValidTarget(Entity target, bool firstTarget) => true;
    protected virtual int SortTargets(Entity e1, Entity e2) 
    {  
        if(sortingMethod == TargetSorting.Unsorted) return UnityEngine.Random.Range(-1, 2);
        return 0; 
    }
}