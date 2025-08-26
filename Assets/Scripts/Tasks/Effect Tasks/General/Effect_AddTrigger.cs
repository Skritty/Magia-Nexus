using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class Effect_AddTrigger : EffectTask
{
    [FoldoutGroup("@GetType()")]
    public bool triggerOnce;
    [FoldoutGroup("@GetType()")]
    public int triggerOrder;
    [FoldoutGroup("@GetType()")]
    public EffectTargetSelector bindingObject = EffectTargetSelector.Target;
    [FoldoutGroup("@GetType()")]
    public Modifier<Trigger> triggerModifier;

    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        if (triggerModifier == null || triggerModifier.Value == null) return;
        if (triggerModifier.Tag == null) triggerModifier.StatTag = new Stat_Triggers();

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

        System.Action triggerCleanup = null;
        Trigger_ModifierGained.Subscribe(x => 
        {
            // Subscribe to trigger if its the first modifier
            if(owner.Stat<Stat_Triggers>().Contains(triggerModifier, out int count) && count == 1)
            {
                triggerCleanup += triggerModifier.Value.SubscribeToTasks(target, binding, triggerOrder, triggerOnce);
            }
        }, triggerModifier, 9999, true);

        System.Action cleanup = null;
        cleanup = Trigger_ModifierLost.Subscribe(x =>
        {
            // If there are no more of this modifier, unsubscribe
            if (!owner.Stat<Stat_Triggers>().Contains(triggerModifier, out _))
            {
                triggerCleanup?.Invoke();
                cleanup?.Invoke();
            }
        }, triggerModifier, 9999);

        target.AddModifier(triggerModifier);
    }
}