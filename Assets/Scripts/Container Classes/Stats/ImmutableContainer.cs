using Sirenix.OdinInspector;
using UnityEngine;

public abstract class ImmutableContainer<T> : IValueContainer<T>, ISolver<T>
{
    protected T _value;

    [ShowInInspector, FoldoutGroup("@GetType()")]
    public virtual T Value
    {
        get
        {
            Solve(BoundObject);
            return _value;
        }
    }
    public object BoundObject { get; set; }
    public bool IsDefaultValue() => Value.Equals(default);
    public bool TryGet<Type>(out Type data)
    {
        data = (Type)(Value as object);
        return data != null;
    }
    public abstract T Solve(object boundObject);

    public void AddTo(IModifiable<T> modifiable) => modifiable.Modifiers.Add(this);
    public void RemoveFrom(IModifiable<T> modifiable) => modifiable.Modifiers.Remove(this);
}