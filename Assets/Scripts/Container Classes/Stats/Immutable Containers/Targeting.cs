using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Random = UnityEngine.Random;

public enum EffectTargetingSelector { Owner, Target }

[Serializable]
public class Targeting : ImmutableContainer<List<Entity>>
{
    [FoldoutGroup("@GetType()"), InfoBox("If all values are left as default, only self will be returned!", visibleIfMemberName: "@targetingConditions.Count == 0")]
    public int numberOfTargets = -1;
    [FoldoutGroup("@GetType()")]
    public int ignoreFrames;
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<TargetingCondition> targetingConditions = new();
    [SerializeReference, FoldoutGroup("@GetType()")]
    public TargetingSorting targetingSorting = null;

    public List<Entity> FindTargets(Entity owner)
    {
        "statContextData".GetStat<StatContext_EntityOwner>().Value = owner;
        return Solve();
    }

    public override List<Entity> Solve()
    {
        Entity owner = "statContextData".GetStat<StatContext_EntityOwner>().Value; // TODO: remove
        List<Entity> targets = new List<Entity>();
        _value = targets;

        if (owner == null || numberOfTargets == 0) return _value;

        // If this is a default targeting, just return the owner (this might not be desired)
        if(targetingConditions.Count == 0 && targetingSorting == null)
        {
            if (owner.GetStat<Stat_Untargetable>().Contains((owner, this), out _)) return _value;
            Ignore(owner, owner);
            targets.Add(owner);
            return _value;
        }

        bool valid;
        foreach (Entity target in Entity.FindObjectsByType<Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) // TODO: Don't use find objects by type
        {
            if (target.GetStat<Stat_Untargetable>().Contains((owner, this), out _)) continue;

            valid = true;
            foreach (TargetingCondition condition in targetingConditions)
            {
                if (!condition.IsValid(owner, target))
                {
                    valid = false;
                    break;
                }
            }
            if (valid)
            {
                targets.Add(target);
                Ignore(owner, target);
            }
        }

        if(targetingSorting != null)
        {
            targets.Sort((x, y) => targetingSorting.SortTargets(owner, x, y));
        }

        if (numberOfTargets >= 0)
        {
            int actualNumberOfTargets = numberOfTargets + (int)owner.GetStat<Stat_AdditionalTargets>().Value;
            if (targets.Count > actualNumberOfTargets)
                targets.RemoveRange(actualNumberOfTargets, targets.Count - actualNumberOfTargets);
        }

        return _value;
    }
    protected void Ignore(object boundObject, object target)
    {
        if (ignoreFrames > 0)
        {
            target.GetStat<Stat_Untargetable>().AddModifier(
                new Modifier<(object, object)>(
                    value: (boundObject, this),
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
public abstract class TargetingCondition
{
    public abstract bool IsValid(object boundObject, Entity target);
}

[Serializable]
public abstract class TargetingSorting
{
    public enum TargetSorting
    {
        Unsorted = 1,
        Lowest = 2,
        Highest = 4
    }
    public TargetSorting sortingMethod = TargetSorting.Unsorted;
    public abstract int SortTargets(object boundObject, Entity e1, Entity e2);
}

public class TargetingCondition_Exclude : TargetingCondition
{
    public List<Entity> excluded = new();

    public TargetingCondition_Exclude() { }
    public TargetingCondition_Exclude(params Entity[] excluded)
    {
        this.excluded.AddRange(excluded);
    }

    public override bool IsValid(object boundObject, Entity target)
    {
        return !excluded.Contains(target);
    }
}

public class TargetingCondition_LineOfSight : TargetingCondition
{
    public Vector3 offset;
    public override bool IsValid(object boundObject, Entity target)
    {
        Transform transform = null;
        switch (boundObject)
        {
            case MonoBehaviour:
                transform = (boundObject as MonoBehaviour).transform;
                break;
            case GameObject:
                transform = (boundObject as GameObject).transform;
                break;
            case Transform:
                transform = boundObject as Transform;
                break;
        }
        if (transform == null) return false;
        return Physics.Raycast(transform.position + offset, target.transform.position - (transform.position + offset));
    }
}

public class TargetingCondition_EntityRelation : TargetingCondition
{
    [Flags]
    public enum TargetFilter
    {
        Self = 1,
        Parent = 2,
        Child = 4,
        Allies = 8,
        Enemies = 16,
    }
    public TargetFilter entitiesAffected = TargetFilter.Enemies;

    public override bool IsValid(object boundObject, Entity target)
    {
        Entity owner = null;
        switch (boundObject)
        {
            case Entity:
                owner = boundObject as Entity;
                break;
            case GameObject:
                owner = (boundObject as GameObject).GetComponent<Entity>();
                break;
        }
        if (owner == null) return false;

        TargetFilter targetType;
        if (target.GetStat<Stat_Faction>().Value != owner.GetStat<Stat_Faction>().Value)
        {
            targetType = TargetFilter.Enemies;
        }
        else if (target == owner)
        {
            targetType = TargetFilter.Self;
        }
        else if (target == (object)owner.GetStat<Stat_Parent>().Value)
        {
            targetType = TargetFilter.Parent;
        }
        else if(owner == (object)target.GetStat<Stat_Parent>().Value)
        {
            targetType = TargetFilter.Child;
        }
        else
        {
            targetType = TargetFilter.Allies;
        }
        return entitiesAffected.HasFlag(targetType);
    }
}

public class TargetingCondition_NotIntangible : TargetingCondition
{
    public override bool IsValid(object boundObject, Entity target)
    {
        return target.GetStat<Stat_Intangable>().Value;
    }
}

public class TargetingCondition_Radial : TargetingCondition
{
    public float radius;
    [Range(0, 180)]
    public float angle = 180;
    public Vector3 offset;

    public override bool IsValid(object boundObject, Entity target)
    {
        Entity owner = null;
        switch (boundObject)
        {
            case Entity:
                owner = boundObject as Entity;
                break;
            case GameObject:
                owner = (boundObject as GameObject).GetComponent<Entity>();
                break;
        }
        if (owner == null) return false;

        // TODO: add proxy targeting center, freeform center, and encapulated in shape
        Vector3 dirToEntity = target.transform.position - owner.transform.position + Quaternion.FromToRotation(Vector3.up, owner.GetStat<Mechanic_Movement>().facingDir) * offset;
        if (dirToEntity.magnitude > radius * boundObject.GetStat<Stat_AoESize>().Value) return false;
        if (angle >= 180) return true;

        Vector3 dirToTarget = boundObject.GetStat<Mechanic_Movement>().facingDir;
        float fromTo = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(dirToEntity, dirToTarget) / (dirToEntity.magnitude * dirToTarget.magnitude));
        if (float.IsNaN(fromTo)) fromTo = 0;
        if (fromTo > angle) return false;
        return true;
    }
}

public class TargetingCondition_Line : TargetingCondition
{
    public float length, width;
    public bool faceFirstTarget;
    private Vector3 dirToTarget;
    public Vector3 offset;

    public override bool IsValid(object boundObject, Entity target)
    {
        Entity owner = null;
        switch (boundObject)
        {
            case Entity:
                owner = boundObject as Entity;
                break;
            case GameObject:
                owner = (boundObject as GameObject).GetComponent<Entity>();
                break;
        }
        if (owner == null) return false;

        if (length == 0 || width == 0) return false;

        // TODO: add proxy targeting center, freeform center, and encapulated in shape
        Vector3 dirToEntity = target.transform.position - owner.transform.position + Quaternion.FromToRotation(Vector3.up, owner.GetStat<Mechanic_Movement>().facingDir) * offset;
        dirToTarget = /*firstTarget && faceFirstTarget ? dirToEntity : */Stats.GetStat<Mechanic_Movement>(owner).facingDir;
        Vector3 projectedDir = Vector3.Project(dirToEntity, dirToTarget);
        if (Vector3.Dot(dirToEntity, dirToTarget) < 0) return false;
        if (projectedDir.magnitude > length * Stats.GetStat<Stat_AoESize>(owner).Value) return false;
        if ((dirToEntity - projectedDir).magnitude > width / 2 * Stats.GetStat<Stat_AoESize>(owner).Value) return false;
        return true;
    }
}

public abstract class TargetingSorting_StatSorted<T> : TargetingSorting
{
    [SerializeReference]
    public IValueContainer<T> stat;
}

public class TargetingSorting_StatSortedFloat : TargetingSorting_StatSorted<float>
{
    public override int SortTargets(object boundObject, Entity e1, Entity e2)
    {
        float e1Stat = e1.GetStat(stat).Value;
        float e2Stat = e2.GetStat(stat).Value;
        float e1Enmity = e1.GetStat<Stat_Enmity>().Value;
        float e2Enmity = e2.GetStat<Stat_Enmity>().Value;
        switch (sortingMethod)
        {
            case TargetSorting.Highest:
                {
                    if (e1Stat * e1Enmity > e2Stat * e2Enmity) return -1;
                    return 1;
                }
            case TargetSorting.Lowest:
                {
                    if (e1Stat / e1Enmity < e2Stat / e2Enmity) return -1;
                    return 1;
                }
            default:
                {
                    if (e1Enmity == e2Enmity) return Random.Range(-1, 2);
                    else if (e1Enmity > e2Enmity) return -1;
                    return 1;
                }
        }
    }
}

public class TargetingSorting_StatSortedFloatPercent : TargetingSorting_StatSorted<float>
{
    [SerializeReference]
    public IValueContainer<float> maximum;

    public override int SortTargets(object boundObject, Entity e1, Entity e2)
    {
        float e1Stat = e1.GetStat(stat).Value / e1.GetStat(maximum).Value;
        float e2Stat = e2.GetStat(stat).Value / e2.GetStat(maximum).Value;
        float e1Enmity = e1.GetStat<Stat_Enmity>().Value;
        float e2Enmity = e2.GetStat<Stat_Enmity>().Value;
        switch (sortingMethod)
        {
            case TargetSorting.Highest:
                {
                    if (e1Stat * e1Enmity > e2Stat * e2Enmity) return -1;
                    else return 1;
                }
            case TargetSorting.Lowest:
                {
                    if (e1Stat / e1Enmity < e2Stat / e2Enmity) return -1;
                    else return 1;
                }
            default:
                {
                    if (e1Enmity == e2Enmity) return Random.Range(-1, 2);
                    else if (e1Enmity > e2Enmity) return -1;
                    return 1;
                }
        }
    }
}

public class TargetingSorting_StatSortedInt : TargetingSorting_StatSorted<int>
{
    public override int SortTargets(object boundObject, Entity e1, Entity e2)
    {
        int e1Stat = e1.GetStat(stat).Value;
        int e2Stat = e2.GetStat(stat).Value;
        float e1Enmity = e1.GetStat<Stat_Enmity>().Value;
        float e2Enmity = e2.GetStat<Stat_Enmity>().Value;
        switch (sortingMethod)
        {
            case TargetSorting.Highest:
                {
                    if (e1Stat * e1Enmity > e2Stat * e2Enmity) return -1;
                    return 1;
                }
            case TargetSorting.Lowest:
                {
                    if (e1Stat / e1Enmity < e2Stat / e2Enmity) return -1;
                    return 1;
                }
            default:
                {
                    if (e1Enmity == e2Enmity) return Random.Range(-1, 2);
                    else if (e1Enmity > e2Enmity) return -1;
                    return 1;
                }
        }
    }
}

public class TargetingSorting_StatSortedIntPercent : TargetingSorting_StatSorted<int>
{
    [SerializeReference]
    public IValueContainer<int> maximum;

    public override int SortTargets(object boundObject, Entity e1, Entity e2)
    {
        int e1Stat = e1.GetStat(stat).Value / e1.GetStat(maximum).Value;
        int e2Stat = e2.GetStat(stat).Value / e2.GetStat(maximum).Value;
        float e1Enmity = e1.GetStat<Stat_Enmity>().Value;
        float e2Enmity = e2.GetStat<Stat_Enmity>().Value;
        switch (sortingMethod)
        {
            case TargetSorting.Highest:
                {
                    if (e1Stat * e1Enmity > e2Stat * e2Enmity) return -1;
                    else return 1;
                }
            case TargetSorting.Lowest:
                {
                    if (e1Stat / e1Enmity < e2Stat / e2Enmity) return -1;
                    else return 1;
                }
            default:
                {
                    if (e1Enmity == e2Enmity) return Random.Range(-1, 2);
                    else if (e1Enmity > e2Enmity) return -1;
                    return 1;
                }
        }
    }
}

public class TargetingSorting_Distance : TargetingSorting
{
    public override int SortTargets(object boundObject, Entity e1, Entity e2)
    {
        Transform transform = null;
        switch (boundObject)
        {
            case MonoBehaviour:
                transform = (boundObject as MonoBehaviour).transform;
                break;
            case GameObject:
                transform = (boundObject as GameObject).transform;
                break;
            case Transform:
                transform = boundObject as Transform;
                break;
        }
        if (transform == null) return 0;

        float e1dist = Vector3.Distance(transform.position, e1.transform.position);
        float e2dist = Vector3.Distance(transform.position, e2.transform.position);
        float e1Enmity = e1.GetStat<Stat_Enmity>().Value;
        float e2Enmity = e2.GetStat<Stat_Enmity>().Value;
        switch (sortingMethod)
        {
            case TargetSorting.Highest:
                {
                    if (e1dist * e1Enmity > e2dist * e2Enmity) return -1;
                    return 1;
                }
            case TargetSorting.Lowest:
                {
                    if (e1dist / e1Enmity < e2dist / e2Enmity) return -1;
                    return 1;
                }
            default:
                {
                    if (e1Enmity == e2Enmity) return Random.Range(-1, 2);
                    else if (e1Enmity > e2Enmity) return -1;
                    return 1;
                }
        }
    }
}