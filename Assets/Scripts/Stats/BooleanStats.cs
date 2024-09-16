using Sirenix.OdinInspector;
using System.Collections.Generic;

public abstract class BooleanStat<T> : GenericStat<T> where T : BooleanStat<T>
{
    private int amount;
    [ShowInInspector]
    public bool Value => amount > 0;
    protected override void Merge(T other)
    {
        amount += other.Value ? 1 : -1;
    }
}