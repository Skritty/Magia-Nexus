using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class Solver<T> : IValueContainer<T>, IModifiable<T>, ISolver<T>, ISerializationCallbackReceiver
{
    protected T _value;
    protected bool changed = true;

    [ShowInInspector, FoldoutGroup("@GetType()")]
    public virtual T Value
    {
        get
        {
            if (changed)
            {
                changed = false;
                Solve();
            }
            return _value;
        }
        set
        {
            Modifiers.Clear();
            Add(value);
            changed = true;
        }
    }

    [field: SerializeReference, PropertyOrder(1), FoldoutGroup("@GetType()"), ReadOnly]
    public List<IValueContainer<T>> Modifiers { get; set; } = new();

    public bool IsDefaultValue() => Value.Equals(default);
    public bool TryGet<Type>(out Type data)
    {
        data = (Type)(Value as object);
        return data != null;
    }

    public System.Action Add(T value)
    {
        ValueContainer<T> modifier = new ValueContainer<T>(value);
        Add(modifier);
        return () => Remove(modifier);
    }

    public virtual void Add(IValueContainer<T> modifier)
    {
        modifier.AddTo(this);
        changed = true;
    }

    public void Remove(IValueContainer<T> modifier)
    {
        modifier.RemoveFrom(this);
        changed = true;
    }

    public void AddTo(IModifiable<T> modifiable) => modifiable.Modifiers.Add(this);
    public void RemoveFrom(IModifiable<T> modifiable) => modifiable.Modifiers.Remove(this);

    public virtual bool Contains(IValueContainer modifier, out int count)
    {
        count = 0;
        foreach (IValueContainer m in Modifiers)
        {
            if (m.Equals(modifier)) count++;
        }
        if (count > 0)
        {
            return true;
        }
        return false;
    }

    public virtual T Solve()
    {
        if(Modifiers.Count > 0)
        {
            _value = Modifiers[0].Value;
        }
        return _value;
    }

    public IModifiable Clone(bool preserveModifiers)
    {
        IModifiable<T> clone = (IModifiable<T>)MemberwiseClone();
        if(preserveModifiers) clone.Modifiers = new List<IValueContainer<T>>(Modifiers);
        return clone;
    }

    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize()
    {
        changed = true;
    }
}