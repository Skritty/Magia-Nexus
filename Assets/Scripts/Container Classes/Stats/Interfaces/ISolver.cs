
/*public interface ISolver
{
    public object BoundObject { get; set; }
}*/
public interface ISolver<T> //: ISolver
{
    public T Solve();
}