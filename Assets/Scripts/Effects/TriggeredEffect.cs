using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[Serializable]
public class TriggeredEffect : PersistentEffect
{
    public TriggeredEffect() { }
    public TriggeredEffect(Trigger trigger, Effect effect) 
    {
        this.trigger = trigger;
        this.effect = effect;
    }
    [SerializeReference]
    public Trigger trigger;
    [SerializeReference]
    public Effect effect;
    public override void OnGained()
    {
        trigger.AddInstance(Owner);
        trigger.Subscribe(OnTrigger);
    }
    public override void OnLost()
    {
        trigger.Unsubscribe(OnTrigger);
        trigger.RemoveInstance(Owner);
    }

    protected void OnTrigger(Trigger trigger)
    {
        effect.Create(this, Owner, trigger);
    }
}