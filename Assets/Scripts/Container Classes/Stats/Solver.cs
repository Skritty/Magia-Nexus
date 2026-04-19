using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class Solver<T> : IDataContainer<T>, IModifiable<T>, ISolver, ISerializationCallbackReceiver
{
    protected T _value;
    protected bool changed;

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
    public List<IDataContainer<T>> Modifiers { get; set; }

    public bool IsDefaultValue() => Value.Equals(default);
    public bool TryGet<Type>(out Type data)
    {
        data = (Type)(Value as object);
        return data != null;
    }

    public System.Action Add(T value)
    {
        DataContainer<T> data = new DataContainer<T>(value);
        Add(data);
        return () => Modifiers.Remove(data);
    }

    public bool TryAdd(IDataContainer modifier)
    {
        IDataContainer<T> cast = modifier as IDataContainer<T>;
        if (cast == null)
        {
            Debug.LogWarning($"{modifier} failed to add!");
            return false;
        }
        Add(cast);
        return true;
    }

    public virtual void Add(IDataContainer<T> modifier)
    {
        Modifiers.Add(modifier);
        changed = true;
    }

    public virtual void Remove(IDataContainer modifier)
    {
        Modifiers.Remove(modifier as IDataContainer<T>);
        changed = true;
    }

    public virtual bool Contains(IDataContainer modifier, out int count)
    {
        count = 0;
        foreach (IDataContainer m in Modifiers)
        {
            if (m.Equals(modifier)) count++;
        }
        if (count > 0)
        {
            return true;
        }
        return false;
    }

    public virtual void Solve()
    {
        if(Modifiers.Count > 0) _value = Modifiers[0].Value;
    }

    public IModifiable Clone(bool preserveModifiers)
    {
        IModifiable<T> clone = (IModifiable<T>)MemberwiseClone();
        if(preserveModifiers) clone.Modifiers = new List<IDataContainer<T>>(Modifiers);
        return clone;
    }

    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize()
    {
        changed = true;
    }
}