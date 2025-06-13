using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class TriggerTask
{
    public bool incompatableTriggerBehavior = true;
    public abstract bool DoTask(Trigger trigger, Entity Owner);
}

[LabelText("Task: Do Effect")]
public class Task_DoEffect : TriggerTask
{
    public EffectTargetSelector proxy;
    public bool useProxyAsOwner;
    [SerializeReference]
    public Effect effect;
    
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (proxy != EffectTargetSelector.None && trigger.Is(out ITriggerData_Effect data))
        {
            Entity proxyEntity = proxy == EffectTargetSelector.Owner ? data.Effect.Owner : data.Effect.Target;
            effect.Create(useProxyAsOwner ? proxyEntity : Owner, trigger, useProxyAsOwner ? null : proxyEntity);
        }
        else
        {
            effect.Create(Owner, trigger);
        }
        return true;
    }
}

[LabelText("Is: Player Character?")]
public class Task_Filter_PlayerOwned : TriggerTask
{
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_OwnerEntity data))
            return data.Entity.Stat<Stat_PlayerOwner>().playerEntity == Owner;
        return incompatableTriggerBehavior;
    }
}

[LabelText("Is: Player Proxy?")]
public class Task_Filter_PlayerProxy : TriggerTask
{
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_OwnerEntity data))
            return data.Entity.Stat<Stat_PlayerOwner>().playerEntity == Owner.Stat<Stat_PlayerOwner>().playerEntity;
        return incompatableTriggerBehavior;
    }
}

[LabelText("Is: Owner?")]
public class Task_Filter_Owner : TriggerTask
{
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_OwnerEntity data2))
            return data2.Entity == Owner;
        else if (trigger.Is(out ITriggerData_Effect data))
            return data.Effect.Owner == Owner;
        return incompatableTriggerBehavior;
    }
}

[LabelText("Is: Target?")]
public class Task_Filter_Target : TriggerTask
{
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_Effect data))
            return data.Effect.Target == Owner;
        return incompatableTriggerBehavior;
    }
}

[LabelText("Is: Specific Action?")]
public class Task_Filter_Action : TriggerTask
{
    public Action action;
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_Action data))
            return data.Action == action;
        return incompatableTriggerBehavior;
    }
}

[LabelText("Is: Specific Spell?")]
public class Task_Filter_Spell : TriggerTask
{
    public List<RuneElement> runes;
    public bool containsPartOf;
    public bool ignoreOrderOfModifiers;
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_Spell data))
        {
            if (!containsPartOf && data.Spell.runes.Count == runes.Count) return false;
            if (runes[0] != data.Spell.runes[0].element) return false;
            if (ignoreOrderOfModifiers)
            {
                List<RuneElement> runesLeft = new List<RuneElement>();
                runesLeft.AddRange(runes);
                foreach(Rune rune in data.Spell.runes)
                {
                    runesLeft.Remove(rune.element);
                }
                if (runesLeft.Count == 0) return true;
            }
            else
            {
                for (int i = 1; i < runes.Count; i++)
                {
                    if (data.Spell.runes.Count == i)
                    {
                        if (containsPartOf) break;
                        return false;
                    }
                    if (data.Spell.runes[i].element != runes[i]) return false;
                }
                return true;
            }
        }
        return incompatableTriggerBehavior;
    }
}

[LabelText("Filter: Damage Types")]
public class Task_Filter_DamageType : TriggerTask
{
    public DamageType damageTypes;
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_DamageInstance data))
            foreach (DamageModifier modifier in data.Damage.damageModifiers)
            {
                if (modifier.damageType.HasFlag(damageTypes)) return true;
            }
        return incompatableTriggerBehavior;
    }
}

[LabelText("Filter: Damage Threshold")]
public class Task_Filter_DamageTreshold : TriggerTask
{
    public float damageThreshold;
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_DamageInstance data))
            if (data.Damage.calculatedDamage >= damageThreshold) return true;
            else return false;
        return incompatableTriggerBehavior;
    }
}

[LabelText("Filter: Has Items")]
public class Task_Filter_HasItems : TriggerTask
{
    [SerializeReference]
    public List<Item> items;
    public int multiples = 1;
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_OwnerEntity data))
        {
            List<Item> itemsLeft = new List<Item>();
            for(int i = 0; i < multiples; i++)
            {
                itemsLeft.AddRange(items);
            }
            foreach (Item item in data.Entity.Stat<Stat_PlayerOwner>().player.items)
            {
                itemsLeft.Remove(item);
            }
            if(itemsLeft.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return incompatableTriggerBehavior;
    }
}

