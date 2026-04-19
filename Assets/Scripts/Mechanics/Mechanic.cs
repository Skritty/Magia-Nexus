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
    public void AddInstance(Entity owner)
    {
        Owner = owner;
        baseStat = true;
        owner.AddStat(this);
    }
    public void Remove(Entity owner)
    {
        owner.RemoveStat(this);
    }
    public virtual void Initialize() { }
    public virtual void Tick() { }
    public virtual void OnDestroy() { }
}