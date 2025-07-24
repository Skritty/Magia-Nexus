using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BooleanPrioritySolver : PrioritySolver<bool>
{
    protected override bool HandleSamePriorityModifiers(List<IDataContainer<bool>> modifiers)
    {
        bool value = false;
        foreach (Stat<bool> modifier in modifiers)
        {
            modifier.Solve();
            return value |= modifier.Value;
        }
        return value;
    }

    public override Stat Clone() => base.Clone();
}
