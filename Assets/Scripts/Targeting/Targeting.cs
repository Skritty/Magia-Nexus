using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Validation;
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
public class Stat_LockTarget : BooleanPrioritySolver, IStat<bool> { }
public class Stat_LockedTargets : ListPrioritySolver<Entity>, IStat<List<Entity>> { }

[Serializable]
public abstract class Targeting
{
    public int ignoreFrames;
    public abstract List<Entity> GetTargets(Entity owner, Entity proxy = null);
    public virtual List<Entity> GetTargets<T>(T triggerData, Entity owner, Entity proxy = null)
    {
        if (Stats.GetStat<Stat_LockTarget>(owner).Value)
        {
            List<Entity> lockedTargets = Stats.GetStat<Stat_LockedTargets>(owner).Value;
            if (lockedTargets == null)
            {
                lockedTargets = new List<Entity>();
                Stats.GetStat<Stat_LockedTargets>(owner).Add(lockedTargets);
            }
            lockedTargets.RemoveAll(x => x == null || x.gameObject.activeSelf == false);
            if (lockedTargets.Count == 0)
            {
                lockedTargets.AddRange(GetTargets(owner, proxy));
            }
            return lockedTargets;
        }
        return GetTargets(owner, proxy);
    }
    protected void AddToIgnored(Entity owner, Entity target)
    {
        if (ignoreFrames > 0)
        {
            target.GetStat<Stat_Untargetable>().AddModifier(
                new Modifier<(Entity, object)>(
                    value: (owner, this), 
                    tickDuration: ignoreFrames));
        }
    }
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
    public bool lineOfSight;
    public bool ignoreIntangeable;
    public int numberOfTargets = -1;
    public Vector3 offset;
    public VFX vfx;

    protected Vector3 GetCenter(Entity owner, Entity proxy)
    {
        if(proxy != null)
        {
            return proxy.transform.position + Quaternion.FromToRotation(Vector3.up, Stats.GetStat<Mechanic_Movement>(owner).facingDir) * offset; // Direction still determined by owner
        }
        else
        {
            return owner.transform.position + Quaternion.FromToRotation(Vector3.up, Stats.GetStat<Mechanic_Movement>(owner).facingDir) * offset;
        }
        
    }

    public override List<Entity> GetTargets(Entity owner, Entity proxy = null)
    {
        if(owner == null) return new List<Entity>();

        List<Entity> targets = new List<Entity>();
        TargetFilter targetType;
        bool firstTarget = true;
        foreach (Entity entity in Entity.FindObjectsByType<Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) // TODO: Don't use find objects by type
        {
            if (!ignoreIntangeable && entity.GetStat<Stat_Intangable>().Value) continue;
            // Can it be targeted?
            if (entity.GetStat<Stat_Untargetable>().Contains((owner, this), out _)) continue;

            // Is it in the affected entities?
            if(entity.GetStat<Stat_Team>().Value != Stats.GetStat<Stat_Team>(owner).Value)
            {
                targetType = TargetFilter.Enemies;
            }
            else if(entity == owner)
            {
                targetType = TargetFilter.Self;
            }
            else if (entity == owner.GetStat<Stat_PlayerCharacter>().Value)
            {
                targetType = TargetFilter.Owner;
            }
            else
            {
                targetType = TargetFilter.Allies;
            }
            if (!entitiesAffected.HasFlag(targetType)) continue;
            if (lineOfSight) if (Physics.Raycast(owner.transform.position + offset, entity.transform.position - (owner.transform.position + offset))) continue;
            if (!IsValidTarget(owner, proxy, entity, firstTarget)) continue;
            if (firstTarget) firstTarget = false;

            // Is it within the targeting range?
            //float distance = Vector3.Distance(entity.transform.position, owner.transform.position);
            //if (distance < targetingRange.x * owner.Stat<Stat_Effect>().aoeMultiplier) continue;
            //if (distance > targetingRange.y * owner.Stat<Stat_Effect>().aoeMultiplier) continue;

            targets.Add(entity);
            AddToIgnored(owner, entity);
        }

        if(sortingMethod != TargetSorting.Unsorted)
            targets.Sort((x, y) => SortTargets(owner, x, y));

        if(numberOfTargets >= 0)
        {
            int actualNumberOfTargets = numberOfTargets + (int)owner.GetStat<Stat_AdditionalTargets>().Value;
            if (numberOfTargets >= 0 && targets.Count > actualNumberOfTargets)
                targets.RemoveRange(actualNumberOfTargets, targets.Count - actualNumberOfTargets);
        }

        if(targets.Count > 0)
        {
            DoFX(owner, proxy, targets);
        }
            
        return targets;
    }
    protected virtual void DoFX(Entity owner, Entity proxy, List<Entity> targets)
    {
        /*if (vfx == null) return;
        foreach(Entity target in targets)
        {
            VFX_Damage damage = vfx.PlayVFX<VFX_Damage>(target.transform, offset, Vector3.up, true);
            damage.ApplyDamage(source as Effect_Damage<Effect>);
        }*/
    }
    protected virtual bool IsValidTarget(Entity owner, Entity proxy, Entity target, bool firstTarget) => true;
    protected virtual int SortTargets(Entity owner, Entity e1, Entity e2) 
    {  
        if(sortingMethod == TargetSorting.Unsorted) return UnityEngine.Random.Range(-1, 2);
        return 0; 
    }
}