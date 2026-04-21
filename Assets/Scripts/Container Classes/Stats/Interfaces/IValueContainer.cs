using System;
using Sirenix.OdinInspector;
using UnityEngine;

public interface IValueContainer
{
    public bool IsDefaultValue();
    public bool TryGet<Type>(out Type data);
}

public interface IValueContainer<T> : IValueContainer
{
    public T Value { get; }
    public void AddTo(IModifiable<T> modifiable);
    public void RemoveFrom(IModifiable<T> modifiable);
}

[Serializable]
public abstract class ValueContainer : IValueContainer
{
    public abstract bool IsDefaultValue();
    public abstract bool TryGet<Type>(out Type data);
}

[Serializable]
public class ValueContainer<T> : ValueContainer, IValueContainer<T>
{
    [field: SerializeReference, FoldoutGroup("@GetType()")]
    public T Value { get; set; }
    public ValueContainer() { }
    public ValueContainer(T value)
    {
        Value = value;
    }
    public override bool IsDefaultValue() => Value.Equals(default(T));
    public override bool TryGet<Type>(out Type data)
    {
        data = (Type)(Value as object);
        return data != null;
    }

    public void AddTo(IModifiable<T> modifiable) => modifiable.Modifiers.Add(this);

    public void RemoveFrom(IModifiable<T> modifiable) => modifiable.Modifiers.Remove(this);
}