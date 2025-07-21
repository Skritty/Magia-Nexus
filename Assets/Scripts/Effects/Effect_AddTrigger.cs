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
    [SerializeReference, FoldoutGroup("@GetType()"), HideReferenceObjectPicker]
    public List<TriggerTask> tasks = new List<TriggerTask>();

    public override void DoEffect(Entity Owner, Entity target, float multiplier, bool triggered)
    {
        if (trigger == null)
        {
            Debug.LogError($"NO TRIGGER SELECTED | {Owner.name} -> {target.name}");
            return;
        }

        System.Action cleaup = trigger.SubscribeDynamic(x => OnTrigger(target, x), null, triggerOrder);
        TriggerModifier<Entity> dummy = new TriggerModifier<Entity>(
            cleaup, value: target, temporary: (duration > 0 ? true : false), tickDuration: duration);
        target.AddModifier<Stat_Triggers>(dummy);
        Trigger_ModifierLost.Subscribe(Unsubscribe, dummy, 0, true);
    }

    private void Unsubscribe(IDataContainer data)
    {
        (data as TriggerModifier<Entity>).Cleanup?.Invoke();
    }

    protected void OnTrigger(Entity target, IDataContainer data)
    {
        foreach (TriggerTask task in tasks)
        {
            if (!task.DoTask(data, target)) break;
        }
    }
}

public class TriggerModifier<T> : IDataContainer<T>, IModifier
{
    public System.Action Cleanup { get; }
    public T Value { get; }
    public IStatTag Tag { get; }
    public Alignment Alignment { get; }
    public int MaxStacks { get; }
    public int StacksAdded { get; } = 1;
    public bool PerPlayer { get; }
    public bool Temporary { get; }
    public int TickDuration { get; }
    public bool RefreshDuration { get; }

    public TriggerModifier() { }

    public TriggerModifier(System.Action cleanup, T value = default, IStatTag tag = default, Alignment alignment = Alignment.Neutral,
        int maxStacks = 0, int stacksAdded = 1, bool perPlayer = false,
        bool temporary = false, int tickDuration = 0, bool refreshDuration = false)
    {
        Cleanup = cleanup;
        Value = value;
        Tag = tag;
        Alignment = alignment;
        MaxStacks = maxStacks;
        StacksAdded = stacksAdded;
        PerPlayer = perPlayer;
        Temporary = temporary;
        TickDuration = tickDuration;
        RefreshDuration = refreshDuration;
    }

    public bool Get<Type>(out Type data)
    {
        data = (Type)(Value as object);
        return data != null;
    }
}