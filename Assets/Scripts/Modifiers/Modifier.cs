using System;
using System.Collections.Generic;

[Serializable]
public abstract class Modifier<T>
{
    public T value;
    public List<Modifier<T>> submodifiers = new List<Modifier<T>>();
    public Effect source; // Optional, for contribution
    public EffectTag tag;
    //public T absolute, mitigated;
    public virtual void AddModifier(Modifier<T> modifier)
    {
        submodifiers.Add(modifier);
    }
    public virtual T Solve() => value;
    public virtual void InverseSolve() { }
}