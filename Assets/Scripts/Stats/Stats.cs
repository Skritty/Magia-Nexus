using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class Stat : IDataContainer, IEquatable<Stat>
{
    public abstract void AddModifier(Stat modifier);
    public abstract void RemoveModifier(Stat modifier);
    public virtual void Solve() { }
    public virtual void InverseSolve() { }
    public abstract bool Equals(Stat other);
    public bool Is<T>(out T data) where T : class, IDataContainer
    {
        data = this as T;
        return data != null;
    }
}
public abstract class Stat<T> : Stat, IDataContainer<T>
{
    [HideInInspector]
    public bool mainStat;
    [field: SerializeField]
    public T Value { get; set; }
    [SerializeReference, HideIf("@!(this is IStatTag)")]
    public List<Modifier<T>> modifiers = new List<Modifier<T>>();
    public override void AddModifier(Stat modifier)
    {
        Modifier<T> mod = (Modifier<T>)modifier;
        if (mod == null) return;
        modifiers.Add(mod);
    }
    public override void RemoveModifier(Stat modifier)
    {
        Modifier<T> mod = (Modifier<T>)modifier;
        modifiers.Remove(mod);
    }
    public override bool Equals(Stat other)
    {
        if (other == null) return false;
        if (other.Is(out IDataContainer<T> data)) return true;
        return false;
    }
}

public interface IModifier<T>
{
    public IStatTag<T> Tag { get; set; }
}
[Serializable]
public abstract class Modifier<T> : Stat<T>, IModifier<T>
{
    [HideInInspector]
    public Effect source; // Optional, for contribution

    [field: SerializeReference]
    public IStatTag<T> Tag { get; set; }
    public override bool Equals(Stat other)
    {
        if (other == null) return false;
        if (other.GetHashCode() == GetHashCode()) return true;
        return false;
    }
}

public interface IStatTag { }
public interface IStatTag<T> 
{
    public T Value { get; set; }
}
public abstract class StatTag<T> : Stat<T>, IStatTag, IStatTag<T> { }

public class Stat_CurrentLife : StatTag<float> { }
public class Stat_MaxLife : StatTag<float> { }
public class Stat_Invulnerable : StatTag<bool> { }
public class Stat_DamageDealt : StatTag<float> { }
public class Stat_DamageTaken : StatTag<float> { }
public class Stat_AoESize : StatTag<float> { }
public class Stat_Projectiles : StatTag<float> { }
public class Stat_Targets : StatTag<float> { }
public class Stat_CastTargets : StatTag<float> { }
public class Stat_Removeable : StatTag<float> { }
public class Stat_Knockback : StatTag<float> { }
public class Stat_MovementSpeed : StatTag<float> { }
public class Stat_Initiative : StatTag<float> { }
public class Stat_Enmity : StatTag<float> { }
public class Stat_SpellPhase : StatTag<float> { }
public class Stat_Summons : StatTag<float> { }
public class Stat_MaxSummons : StatTag<float> { }
public class Stat_TargetingMethod : StatTag<Targeting> { }
public class Stat_MovementTargetingMethod : StatTag<Targeting> { }
