using System.Collections.Generic;

public class BooleanPrioritySolver : PrioritySolver<bool>
{
    protected override bool HandleSamePriorityModifiers(List<IDataContainer<bool>> modifiers)
    {
        bool value = false;
        foreach (IDataContainer<bool> modifier in modifiers)
        {
            (modifier as ISolver<bool>)?.Solve();
            return value |= modifier.Value;
        }
        return value;
    }
}
