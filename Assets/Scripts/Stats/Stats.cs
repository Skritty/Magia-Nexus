using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[Serializable]
public abstract class Stat : IDataContainer, IEqualityComparer<Stat>
{
    public abstract bool Get<Type>(out Type data);
    public virtual void Tick(Entity Owner) { }
    public abstract void AddModifier(IDataContainer modifier);
    public abstract void RemoveModifier(IDataContainer modifier);
    public abstract bool ContainsModifier(IDataContainer modifier, out int count);
    public virtual void Solve() { }
    //public virtual void InverseSolve() { }

    public bool Equals(Stat x, Stat y)
    {
        return x.Equals(y);
    }

    public int GetHashCode(Stat obj)
    {
        return GetType().GetHashCode();
    }
}

public abstract class Stat<T> : Stat, IDataContainer<T>
{
    public Action<T> OnChange;
    protected T _value;
    private bool changed;
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
    }

    [field: SerializeReference, HideIf("@!(this is IStatTag)")]
    public List<IDataContainer<T>> Modifiers { get; } = new();

    public override bool Get<Type>(out Type data)
    {
        data = (Type)(Value as object);
        return data != null;
    }

    public void AddModifier(T value)
    {
        Modifiers.Add(new DataContainer<T>(value));
        changed = true;
    }

    public override void AddModifier(IDataContainer modifier)
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
}

public class ListStat<T> : Stat<List<T>> { }
public class QueueStat<T> : Stat<Queue<T>> { }

public interface IModifier : IDataContainer
{
    public IStatTag Tag { get; }
    public Alignment Alignment { get; }
    public int MaxStacks { get; }
    public int StacksAdded { get; }
    public bool PerPlayer { get; }
    public bool Temporary { get; }
    public int TickDuration { get; }
    public bool RefreshDuration { get; }
}

public interface IModifier<T> : IModifier, IDataContainer<T>
{
    public List<IDataContainer<T>> Modifiers { get; }
    public void AddModifier(IDataContainer modifier);
}

public class DummyModifier<T> : IDataContainer<T>, IModifier
{
    public T Value { get; }
    public IStatTag Tag { get; }
    public Alignment Alignment { get; }
    public int MaxStacks { get; }
    public int StacksAdded { get; } = 1;
    public bool PerPlayer { get; }
    public bool Temporary { get; }
    public int TickDuration { get; }
    public bool RefreshDuration { get; }

    public DummyModifier() { }

    public DummyModifier(T value = default, IStatTag tag = default, Alignment alignment = Alignment.Neutral,
        int maxStacks = 0, int stacksAdded = 1, bool perPlayer = false, 
        bool temporary = false, int tickDuration = 0, bool refreshDuration = false)
    {
        Value = value;
        Tag = tag;
        Alignment = alignment;
        MaxStacks = maxStacks;
        StacksAdded = stacksAdded;
        PerPlayer = perPlayer;
        Temporary = temporary;
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
    public void AddModifier(IDataContainer modifier);
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