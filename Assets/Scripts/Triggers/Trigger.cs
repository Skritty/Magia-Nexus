using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using TwitchLib.Api.Helix;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Stores methods to call and holds data pertaining to the callback
/// </summary>
[Serializable]
public abstract class Trigger
{
    public class TriggerSubscription
    {
        public Entity source;
        private EffectTag tags;
        public Action<Trigger> method;
        public int order = 0;
        public bool remove;

        public TriggerSubscription(Entity source, Action<Trigger> method, EffectTag tags, int order)
        {
            this.source = source;
            this.tags = tags;
            this.method = method;
            this.order = order;
        }

        public void Invoke(Trigger trigger, EffectTag tags)
        {
            if (tags.HasFlag(tags))
            {
                method?.Invoke(trigger);
            }
        }
    }

    private List<TriggerSubscription> subscriptions = new List<TriggerSubscription>();
    public DataType Data<DataType>() => IBoundInstances<Trigger, DataType>.GetInstance(this, false);
    public Effect TriggeringEffect { get; set; }
    public abstract void AddInstance(Entity source);
    public abstract void RemoveInstance(Entity source);
    public virtual void Subscribe(Entity source, Action<Trigger> method, EffectTag tags, int order, bool bindToSource = true)
    {
        if (bindToSource)
        {
            Subscribe(source, method, tags, order);
        }
        else
        {
            subscriptions.Add(new TriggerSubscription(source, method, tags, order));
            subscriptions.Sort((x, y) =>
            {
                if (x.order == y.order) return 0;
                if (x.order > y.order) return 1;
                return -1;
            });
        }
    }
    protected abstract void Subscribe(Entity source, Action<Trigger> method, EffectTag tags, int order);
    public virtual void Unsubscribe(Entity source, Action<Trigger> method)
    {
        foreach (TriggerSubscription subscription in subscriptions)
        {
            if(subscription.source == source)
            {
                subscription.remove = true;
            }
        }
    }
    public void Invoke(Effect triggeringEffect, EffectTag tags)
    {
        TriggeringEffect = triggeringEffect;
        subscriptions.RemoveAll(x => x.source == null);
        foreach (TriggerSubscription subscription in subscriptions)
        {
            subscription.Invoke(this, tags);
        }
    }
    public void Invoke<T>(T data, Effect triggeringEffect, EffectTag tags)
    {
        TriggeringEffect = triggeringEffect;
        if(data is Effect)
        {
            IBoundInstances<Trigger, Effect>.AddInstance(data as Effect, this);
        }
        IBoundInstances<Trigger, T>.AddInstance(data, this);
        subscriptions.RemoveAll(x => x.source == null);
        foreach (TriggerSubscription subscription in subscriptions)
        {
            subscription.Invoke(this, tags);
        }
        subscriptions.RemoveAll(x => x.remove);
        IBoundInstances<Trigger, T>.RemoveInstance(this);
    }
}

public abstract class Trigger<T> : Trigger where T : Trigger<T>
{
    
    public override void AddInstance(Entity owner)
    {
        IBoundInstances<Entity, T>.AddInstance((T)this, owner);
    }
    public override void RemoveInstance(Entity owner)
    {
        IBoundInstances<Entity, T>.RemoveInstance(owner);
    }
    protected override void Subscribe(Entity source, Action<Trigger> method, EffectTag tags, int order)
    {
        source.Subscribe<T>(method, tags, order);
    }
}

public static class TriggerExtensions
{
    public static void Subscribe<T>(this Entity source, Action<Trigger> method, EffectTag tags = EffectTag.Global, int order = 0) where T : Trigger
    {
        IBoundInstances<Entity, T>.GetOrCreateInstance(source).Subscribe(source, method, tags, order, false);
    }
    public static void Unsubscribe<T>(this Entity source, Action<Trigger> method) where T : Trigger
    {
        IBoundInstances<Entity, T>.GetOrCreateInstance(source).Unsubscribe(source, method);
    }
    public static void Trigger<T>(this Entity source, Effect triggeringEffect = null, EffectTag tags = EffectTag.Global) where T : Trigger
    {
        IBoundInstances<Entity, T>.GetInstance(source, false)?.Invoke(triggeringEffect, tags);
    }
    public static void Trigger<T>(this Entity source, dynamic data, Effect triggeringEffect = null, EffectTag tags = EffectTag.None) where T : Trigger
    {
        IBoundInstances<Entity, T>.GetInstance(source, false)?.Invoke(data, triggeringEffect, tags);
    }
}


