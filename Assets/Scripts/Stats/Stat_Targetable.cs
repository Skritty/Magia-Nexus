using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Targetable : GenericStat<Stat_Targetable>
{
    [ShowInInspector, FoldoutGroup("Targetable")]
    public bool untargetable = true;
    [ShowInInspector, FoldoutGroup("Targetable")]
    private List<(Entity, Effect)> untargetableFromSources = new List<(Entity, Effect)>();
    public void AddIgnored(Entity entity, Effect effect)
    {
        (Entity, Effect) source = (entity, effect);
        if (!untargetableFromSources.Contains(source))
            untargetableFromSources.Add(source);
    }

    public void RemoveIgnored(Entity entity, Effect effect)
    {
        (Entity, Effect) source = (entity, effect);
        if (untargetableFromSources.Contains(source))
            untargetableFromSources.Remove(source);
    }

    public bool IsTargetable(Entity entity, Effect effect)
    {
        if (untargetable) return false;
        (Entity, Effect) source = (entity, effect);
        if (untargetableFromSources.Contains(source)) return false;
        return true;
    }
}
