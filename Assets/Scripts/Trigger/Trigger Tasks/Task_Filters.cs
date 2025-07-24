using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

// These filters are for use primarily with Effect_AddTrigger in the inspector, but can be used in code as well

#region Entity Filters
[LabelText("Is: Player Character?")]
public class Task_Filter_IsPlayerCharacter : TriggerTask<Entity>
{
    public override bool DoTask(Entity entity, Entity Owner)
    {
        return entity.GetMechanic<Mechanic_PlayerOwner>().playerEntity == Owner;
    }
}

[LabelText("Is: Player Proxy?")]
public class Task_Filter_IsPlayerProxy : TriggerTask<Entity>
{
    public override bool DoTask(Entity entity, Entity Owner)
    {
        return entity.GetMechanic<Mechanic_PlayerOwner>().playerEntity == Owner.GetMechanic<Mechanic_PlayerOwner>().playerEntity;
    }
}

[LabelText("Filter: Has Items")]
public class Task_Filter_HasItems : TriggerTask<Entity>
{
    [SerializeReference]
    public List<Item> items;
    public int multiples = 1;
    public override bool DoTask(Entity entity, Entity Owner)
    {
        if (entity.GetMechanic<Mechanic_PlayerOwner>() == null) return false;

        List<Item> itemsLeft = new List<Item>();
        for (int i = 0; i < multiples; i++)
        {
            itemsLeft.AddRange(items);
        }
        foreach (Item item in entity.GetMechanic<Mechanic_PlayerOwner>().player.items)
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
public class Task_Filter_ActivationThreshold : TriggerTask<Entity>
{
    public int thresholdInclusive;
    [ShowInInspector, ReadOnly]
    public Dictionary<Entity, int> count = new();
    public override bool DoTask(Entity entity, Entity Owner)
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
public class Task_Filter_IsOwner<T> : TriggerTask<T> where T : Effect
{
    public override bool DoTask(T effect, Entity Owner)
    {
        return effect.Owner == Owner;
    }
}

[LabelText("Is: Target?")]
public class Task_Filter_IsTarget<T> : TriggerTask<T> where T : Effect
{
    public override bool DoTask(T effect, Entity Owner)
    {
        return effect.Target == Owner;
    }
}

[LabelText("Filter: Is Targetable By")]
public class Task_Filter_IsTargetableBy<T> : TriggerTask<T> where T : Effect
{
    public EffectTargetingSelector selector;
    [SerializeReference]
    public Targeting targeting;
    public override bool DoTask(T effect, Entity Owner)
    {
        return targeting.GetTargets(effect, effect.Owner)
            .Contains(selector == EffectTargetingSelector.Owner ? effect.Target : effect.Owner);
    }
}

[LabelText("Filter: Damage Types")]
public class Task_Filter_DamageType : TriggerTask<DamageInstance>
{
    public DamageType damageTypes;
    public override bool DoTask(DamageInstance damage, Entity Owner)
    {
        foreach (DamageSolver modifier in damage.damageModifiers)
        {
            if (modifier.damageType.HasFlag(damageTypes)) return true;
        }
        return false;
    }
}

[LabelText("Filter: Damage Threshold")]
public class Task_Filter_DamageTreshold : TriggerTask<DamageInstance>
{
    public float damageThreshold;
    public override bool DoTask(DamageInstance damage, Entity Owner)
    {
        if (damage.finalDamage >= damageThreshold) return true;
        else return false;
    }
}
#endregion

#region Player Filters
[LabelText("Filter: Player Threshold")]
public class Task_Filter_PlayerActivationThreshold : TriggerTask<Viewer>
{
    public int thresholdInclusive;
    [ShowInInspector, ReadOnly]
    public Dictionary<Viewer, int> count = new();
    public bool lifetimeThreshold;
    public override bool DoTask(Viewer viewer, Entity Owner)
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
public class Task_Filter_LeadboardPosition : TriggerTask<Viewer>
{
    public int topNumber;
    public override bool DoTask(Viewer viewer, Entity Owner)
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
public class Task_Filter_Winstreak : TriggerTask<Viewer>
{
    public int threshold;
    public override bool DoTask(Viewer viewer, Entity Owner)
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
public class Task_Filter_Wins : TriggerTask<Viewer>
{
    public int threshold;
    public bool invert;
    public override bool DoTask(Viewer viewer, Entity Owner)
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
public class Task_Filter_Losses : TriggerTask<Viewer>
{
    public int threshold;
    public bool invert;
    public override bool DoTask(Viewer viewer, Entity Owner)
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
public class Task_Filter_Deaths : TriggerTask<Viewer>
{
    public int threshold;
    public bool invert;
    public override bool DoTask(Viewer viewer, Entity Owner)
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
public class Task_Filter_GoldSpent : TriggerTask<Viewer>
{
    public int threshold;
    public bool invert;
    public override bool DoTask(Viewer viewer, Entity Owner)
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
public class Task_Filter_IsSpecificAction : TriggerTask<Action>
{
    public Action comparison;
    public override bool DoTask(Action action, Entity Owner)
    {
        return comparison == action;
    }
}

[LabelText("Is: Specific Spell?")]
public class Task_Filter_Spell : TriggerTask<Spell>
{
    public List<RuneElement> runes;
    public bool containsPartOf;
    public bool ignoreOrderOfModifiers;
    public override bool DoTask(Spell spell, Entity Owner)
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
#endregion







