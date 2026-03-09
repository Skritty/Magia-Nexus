using UnityEngine;

public class Task_WorldEvent_SpawnWorldEvent<T> : ITask<T>
{
    public WorldEvent worldEvent;
    [SerializeReference]
    public WorldEventSpawnLogic spawnLogic;
    public bool DoTask(T data)
    {
        worldEvent.TrySpawn(spawnLogic);
        return true;
    }
}