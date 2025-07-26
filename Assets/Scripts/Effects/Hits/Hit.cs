using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class Hit : Effect
{
    [FoldoutGroup("@GetType()")]
    public bool triggerPlayerCharacter;
    [FoldoutGroup("@GetType()")]
    public Trigger_PreHit preHit = new Trigger_PreHit();
    [FoldoutGroup("@GetType()")]
    public Trigger_PostHit postHit = new Trigger_PostHit();
    public List<TriggerTask> preOnHitEffects = new();
    public List<TriggerTask> postOnHitEffects = new();
    [FoldoutGroup("@GetType()")]
    public List<Rune> runes = new();

    public void PreHitTriggers()
    {
        System.Action cleanup = preHit.AddTaskOwner(Owner, preOnHitEffects);
        Trigger_PreHit.Invoke(this, this, Source, Owner, Target);
        cleanup.Invoke();
    }

    public void PostHitTriggers()
    {
        System.Action cleanup = postHit.AddTaskOwner(Owner, postOnHitEffects);
        Trigger_PostHit.Invoke(this, this, Source, Owner, Target);
        cleanup.Invoke();
    }

    public Hit Clone()
    {
        Hit clone = (Hit)MemberwiseClone();
        clone.runes = new List<Rune>(runes);
        return clone;
    }
}

public class Effect
{
    public float EffectMultiplier { get; set; }
    public EffectTask<Effect> Source { get; set; }
    public Entity Owner { get; set; }
    public Entity Target { get; set; }
}