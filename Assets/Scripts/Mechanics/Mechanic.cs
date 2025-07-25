using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using System;
public enum MergeBehavior { Additive, Multiplicative, Force }

[Serializable]
public abstract class Mechanic
{
    [HideInInspector]
    public Entity Owner;
    [HideInInspector]
    public bool baseStat;
    [ShowIf("@Owner == null")]
    public MergeBehavior mergeBehavior;
    public abstract void AddInstance(Entity owner);
    public abstract void RemoveInstance(Entity owner);
    protected virtual void Initialize() { }
    public virtual void Tick() { }
    public virtual void OnDestroy() { }

    /// <summary>
    /// Modifies the equivalent stat on the target entity
    /// </summary>
    public abstract void ModifyStat(Entity target);
}

public abstract class Mechanic<T> : Mechanic, IBoundInstances<Entity, T> where T : Mechanic<T>
{
    public override void AddInstance(Entity owner)
    {
        this.Owner = owner;
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