using UnityEngine;
using Sirenix.OdinInspector;

public class Stat_Parent : PrioritySolver<object>, IStat<object> { }
public class InheritFromParent<T> : ImmutableContainer<T>
{
    [SerializeReference]
    public IValueContainer<T> referenceStat;

    public override T Solve(object boundObject) => _value = boundObject.GetStat<Stat_Parent>().Value.GetStat(referenceStat).Value;
}

public class InheritFromStat<T> : ImmutableContainer<T>
{
    [SerializeReference, InfoBox("Can cause cyclical referecing!!", infoMessageType: InfoMessageType.Warning)]
    public IValueContainer<T> referenceStat;

    public override T Solve(object boundObject) => _value = boundObject.GetStat(referenceStat).Value;
}