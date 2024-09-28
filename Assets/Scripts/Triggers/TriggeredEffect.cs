using Sirenix.OdinInspector;
using System;
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
    [FoldoutGroup("@GetType()")]
    public bool exactTagMatch;
    [FoldoutGroup("@GetType()")]
    public int triggerOrder;
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Trigger trigger;
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Effect effect;
    
    public override void OnGained()
    {
        trigger.AddInstance(Owner);
        trigger.Subscribe(OnTrigger, Tags, exactTagMatch, triggerOrder);
    }
    public override void OnLost()
    {
        trigger.Unsubscribe(OnTrigger);
        trigger.RemoveInstance(Owner);
    }

    protected void OnTrigger(Trigger trigger)
    {
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