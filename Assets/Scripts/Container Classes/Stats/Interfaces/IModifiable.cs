using System.Collections.Generic;

public interface IModifiable
{
    public bool Contains(IValueContainer modifier, out int count);
}

public interface IModifiable<T> : IModifiable
{
    public List<IValueContainer<T>> Modifiers { get; set; }
    public void Add(IValueContainer<T> modifier);
    public void Remove(IValueContainer<T> modifier);
}