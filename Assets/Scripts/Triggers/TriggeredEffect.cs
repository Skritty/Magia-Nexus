using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TriggeredEffect : PersistentEffect
{
    public TriggeredEffect() { }
    public TriggeredEffect(Trigger trigger, Effect effect) 
    {
        this.trigger = trigger;
        this.effect = effect;
    }
    [InfoBox("Each effect tag is a trigger that must match the tags of the triggering effect. (ie. Trigger when dealing piercing+attack+projectile OR on non-projectile)")]
    [FoldoutGroup("@GetType()")]
    public int triggerOrder;
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Trigger trigger;
    [SerializeReference, FoldoutGroup("@GetType()")]
    public TriggerTest trigger2;
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<TriggerTask> tasks;
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Effect effect;
    
    public override void OnGained()
    {
        foreach (KeyValuePair<EffectTag, float> tag in effectTags)
        {
            trigger.Subscribe(Target, OnTrigger, tag.Key, triggerOrder);
        }
    }
    public override void OnLost()
    {
        trigger.Unsubscribe(Target, OnTrigger);
        trigger.RemoveInstance(Target);
    }

    protected void OnTrigger(Trigger trigger)
    {
        foreach(TriggerTask task in tasks)
        {

        }
        effect.Create(Owner, trigger, trigger.TriggeringEffect);
    }
}

// Shotgun of 10 projectiles (2 damage total, 0.2 each) -> triggers explosion of 1 damage
// With effect multiplier chain: 10 projectiles + 10 explosions (0.2) => 4 damage
// Without effect multiplier chain: 10 projectiles + 10 explosions (1) => 12 damage

// Shotgun of 10 projectiles (2 damage total, 0.2 each) -> triggers ignite (max 1 stack) of 1dps
// With effect multiplier chain: 10 projectiles + ignite (0.2dps) => 2.2 damage
// Without effect multiplier chain: 10 projectiles + ignite (1dps) => 3 damage

// Attack (2 damage) -> triggers ignite (max 1 stack) of 1dps
// With effect multiplier chain: 1 attack + ignite (2dps) => 4 damage
// Without effect multiplier chain: 1 attack + ignite (1dps) => 3 damage