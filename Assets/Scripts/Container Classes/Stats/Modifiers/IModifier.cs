using System.Collections.Generic;

public interface IModifier : IDataContainer
{
    public IStatTag Tag { get; }
    public Alignment Alignment { get; }
    public int MaxStacks { get; }
    public int StacksAdded { get; }
    public bool PerPlayer { get; }
    public int TickDuration { get; }
    public bool RefreshDuration { get; }
}

public interface IModifier<T> : IModifier, IModifiable, IDataContainer<T>
{
    public IStatTag<T> StatTag { get; }
    public List<IDataContainer<T>> Modifiers { get; }
}