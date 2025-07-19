using System.Collections.Generic;
using System.Linq;

public class ListPrioritySolver<T> : PrioritySolver<IList<T>>
{
    public override void Solve()
    {
        Value.Clear();

        var ordered = Modifiers.OrderByDescending(x =>
        {
            if (x is PrioritySolver<IList<T>>)
            {
                return (x as PrioritySolver<IList<T>>).Priority;
            }
            else
            {
                return 0;
            }
        }).Reverse();

        foreach (IDataContainer<IList<T>> modifier in ordered)
        {
            for (int i = 0; i < modifier.Value.Count; i++)
            {
                if (Value.Count <= i) Value.Add(modifier.Value[i]);
                else Value[i] = modifier.Value[i];
            }
        }
    }
}