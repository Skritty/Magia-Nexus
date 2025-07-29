using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class Stat : IDataContainer, IEqualityComparer<Stat>
{
    public abstract bool Get<Type>(out Type data);
    public virtual void Tick(Entity Owner) { }
    public abstract void AddModifier<Data>(Data modifier) where Data : IDataContainer;
    public abstract void RemoveModifier(IDataContainer modifier);
    public abstract bool ContainsModifier(IDataContainer modifier, out int count);
    public virtual void Solve() { Debug.Log("default solve"); }
    //public virtual void InverseSolve() { }

    public bool Equals(Stat x, Stat y)
    {
        return x.Equals(y);
    }

    public int GetHashCode(Stat obj)
    {
        return GetType().GetHashCode();
    }
    public abstract Stat Clone();
}

public interface IModifiable
{
    public void AddModifier<Data>(Data modifier) where Data : IDataContainer;
    public void RemoveModifier(IDataContainer modifier);
    public bool ContainsModifier(IDataContainer modifier, out int count);
}

public abstract class Stat<T> : Stat, IDataContainer<T>, IModifiable
{
    public Action<T> OnChange;
    [SerializeField, HideInInspector]
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
            /*if(_value == null)
            {
                Debug.LogError($"{GetType()} is null!");
            }*/
            return _value;
        }
        protected set
        {
            Modifiers.Clear();
            AddModifier(value);
        }
    }

    [field: SerializeReference, PropertyOrder(1), FoldoutGroup("@GetType()"), ReadOnly]
    public List<IDataContainer<T>> Modifiers { get; set; } = new();

    public override bool Get<Type>(out Type data)
    {
        data = (Type)(Value as object);
        return data != null;
    }

    public System.Action AddModifier(T value)
    {
        DataContainer<T> data = new DataContainer<T>(value);
        Modifiers.Add(data);
        changed = true;
        OnChange?.Invoke(Value);
        return () => Modifiers.Remove(data);
    }

    public override void AddModifier<Data>(Data modifier)
    {
        if (modifier is IDataContainer<T>)
        {
            Modifiers.Add(modifier as IDataContainer<T>);
            changed = true;
            OnChange?.Invoke(Value);
        }
    }

    public override void RemoveModifier(IDataContainer modifier)
    {
        if (modifier is IDataContainer<T>)
        {
            Modifiers.Remove(modifier as IDataContainer<T>);
            changed = true;
            OnChange?.Invoke(Value);
        }
    }

    public override bool ContainsModifier(IDataContainer modifier, out int count)
    {
        count = 0;
        foreach (IDataContainer<T> m in Modifiers)
        {
            if (m.Equals(modifier)) count++;
        }
        if (count > 0)
        {
            return true;
        }
        return false;
    }

    public override Stat Clone()
    {
        Stat<T> clone = (Stat<T>)MemberwiseClone();
        clone.Modifiers = new List<IDataContainer<T>>(Modifiers);
        return clone;
    }
}