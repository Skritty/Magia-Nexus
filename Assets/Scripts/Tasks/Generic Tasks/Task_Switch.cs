using System.Collections.Generic;
using UnityEngine;

public class Stat_Switches : DictionaryStat<object, bool>, IStat<bool> { }
public class Task_Switch<T> : ITaskOwned<Entity, T>
{
    [SerializeReference]
    public List<ITask<T>> filterTasks;
    [SerializeReference]
    public List<ITask<T>> falseTasks;
    [SerializeReference]
    public List<ITask<T>> trueTasks;

    public bool DoTask(T data) => false;
    public bool DoTask(Entity owner, T data)
    {
        foreach (ITask<T> task1 in filterTasks)
        {
            if (!task1.DoTask(data)) return IsFalse(owner, data);
        }
        return IsTrue(owner, data);
    }

    private bool IsFalse(Entity owner, T data)
    {
        if (!owner.Stat<Stat_Switches>()[this]) return false;
        owner.Stat<Stat_Switches>()[this] = false;

        foreach (ITask<T> task2 in falseTasks)
        {
            if (!task2.DoTask(data)) return true;
        }
        return true;
    }

    private bool IsTrue(Entity owner, T data)
    {
        if (owner.Stat<Stat_Switches>()[this]) return false;
        owner.Stat<Stat_Switches>()[this] = true;

        foreach (ITask<T> task2 in trueTasks)
        {
            if (!task2.DoTask(data)) return true;
        }
        return true;
    }
}