[LabelText("Filter: Targetable")]
public class Task_Filter_Targetable : TriggerTask
{
    public EffectTargetingSelector selector;
    [SerializeReference]
    public Targeting targeting;
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_Effect data))
            return targeting.GetTargets(data.Effect, data.Effect.Owner)
                .Contains(selector == EffectTargetingSelector.Owner ? data.Effect.Target : data.Effect.Owner);
        return incompatableTriggerBehavior;
    }
}

[LabelText("Task: Add Runes to Damage Instance")]
public class Task_ModifyDamageInstanceRunes : TriggerTask
{
    [SerializeReference]
    public List<Rune> runes;
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_DamageInstance data))
        {
            data.Damage.runes.AddRange(runes);
        }
        return incompatableTriggerBehavior;
    }
}

[LabelText("Filter: Threshold")]
public class Task_Filter_ActivationThreshold : TriggerTask
{
    public int thresholdInclusive;
    [ShowInInspector, ReadOnly]
    public Dictionary<Entity, int> count = new();
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_OwnerEntity data))
        {
            Entity entity = data.Entity;
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
        return incompatableTriggerBehavior;
    }
}

[LabelText("Filter: Player Threshold")]
public class Task_Filter_PlayerActivationThreshold : TriggerTask
{
    public int thresholdInclusive;
    [ShowInInspector, ReadOnly]
    public Dictionary<Viewer, int> count = new();
    public bool lifetimeThreshold;
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_Player data))
        {
            Viewer player = data.Player;
            count.TryAdd(player, 0);
            count[player]++;
            if (lifetimeThreshold)
            {
                // Save to file/database
            }
            if (count[player] >= thresholdInclusive)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return incompatableTriggerBehavior;
    }
}

[LabelText("Filter: Leaderboard Position")]
public class Task_Filter_LeadboardPosition : TriggerTask
{
    public int topNumber;
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_Player data))
        {
            if (Array.FindIndex(GameManager.ViewersScoreOrdered, x => x == data.Player) < topNumber)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return incompatableTriggerBehavior;
    }
}

[LabelText("Filter: Winstreak")]
public class Task_Filter_Winstreak : TriggerTask
{
    public int threshold;
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_Player data))
        {
            if (data.Player.winstreak >= threshold)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return incompatableTriggerBehavior;
    }
}

[LabelText("Filter: Wins")]
public class Task_Filter_Wins : TriggerTask
{
    public int threshold;
    public bool invert;
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_Player data))
        {
            if (data.Player.wins >= threshold)
            {
                return invert ? false : true;
            }
            else
            {
                return invert ? true : false;
            }
        }
        return incompatableTriggerBehavior;
    }
}

[LabelText("Filter: Losses")]
public class Task_Filter_Losses : TriggerTask
{
    public int threshold;
    public bool invert;
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_Player data))
        {
            if (data.Player.losses >= threshold)
            {
                return invert ? false : true;
            }
            else
            {
                return invert ? true : false;
            }
        }
        return incompatableTriggerBehavior;
    }
}

[LabelText("Filter: Deaths")]
public class Task_Filter_Deaths : TriggerTask
{
    public int threshold;
    public bool invert;
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_Player data))
        {
            if (data.Player.deaths >= threshold)
            {
                return invert ? false : true;
            }
            else
            {
                return invert ? true : false;
            }
        }
        return incompatableTriggerBehavior;
    }
}

[LabelText("Filter: Gold Spent")]
public class Task_Filter_GoldSpent : TriggerTask
{
    public int threshold;
    public bool invert;
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_Player data))
        {
            int goldSpent = data.Player.totalGold - data.Player.gold; // TODO: make lifetime gold spent
            if (goldSpent >= threshold)
            {
                return invert ? false : true;
            }
            else
            {
                return invert ? true : false;
            }
        }
        return incompatableTriggerBehavior;
    }
}

[LabelText("Task: Unlock Class")]
public class Task_UnlockClass : TriggerTask
{
    public string classUnlock;
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_Player data))
        {
            data.Player.unlockedClasses.Add(classUnlock);
        }
        return incompatableTriggerBehavior;
    }
}