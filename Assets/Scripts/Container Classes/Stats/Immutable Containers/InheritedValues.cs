using UnityEngine;
using Sirenix.OdinInspector;

public class Stat_Parent : PrioritySolver<object>, IStat<object> { }

/// <summary>
/// NOTE: Remember to set the context data!
/// </summary>
public class InheritFromParent<T> : ImmutableContainer<T>
{
    [SerializeReference]
    public IValueContainer<T> referenceStat;

    public override T Solve() 
    {
        Entity owner = "statContextData".GetStat<StatContext_EntityOwner>().Value;
        if (owner == null) return default;
        return _value = owner.GetStat<Stat_Parent>().Value.GetStat(referenceStat).Value;
    } 
}

/// <summary>
/// NOTE: Remember to set the context data!
/// </summary>
public class InheritFromStat<T> : ImmutableContainer<T>
{
    [SerializeReference, InfoBox("Can cause cyclical referecing!!", infoMessageType: InfoMessageType.Warning)]
    public IValueContainer<T> referenceStat;

    public override T Solve()
    {
        Entity owner = "statContextData".GetStat<StatContext_EntityOwner>().Value;
        if (owner == null) return default;
        return _value = owner.GetStat(referenceStat).Value;
    }
}