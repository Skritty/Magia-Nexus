using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Effect_AddTrigger : EffectTask
{
    [FoldoutGroup("@GetType()")]
    public bool triggerOnce;
    [FoldoutGroup("@GetType()")]
    public int duration;
    [FoldoutGroup("@GetType()")]
    public int triggerOrder;
    [FoldoutGroup("@GetType()")]
    public EffectTargetSelector bindingObject = EffectTargetSelector.Target;
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Trigger trigger;

    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        if (trigger == null) return;

        object binding = null;
        switch (bindingObject)
        {
            case EffectTargetSelector.None:
                binding = 0;
                break;
            case EffectTargetSelector.Owner:
                binding = owner;
                break;
            case EffectTargetSelector.Target:
                binding = target;
                break;
        }

        System.Action cleanup = trigger.SubscribeToTasks(target, binding, triggerOrder, triggerOnce);
        Modifier<Trigger> dummy = new Modifier<Trigger>(value: trigger, tickDuration: duration);
        target.AddModifier<Trigger, Stat_Triggers>(dummy);
        Trigger_ModifierLost.Subscribe(_ => cleanup?.Invoke(), dummy, 9999, true);
    }
}