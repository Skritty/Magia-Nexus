using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TwitchLib.Api.Helix.Models.Search;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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

public abstract class Stat<T> : Stat, IDataContainer<T>
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
                Solve();
                changed = false;
            }
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
        if(modifier is IDataContainer<T>)
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
        foreach(IDataContainer<T> m in Modifiers)
        {
            if (m.Equals(modifier)) count++;
        }
        if(count > 0)
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

public class ListStat<T> : Stat<T>, IEnumerable<T>
{
    public int Count => Modifiers.Count;
    public T this[int i]
    {
        get
        {
            if (Modifiers.Count >= i) return default;
            return Modifiers[i].Value;
        }
    }

    public T[] ToArray
    {
        get
        {
            T[] array = new T[Modifiers.Count];
            for(int i = 0; i < Modifiers.Count; i++)
            {
                array[i] = Modifiers[i].Value;
            }
            return array;
        }
    }

    public bool Contains(T value)
    {
        return Modifiers.Exists(x => x.Value.Equals(value));
    }

    public void Remove(T value)
    {
        foreach(IDataContainer<T> data in ToArray)
        {
            if(data is DataContainer && value.Equals(data.Value))
            {
                Modifiers.Remove(data);
                return;
            }
        }
    }

    public void Clear()
    {
        Modifiers.Clear();
        changed = true;
        OnChange?.Invoke(Value);
    }

    public void AddModifiers(ICollection<T> values)
    {
        foreach (T value in values)
        {
            AddModifier(value);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        List<T> array = new();
        foreach(IDataContainer<T> data in Modifiers)
        {
            array.Add(data.Value);
        }
        return array.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class QueueStat<T> : Stat<Queue<T>> { }

public interface IModifier : IDataContainer
{
    public IStatTag Tag { get; }
    public Alignment Alignment { get; }
    public int MaxStacks { get; }
    public int StacksAdded { get; }
    public bool PerPlayer { get; }
    public int TickDuration { get; }
    public bool RefreshDuration { get; }
}

public interface IModifier<T> : IModifier, IDataContainer<T>
{
    public List<IDataContainer<T>> Modifiers { get; }
    public void AddModifier<Data>(Data modifier) where Data : IDataContainer;
}

public class DummyModifier<T> : IDataContainer<T>, IModifier
{
    [SerializeField, FoldoutGroup("@GetType()")]
    public T Value { get; }
    [field: SerializeReference, FoldoutGroup("Modifier")]
    public IStatTag Tag { get; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public Alignment Alignment { get; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public int MaxStacks { get; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public int StacksAdded { get; } = 1;
    [field: SerializeField, FoldoutGroup("Modifier")]
    public bool PerPlayer { get; }
    [field: ShowInInspector, FoldoutGroup("Modifier")]
    public int TickDuration { get; }
    [field: ShowInInspector, FoldoutGroup("Modifier"), ReadOnly]
    public bool RefreshDuration { get; }

    public DummyModifier() { }

    public DummyModifier(T value = default, IStatTag tag = default, Alignment alignment = Alignment.Neutral,
        int maxStacks = 0, int stacksAdded = 1, bool perPlayer = false, 
        int tickDuration = 0, bool refreshDuration = false)
    {
        Value = value;
        Tag = tag;
        Alignment = alignment;
        MaxStacks = maxStacks;
        StacksAdded = stacksAdded;
        PerPlayer = perPlayer;
        TickDuration = tickDuration;
        RefreshDuration = refreshDuration;
    }

    public bool Get<T1>(out T1 data)
    {
        IDataContainer<T1> container = (IDataContainer<T1>)this;
        if (container == null) data = default;
        else data = container.Value;
        return container != null;
    }
}

[Flags]
public enum Alignment 
{ 
    Neutral = 0, 
    Buff = 1, 
    Debuff = 2 
}

public interface IStatTag : IDataContainer 
{
    public void AddModifier<Data>(Data modifier) where Data : IDataContainer;
}
public class Stat_Triggers : Stat<Alignment>, IStatTag { }
public class Stat_PreventExpire : EnumPrioritySolver<Alignment>, IStatTag { }
public class Stat_AoESize : NumericalSolver, IStatTag { }
public class Stat_Projectiles : NumericalSolver, IStatTag { }
public class Stat_Targets : NumericalSolver, IStatTag { }
public class Stat_CastTargets : NumericalSolver, IStatTag { }
public class Stat_Removeable : NumericalSolver, IStatTag { }
public class Stat_Knockback : NumericalSolver, IStatTag { }
public class Stat_Enmity : NumericalSolver, IStatTag { }
public class Stat_Untargetable : ListStat<(Entity, object)>, IStatTag { }
public class Stat_Team : PrioritySolver<int>, IStatTag { }
public class Stat_SummonCount : NumericalSolver, IStatTag { }
public class Stat_Summons : ListStat<Entity>, IStatTag { }
public class Stat_Proxies : ListStat<Entity>, IStatTag { }
public class Stat_RuneCrystals : ListStat<Rune>, IStatTag { }
public class Stat_TeamPlayers : NumericalSolver, IStatTag { }
public class Stat_MaxSummons : NumericalSolver, IStatTag { }
public class Stat_TargetingMethod : PrioritySolver<Targeting>, IStatTag { }
public class Stat_MovementTargetingMethod : PrioritySolver<Targeting>, IStatTag { }