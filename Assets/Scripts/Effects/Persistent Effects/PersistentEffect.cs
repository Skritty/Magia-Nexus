using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public abstract class PersistentEffect: Effect
{
    [BoxGroup("Persistent Effect"), ReadOnly]
    public int tick;
    [BoxGroup("Persistent Effect")]
    public int tickDuration = -1;
    [BoxGroup("Persistent Effect")]
    public int maxStacks = 1;
    [BoxGroup("Persistent Effect")]
    public bool refreshDuration = true;

    public override void Activate()
    {
        if (!target.Stat<Stat_PersistentEffects>().AcceptsEffect(source, this)) return;

        PersistentEffect clone = Clone();
        clone.tick = 0;

        target.Stat<Stat_PersistentEffects>().AddEffect(clone);

        clone.OnGained();
    }

    public virtual void OnGained() { }
    public virtual void OnAction() { }
    public virtual void OnTick() { }
    public virtual void OnLost() { }

    public PersistentEffect Clone()
    {
        return (PersistentEffect)MemberwiseClone();
    }
}