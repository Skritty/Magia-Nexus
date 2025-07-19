public interface IDataContainer 
{
    public bool Get<T>(out T data);
}

public interface IDataContainer<T> : IDataContainer
{
    public T Value { get; }
}

public class DataContainer : IDataContainer
{
    public bool Get<T>(out T data)
    {
        IDataContainer<T> container = (this as IDataContainer<T>);
        if (container == null) data = default;
        else data = container.Value;
        return container != null;
    }
}
public class DataContainer<T> : DataContainer
{
    public T Value { get; set; }

    public DataContainer() { }
    public DataContainer(T value)
    {
        Value = value;
    }
}