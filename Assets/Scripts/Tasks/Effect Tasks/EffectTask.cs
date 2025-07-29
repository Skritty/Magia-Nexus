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
    ITaskOwned<Entity, Action>
{
    [FoldoutGroup("@GetType()")]
    public bool useTargetAsProxy;
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Targeting targetSelector = new Targeting_Self();

    public abstract void DoEffect(Entity owner, Entity target, float multiplier, bool triggered);

    public bool DoTaskNoData(Entity owner) => DoTask(owner, null, null);
    public bool DoTask(Entity owner, Entity data)
    {
        DoEffect(owner, data, 1, true);
        return true;
    }
    public bool DoTask(Entity owner, Effect data)
    {
        return DoTask(owner, data, useTargetAsProxy ? data.Target : null);
    }
    public bool DoTask(Entity owner, Effect data, Entity proxy)
    {
        List<Entity> targets = null;
        targets = targetSelector.GetTargets(data, owner, proxy);
        foreach (Entity target in targets)
        {
            DoEffect(owner, target, data != null ? data.EffectMultiplier : 1, data != null);
        }
        return true;
    }
    public bool DoTask(Entity owner, DamageInstance data) => DoTask(owner, data as Effect);
    public bool DoTask(Entity owner, Hit data) => DoTask(owner, data as Effect);
    public bool DoTask(Entity owner, Spell data) => DoTask(owner, data as Effect);
    public bool DoTask(Entity owner, Action data) => DoTask(owner, null, null);

    public EffectTask Clone()
    {
        EffectTask clone = (EffectTask)MemberwiseClone();
        clone.targetSelector = targetSelector.Clone();
        return clone;
    }

    
}