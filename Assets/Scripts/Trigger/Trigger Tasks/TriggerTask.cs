using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Skritty.Tools.Utilities;
using UnityEngine;

[Serializable]
public abstract class TriggerTask
{
    public bool incompatableTriggerBehavior = true;
    public abstract bool DoTask(IDataContainer data, Entity Owner);
}

public abstract class TriggerTask<T> : TriggerTask
{
    public override bool DoTask(IDataContainer data, Entity Owner)
    {
        if (data.Get(out T value)) return DoTask(value, Owner);
        return incompatableTriggerBehavior;
    }
    public abstract bool DoTask(T data, Entity Owner);
}

// Source + Owner on effect, immutable
// Multiplier + Target carried through chain and can change
// Effect (2x)
// |-> Trigger<Effect>
//      |-> Effect (2x -> 4x)

// Task
// EffectInfo: source, owner, target, multiplier, task

public abstract class EffectTask : TriggerTask<EffectTask>
{
    protected int _sourceID;
    //[FoldoutGroup("@GetType()"), ShowInInspector, ReadOnly]
    public int SourceID
    {
        get
        {
            if (_sourceID == 0) _sourceID = GetHashCode();
            return _sourceID;
        }
    }

    [FoldoutGroup("@GetType()")]
    public float effectMultiplier = 1;
    [FoldoutGroup("@GetType()")]
    public int ignoreFrames;
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Targeting targetSelector = new Targeting_Self();

    public override bool DoTask(IDataContainer data, Entity Owner)
    {
        data.Get(out EffectTask value);
        return DoTask(value, Owner);
    }

    public override bool DoTask(EffectTask data, Entity Owner)
    {
        foreach(Entity target in targetSelector.GetTargets(null, Owner))
        {
            if (ignoreFrames > 0)
                new PE_IgnoreEntity(this, ignoreFrames);
            DoEffect(Owner, target, effectMultiplier * (data == null ? 1 : data.effectMultiplier), data != null);
        }
        return true;
    }

    public abstract void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered);
}

[LabelText("Task: Grant Buff")]
public class Effect_GrantModifer : EffectTask
{
    public IModifier modifier;
    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        Target.AddModifier(modifier);
    }
}

[LabelText("Task: Do Random Effect")]
public class Task_DoRandomEffect : TriggerTask
{
    public EffectTargetSelector proxy;
    public bool useProxyAsOwner;
    [SerializeReference]
    public List<Effect> effects;
    public List<Action> actions; // TODO: actions should NOT be a source of truth

    public override bool DoTask(Trigger trigger, Entity Owner)
    {
        WeightedChance<Effect> random = new WeightedChance<Effect>();
        if (actions.Count > 0)
        {
            foreach (Action action in actions)
            {
                random.Add(action.effects[0], 1);
            }
        }
        else
        {
            foreach (Effect e in effects)
            {
                random.Add(e, 1);
            }
        }
            
        if (proxy != EffectTargetSelector.None && trigger.Get(out Effect effect))
        {
            Entity proxyEntity = proxy == EffectTargetSelector.Owner ? effect.Owner : effect.Target;
            random.GetRandomEntry().CreateFromTrigger(useProxyAsOwner ? proxyEntity : Owner, effect, useProxyAsOwner ? null : proxyEntity);
        }
        else
        {
            random.GetRandomEntry().Create(Owner);
        }
        return true;
    }
}



[LabelText("Task: Unlock Class")]
public class Task_UnlockClass : TriggerTask<Viewer>
{
    public string classUnlock;
    public override bool DoTask(Viewer viewer, Entity Owner)
    {
        viewer.unlockedClasses.Add(classUnlock);
        return true;
    }
}

[LabelText("Task: Add Runes to Damage Instance")]
public class Task_ModifyDamageInstanceRunes : TriggerTask<DamageInstance>
{
    [SerializeReference]
    public List<Rune> runes;
    public override bool DoTask(DamageInstance damage, Entity Owner)
    {
        damage.runes.AddRange(runes);
    }
}