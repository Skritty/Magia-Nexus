using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

public abstract class InheritedModifier<T> : IValueContainer<T>, ISolver
{
    protected T _value;

    [ShowInInspector, FoldoutGroup("@GetType()")]
    public virtual T Value
    {
        get
        {
            Solve();
            return _value;
        }
    }

    public bool IsDefaultValue() => Value.Equals(default);
    public bool TryGet<Type>(out Type data)
    {
        data = (Type)(Value as object);
        return data != null;
    }
    public abstract void Solve();

    public void AddTo(IModifiable<T> modifiable) { }

    public void RemoveFrom(IModifiable<T> modifiable) { }
}

public class Stat_Parent : PrioritySolver<object>, IStat<object> { }

public class InheritFromParent<T> : InheritedModifier<T>
{
    public Entity self;
    [SerializeReference]
    public IValueContainer<T> referenceStat;

    public override void Solve() => _value = self.GetStat<Stat_Parent>().Value.GetStat(referenceStat).Value;
}