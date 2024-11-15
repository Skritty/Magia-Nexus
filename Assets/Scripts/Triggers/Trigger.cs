using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using TwitchLib.Api.Helix;
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

public class Trigger_Immediate : Trigger<Trigger_Immediate>
{
    public override void Subscribe(Entity source, Action<Trigger> method, EffectTag tags, int order, bool globalSubscribe = false)
    {
        method.Invoke(this);
    }
    public override void Unsubscribe(Entity source, Action<Trigger> method) { }
}
public class Trigger_Expire : Trigger<Trigger_Expire> { }
public class Trigger_ProjectileCreated : Trigger<Trigger_ProjectileCreated> { }
public class Trigger_MovementDirectionCalc : Trigger<Trigger_MovementDirectionCalc> { }
public class Trigger_OnHit : Trigger<Trigger_OnHit> { }
public class Trigger_OnDamageDealt : Trigger<Trigger_OnDamageDealt> { }
public class Trigger_WhenHit : Trigger<Trigger_WhenHit> { }
public class Trigger_OnDamageRecieved : Trigger<Trigger_OnDamageRecieved> { }
public class Trigger_OnDie : Trigger<Trigger_OnDie> { }
public class Trigger_OnActivateEffect : Trigger<Trigger_OnActivateEffect> { }
public class Trigger_OnProjectilePierce : Trigger<Trigger_OnProjectilePierce> { }
public class Trigger_OnRuneUsed : Trigger<Trigger_OnRuneUsed> { }
public class Trigger_OnActionStart : Trigger<Trigger_OnActionStart> { }
public class Trigger_OnPersistantEffectApplied : Trigger<Trigger_OnPersistantEffectApplied> { }
public class Trigger_OnSpellEffectApplied : Trigger<Trigger_OnSpellEffectApplied> { }