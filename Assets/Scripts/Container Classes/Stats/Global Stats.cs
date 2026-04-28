using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;


/*[Serializable]
public class Stat<T, Self> : IBoundInstance<object, Self>, IDataContainer<T>, IModifiable<T>, IInheritableModifiers<T> where Self : Stat<T, Self>
{
    [ShowInInspector, FoldoutGroup("@GetType()")]
    public T Value
    {
        get
        {
            if(Modifiers.Count == 0) return default;
            return Modifiers[0].Value;
        }
        protected set
        {
            Modifiers.Clear();
            Set(value);
        }
    }

    public static T GetValue(object boundObject) => IBoundInstance<object, Self>.GetInstance(boundObject).Value;
    public static void SetValue(object boundObject, T value) => IBoundInstance<object, Self>.GetInstance(boundObject).Value = value;

    [field: SerializeReference]
    public ModifierInheritence<T> ModifierInheritMethod { get; set; } = new NoInherit<T>();
    [field: SerializeReference, PropertyOrder(1), FoldoutGroup("@GetType()"), ReadOnly]
    private List<IDataContainer<T>> _modifiers = new();
    public List<IDataContainer<T>> Modifiers
    {
        get
        {
            if (ModifierInheritMethod == null) return _modifiers;
            List<IDataContainer<T>> modifiers = ModifierInheritMethod.InheritedModifiers();
            return modifiers == null ? _modifiers : modifiers;
        }
        set
        {
            _modifiers = value;
        }
    }
    
    public Stat() { }
    public Stat(T value)
    {
        Value = value;
    }
    public bool IsDefaultValue() => Value.Equals(default(T));
    public bool Get<Type>(out Type data)
    {
        data = (Type)(Value as object);
        return data != null;
    }

    public void Set(T value)
    {
        DataContainer<T> data = new DataContainer<T>(value);
        Modifiers.Add(data);
    }

    public bool TryAdd(IDataContainer modifier)
    {
        IDataContainer<T> cast = (IDataContainer<T>)modifier;
        if (cast == null) return false;
        Modifiers.Add(cast);
        return true;
    }

    public void Add(IDataContainer<T> modifier)
    {
        Modifiers.Add(modifier);
    }

    public void Remove(IDataContainer modifier)
    {
        Modifiers.Remove(modifier as IDataContainer<T>);
    }

    public bool Contains(IDataContainer modifier, out int count)
    {
        count = 0;
        foreach (IDataContainer m in Modifiers)
        {
            if (m.Equals(modifier)) count++;
        }
        if (count > 0)
        {
            return true;
        }
        return false;
    }

    public IModifiable Clone(bool preserveModifiers)
    {
        IModifiable<T> clone = (IModifiable<T>)MemberwiseClone();
        if(preserveModifiers) clone.Modifiers = new List<IDataContainer<T>>(Modifiers);
        return clone;
    }
}*/
public class Stat_Triggers : ListStat<Trigger>, IStat<List<Trigger>> { }
public class Stat_PreventExpire : EnumPrioritySolver<Alignment>, IStat<Alignment> { }
public class Stat_AoESize : NumericalSolver, IStat<float> { }
public class Stat_AdditionalTargets : NumericalSolver, IStat<float> { }
public class Stat_Removeable : NumericalSolver, IStat<float> { }
public class Stat_Knockback : NumericalSolver, IStat<float> { }
public class Stat_Enmity : NumericalSolver, IStat<float> { }
public class Stat_Intangable : PrioritySolver<bool>, IStat<bool> { }
public class Stat_Untargetable : ListStat<(object owner, object source)>, IStat<List<(object owner, object source)>> { }
public class Stat_TargetingMethod : PrioritySolver<Targeting>, IStat<Targeting> { }
public class Stat_Dummy : ListStat<bool>, IStat<List<bool>> { }
