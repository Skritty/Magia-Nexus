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
    [SerializeReference]
    public Effect effect;
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        effect.Create(Owner, trigger);
        return true;
    }
}

[LabelText("Is: Player Character?")]
public class Task_Filter_PlayerOwned : TriggerTask
{
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_OwnerEntity data))
            return data.Owner.Stat<Stat_PlayerOwner>().playerEntity == Owner;
        return incompatableTriggerBehavior;
    }
}

[LabelText("Is: Player Proxy?")]
public class Task_Filter_PlayerProxy : TriggerTask
{
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_OwnerEntity data))
            return data.Owner.Stat<Stat_PlayerOwner>().playerEntity == Owner.Stat<Stat_PlayerOwner>().playerEntity;
        return incompatableTriggerBehavior;
    }
}

[LabelText("Is: Owner?")]
public class Task_Filter_Owner : TriggerTask
{
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_OwnerEntity data2))
            return data2.Owner == Owner;
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

[LabelText("Filter: Damage Types")]
public class Task_Filter_DamageType : TriggerTask
{
    [SerializeReference]
    public DamageType damageTypes;
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        if (trigger.Is(out ITriggerData_DamageInstance data))
            foreach (EffectModifier modifier in data.Damage.damageModifiers)
            {
                if (modifier.damageType.HasFlag(damageTypes)) return true;
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

[LabelText("Task: Targetable")]
public class Task_ModifyDamageInstanceRunes : TriggerTask
{
    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        return incompatableTriggerBehavior;
    }
}