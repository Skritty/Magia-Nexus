using System.Collections.Generic;
using System.Linq;

public class ListPrioritySolver<T> : PrioritySolver<IList<T>>
{
    public override void Solve()
    {
        Value.Clear();
        foreach (PrioritySolver<IList<T>> modifier in Modifiers.OrderByDescending(x => (x as PrioritySolver<IList<T>>).Priority).Reverse())
        {
            for (int i = 0; i < modifier.Value.Count; i++)
            {
                if (Value.Count <= i) Value.Add(modifier.Value[i]);
                else Value[i] = modifier.Value[i];
            }
        }
    }
}