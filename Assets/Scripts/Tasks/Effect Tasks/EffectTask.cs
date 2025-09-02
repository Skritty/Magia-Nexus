using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


[Serializable]
public abstract class EffectTask : 
    ITaskOwned<Entity, Effect>,
    ITaskOwned<Entity, DamageInstance>,
    ITaskOwned<Entity, Hit>,
    ITaskOwned<Entity, Spell>,
    ITaskOwned<Entity, Entity>,
    ITaskOwned<Entity, Action>,
    ITaskOwned<Entity, Rune>
{
    [FoldoutGroup("@GetType()")]
    public bool useTargetAsProxy;
    [FoldoutGroup("@GetType()")]
    public bool propagateTarget;
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Targeting targetSelector = new Targeting_Self();

    public abstract void DoEffect(Entity owner, Entity target, float multiplier, bool triggered);
    public bool DoTaskNoData(Entity owner, Entity proxy = null) => DoTaskTargetSelector(owner, null, false, proxy);
    public bool DoTask(Entity owner, Entity target, Entity proxy = null)
    {
        if (propagateTarget)
        {
            return DoTask(owner, target, null, true, useTargetAsProxy ? target : proxy);
        }
        else
        {
            return DoTaskTargetSelector(owner, null, true, useTargetAsProxy ? target : proxy);
        }
    }
    public virtual bool DoTask(Entity owner, Entity target, Effect data, bool triggered, Entity proxy = null)
    {
        DoEffect(owner, target, data != null ? data.EffectMultiplier : owner.Stat<Stat_EffectMultiplier>().Value, data != null);
        return true;
    }
    public bool DoTaskTargetSelector(Entity owner, Effect data, bool triggered, Entity proxy = null)
    {
        List<Entity> targets = null;
        targets = targetSelector.GetTargets(data, owner, proxy);
        foreach (Entity target in targets)
        {
            DoTask(owner, target, data, triggered, proxy);
        }
        return true;
    }
    
    #region Triggered
    public bool DoTask(Entity owner, Effect data)
    {
        if (propagateTarget)
        {
            return DoTask(owner, data.Target, data, true, useTargetAsProxy ? data.Target : null);
        }
        else
        {
            return DoTaskTargetSelector(owner, data, true, useTargetAsProxy ? data.Target : null);
        }
    }
    public bool DoTask(Entity owner, Entity target)
    {
        if (propagateTarget)
        {
            return DoTask(owner, target, null, true, useTargetAsProxy ? target : null);
        }
        else
        {
            return DoTaskTargetSelector(owner, null, true, useTargetAsProxy ? target : null);
        }
    }
    public bool DoTask(Entity owner) => DoTaskTargetSelector(owner, null, true);
    public bool DoTask(Entity owner, DamageInstance data) => DoTask(owner, data as Effect);
    public bool DoTask(Entity owner, Hit data) => DoTask(owner, data as Effect);
    public bool DoTask(Entity owner, Spell data) => DoTask(owner, data as Effect);
    public bool DoTask(Entity owner, Action data) => DoTaskTargetSelector(owner, null, true);
    public bool DoTask(Entity owner, Rune data) => DoTaskTargetSelector(owner, null, true);
    public bool DoTask(Effect data) => false;
    public bool DoTask(DamageInstance data) => false;
    public bool DoTask(Hit data) => false;
    public bool DoTask(Spell data) => false;
    public bool DoTask(Action data) => false;
    public bool DoTask(Rune data) => false;
    #endregion
    public EffectTask Clone()
    {
        EffectTask clone = (EffectTask)MemberwiseClone();
        clone.targetSelector = targetSelector.Clone();
        return clone;
    }
}