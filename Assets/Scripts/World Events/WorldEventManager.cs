using System.Collections.Generic;
using Sirenix.OdinInspector;
using Skritty.Tools.Utilities;
using UnityEngine;

public class WorldEventManager : Singleton<WorldEventManager>
{
    public List<WorldEvent> possibleWorldEvents = new();
    public List<WorldEvent> initialWorldEvents = new();
    [ShowInInspector, ReadOnly]
    public Dictionary<MultidimensionalPosition, WorldEvent> activeWorldEvents = new(); // TODO: worldevent list
    private HashSet<string> flagKeywords = new();
    private int eventTick = 0;

    public void Initialize()
    {
        foreach (WorldEvent worldEvent in initialWorldEvents)
        {
            AddActiveWorldEvent(worldEvent);
        }
    }

    public void GenerateChunkEvent(NTree<TileSuperposition> terrainMap, MultidimensionalPosition position)
    {
        if (!activeWorldEvents.ContainsKey(position)) return;
        activeWorldEvents[position].CreateEncounter(terrainMap, position);
    }

    private void FixedUpdate()
    {
        eventTick++;
        Trigger_WorldEventTick.Invoke(eventTick);
    }

    public void SetFlag(string flag, bool remove = false)
    {
        bool changed = false;
        if (remove)
        {
            if (flagKeywords.Contains(flag))
            {
                flagKeywords.Remove(flag);
                changed = true;
            }
        }
        else if (!flagKeywords.Contains(flag))
        {
            flagKeywords.Add(flag);
            changed = true;
        }
        if (!changed) return;
        foreach (WorldEvent worldEvent in possibleWorldEvents.ToArray()) 
        {
            changed = true;
            foreach (string reqFlag in worldEvent.requiredSpawnFlags)
            {
                if (!flagKeywords.Contains(reqFlag))
                {
                    changed = false;
                    break;
                }
            }
            if (changed)
            {
                possibleWorldEvents.Remove(worldEvent);
                AddActiveWorldEvent(worldEvent);
            }
        }
    }

    public bool IsFlagSet(string flag) => flagKeywords.Contains(flag);

    public void AddActiveWorldEvent(WorldEvent worldEvent)
    {
        MultidimensionalPosition position = worldEvent.spawnChunk.GetSpawnChunkPosition();
        activeWorldEvents.Add(position, worldEvent);
    }
}
