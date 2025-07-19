using System.Collections.Generic;
using UnityEngine;

public class BooleanPrioritySolver : PrioritySolver<bool>
{
    protected override bool HandleSamePriorityModifiers(List<Stat<bool>> modifiers)
    {
        bool value = false;
        foreach (Stat<bool> modifier in modifiers)
        {
            modifier.Solve();
            return value |= modifier.Value;
        }
        return value;
    }
}