public class TriggerTest { }
public class TriggerTest<T> where T : TriggerTest<T>
{
    protected static List<TriggerSubscription> subscriptions = new List<TriggerSubscription>();
    protected class TriggerSubscription
    {
        public Action<T> method;
        public Func<T, bool> conditional;
        public int order = 0;

        public TriggerSubscription(Action<T> method, Func<T, bool> conditional, int order = 0)
        {
            this.method = method;
            this.conditional = conditional;
            this.order = order;
        }
    }

    /// <summary>
    /// Subscribes a method to this Trigger. Be sure to set conditionals and use the unsubscribe method that is returned.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="conditional">Conditions required</param>
    /// <param name="order">Lower numbers will happen earlier in trigger order</param>
    /// <returns>Unsubscribe action</returns>
    public static System.Action Subscribe(Action<T> method, Func<T, bool> conditional, int order = 0)
    {
        TriggerSubscription subscription = new TriggerSubscription(method, conditional, order);
        subscriptions.Add(subscription);
        subscriptions.Sort((x, y) => x.order - y.order);
        return () => subscriptions.Remove(subscription);
    }

    public void Trigger()
    {
        foreach (TriggerSubscription subscription in subscriptions)
        {
            if (!subscription.conditional.Invoke(this as T)) continue;
            subscription.method.Invoke(this as T);
        }
    }
}

public class Trigger_Test : TriggerTest<Trigger_Test>
{
    public int storedData;
    public Trigger_Test(int storedData)
    {
        this.storedData = storedData;
        Trigger();
    }

    public void Example()
    {
        var unsubscribe = Trigger_Test.Subscribe(
            x => Debug.Log($"Does this work? {x.storedData}"), 
            x => x.storedData > 0, 
            -5);
        new Trigger_Test(5);
        unsubscribe.Invoke();
    }
}

public class Trigger_Immediate : Trigger<Trigger_Immediate>
{
    public override void Subscribe(Entity source, Action<Trigger> method, EffectTag tags, int order, bool globalSubscribe = false)
    {
        method.Invoke(this);
    }
    public override void Unsubscribe(Entity source, Action<Trigger> method) { }
}
public class Trigger_OnRemove : Trigger<Trigger_OnRemove>
{
    public override void Subscribe(Entity source, Action<Trigger> method, EffectTag tags, int order, bool globalSubscribe = false) { }
    public override void Unsubscribe(Entity source, Action<Trigger> method) 
    {
        method.Invoke(this);
        base.Unsubscribe(source, method);
    }
}
public class Trigger_Expire : Trigger<Trigger_Expire> { }
public class Trigger_ProjectileCreated : Trigger<Trigger_ProjectileCreated> { }
public class Trigger_MovementDirectionCalc : Trigger<Trigger_MovementDirectionCalc> { }
public abstract class Trigger_Effect<T> : Trigger<T> where T : Trigger_Effect<T>
{
    public Effect effect;
}
public abstract class Trigger_Damage<T> : Trigger_Effect<T> where T : Trigger_Effect<T> 
{
    public DamageInstance damage => effect as DamageInstance;
}
public class Trigger_OnHit : Trigger_Damage<Trigger_OnHit> { }
public class Trigger_OnDamageDealt : Trigger<Trigger_OnDamageDealt> { }
public class Trigger_WhenHit : Trigger<Trigger_WhenHit> { }
public class Trigger_OnDamageRecieved : Trigger<Trigger_OnDamageRecieved> { }
public class Trigger_OnDie : Trigger<Trigger_OnDie> { }
public class Trigger_OnActivateEffect : Trigger<Trigger_OnActivateEffect> { }
public class Trigger_OnProjectilePierce : Trigger<Trigger_OnProjectilePierce> { }
public class Trigger_OnRuneUsed : Trigger<Trigger_OnRuneUsed> { }
public class Trigger_OnActionStart : Trigger<Trigger_OnActionStart> { }
public class Trigger_OnPersistantEffectApplied : Trigger<Trigger_OnPersistantEffectApplied> { }
public class Trigger_OnSpellCast : Trigger<Trigger_OnSpellCast> { }
public class Trigger_OnSpellStageIncrement : Trigger<Trigger_OnSpellStageIncrement> { }
public class Trigger_OnSpellMaxStage : Trigger<Trigger_OnSpellMaxStage> { }
public class Trigger_OnSpellEffectApplied : Trigger<Trigger_OnSpellEffectApplied> { }