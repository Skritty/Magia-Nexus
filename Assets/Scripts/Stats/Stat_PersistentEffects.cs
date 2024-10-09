using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_PersistentEffects : GenericStat<Stat_PersistentEffects>
{
    protected override void Initialize()
    {
        // Set up any persistent effects that were on an instantiated entity
        foreach (PersistentEffect effect in persistentEffects)
        {
            effect.SetInfo(effect, Owner, Owner);
            effect.OnGained();
        }
    }

    [FoldoutGroup("Persistent Effects")]
    [SerializeReference]
    public List<PersistentEffect> persistentEffects = new List<PersistentEffect>();

    public void ApplyEffect(PersistentEffect effect)
    {
        if (!AcceptsEffect(effect)) return;
        effect.tick = 0;
        persistentEffects.Add(effect);
        effect.OnGained();
    }

    private bool AcceptsEffect(PersistentEffect effect)
    {
        int stacks = 0;
        foreach (PersistentEffect e in persistentEffects)
        {
            if (e.Source == effect.Source && e.Owner == effect.Owner)
            {
                stacks++;
                if (e.refreshDuration)
                {
                    e.ApplyContribution();
                    e.tick = 0;
                }
            }
        }
        if (effect.maxStacks >= 0 && stacks >= effect.maxStacks) return false;
        return true;
    }

    public override void Tick()
    {
        foreach (PersistentEffect effect in persistentEffects.ToArray())
        {
            effect.tick++;
            if(effect.tickDuration >= 0 && effect.tick >= effect.tickDuration)
            {
                effect.OnLost();
                effect.ApplyContribution();
                persistentEffects.Remove(effect);
                continue;
            }
            effect.OnTick();
        }
    }

    public override void OnDestroy()
    {
        foreach (PersistentEffect effect in persistentEffects)
        {
            effect.OnLost();
            effect.ApplyContribution();
        }
        persistentEffects.Clear();
    }
}