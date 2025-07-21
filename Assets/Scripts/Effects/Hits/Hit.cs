using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Hit : Effect
{
    [FoldoutGroup("@GetType()")]
    public bool preventTriggers;
    [FoldoutGroup("@GetType()")]
    public bool triggerPlayerOwner;
    [FoldoutGroup("@GetType()")]
    public List<Rune> runes = new();
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<TriggerTask> onHitEffects = new();
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<TriggerTask> postOnHitEffects = new();

    public void PreHitTriggers()
    {
        new Trigger_PreHit(this, this, Source, Owner, Target);
        foreach (TriggerTask task in onHitEffects)
        {
            if (!task.DoTask(null, Owner)) break;
        }
        
    }

    public void PostHitTriggers()
    {
        new Trigger_Hit(this, this, Source, Owner, Target);
        foreach (TriggerTask task in postOnHitEffects)
        {
            if (!task.DoTask(null, Owner)) break;
        }
    }

    public Hit Clone()
    {
        Hit clone = (Hit)MemberwiseClone();
        clone.runes = new List<Rune>(runes);
        clone.onHitEffects = new List<TriggerTask>(onHitEffects);
        clone.postOnHitEffects = new List<TriggerTask>(postOnHitEffects);
        return clone;
    }
}

public class Effect
{
    public float EffectMultiplier { get; set; }
    public EffectTask Source { get; set; }
    public Entity Owner { get; set; }
    public Entity Target { get; set; }
}