using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public abstract class Trigger : IDataContainer
{
    public bool Is<T>(out T data) where T : class, IDataContainer
    {
        data = this as T;
        return data != null;
    }
    public abstract void Invoke(params object[] bindingObjects);
    public abstract System.Action Subscribe(Action<Trigger> method, object bindingObject, int order = 0);
}

public abstract class Trigger<T> : Trigger where T : Trigger
{
    protected static Dictionary<object, List<TriggerSubscription>> bindings = new();
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

    public override System.Action Subscribe(Action<Trigger> method, object bindingObject, int order = 0) => Subscribe(method, bindingObject, order);

    /// <summary>
    /// Subscribes a method to this Trigger. Be sure to set conditionals and use the unsubscribe method that is returned.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="order">Lower numbers will happen earlier in trigger order</param>
    /// <returns>Unsubscribe action</returns>
    public static System.Action Subscribe(Action<T> method, object bindingObject = null, int order = 0)
    {
        if (bindingObject == null)
        {
            bindingObject = 0;
        }
        if (!bindings.ContainsKey(bindingObject))
        {
            bindings.Add(bindingObject, new List<TriggerSubscription>());
        }

        TriggerSubscription subscription = new TriggerSubscription(method, order);
        bindings[bindingObject].Add(subscription);
        bindings[bindingObject].Sort((x, y) => x.order - y.order);

        foreach(KeyValuePair<object, List<TriggerSubscription>> binding in bindings.ToArray())
        {
            if(binding.Key == null) bindings.Remove(binding.Key);
        }

        return () => bindings[bindingObject].Remove(subscription);
    }

    public override void Invoke(params object[] bindingObjects)
    {
        foreach (object bindingObject in bindingObjects.Distinct())
        {
            // Bound Triggers
            if (bindings.ContainsKey(bindingObject))
                foreach (TriggerSubscription subscription in bindings[bindingObject].ToArray())
                {
                    subscription.method.Invoke(this as T);
                }
        }

        // Global Triggers
        if (bindings.ContainsKey(0))
            foreach (TriggerSubscription subscription in bindings[0].ToArray())
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

    /*public static System.Action Subscribe(Action<Trigger_OnSomethingHappens> method, int order = 0, int two = 2)
    {
        TriggerSubscription subscription = new TriggerSubscription(method, order);
        subscriptions.Add(subscription);
        subscriptions.Sort((x, y) => x.order - y.order);
        return () => subscriptions.Remove(subscription);
    }*/

    public void Example()
    {
        var unsubscribe = Trigger_OnSomethingHappens.Subscribe(
            x => Debug.Log($"Does this work?"),
            -5);
        new Trigger_OnSomethingHappens(5);
        unsubscribe.Invoke();
    }
}

public class Trigger_Immediate : Trigger<Trigger_Immediate>
{
    public override System.Action Subscribe(Action<Trigger> method, object bindingObject, int order = 0)
    {
        return () => {};
    }
}
public class Trigger_OnRemove : Trigger<Trigger_OnRemove>
{
    public override System.Action Subscribe(Action<Trigger> method, object bindingObject, int order = 0)
    {
        return () => method.Invoke(this);
    }
}
public class Trigger_GameEnd : Trigger<Trigger_GameEnd>, IDataContainer_Player 
{
    private Viewer _player;
    public Viewer Player => _player;
    public Trigger_GameEnd() { }
    public Trigger_GameEnd(Viewer player, params object[] bindingObjects)
    {
        _player = player;
        Invoke(bindingObjects);
    }
}
public class Trigger_RoundEnd : Trigger<Trigger_RoundEnd>, IDataContainer_Player
{
    private Viewer _player;
    public Viewer Player => _player;
    public Trigger_RoundEnd() { }
    public Trigger_RoundEnd(Viewer player, params object[] bindingObjects)
    {
        _player = player;
        Invoke(bindingObjects);
    }
}
