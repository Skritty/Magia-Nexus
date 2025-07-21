using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Skritty.Tools.Utilities;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

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
        if (data.Get(out T value)) return DoTask(value, Owner); // TODO YOU WERE HERE
        return incompatableTriggerBehavior;
    }
    public abstract bool DoTask(T data, Entity Owner);
    public TriggerTask Clone()
    {
        TriggerTask clone = (TriggerTask)MemberwiseClone();
        return clone;
    }
}

// Source + Owner on effect, immutable
// Multiplier + Target carried through chain and can change
// Effect (2x)
// |-> Trigger<Effect>
//      |-> Effect (2x -> 4x)

// Task
// EffectInfo: source, owner, target, multiplier, task

public abstract class EffectTask : TriggerTask<Effect>
{
    [SerializeField, FoldoutGroup("@GetType()")]
    private float effectMultiplier = 1;
    [FoldoutGroup("@GetType()")]
    public int ignoreFrames;
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Targeting targetSelector = new Targeting_Self();

    public override bool DoTask(IDataContainer data, Entity owner)
    {
        data.Get(out Effect value);
        return DoTask(value, owner);
    }

    public override bool DoTask(Effect data, Entity owner)
    {
        List<Entity> targets = null;
        if(data == null)
        {
            targets = targetSelector.GetTargets(this, owner);
        }
        else
        {
            targets = targetSelector.GetTargets(this, data, owner);
        }
        foreach (Entity target in targets)
        {
            if (ignoreFrames > 0)
            {
                target.AddModifier<Stat_Untargetable>(new DummyModifier<(Entity, object)>(
                        value: (owner, this),
                        temporary: true, tickDuration: ignoreFrames
                        ));
            }
            DoEffect(owner, target, effectMultiplier * (data == null ? 1 : data.EffectMultiplier), data != null);
        }
        return true;
    }

    public abstract void DoEffect(Entity owner, Entity target, float multiplier, bool triggered);

    public new EffectTask Clone()
    {
        EffectTask clone = base.Clone() as EffectTask;
        clone.targetSelector = targetSelector.Clone();
        return clone;
    }
}

[LabelText("Task: Grant Buff")]
public class Effect_GrantModifer : EffectTask
{
    public Effect_GrantModifer() { }
    public Effect_GrantModifer(IModifier modifier) 
    {
        this.modifier = modifier;
    }

    public IModifier modifier;
    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        Target.AddModifier(modifier);
    }
}

[LabelText("Task: Do Random Effect")]
public class Task_DoRandomEffect : EffectTask
{
    public EffectTargetSelector proxy;
    public bool useProxyAsOwner;
    [SerializeReference]
    public List<EffectTask> effects;
    public List<Action> actions; // TODO: actions should NOT be a source of truth

    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        WeightedChance<EffectTask> random = new WeightedChance<EffectTask>();
        if (actions.Count > 0)
        {
            foreach (Action action in actions)
            {
                random.Add(action.effects[0], 1);
            }
        }
        else
        {
            foreach (EffectTask e in effects)
            {
                random.Add(e, 1);
            }
        }

        if (proxy != EffectTargetSelector.None)
        {
            Entity proxyEntity = proxy == EffectTargetSelector.Owner ? owner : target;
        }
        else
        {
            random.GetRandomEntry().DoTask(this, owner);
        }
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
        return true;
    }
}