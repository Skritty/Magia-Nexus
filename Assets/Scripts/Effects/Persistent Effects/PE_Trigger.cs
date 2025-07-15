using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PE_Trigger : PersistentEffect
{
    [FoldoutGroup("@GetType()")]
    public int triggerOrder;
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Trigger trigger;
    [SerializeReference, FoldoutGroup("@GetType()"), HideReferenceObjectPicker]
    public List<TriggerTask> tasks = new List<TriggerTask>();
    
    protected System.Action unsubscribe;

    public override void OnGained()
    {
        if(trigger == null)
        {
            Debug.LogError($"NO TRIGGER SELECTED | {Owner.name} -> {Target.name}: {GetUID()}");
            return;
        }
        unsubscribe = trigger.SubscribeDynamic(OnTrigger, null, triggerOrder);
    }

    public override void OnLost()
    {
        unsubscribe?.Invoke();
    }

    protected void OnTrigger(IDataContainer data)
    {
        foreach (TriggerTask task in tasks)
        {
            if (!task.DoTask(data, Target)) break;
        }
    }
}

public class PE_Trigger<T> : PE_Trigger
{
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Trigger<T> trigger2;
    [SerializeReference, FoldoutGroup("@GetType()"), HideReferenceObjectPicker]
    public List<TriggerTask<T>> tasks2 = new List<TriggerTask<T>>();

    public override void OnGained()
    {
        if (trigger == null)
        {
            Debug.LogError($"NO TRIGGER SELECTED | {Owner.name} -> {Target.name}: {GetUID()}");
            return;
        }
        unsubscribe = trigger2.SubscribeGeneric(OnTrigger, null, triggerOrder);
    }

    public override void OnLost()
    {
        unsubscribe?.Invoke();
    }

    protected void OnTrigger(T data)
    {
        foreach (TriggerTask<T> task2 in tasks)
        {
            if (!task2.DoTask(data, Target)) break; // TODO THIS IS TERRIBLE
        }
    }
}

public class PE_TriggerEffect : PE_Trigger<DamageInstanceOLD> { }