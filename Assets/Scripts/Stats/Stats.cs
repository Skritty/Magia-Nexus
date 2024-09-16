using Sirenix.OdinInspector;
using System;
using UnityEngine;
public enum MergeBehavior { Additive, Multiplicative, Force }

[Serializable]
public abstract class BaseStat
{
    [HideInInspector]
    public Entity owner;
    [HideInInspector]
    public bool baseStat;
    [ShowIf("@owner == null")]
    public MergeBehavior mergeBehavior;
    public abstract void AddInstance(Entity owner);
    public abstract void RemoveInstance(Entity owner);
    protected virtual void Initialize() { }
    public virtual void Tick() { }

    /// <summary>
    /// Modifies the equivalent stat on the target entity
    /// </summary>
    public abstract void ModifyStat(Entity target);
}

public abstract class GenericStat<T> : BaseStat, IBoundInstances<Entity, T> where T : GenericStat<T>
{
    public override void AddInstance(Entity owner)
    {
        this.owner = owner;
        baseStat = true;
        IBoundInstances<Entity, T>.AddInstance((T)this, owner);
        Initialize();
    }

    public override void RemoveInstance(Entity owner)
    {
        IBoundInstances<Entity, T>.RemoveInstance(owner);
    }

    public override void ModifyStat(Entity owner)
    {
        T other = IBoundInstances<Entity, T>.GetInstance(owner);
        if (other == null) return;
        Merge(other);
    }
    protected virtual void Merge(T other) { }
}