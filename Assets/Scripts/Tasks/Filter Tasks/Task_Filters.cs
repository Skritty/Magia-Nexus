using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

// These filters are for use primarily with Effect_AddTrigger in the inspector, but can be used in code as well

#region Entity Filters
[LabelText("Is: Player Character?")]
public class Task_Filter_IsPlayerCharacter : ITask<Entity>
{
    public bool DoTask(Entity entity)
    {
        return entity.Stat<Stat_PlayerCharacter>().Value == entity;
    }
}

[LabelText("Is: Proxy?")]
public class Task_Filter_IsProxy : ITaskOwned<Entity, Entity>
{
    public bool DoTask(Entity owner, Entity entity)
    {
        return entity.Stat<Stat_PlayerCharacter>().Value != entity;
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
        if (entity.GetMechanic<Mechanic_Character>() == null) return false;

        List<Item> itemsLeft = new List<Item>();
        for (int i = 0; i < multiples; i++)
        {
            itemsLeft.AddRange(items);
        }
        foreach (Item item in entity.Stat<Stat_Viewer>().Value.items)
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
    public EffectTargetingSelector selector;
    [SerializeReference]
    public Targeting targeting;
    public bool DoTask(T effect)
    {
        return targeting.GetTargets(effect, effect.Owner)
            .Contains(selector == EffectTargetingSelector.Owner ? effect.Target : effect.Owner);
    }
}

[LabelText("Filter: Damage Types")]
public class Task_Filter_DamageType : ITask<DamageInstance>, ITask<Effect>
{
    public DamageType damageTypes;
    public bool DoTask(DamageInstance damage)
    {
        foreach (Modifier_Damage modifier in damage.damageModifiers)
        {
            if (modifier.DamageType.HasFlag(damageTypes)) return true;
        }
        return false;
    }
    public bool DoTask(Effect damage)
    {
        if (damage is not DamageInstance) return false;
        foreach (Modifier_Damage modifier in (damage as DamageInstance).damageModifiers)
        {
            if (modifier.DamageType.HasFlag(damageTypes)) return true;
        }
        return false;
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
public class Task_Filter_IsSpecificAction : ITask<Action>
{
    public Action comparison;
    public bool DoTask(Action action)
    {
        return comparison == action;
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

[LabelText("Filter: Has Any Targets")]
public class Task_Filter_HasAnyTargets<T> : ITaskOwned<Entity, T>
{
    [SerializeReference]
    public Targeting targeting;

    public bool DoTask(Entity owner, T data)
    {
        return targeting.GetTargets(owner).Count > 0;
    }

    public bool DoTask(T data)
    {
        return false;
    }
}
#endregion







