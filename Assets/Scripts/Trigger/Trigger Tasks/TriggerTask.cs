using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Skritty.Tools.Utilities;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[Serializable]
public abstract class TriggerTask
{
    public bool incompatableTriggerBehavior = true;
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
public abstract class EffectTask : TriggerTask<Effect>
{
    [FoldoutGroup("@GetType()")]
    public float effectMultiplier = 1;
    [FoldoutGroup("@GetType()")]
    public int ignoreFrames;
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Targeting targetSelector = new Targeting_Self();

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
                target.AddModifier(new DummyModifier<(Entity, object)>(
                        value: (owner, this), tickDuration: ignoreFrames
                        ));
            }
            DoEffect(owner, target, effectMultiplier * (data == null ? 1 : data.EffectMultiplier), data != null);
        }
        return true;
    }

    public abstract void DoEffect(Entity owner, Entity target, float multiplier, bool triggered);

    public EffectTask Clone()
    {
        EffectTask clone = (EffectTask)MemberwiseClone();
        clone.targetSelector = targetSelector.Clone();
        return clone;
    }
}

[LabelText("Task: Grant Buff")]
public class Effect_GrantModifer : EffectTask
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
            random.GetRandomEntry().DoTask(null, owner);
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