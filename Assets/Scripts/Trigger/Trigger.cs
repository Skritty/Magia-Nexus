using System;
using System.Collections.Generic;
using System.Linq;
using TwitchLib.Api.Helix.Models.Subscriptions;
using UnityEngine;

[Serializable]
public abstract class Trigger : IDataContainer
{
    public bool Get<T>(out T data)
    {
        IDataContainer<T> container = (this as IDataContainer<T>);
        if (container == null) data = default;
        else data = container.Value;
        return container != null;
    }

    public abstract void Invoke(params object[] bindingObjects);
    public abstract System.Action SubscribeDynamic(Action<IDataContainer> method, object bindingObject, int order = 0);
}

public abstract class Trigger<T> : Trigger, IDataContainer<T>
{
    protected T _value;
    public T Value => _value;
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

    public virtual System.Action SubscribeGeneric(Action<T> method, object bindingObject, int order = 0) => Subscribe(method, bindingObject, order);
    public override System.Action SubscribeDynamic(Action<IDataContainer> method, object bindingObject, int order = 0) => Subscribe(_ => method.Invoke(this), bindingObject, order);

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

    public override void Invoke(params object[] bindingObjects)
    {
        foreach (object bindingObject in bindingObjects.Distinct())
        {
            // Bound Triggers
            if (bindings.ContainsKey(bindingObject))
                foreach (TriggerSubscription subscription in bindings[bindingObject].ToArray())
                {
                    subscription.method.Invoke(Value);
                    if (subscription.triggerOnce) bindings[bindingObject].Remove(subscription);
                }
        }

        // Global Triggers
        if (bindings.ContainsKey(0))
            foreach (TriggerSubscription subscription in bindings[0].ToArray())
            {
                subscription.method.Invoke(Value);
                if (subscription.triggerOnce) bindings[0].Remove(subscription);
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

public class Trigger_Immediate : Trigger<IDataContainer>
{
    public override System.Action SubscribeGeneric(Action<IDataContainer> method, object bindingObject, int order = 0)
    {
        Invoke();
        return () => { };
    }
    public override System.Action SubscribeDynamic(Action<IDataContainer> method, object bindingObject, int order = 0) => SubscribeGeneric(method, bindingObject, order);
}
public class Trigger_OnRemove : Trigger<IDataContainer>
{
    public override System.Action SubscribeGeneric(Action<IDataContainer> method, object bindingObject, int order = 0)
    {
        return () => Invoke();
    }
    public override System.Action SubscribeDynamic(Action<IDataContainer> method, object bindingObject, int order = 0) => SubscribeGeneric(method, bindingObject, order);
}
public class Trigger_GameEnd : Trigger<Viewer>
{
    public Trigger_GameEnd() { }
    public Trigger_GameEnd(Viewer player, params object[] bindingObjects)
    {
        Value = player;
        Invoke(bindingObjects);
    }
}
public class Trigger_RoundEnd : Trigger<Viewer>
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
