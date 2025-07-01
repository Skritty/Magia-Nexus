using System;
using System.Collections.Generic;

[Serializable]
public class BooleanModifier : PriorityModifier<bool>
{
    public BooleanModifier() { }

    protected override bool HandleSamePriorityModifiers(List<PriorityModifier<bool>> modifiers)
    {
        bool result = false;
        foreach (PriorityModifier<bool> modifier in modifiers)
        {
            result |= modifier.Value;
        }
        return result;
    }
}