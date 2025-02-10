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
    private System.Action unsubscribe;

    public override void OnGained()
    {
        if(trigger == null)
        {
            Debug.LogError($"NO TRIGGER SELECTED | {Owner.name} -> {Target.name}: {GetUID()}");
            return;
        }
        unsubscribe = trigger.Subscribe(OnTrigger, triggerOrder);
    }

    public override void OnLost()
    {
        unsubscribe?.Invoke();
    }

    protected void OnTrigger(Trigger trigger)
    {
        foreach (TriggerTask task in tasks)
        {
            if (!task.DoTask(trigger, Target)) break;
        }
    }
}
