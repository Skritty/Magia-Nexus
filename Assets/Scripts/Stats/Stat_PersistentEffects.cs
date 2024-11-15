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
        // Check stack limits
        int stacks = 0;
        foreach (PersistentEffect e in persistentEffects)
        {
            // Same effect?
            if (e.Source == effect.Source)
            {
                // Refresh the duration
                if (e.refreshDuration)
                {
                    e.ApplyToAllStacks(() => e.ApplyContribution());
                    e.tick = 0;
                }

                // Can be stacked as a single effect? (is e the same owner and can effect be applied it?)
                // (is the duration infinite or the tick is the same?)
                if (!(effect.perPlayer && e.Owner != effect.Owner) && (effect.tickDuration < 0 || e.refreshDuration))
                {
                    e.stacks = Mathf.Clamp(e.stacks + effect.stacks, 0, e.maxStacks);
                    effect.ApplyToAllStacks(() => effect.OnGained());
                    return;
                }
                stacks++;
            }
        }
        if (effect.maxStacks >= 0 && stacks >= effect.maxStacks) return;

        // Add effect
        effect.tick = 0;
        persistentEffects.Add(effect);
        effect.OnGained();
    }

    public List<PersistentEffect> GetEffectsViaReference(PersistentEffect reference, Entity owner = null)
    {
        List<PersistentEffect> effects = new List<PersistentEffect>();
        foreach (PersistentEffect e in persistentEffects.ToArray())
        {
            if (reference.GetUID() == e.Source.GetUID() && (owner == null || owner == e.Owner))
            {
                effects.Add(e);
            }
        }
        return effects;
    }

    public void AddOrRemoveSimilarEffect(PersistentEffect reference, Entity owner = null)
    {
        foreach (PersistentEffect e in persistentEffects.ToArray())
        {
            if (reference.GetUID() == e.Source.GetUID() && (owner == null || owner == e.Owner))
            {
                if(reference.stacks > 0)
                {
                    reference.ApplyToAllStacks(() => reference.Create(owner));
                }
                else if(reference.stacks < 0)
                {
                    e.ApplyToAllStacks(() => e.OnLost(), Mathf.Clamp(-reference.stacks, 0, e.stacks));
                    e.stacks += reference.stacks;
                    if (e.stacks <= 0) persistentEffects.Remove(e);
                }
            }
        }
    }

    public override void Tick()
    {
        foreach (PersistentEffect effect in persistentEffects.ToArray())
        {
            effect.tick++;
            if(effect.tickDuration >= 0 && effect.tick >= effect.tickDuration)
            {
                effect.ApplyToAllStacks(() =>
                {
                    effect.OnLost();
                    effect.ApplyContribution();
                });
                persistentEffects.Remove(effect);
                continue;
            }
            effect.ApplyToAllStacks(() => effect.OnTick());
        }
    }

    public override void OnDestroy()
    {
        foreach (PersistentEffect effect in persistentEffects)
        {
            effect.ApplyToAllStacks(() =>
            {
                effect.OnLost();
                effect.ApplyContribution();
            });
        }
        persistentEffects.Clear();
    }
}