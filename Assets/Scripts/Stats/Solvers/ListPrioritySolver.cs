using System.Collections.Generic;
using System.Linq;

public class ListPrioritySolver<T> : PrioritySolver<List<T>>
{
    public override void Solve()
    {
        _value = new List<T>();

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

        foreach (IDataContainer<List<T>> modifier in ordered)
        {
            for (int i = 0; i < modifier.Value.Count; i++)
            {
                if (_value.Count <= i) _value.Add(modifier.Value[i]);
                else _value[i] = modifier.Value[i];
            }
        }
    }
}