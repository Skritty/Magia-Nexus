using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_PersistentEffects : GenericStat<Stat_PersistentEffects>
{
    protected override void Initialize()
    {
        foreach (PersistentEffect status in persistentEffects)
        {
            status.source = owner;
            status.owner = owner;
            status.target = owner;
            status.OnGained();
        }
    }

    [FoldoutGroup("Persistent Effects")]
    [SerializeReference]
    public List<PersistentEffect> persistentEffects = new List<PersistentEffect>();

    public bool AcceptsEffect(object source, PersistentEffect effect)
    {
        int stacks = 0;
        foreach (PersistentEffect e in persistentEffects)
        {
            if (e.source == source)
            {
                stacks++;
                if (e.refreshDuration)
                {
                    e.tick = 0;
                }
            }
        }
        if (stacks >= effect.maxStacks) return false;
        return true;
    }

    public void AddEffect(PersistentEffect effect)
    {
        persistentEffects.Add(effect);
    }

    public void OnAction()
    {
        foreach (PersistentEffect effect in persistentEffects)
        {
            effect.OnAction();
        }
    }

    public override void Tick()
    {
        foreach (PersistentEffect effect in persistentEffects.ToArray())
        {
            effect.tick++;
            if(effect.tickDuration >= 0 && effect.tick >= effect.tickDuration)
            {
                effect.OnLost();
                persistentEffects.Remove(effect);
                continue;
            }
            effect.OnTick();
        }
    }
}