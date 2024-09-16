using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Targetable : BooleanStat<Stat_Targetable>
{
    [ShowInInspector, FoldoutGroup("Targetable")]
    private Dictionary<object, List<Entity>> ignored = new Dictionary<object, List<Entity>>();
    public void AddIgnored(object source, Entity target)
    {
        if (!ignored.ContainsKey(source))
        {
            ignored.Add(source, new List<Entity>());
        }
        ignored[source].Add(target);
    }

    public void RemoveIgnored(object source, Entity target)
    {
        if (ignored.ContainsKey(source))
            ignored[source].Remove(target);
    }

    public bool IsTargetable(object source, Entity target)
    {
        if (Value) return false;
        if (ignored.ContainsKey(source) && ignored[source].Contains(target))
            return true;
        return false;
    }
}
