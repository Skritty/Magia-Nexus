public interface IDataContainer 
{
    public bool Get<T>(out T data);
}

public interface IDataContainer<T> : IDataContainer
{
    public T Value { get; set; }
}