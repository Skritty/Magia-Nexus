using System.Collections.Generic;

public interface IModifiable
{
    public bool Contains(IDataContainer modifier, out int count);
    public IModifiable Clone(bool preserveModifiers = true);
}

public interface IModifiable<T> : IModifiable
{
    public List<IDataContainer<T>> Modifiers { get; set; }
}