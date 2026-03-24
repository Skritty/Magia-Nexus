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
        public Entity encounterSpawn;
    }

    [Serializable]
    public class WorldEventCondition
    {
        public bool flag;
        [SerializeReference]
        public Trigger trigger;

        public virtual void SetFlag()
        {
            flag = true;
        }
    }

    public void CreateEncounter(NTree<TileSuperposition> terrainMap)
    {

    }
}

[Serializable]
public abstract class WorldEventSpawnLogic
{
    public abstract ChunkTile GetSpawnChunk();
}

public class RandomChunk : WorldEventSpawnLogic
{
    public TileSuperposition chunkFilter;
    public override ChunkTile GetSpawnChunk()
    {
        return MapGenerationManager.Instance.GetRandomChunk(chunkFilter);
    }
}

public class RandomAdjacentChunk : WorldEventSpawnLogic
{
    public TileSuperposition chunkFilter;
    public override ChunkTile GetSpawnChunk()
    {
        // TODO: store previous/current chunk position as a stat on the world event
        return MapGenerationManager.Instance.GetRandomChunk(chunkFilter);
    }
}

public class Stat_WorldEventTickCounter : Stat_Counter { }
public class Trigger_WorldEventTick : Trigger<Trigger_WorldEventTick, int> { }
public class Trigger_WorldEventCompleted : Trigger<Trigger_WorldEvent_EnemiesDefeated, WorldEvent> { }
public class Trigger_WorldEvent_TimeExpired : Trigger<Trigger_WorldEvent_EnemiesDefeated, WorldEvent> { }
public class Trigger_WorldEvent_EnemiesDefeated : Trigger<Trigger_WorldEvent_EnemiesDefeated, WorldEvent> { }
public class Trigger_WorldEvent_PuzzleSolved : Trigger<Trigger_WorldEvent_EnemiesDefeated, WorldEvent> { }
public class Trigger_WorldEvent_ObjectiveComplete : Trigger<Trigger_WorldEvent_EnemiesDefeated, WorldEvent> { }