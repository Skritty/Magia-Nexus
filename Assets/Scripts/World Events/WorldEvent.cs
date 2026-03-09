using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WorldEvent")]
public class WorldEvent : ScriptableObject
{
    // Only one world event may exist at a time! Fix this at some point please.
    public int ticksUntilResolution;
    [SerializeReference]
    public List<ITask<WorldEvent>> spawnConditions = new();
    [SerializeReference]
    public List<Trigger> outcomes = new();
    private System.Action cleanup;

    public void TrySpawn(WorldEventSpawnLogic spawnLogic)
    {
        foreach (ITask<WorldEvent> condition in spawnConditions)
        {
            if (!condition.DoTask(this)) return;
        }
        foreach (Trigger outcome in outcomes)
        {
            cleanup += outcome.SubscribeToTasks(this, 0, triggerOnce: true);
        }
    }

    public void Despawn()
    {
        // Despawn after timer is done
        cleanup?.Invoke();
    }

    public void CreateEncounter()
    {
        //MapGenerationManager.Instance.GetChunk();
        // TODO
    }
}

public class WorldEventCondition
{
    public bool condition; // TODO
}

public abstract class WorldEventSpawnLogic
{
    public abstract ChunkTile GetSpawnChunk();
}

public class RandomAdjacent : WorldEventSpawnLogic
{
    public TileSuperposition biomes;
    public override ChunkTile GetSpawnChunk()
    {
        throw new System.NotImplementedException();
    }
}

public class Trigger_WorldEvent_TimeExpired : Trigger<Trigger_WorldEvent_EnemiesDefeated, WorldEvent> { }
public class Trigger_WorldEvent_EnemiesDefeated : Trigger<Trigger_WorldEvent_EnemiesDefeated, WorldEvent> { }
public class Trigger_WorldEvent_PuzzleSolved : Trigger<Trigger_WorldEvent_EnemiesDefeated, WorldEvent> { }
public class Trigger_WorldEvent_ObjectiveComplete : Trigger<Trigger_WorldEvent_EnemiesDefeated, WorldEvent> { }