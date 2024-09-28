using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public abstract class PersistentEffect: Effect
{
    [FoldoutGroup("PersistentEffect", -9), ReadOnly]
    public int tick;
    [FoldoutGroup("PersistentEffect", -9)]
    public int tickDuration = -1;
    [FoldoutGroup("PersistentEffect", -9)]
    public int maxStacks = 1;
    [FoldoutGroup("PersistentEffect", -9)]
    public bool refreshDuration = true;

    public override void DoEffect()
    {
        Owner.Trigger<Trigger_OnActivateEffect>(this, this);
        Activate();
    }

    public override void Activate()
    {
        Owner = Owner.Stat<Stat_PlayerOwner>().playerEntity;
        Target.Stat<Stat_PersistentEffects>().ApplyEffect(this);
    }

    public virtual void OnGained() { }
    public virtual void OnTick() { }
    public virtual void OnLost() { }

    public virtual void ApplyContribution()
    {
        if (!contributeToAssists || tick <= 0) return;
        Owner.Stat<Stat_PlayerOwner>().ApplyContribution(Target, baseEffectMultiplier * tick);
    }

    public PersistentEffect Clone()
    {
        return (PersistentEffect)MemberwiseClone();
    }
}