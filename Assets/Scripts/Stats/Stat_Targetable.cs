using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Targetable : GenericStat<Stat_Targetable>
{
    [ShowInInspector, FoldoutGroup("Targetable")]
    public bool untargetable = true;
    [ShowInInspector, FoldoutGroup("Targetable")]
    private HashSet<object> untargetableFromSources = new HashSet<object>();
    public void AddIgnored(object source)
    {
        if (!untargetableFromSources.Contains(source))
            untargetableFromSources.Add(source);
    }

    public void RemoveIgnored(object source)
    {
        if (untargetableFromSources.Contains(source))
            untargetableFromSources.Remove(source);
    }

    public bool IsTargetable(object source)
    {
        if (untargetable) return false;
        if (untargetableFromSources.Contains(source)) return false;
        return true;
    }
}
