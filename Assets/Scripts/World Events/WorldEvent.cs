using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WorldEvent")]
public class WorldEvent : ScriptableObject
{
    public List<string> requiredSpawnFlags = new();
    [SerializeReference]
    public List<Trigger> outcomes = new();
    [SerializeReference]
    public WorldEventSpawnLogic spawnChunk;
    public List<EncounterSpawn> encounterSpawns = new();

    [Serializable]
    public class EncounterSpawn
    {
        public Texture2D spawnProbabilityMap;
        [SerializeReference]
        public List<ITask<MultidimensionalPosition>> encounterSpawnTasks;
    }

    public void CreateEncounter(NTree<TileSuperposition> terrainMap, MultidimensionalPosition chunkPosition)
    {
        foreach(EncounterSpawn spawn in encounterSpawns)
        {
            foreach(ITask<MultidimensionalPosition> task in spawn.encounterSpawnTasks)
            {
                if (!task.DoTask(chunkPosition)) break; // TODO:P not chunk position, tile position
            }
        }
    }
}

[Serializable]
public abstract class WorldEventSpawnLogic
{
    public abstract MultidimensionalPosition GetSpawnChunkPosition();
}

public class RandomChunk : WorldEventSpawnLogic
{
    public TileSuperposition chunkFilter;
    public override MultidimensionalPosition GetSpawnChunkPosition()
    {
        return MapGenerationManager.Instance.GetRandomChunkPosition(chunkFilter);
    }
}

public class RandomAdjacentChunk : WorldEventSpawnLogic
{
    public TileSuperposition chunkFilter;
    public override MultidimensionalPosition GetSpawnChunkPosition()
    {
        // TODO: store previous/current chunk position as a stat on the world event
        return MapGenerationManager.Instance.GetRandomChunkPosition(chunkFilter);
    }
}

public class Stat_WorldEventTickCounter : Stat_Counter { }
public class Trigger_WorldEventTick : Trigger<Trigger_WorldEventTick, int> { }
public class Trigger_WorldEventCompleted : Trigger<Trigger_WorldEvent_EnemiesDefeated, WorldEvent> { }
public class Trigger_WorldEvent_TimeExpired : Trigger<Trigger_WorldEvent_EnemiesDefeated, WorldEvent> { }
public class Trigger_WorldEvent_EnemiesDefeated : Trigger<Trigger_WorldEvent_EnemiesDefeated, WorldEvent> { }
public class Trigger_WorldEvent_PuzzleSolved : Trigger<Trigger_WorldEvent_EnemiesDefeated, WorldEvent> { }
public class Trigger_WorldEvent_ObjectiveComplete : Trigger<Trigger_WorldEvent_EnemiesDefeated, WorldEvent> { }