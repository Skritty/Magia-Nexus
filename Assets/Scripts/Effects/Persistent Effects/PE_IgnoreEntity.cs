using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_IgnoreEntity : PersistentEffect
{
    public PE_IgnoreEntity() { }
    public PE_IgnoreEntity(Effect inherit, int tick)
    {
        _source = inherit.Source;
        _owner = inherit.Owner;
        _target = inherit.Target;
        effectMultiplier *= inherit.effectMultiplier;
        tickDuration = tick;
        refreshDuration = false;
        perPlayer = true;
        maxStacks = 999;
        DoEffect();
    }

    public override void OnGained()
    {
        Target.Stat<Stat_Targetable>().AddIgnored(Owner, Source);
    }

    public override void OnLost()
    {
        Target.Stat<Stat_Targetable>().RemoveIgnored(Owner, Source);
    }
}