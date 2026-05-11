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

    public override T Solve() => _value = 
        "statContextData".GetStat<StatContext_EntityOwner>().Value
        .GetStat<Stat_Parent>().Value.GetStat(referenceStat).Value;
}

/// <summary>
/// NOTE: Remember to set the context data!
/// </summary>
public class InheritFromStat<T> : ImmutableContainer<T>
{
    [SerializeReference, InfoBox("Can cause cyclical referecing!!", infoMessageType: InfoMessageType.Warning)]
    public IValueContainer<T> referenceStat;

    public override T Solve() => _value = 
        "statContextData".GetStat<StatContext_EntityOwner>().Value
        .GetStat(referenceStat).Value;
}