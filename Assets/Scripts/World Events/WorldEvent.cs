using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using Skritty.Tools.Utilities;
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
        public TileSuperposition spawnTile;
        public int mapOffsetX, mapOffsetY;
        public int verticalMargin = 16;
        public Vector3 offset;
        [SerializeReference]
        public List<ITask<Vector3>> encounterSpawnTasks;
    }

    public void CreateEncounter(NTree<TileSuperposition> terrainMap, MultidimensionalPosition chunkPosition)
    {
        int chunkDim = MapGenerationManager.Instance.chunkDimensions;
        MultidimensionalPosition chunkCorner = new((ushort)(chunkPosition[0] * chunkDim), (ushort)(chunkPosition[1] * chunkDim), (ushort)(chunkPosition[2] * chunkDim));
        WeightedChance<MultidimensionalPosition> weightedSpawn = new();
        foreach(EncounterSpawn spawn in encounterSpawns)
        {
            weightedSpawn.Clear();

            for (int x = 0; x < spawn.spawnProbabilityMap.width; x++)
            {
                for (int z = 0; z < spawn.spawnProbabilityMap.height; z++)
                {
                    for (int y = -spawn.verticalMargin; y < chunkDim + spawn.verticalMargin; y++) // TODO: skip around y randomly
                    {
                        TileSuperposition tile = terrainMap[chunkCorner[0] + x - spawn.mapOffsetX, chunkCorner[1] + y, chunkCorner[2] + z + spawn.mapOffsetY];
                        if (tile == null || !tile.ContainsSubset(spawn.spawnTile)) continue;
                        Color c = spawn.spawnProbabilityMap.GetPixel(x, z);
                        if (c.r == 0) continue;
                        weightedSpawn.Add(chunkCorner + new MultidimensionalPosition((ushort)x, (ushort)y, (ushort)z), c.r);
                        break;
                    }
                }
            }
            if (weightedSpawn.Count == 0) continue;
            MultidimensionalPosition position = weightedSpawn.GetRandomEntry();

            foreach (ITask<Vector3> task in spawn.encounterSpawnTasks)
            {
                if (!task.DoTask(spawn.offset + new Vector3(position[0], position[1], position[2]))) break;
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