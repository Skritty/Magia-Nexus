using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

[Serializable]
public abstract class TriggerTask
{
    public abstract bool TriggerRecieved(Trigger trigger, Entity Owner);
}

public abstract class TriggerTask<T> : TriggerTask where T : Trigger
{
    public override bool TriggerRecieved(Trigger trigger, Entity Owner)
    {
        if(trigger is T)
            return DoTask(trigger as T, Owner);
        return true;
    }
    protected abstract bool DoTask(T data, Entity Owner);
}

[LabelText("Do Effect")]
public class Task_DoEffect : TriggerTask<Trigger>
{
    [SerializeReference]
    public Effect effect;
    protected override bool DoTask(Trigger data, Entity Owner)
    {
        Trigger_Effect effectTrigger = data as Trigger_Effect;
        if (effectTrigger != null)
        {
            effect.Create(effectTrigger.effect);
        }
        else
        {
            effect.Create(Owner, data);
        }
        return true;
    }
}

[LabelText("Entity Owner")]
public class Task_Filter_EntityOwner : TriggerTask<Trigger_Entity>
{
    protected override bool DoTask(Trigger_Entity data, Entity Owner)
    {
        return data.entity.Stat<Stat_PlayerOwner>().Owner == Owner;
    }
}

[LabelText("Effect Owner")]
public class Task_Filter_EffectOwner : TriggerTask<Trigger_Effect>
{
    protected override bool DoTask(Trigger_Effect data, Entity Owner)
    {
        return data.effect.Owner == Owner;
    }
}

[LabelText("Effect Target")]
public class Task_Filter_EffectTarget : TriggerTask<Trigger_Effect>
{
    protected override bool DoTask(Trigger_Effect data, Entity Owner)
    {
        return data.effect.Target == Owner;
    }
}

[LabelText("Filter: Effect Tags")]
public class Task_Filter_EffectTags : TriggerTask<Trigger_Effect>
{
    [SerializeReference]
    public EffectTag tags;
    protected override bool DoTask(Trigger_Effect data, Entity Owner)
    {
        foreach(KeyValuePair<EffectTag, float> tag in data.effect.effectTags)
        {
            if (tag.Key.HasFlag(tags)) return true;
        }
        return false;
    }
}

[LabelText("Filter: Targetable")]
public class Task_Filter_Targetable : TriggerTask<Trigger_Effect>
{
    public EffectTargetingSelector selector;
    [SerializeReference]
    public Targeting targeting;
    protected override bool DoTask(Trigger_Effect data, Entity Owner)
    {
        return targeting.GetTargets(data.effect, data.effect.Owner)
                .Contains(selector == EffectTargetingSelector.Owner ? data.effect.Target : data.effect.Owner);
    }
}

public class Task_ModifyDamageInstanceRunes : TriggerTask<Trigger_OnSomethingHappens>
{
    protected override bool DoTask(Trigger_OnSomethingHappens data, Entity Owner)
    {
        Debug.Log(data.storedData);
        return true;
    }
}