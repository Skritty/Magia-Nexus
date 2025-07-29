using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Effect_AddTrigger : EffectTask
{
    [FoldoutGroup("@GetType()")]
    public int duration;
    [FoldoutGroup("@GetType()")]
    public int triggerOrder;
    [FoldoutGroup("@GetType()")]
    public EffectTargetSelector bindingObject;
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Trigger trigger;
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<ITaskOwned<Entity, dynamic>> tasks;

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

        System.Action cleanup = trigger.SubscribeMethodToTasks(target, x => DoTasks(target, x), binding, triggerOrder);
        DummyModifier<Entity> dummy = new DummyModifier<Entity>(value: target, tickDuration: duration);
        target.AddModifier<Stat_Triggers>(dummy);
        Trigger_ModifierLost.Subscribe(_ => cleanup?.Invoke(), dummy, 0, true);
    }

    private void DoTasks(Entity owner, dynamic data)
    {
        foreach(ITaskOwned<Entity, dynamic> task in tasks)
        {
            task.DoTask(owner, data);
        }
    }
}