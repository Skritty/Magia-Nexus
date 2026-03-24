using System.Collections.Generic;
using Skritty.Tools.Utilities;
using UnityEngine;

public class WorldEventManager : Singleton<WorldEventManager>
{
    public List<WorldEvent> possibleWorldEvents = new();
    public List<WorldEvent> activeWorldEvents = new();
    private HashSet<string> flagKeywords;
    private int eventTick = 0;

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
        worldEvent.spawnChunk.GetSpawnChunk().worldEvents.Add(worldEvent);
        activeWorldEvents.Add(worldEvent);
    }
}
