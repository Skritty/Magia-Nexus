using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public abstract class Trigger
{
    public abstract System.Action AddTaskOwner(Entity owner, ICollection<TriggerTask> additionalTasks = null, int order = 0);
}

public abstract class Trigger<Endpoint, T> : Trigger
{
    [SerializeReference]
    public List<TriggerTask<T>> tasks = new();
    protected static Dictionary<object, List<TriggerSubscription>> bindings = new();
    protected class TriggerSubscription
    {
        public Action<T> method;
        public int order = 0;
        public bool triggerOnce = false;

        public TriggerSubscription(Action<T> method, int order = 0, bool triggerOnce = false)
        {
            this.method = method;
            this.order = order;
            this.triggerOnce = triggerOnce;
        }
    }

    public override System.Action AddTaskOwner(Entity owner, ICollection<TriggerTask> additionalTasks, int order = 0)
    {
        return Subscribe(data => DoTriggerTasks(data, additionalTasks, owner), owner, order);
    }

    private void DoTriggerTasks(T data, ICollection<TriggerTask> additionalTasks, Entity owner)
    {
        foreach (TriggerTask<T> task in tasks)
        {
            if (!task.DoTask(data, owner)) return;
        }

        if (additionalTasks == null) return;
        foreach (TriggerTask task in additionalTasks)
        {
            try
            {
                task.DoTaskNoData(owner);
            }
            catch
            {
                Debug.LogError($"{task.GetType().Name} on {owner.gameObject.name} is invalid for null data input!!");
            }
        }
    }

    /// <summary>
    /// Subscribes a method to this Trigger. Be sure to set conditionals and use the unsubscribe method that is returned.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="order">Lower numbers will happen earlier in trigger order</param>
    /// <returns>Unsubscribe action</returns>
    public static System.Action Subscribe(Action<T> method, object bindingObject = null, int order = 0, bool triggerOnce = false)
    {
        if (bindingObject == null)
        {
            bindingObject = 0;
        }
        if (!bindings.ContainsKey(bindingObject))
        {
            bindings.Add(bindingObject, new List<TriggerSubscription>());
        }

        TriggerSubscription subscription = new TriggerSubscription(method, order, triggerOnce);
        bindings[bindingObject].Add(subscription);
        bindings[bindingObject].Sort((x, y) => x.order - y.order);

        foreach(KeyValuePair<object, List<TriggerSubscription>> binding in bindings.ToArray())
        {
            if(binding.Key == null) bindings.Remove(binding.Key);
        }

        return () => bindings[bindingObject].Remove(subscription);
    }

    public static void Invoke(T data, params object[] bindingObjects)
    {
        foreach (object bindingObject in bindingObjects.Distinct())
        {
            // Bound Triggers
            if (bindings.ContainsKey(bindingObject))
                foreach (TriggerSubscription subscription in bindings[bindingObject].ToArray())
                {
                    subscription.method.Invoke(data);
                    if (subscription.triggerOnce) bindings[bindingObject].Remove(subscription);
                }
        }
        // Global Triggers
        if (bindings.ContainsKey(0))
            foreach (TriggerSubscription subscription in bindings[0].ToArray())
            {
                subscription.method.Invoke(data);
                if (subscription.triggerOnce) bindings[0].Remove(subscription);
            }
    }
}