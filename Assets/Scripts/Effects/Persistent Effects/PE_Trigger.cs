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
            Debug.LogError($"NO TRIGGER SELECTED | {Owner} -> {Target}: {GetUID()}");
        }
        unsubscribe = trigger.Subscribe(OnTrigger);
    }

    public override void OnLost()
    {
        unsubscribe?.Invoke();
    }

    protected void OnTrigger(Trigger trigger)
    {
        foreach (TriggerTask task in tasks)
        {
            if (!task.TriggerRecieved(trigger, Owner)) break;
        }
    }
}
