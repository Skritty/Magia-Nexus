using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Effect_AddTrigger : EffectTask
{
    [FoldoutGroup("@GetType()")]
    public int duration;
    [FoldoutGroup("@GetType()")]
    public int triggerOrder;
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Trigger trigger;

    public override void DoEffect(Entity Owner, Entity target, float multiplier, bool triggered)
    {
        if (trigger == null) return;

        System.Action cleanup = trigger.AddTaskOwner(Owner, order: triggerOrder);
        DummyModifier<Entity> dummy = new DummyModifier<Entity>(value: target, tickDuration: duration);
        target.AddModifier<Stat_Triggers>(dummy);
        Trigger_ModifierLost.Subscribe(_ => cleanup?.Invoke(), dummy, 0, true);
    }
}