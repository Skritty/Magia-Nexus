public interface IStat : IDataContainer, IModifiable { }
public interface IStat<T> : IStat, IDataContainer<T>, IModifiable<T>
{
    //public T GetValue(object boundObject) => this.GetBoundInstance(boundObject).Value;
}