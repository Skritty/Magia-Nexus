using UnityEngine;

public abstract class Stat_Counter : DictionaryStat<object, int>, IStat<int> { }
public class Stat_Counter_Spellcasts : Stat_Counter { }
public class Task_Filter_Count<T> : ITaskOwned<Entity, T>
{
    [SerializeReference]
    public Stat_Counter counter;
    public int threshold;
    public bool DoTask(T data) => false;
    public bool DoTask(Entity owner, T data)
    {
        if (++(owner.Stat(counter) as Stat_Counter)[this] >= threshold) return true;
        return false;
    }
}
public class Task_Filter_CountData<T> : ITaskOwned<Entity, T>
{
    [SerializeReference]
    public Stat_Counter counter;
    public int threshold;
    public bool DoTask(T data) => false;
    public bool DoTask(Entity owner, T data)
    {
        if (++(owner.Stat(counter) as Stat_Counter)[data] >= threshold) return true;
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
    public int threshold;
    public EffectTargetSelector selector;
    
    public bool DoTask(Entity owner, Effect data)
    {
        Entity entity = data.Owner;
        if (selector == EffectTargetSelector.Target) entity = data.Target;
        if (++(owner.Stat(counter) as Stat_Counter)[entity] >= threshold) return true;
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