using System.Collections.Generic;
using UnityEngine;

public class Task_WorldEvent_SpawnWorldEvent<T> : ITask<T>
{
    public WorldEvent worldEvent;
    [SerializeReference]
    public WorldEventSpawnLogic spawnLogic;
    public bool DoTask(T data)
    {
        //worldEvent.TrySpawn(spawnLogic);
        return true;
    }
}

public class Task_WorldEvent_SetFlag<T> : ITask<T>
{
    public string flag;
    public bool DoTask(T chunk)
    {
        WorldEventManager.Instance.SetFlag(flag);
        return true;
    }
}

public class Task_WorldEvent_ChunkContainsWorldEvents<T> : ITask<T>
{
    public List<WorldEvent> requiredWorldEvents;
    public bool DoTask(T chunk)
    {
        return true;
    }
}