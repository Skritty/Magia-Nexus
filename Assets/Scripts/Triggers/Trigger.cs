using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using TwitchLib.Api.Helix;
using TwitchLib.Api.Helix.Models.Subscriptions;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public abstract class Trigger
{
    public abstract System.Action Subscribe(Action<Trigger> method, int order = 0);
}

public abstract class Trigger<T> : Trigger where T : Trigger
{
    protected static List<TriggerSubscription> subscriptions = new List<TriggerSubscription>();
    protected class TriggerSubscription
    {
        public Action<T> method;
        public int order = 0;

        public TriggerSubscription(Action<T> method, int order = 0)
        {
            this.method = method;
            this.order = order;
        }
    }

    public override System.Action Subscribe(Action<Trigger> method, int order = 0) => Subscribe(method, order);

    /// <summary>
    /// Subscribes a method to this Trigger. Be sure to set conditionals and use the unsubscribe method that is returned.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="order">Lower numbers will happen earlier in trigger order</param>
    /// <returns>Unsubscribe action</returns>
    public static System.Action Subscribe(Action<T> method, int order = 0)
    {
        TriggerSubscription subscription = new TriggerSubscription(method, order);
        subscriptions.Add(subscription);
        subscriptions.Sort((x, y) => x.order - y.order);
        return () => subscriptions.Remove(subscription);
    }

    protected void Invoke()
    {
        foreach (TriggerSubscription subscription in subscriptions)
        {
            subscription.method.Invoke(this as T);
        }
    }
}

public class Trigger_OnSomethingHappens : Trigger<Trigger_OnSomethingHappens>
{
    public int storedData;
    public Trigger_OnSomethingHappens(int storedData)
    {
        this.storedData = storedData;
        Invoke();
    }

    public void Example()
    {
        var unsubscribe = Trigger_OnSomethingHappens.Subscribe(
            x => Debug.Log($"Does this work? {x.storedData}"),
            -5);
        new Trigger_OnSomethingHappens(5);
        unsubscribe.Invoke();
    }
}

public class Trigger_Immediate : Trigger<Trigger_Immediate>
{
    public override System.Action Subscribe(Action<Trigger> method, int order = 0)
    {
        return () => {};
    }
}
public class Trigger_OnRemove : Trigger<Trigger_OnRemove>
{
    public override System.Action Subscribe(Action<Trigger> method, int order = 0)
    {
        return () => method.Invoke(this);
    }
}