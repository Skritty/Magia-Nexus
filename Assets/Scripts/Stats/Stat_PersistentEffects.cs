using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                    e.DoForAllStacks(() => e.ApplyContribution());
                    e.tick = 0;
                }

                // Can be stacked as a single effect? (is e the same owner and can effect be applied it?)
                // (is the duration infinite or the tick is the same?)
                if (!(effect.perPlayer && e.Owner != effect.Owner) && (effect.tickDuration < 0 || e.refreshDuration))
                {
                    int stacksAdded = 0;
                    if (e.maxStacks < 0)
                    {
                        stacksAdded = effect.stacks;
                    }
                    else
                    {
                        stacksAdded = Mathf.Clamp(effect.stacks, 0, e.maxStacks - e.stacks);
                    }
                    e.stacks += stacksAdded;
                    e.DoForAllStacks(() => e.OnGained(), stacksAdded);
                    return;
                }
                stacks += e.stacks;
            }
        }
        if (effect.maxStacks >= 0 && stacks >= effect.maxStacks) return;

        // Add effect
        effect.tick = 0;
        persistentEffects.Add(effect);
        effect.DoForAllStacks(() => effect.OnGained());
    }

    public List<PersistentEffect> GetEffectsViaReference(PersistentEffect reference, Entity owner = null)
    {
        List<PersistentEffect> effects = new List<PersistentEffect>();
        foreach (PersistentEffect e in persistentEffects)
        {
            if (reference.GetUID() == e.Source.GetUID() && (owner == null || owner == e.Owner))
            {
                effects.Add(e);
            }
        }
        return effects;
    }

    public List<T> GetEffects<T>() where T : PersistentEffect
    {
        List<T> effects = new List<T>();
        foreach (PersistentEffect e in persistentEffects)
        {
            if (e is T)
            {
                effects.Add(e as T);
            }
        }
        return effects;
    }

    public void AddOrRemoveSimilarEffect(PersistentEffect reference, int stacks, Entity owner = null, bool addInCaseNoneFound = true)
    {
        foreach (PersistentEffect e in persistentEffects.ToArray())
        {
            if (reference.GetUID() == e.Source.GetUID() && (owner == null || owner == e.Owner))
            {
                if(stacks > 0)
                {
                    reference.Create(owner);
                }
                else if(stacks < 0)
                {
                    e.DoForAllStacks(() => e.OnLost(), Mathf.Clamp(-stacks, 0, e.stacks));
                    e.stacks += stacks;
                    if (e.stacks <= 0) persistentEffects.Remove(e);
                }
                return;
            }
        }
        if (addInCaseNoneFound)
        {
            reference.Create(owner);
        }
    }

    public PersistentEffect RemoveRandomEffect(PersistentEffect.Alignment alignment, int stacks)
    {
        List<PersistentEffect> shuffle = persistentEffects.ToList();
        shuffle.Sort((x, y) =>
         {
             if (alignment != x.alignment) return 1;
             return Random.Range(-1, 2);
         });
        AddOrRemoveSimilarEffect(shuffle[0], stacks);
        return shuffle[0];
    }

    public T RemoveEffect<T>(int stacks) where T : PersistentEffect
    {
        T e = persistentEffects.Find(x => x.GetType() is T) as T;
        AddOrRemoveSimilarEffect(e, stacks);
        return e;
    }

    public override void Tick()
    {
        foreach (PersistentEffect effect in persistentEffects.ToArray())
        {
            effect.tick++;
            if(effect.tickDuration >= 0 && effect.tick >= effect.tickDuration)
            {
                effect.DoForAllStacks(() =>
                {
                    effect.OnLost();
                    effect.ApplyContribution();
                });
                persistentEffects.Remove(effect);
                continue;
            }
            effect.DoForAllStacks(() => effect.OnTick());
        }
    }

    public override void OnDestroy()
    {
        foreach (PersistentEffect effect in persistentEffects)
        {
            effect.DoForAllStacks(() =>
            {
                effect.OnLost();
                effect.ApplyContribution();
            });
        }
        persistentEffects.Clear();
    }
}