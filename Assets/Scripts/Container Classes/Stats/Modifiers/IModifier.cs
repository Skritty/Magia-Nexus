public interface IModifier : IDataContainer
{
    public IStat Tag { get; }
    public Alignment Alignment { get; }
    public int MaxStacks { get; }
    public int StacksAdded { get; }
    public bool PerPlayer { get; }
    public int TickDuration { get; }
    public bool RefreshDuration { get; }
}

public interface IModifier<T> : IModifier, IDataContainer<T>
{
    public IStat<T> StatTag { get; }
}