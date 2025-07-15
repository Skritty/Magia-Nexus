using Sirenix.OdinInspector;
using System;
using System.Collections;
/*using System.Collections.Generic;
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
    public int stacks = 1;
    [FoldoutGroup("PersistentEffect", -9)]
    public bool perPlayer;
    [FoldoutGroup("PersistentEffect", -9)]
    public bool refreshDuration = true;
    [FoldoutGroup("PersistentEffect", -9)]
    public bool contributeToAssists = false;
    public enum Alignment { Neutral, Buff, Debuff }
    [FoldoutGroup("PersistentEffect", -9)]
    public Alignment alignment;
    [FoldoutGroup("PersistentEffect", -9)]
    public VFX_PersistentEffect vfx;

    public void DoForAllStacks(System.Action method, int overrideAmount = -1)
    {
        for (int i = 0; i < (overrideAmount > -1 ? overrideAmount : stacks); i++)
        {
            method?.Invoke();
        }
    }

    public override void Activate()
    {
        //Owner = Owner.Stat<Stat_PlayerOwner>().playerEntity;
        Target.GetMechanic<Stat_PersistentEffects>().ApplyEffect(this);
    }
    public void Gained()
    {
        if(vfx != null)
        {
            vfx = vfx.PlayVFX<VFX_PersistentEffect>(Target.transform, Vector3.zero, Vector3.up, true);
            vfx.ApplyPersistentEffect(this);
        }
        OnGained();
    }
    public virtual void OnGained() { }
    public virtual void OnTick() { }
    public void Lost()
    {
        if (vfx != null)
        {
            vfx.StopVFX();
        }
        OnLost();
    }
    public virtual void OnLost() { }

    public virtual void ApplyContribution()
    {
        if (!contributeToAssists || tick <= 0) return;
        Owner.GetMechanic<Stat_PlayerOwner>().ApplyContribution(Target, effectMultiplier * tick);
    }
}*/