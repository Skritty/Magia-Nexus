using UnityEngine;
public class Effect_Counter : EffectTask
{
    [SerializeReference]
    public Stat_Counter counter;
    public EffectTargetSelector selector;
    public int countIncrease;
    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        Entity entity = owner;
        if (selector == EffectTargetSelector.Target) entity = target;
        (owner.Stat(counter) as Stat_Counter)[entity] += countIncrease;
    }
}
public class Task_Filter_Count<T> : ITaskOwned<Entity, T>
{
    [SerializeReference]
    public Stat_Counter counter;
    public int countIncrease;
    public int threshold;
    public bool DoTask(T data) => false;
    public bool DoTask(Entity owner, T data)
    {
        if (((owner.Stat(counter) as Stat_Counter)[this] += countIncrease) >= threshold) return true;
        return false;
    }
}
public class Task_Filter_CountData<T> : ITaskOwned<Entity, T>
{
    [SerializeReference]
    public Stat_Counter counter;
    public int countIncrease;
    public int threshold;
    public bool DoTask(T data) => false;
    public bool DoTask(Entity owner, T data)
    {
        if (((owner.Stat(counter) as Stat_Counter)[data] += countIncrease) >= threshold) return true;
        return false;
    }
}

public class Task_Filter_CountEffect<T> : 
    ITaskOwned<Entity, Effect>, 
    ITaskOwned<Entity, Spell>,
    ITaskOwned<Entity, Hit>,
    ITaskOwned<Entity, DamageInstance>
{
    [SerializeReference]
    public Stat_Counter counter;
    public int countIncrease;
    public int threshold;
    public EffectTargetSelector selector;
    
    public bool DoTask(Entity owner, Effect data)
    {
        Entity entity = data.Owner;
        if (selector == EffectTargetSelector.Target) entity = data.Target;
        if (((owner.Stat(counter) as Stat_Counter)[entity] += countIncrease) >= threshold) return true;
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