using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Skritty.Tools.Utilities;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public abstract class TriggerTask
{
    public abstract bool DoTaskNoData(Entity Owner);
}

[Serializable]
public abstract class TriggerTask<T> : TriggerTask
{
    /// <summary>
    /// WARNING: the data input WILL be null (or default I guess)
    /// </summary>
    public override bool DoTaskNoData(Entity Owner)
    {
        return DoTask(default, Owner);
    }
    public abstract bool DoTask(T data, Entity Owner);
}

[Serializable]
public abstract class EffectTask<T> : TriggerTask<T>
{
    [FoldoutGroup("@GetType()")]
    public int ignoreFrames;
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Targeting targetSelector = new Targeting_Self();

    public override bool DoTask(T data, Entity owner)
    {
        Effect effect = data as Effect;
        List<Entity> targets = null;
        if(effect == null)
        {
            targets = targetSelector.GetTargets(this, owner);
        }
        else
        {
            targets = targetSelector.GetTargets(this, effect, owner);
        }
        foreach (Entity target in targets)
        {
            if (ignoreFrames > 0)
            {
                target.AddModifier(new DummyModifier<(Entity, object)>(
                        value: (owner, this), tickDuration: ignoreFrames
                        ));
            }
            DoEffect(owner, target, effect == null ? 1 : effect.EffectMultiplier, data != null);
        }
        return true;
    }

    public abstract void DoEffect(Entity owner, Entity target, float multiplier, bool triggered);

    public EffectTask<T> Clone()
    {
        EffectTask<T> clone = (EffectTask<T>)MemberwiseClone();
        clone.targetSelector = targetSelector.Clone();
        return clone;
    }
}

[LabelText("Task: Grant Buff")]
public class Effect_GrantModifer<T> : EffectTask<T>
{
    [SerializeReference, FoldoutGroup("@GetType()")]
    public IModifier modifier;

    public Effect_GrantModifer() { }
    public Effect_GrantModifer(IModifier modifier) 
    {
        this.modifier = modifier;
    }

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        if(multiplier != 1 && modifier is NumericalModifier)
        {
            NumericalModifier clone = (NumericalModifier)(modifier as Stat).Clone();
            Target.AddModifier(clone);
            clone.step = CalculationStep.Multiplicative;
            clone.AddModifier(multiplier);
            return;
        }
        else
        {
            Target.AddModifier(modifier);
        }
    }
}

[LabelText("Task: Do Random Effect")]
public class Task_DoRandomEffect<T> : EffectTask<T>
{
    public EffectTargetSelector proxy;
    public bool useProxyAsOwner;
    [SerializeReference]
    public List<EffectTask<Effect>> effects;
    public List<Action> actions; // TODO: actions should NOT be a source of truth

    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        WeightedChance<EffectTask<Effect>> random = new WeightedChance<EffectTask<Effect>>();
        if (actions.Count > 0)
        {
            foreach (Action action in actions)
            {
                random.Add(action.effects[0], 1);
            }
        }
        else
        {
            foreach (EffectTask<Effect> e in effects)
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
            random.GetRandomEntry().DoTask(default, owner);
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