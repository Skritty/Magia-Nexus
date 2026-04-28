using System.Collections.Generic;

public class BooleanPrioritySolver : PrioritySolver<bool>
{
    protected override bool HandleSamePriorityModifiers(List<IValueContainer<bool>> modifiers)
    {
        bool value = false;
        foreach (IValueContainer<bool> modifier in modifiers)
        {
            modifier.BoundObject = BoundObject;
            return value |= modifier.Value;
        }
        return value;
    }
}
