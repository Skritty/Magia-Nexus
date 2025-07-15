using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class Stat : IDataContainer
{
    public virtual void Tick(Entity Owner) { }
    public abstract void AddModifier(IModifier modifier);
    public abstract void RemoveModifier(IModifier modifier);
    public virtual void Solve() { }
    //public virtual void InverseSolve() { }
    public bool Get<T>(out T data)
    {
        IDataContainer<T> container = (this as IDataContainer<T>);
        if (container == null) data = default;
        else data = container.Value;
        return container != null;
    }
}

public abstract class Stat<T> : Stat, IDataContainer<T>
{
    public Action<T> OnChange;
    private T precalculatedModifier;
    public virtual T Value 
    { 
        get
        {
            return precalculatedModifier;
        }
        set
        {
            OnChange?.Invoke(value);
            precalculatedModifier = value;
        }
    }

    [field: SerializeReference, HideIf("@!(this is IStatTag)")]
    public List<IModifier<T>> Modifiers { get; } = new();
    public override void AddModifier(IModifier modifier)
    {
        IModifier<T> mod = (IModifier<T>)modifier;
        if (mod == null) return;
        Modifiers.Add(mod);
        Solve();
    }
    public override void RemoveModifier(IModifier modifier)
    {
        IModifier<T> mod = (IModifier<T>)modifier;
        Modifiers.Remove(mod);
        Solve();
    }
}

public interface IModifier : IDataContainer 
{
    public EffectTask Source {  get; set; }
    public IStatTag Tag { get; set; }

    public int MaxStacks { get; set; }
    public int Stacks { get; set; }
    public bool PerPlayer { get; set; }
    public Alignment Alignment { get; set; }

    public bool Temporary { get; set; }
    public int Tick { get; set; }
    public int TickDuration { get; set; }
    public bool RefreshDuration { get; set; }
    public void Solve();
}

public class ListStat<T> : Stat<List<T>> { }

public interface IModifier<T> : IModifier, IDataContainer<T> 
{
    public List<IModifier<T>> Modifiers { get; }
}

[Flags]
public enum Alignment 
{ 
    Neutral = 0, 
    Buff = 1, 
    Debuff = 2 
}

public interface IStatTag : IDataContainer { }
public class Stat_PreventExpire : EnumPrioritySolver<Alignment>, IStatTag { }
public class Stat_AoESize : NumericalSolver, IStatTag { }
public class Stat_Projectiles : NumericalSolver, IStatTag { }
public class Stat_Targets : NumericalSolver, IStatTag { }
public class Stat_CastTargets : NumericalSolver, IStatTag { }
public class Stat_Removeable : NumericalSolver, IStatTag { }
public class Stat_Knockback : NumericalSolver, IStatTag { }
public class Stat_Enmity : NumericalSolver, IStatTag { }
public class Stat_Untargetable : ListStat<(Entity, int)>, IStatTag { }
public class Stat_Team : PrioritySolver<int>, IStatTag { }
public class Stat_SummonCount : NumericalSolver, IStatTag { }
public class Stat_Summons : ListStat<Entity>, IStatTag { }
public class Stat_Proxies : ListStat<Entity>, IStatTag { }
public class Stat_RuneCrystals : ListStat<Rune>, IStatTag { }
public class Stat_TeamPlayers : NumericalSolver, IStatTag { }
public class Stat_MaxSummons : NumericalSolver, IStatTag { }
public class Stat_TargetingMethod : PrioritySolver<Targeting>, IStatTag { }
public class Stat_MovementTargetingMethod : PrioritySolver<Targeting>, IStatTag { }