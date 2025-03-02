using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_IgnoreEntity : PersistentEffect
{
    private Effect ignoredSource;
    public PE_IgnoreEntity() { }
    public PE_IgnoreEntity(Effect inherit, int tick)
    {
        ignoredSource = inherit.Source;
        _source = this;
        _owner = inherit.Owner;
        _target = inherit.Target;
        tickDuration = tick;
        refreshDuration = false;
        perPlayer = true;
        maxStacks = -1;
        DoEffect();
    }

    public override void OnGained()
    {
        Target.Stat<Stat_Targetable>().AddIgnored(Owner, ignoredSource);
    }

    public override void OnLost()
    {
        Target.Stat<Stat_Targetable>().RemoveIgnored(Owner, ignoredSource);
    }
}