using UnityEngine;

public class Stat_CounterInt : ValueContainer<int>, IStat<int> { }
public class Stat_CounterFloat : ValueContainer<float>, IStat<float> { }
public class Effect_Counter : EffectTask
{
    public EffectTargetSelector selector;
    public int countIncrease;
    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        Entity entity = owner;
        if (selector == EffectTargetSelector.Target) entity = target;
        (GetHashCode() + entity.GetHashCode()).GetStat<Stat_CounterInt>().Value += countIncrease;
    }
}
public class Task_Filter_Count<T> : ITaskOwned<Entity, T>
{
    public int countIncrease;
    public int threshold;
    public bool DoTask(T data) => false;
    public bool DoTask(Entity owner, T data)
    {
        if (((GetHashCode() + owner.GetHashCode() + data.GetHashCode()).GetStat<Stat_CounterInt>().Value += countIncrease) >= threshold) return true;
        return false;
    }
}
public class Task_Filter_CountData<T> : ITaskOwned<Entity, T>
{
    public int countIncrease;
    public int threshold;
    public bool DoTask(T data) => false;
    public bool DoTask(Entity owner, T data)
    {
        if (((GetHashCode()+owner.GetHashCode()+data.GetHashCode()).GetStat<Stat_CounterInt>().Value += countIncrease) >= threshold) return true;
        return false;
    }
}

public class Task_Filter_CountEffect<T> : 
    ITaskOwned<Entity, Effect>, 
    ITaskOwned<Entity, Spell>,
    ITaskOwned<Entity, Hit>,
    ITaskOwned<Entity, DamageInstance>
{
    public int countIncrease;
    public int threshold;
    public EffectTargetSelector selector;
    
    public bool DoTask(Entity owner, Effect data)
    {
        Entity entity = data.Owner;
        if (selector == EffectTargetSelector.Target) entity = data.Target;
        if (((GetHashCode() + entity.GetHashCode() + data.GetHashCode()).GetStat<Stat_CounterInt>().Value += countIncrease) >= threshold) return true;
        return false;
    }

    public bool DoTask(Effect data) => false;

    public bool DoTask(Entity owner, Spell data) => DoTask(owner, data as Effect);

    public bool DoTask(Spell data) => false;

    public bool DoTask(Entity owner, Hit data) => DoTask(owner, data as Effect);

    public bool DoTask(Hit data) => false;

    public bool DoTask(Entity owner, DamageInstance data) => DoTask(owner, data as Effect);

    public bool DoTask(DamageInstance data) => false;
}