using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

// These filters are for use primarily with Effect_AddTrigger in the inspector, but can be used in code as well

#region Entity Filters
[LabelText("Is: Player Character?")]
public class Task_Filter_IsPlayerCharacter : ITask<Entity>
{
    public bool DoTask(Entity entity)
    {
        return Stats.GetStat<Stat_PlayerCharacter>(entity).Value == entity;
    }
}

[LabelText("Is: Proxy?")]
public class Task_Filter_IsProxy : ITaskOwned<Entity, Entity>
{
    public bool DoTask(Entity owner, Entity entity)
    {
        return Stats.GetStat<Stat_PlayerCharacter>(entity).Value != entity;
    }

    public bool DoTask(Entity data) => false;
}

[LabelText("Filter: Has Items")]
public class Task_Filter_HasItems : ITask<Entity>
{
    [SerializeReference]
    public List<Item> items;
    public int multiples = 1;
    public bool DoTask(Entity entity)
    {
        if (entity.GetStat<Mechanic_Character>() == null) return false;

        List<Item> itemsLeft = new List<Item>();
        for (int i = 0; i < multiples; i++)
        {
            itemsLeft.AddRange(items);
        }
        foreach (Item item in Stats.GetStat<Stat_Viewer>(entity).Value.items)
        {
            itemsLeft.Remove(item);
        }
        if (itemsLeft.Count == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

[LabelText("Filter: Threshold")]
public class Task_Filter_ActivationThreshold : ITask<Entity>
{
    public int thresholdInclusive;
    [ShowInInspector, ReadOnly]
    public Dictionary<Entity, int> count = new();
    public bool DoTask(Entity entity)
    {
        count.TryAdd(entity, 0);
        count[entity]++;
        if (count[entity] >= thresholdInclusive)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
#endregion

#region Effect Filters
[LabelText("Is: Owner?")]
public class Task_Filter_IsOwner : ITaskOwned<Entity, Entity>, ITaskOwned<Entity, Effect>
{
    public bool DoTask(Entity data) => false;
    public bool DoTask(Effect data) => false;
    public bool DoTask(Entity owner, Entity data)
    {
        return data == owner;
    }
    public bool DoTask(Entity owner, Effect data)
    {
        return data.Owner == owner;
    }
}

[LabelText("Is: Target?")]
public class Task_Filter_IsTarget<T> : ITaskOwned<Entity, T> where T : Effect
{
    public bool DoTask(Entity owner, T effect)
    {
        return effect.Target.Equals(owner);
    }

    public bool DoTask(T data) => false;
}

[LabelText("Filter: Is Targetable By")]
public class Task_Filter_IsTargetableBy<T> : ITask<T> where T : Effect
{
    public EffectTargetingSelector targetingOwner;
    public EffectTargetingSelector targetableBy;
    [SerializeReference]
    public Targeting targeting;
    public bool DoTask(T effect)
    {
        return targeting.FindTargets(targetingOwner == EffectTargetingSelector.Owner ? effect.Target : effect.Owner)
            .Contains(targetableBy == EffectTargetingSelector.Owner ? effect.Target : effect.Owner);
    }
}

[LabelText("Filter: Damage Types")]
public class Task_Filter_DamageType : ITask<DamageInstance>, ITask<Hit>, ITask<Effect>
{
    public DamageType damageTypes;
    public bool excluded;
    public bool DoTask(DamageInstance damage)
    {
        foreach (Modifier_Damage modifier in damage.damageModifiers)
        {
            if (excluded)
            {
                if (((~(int)modifier.DamageType) & ((int)damageTypes)) != (int)damageTypes) return false;
            }
            else
            {
                if ((((int)modifier.DamageType) & ((int)damageTypes)) == (int)damageTypes) return true;
            }
            
        }
        return false ^ excluded;
    }
    public bool DoTask(Effect damage)
    {
        if (damage is not DamageInstance) return false;
        return DoTask(damage as DamageInstance);
    }

    public bool DoTask(Hit damage)
    {
        if (damage is not DamageInstance) return false;
        return DoTask(damage as DamageInstance);
    }
}

[LabelText("Filter: Damage Threshold")]
public class Task_Filter_DamageTreshold : ITask<DamageInstance>
{
    public float damageThreshold;
    public bool DoTask(DamageInstance damage)
    {
        if (damage.finalDamage >= damageThreshold) return true;
        else return false;
    }
}
#endregion

#region Player Filters
[LabelText("Filter: Player Threshold")]
public class Task_Filter_PlayerActivationThreshold : ITask<Viewer>
{
    public int thresholdInclusive;
    [ShowInInspector, ReadOnly]
    public Dictionary<Viewer, int> count = new();
    public bool lifetimeThreshold;
    public bool DoTask(Viewer viewer)
    {
        count.TryAdd(viewer, 0);
        count[viewer]++;
        if (lifetimeThreshold)
        {
            // TODO: Save to file/database
        }
        if (count[viewer] >= thresholdInclusive)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

[LabelText("Filter: Leaderboard Position")]
public class Task_Filter_LeadboardPosition : ITask<Viewer>
{
    public int topNumber;
    public bool DoTask(Viewer viewer)
    {
        if (Array.FindIndex(GameManager.ViewersScoreOrdered, x => x == viewer) < topNumber)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

[LabelText("Filter: Winstreak")]
public class Task_Filter_Winstreak : ITask<Viewer>
{
    public int threshold;
    public bool DoTask(Viewer viewer)
    {
        if (viewer.winstreak >= threshold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

[LabelText("Filter: Wins")]
public class Task_Filter_Wins : ITask<Viewer>
{
    public int threshold;
    public bool invert;
    public bool DoTask(Viewer viewer)
    {
        if (viewer.wins >= threshold)
        {
            return invert ? false : true;
        }
        else
        {
            return invert ? true : false;
        }
    }
}

[LabelText("Filter: Losses")]
public class Task_Filter_Losses : ITask<Viewer>
{
    public int threshold;
    public bool invert;
    public bool DoTask(Viewer viewer)
    {
        if (viewer.losses >= threshold)
        {
            return invert ? false : true;
        }
        else
        {
            return invert ? true : false;
        }
    }
}

[LabelText("Filter: Deaths")]
public class Task_Filter_Deaths : ITask<Viewer>
{
    public int threshold;
    public bool invert;
    public bool DoTask(Viewer viewer)
    {
        if (viewer.deaths >= threshold)
        {
            return invert ? false : true;
        }
        else
        {
            return invert ? true : false;
        }
    }
}

[LabelText("Filter: Gold Spent")]
public class Task_Filter_GoldSpent : ITask<Viewer>
{
    public int threshold;
    public bool invert;
    public bool DoTask(Viewer viewer)
    {
        int goldSpent = viewer.totalGold - viewer.gold; // TODO: make lifetime gold spent
        if (goldSpent >= threshold)
        {
            return invert ? false : true;
        }
        else
        {
            return invert ? true : false;
        }
    }
}
#endregion

#region Other Filters
[LabelText("Is: Specific Action?")]
public class Task_Filter_IsSpecificAction : ITask<Skill>
{
    public Skill comparison;
    public bool DoTask(Skill action)
    {
        return comparison == action;
    }
}

[LabelText("Filter: Skill Tags")]
public class Task_Filter_SkillTags : ITask<Skill>, ITaskOwned<Entity, (Entity entity, Skill skill)>
{
    public DamageType tags;
    public bool DoTask(Skill skill)
    {
        return skill.tags.HasFlag(tags);
    }

    public bool DoTask(Entity owner, (Entity entity, Skill skill) data)
    {
        return data.skill.tags.HasFlag(tags);
    }

    public bool DoTask((Entity entity, Skill skill) data)
    {
        return data.skill.tags.HasFlag(tags);
    }
}

[LabelText("Is: Entity the Main Target")]
public class Task_Filter_IsMainTarget : ITaskOwned<Entity, Entity>, ITaskOwned<Entity, (Entity entity, Skill skill)>
{
    public bool DoTask(Entity owner, Entity data)
    {
        return owner.GetStat<Stat_Targets>().Value == data;
    }

    public bool DoTask(Entity data)
    {
        return false;
    }

    public bool DoTask(Entity owner, (Entity entity, Skill skill) data)
    {
        return owner.GetStat<Stat_Targets>().Value == data.entity;
    }

    public bool DoTask((Entity entity, Skill skill) data)
    {
        return false;
    }
}

[LabelText("Is: Entity's Main Target In Range")]
public class Task_Filter_IsMainTargetInRage<T> : ITaskOwned<Entity, T>
{
    public Vector2 range;
    public bool DoTask(Entity owner, T data)
    {
        Entity target = owner.GetStat<Stat_Targets>().Value;
        if (target == null) return false;
        float dist = Vector3.Distance(target.transform.position, owner.transform.position);
        if (dist >= range.x && dist <= range.y) return true;
        return false;
    }

    public bool DoTask(T data)
    {
        return false;
    }
}

[LabelText("Is: Specific Spell?")]
public class Task_Filter_Spell : ITask<Spell>
{
    public List<RuneElement> runes;
    public bool containsPartOf;
    public bool ignoreOrderOfModifiers;
    public bool DoTask(Spell spell)
    {
        if (!containsPartOf && spell.runes.Count == runes.Count) return false;
        if (runes[0] != spell.runes[0].element) return false;
        if (ignoreOrderOfModifiers)
        {
            List<RuneElement> runesLeft = new List<RuneElement>();
            runesLeft.AddRange(runes);
            foreach (Rune rune in spell.runes)
            {
                runesLeft.Remove(rune.element);
            }
            if (runesLeft.Count == 0) return true;
            else return false;
        }
        else
        {
            for (int i = 1; i < runes.Count; i++)
            {
                if (spell.runes.Count == i)
                {
                    if (containsPartOf) break;
                    return false;
                }
                if (spell.runes[i].element != runes[i]) return false;
            }
            return true;
        }
    }
}

[LabelText("Is: Matching?")]
public class Task_Filter_Matching<T> : ITask<T>
{
    [SerializeReference]
    public T comparison;
    public bool DoTask(T data)
    {
        return data.Equals(comparison);
    }
}

public class StatContext_EntityOwner : ValueContainer<Entity> { }
[LabelText("Filter: Has Targets in Targeting")]
public class Task_Filter_HasAnyTargets<T> : ITaskOwned<Entity, T>
{
    [SerializeReference]
    public Targeting targeting;

    public bool DoTask(Entity owner, T data)
    {
        return targeting.FindTargets(owner).Count > 0;
    }

    public bool DoTask(T data)
    {
        return false;
    }
}

[LabelText("Is: Main Target In Range?")]
public class Task_Filter_MainTargetInRange<T> : ITaskOwned<Entity, T>
{
    public bool invert;
    [SerializeReference]
    public Vector2 distanceRange;

    public bool DoTask(Entity owner, T data)
    {
        Entity mainTarget = owner.GetStat<Stat_Targets>().Value;
        if (mainTarget == null) return false;
        float dist = Vector3.Distance(mainTarget.transform.position, owner.transform.position);
        return (dist >= distanceRange.x && dist <= distanceRange.y) ^ invert;
    }

    public bool DoTask(T data)
    {
        return false;
    }
}

[LabelText("Filter: Has Targets")]
public class Task_Filter_HasTargets<T> : ITaskOwned<Entity, T>
{
    public bool invert;

    public bool DoTask(Entity owner, T data)
    {
        bool targets = owner.GetStat<Stat_Targets>().Value != null;
        return targets ^ invert;
    }

    public bool DoTask(T data)
    {
        return false;
    }
}

[LabelText("Filter: Targets Have No Targets")]
public class Task_Filter_TargetsHaveNoTargets<T> : ITaskOwned<Entity, T>
{
    public Targeting targeting;
    public bool DoTask(Entity owner, T data)
    {
        foreach(Entity target in targeting.FindTargets(owner))
        {
            if (target.GetStat<Stat_Targets>().Value != null) return false;
        }
        return true;
    }

    public bool DoTask(T data)
    {
        return false;
    }
}

public class Task_Filter_FocusRange<T> : ITaskOwned<Entity, T>
{
    public Vector2 percentageRange;
    public bool DoTask(Entity owner, T data)
    {
        float focusPercent = owner.GetStat<Stat_CurrentFocus>().Value / owner.GetStat<Stat_MaximumFocus>().Value;
        return focusPercent >= percentageRange.x && focusPercent <= percentageRange.y;
    }

    public bool DoTask(T data)
    {
        return false;
    }
}
#endregion







