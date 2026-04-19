using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class InheritedModifier<T> : IDataContainer<T>, ISolver
{
    public T Value { get; protected set; }
    public bool IsDefaultValue() => Value.Equals(default);
    public bool TryGet<Type>(out Type data)
    {
        data = (Type)(Value as object);
        return data != null;
    }
    public abstract void Solve();
}

public class Stat_Parent : PrioritySolver<object>, IStat<object> { }

public class InheritFromParent<T> : InheritedModifier<T>
{
    public Entity self;
    [SerializeReference]
    public IStat<T> referenceStat;

    public override void Solve()
    {
        Value = self.GetStat<Stat_Parent>().GetStat(referenceStat).Value;
    }
}