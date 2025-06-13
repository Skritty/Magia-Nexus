using System;

[Serializable]
public class BooleanModifier : Modifier<bool>
{
    public BooleanModifier() { }
    /// <summary>
    /// Booleans in the same priority level use OR
    /// </summary>
    public byte priority;
    public void AddModifier(Modifier<bool> modifier, byte priority = 0)
    {
        AddModifier(modifier);
        this.priority = priority;
    }

    public override bool Solve()
    {
        bool result = false;
        byte highestPriority = 0;
        foreach (BooleanModifier modifier in submodifiers)
        {
            if (modifier.priority > highestPriority) highestPriority = modifier.priority;
        }
        foreach (BooleanModifier modifier in submodifiers)
        {
            if (modifier.priority == highestPriority)
            {
                result |= modifier.value;
            }
        }
        return result;
    }
}