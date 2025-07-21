using System.Collections.Generic;

public interface IDataContainer 
{
    public bool Get<Type>(out Type data);
}

public interface IDataContainer<T> : IDataContainer
{
    public T Value { get; }
}

public abstract class DataContainer : IDataContainer
{
    public abstract bool Get<Type>(out Type data);
}
public class DataContainer<T> : DataContainer, IDataContainer<T>
{
    public T Value { get; set; }
    public DataContainer() { }
    public DataContainer(T value)
    {
        Value = value;
    }
    public override bool Get<Type>(out Type data)
    {
        data = (Type)(Value as object);
        return data != null;
    }
}