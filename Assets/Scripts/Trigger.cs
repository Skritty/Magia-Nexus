using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using UnityEngine;

/// <summary>
/// Stores methods to call and holds data pertaining to the callback
/// </summary>
[Serializable]
public abstract class Trigger
{
    protected Action<Trigger> ToTrigger;
    public DataType Data<DataType>() => IBoundInstances<Trigger, DataType>.GetInstance(this, false);
    public abstract void AddInstance(Entity source);
    public abstract void RemoveInstance(Entity source);
    public virtual void Subscribe(Action<Trigger> method)
    {
        ToTrigger += method;
    }
    public virtual void Unsubscribe(Action<Trigger> method)
    {
        ToTrigger -= method;
    }
    public void Invoke()
    {
        if (ToTrigger == null) return;
        ToTrigger.Invoke(this);
    }
    public void Invoke<T>(T data)
    {
        if (ToTrigger == null) return;
        IBoundInstances<Trigger, T>.AddInstance(data, this);
        ToTrigger.Invoke(this);
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
}

public static class TriggerExtensions
{
    public static void Subscribe<T>(this Entity source, Action<Trigger> method) where T : Trigger
    {
        IBoundInstances<Entity, T>.GetOrCreateInstance(source).Subscribe(method);
    }
    public static void Unsubscribe<T>(this Entity source, Action<Trigger> method) where T : Trigger
    {
        IBoundInstances<Entity, T>.GetOrCreateInstance(source).Unsubscribe(method);
    }
    public static void Trigger<T>(this Entity source) where T : Trigger
    {
        IBoundInstances<Entity, T>.GetInstance(source, false)?.Invoke();
    }
    public static void Trigger<T>(this Entity source, dynamic data) where T : Trigger
    {
        IBoundInstances<Entity, T>.GetInstance(source, false)?.Invoke(data);
    }
}

public class Trigger_Immediate : Trigger<Trigger_Immediate>
{
    public override void Subscribe(Action<Trigger> method)
    {
        method.Invoke(this);
    }
    public override void Unsubscribe(Action<Trigger> method) { }
}
public class Trigger_Expire : Trigger<Trigger_Expire> { }
public class Trigger_OnItemTick : Trigger<Trigger_OnItemTick> { }
public class Trigger_ProjectileCreated : Trigger<Trigger_ProjectileCreated> { }
public class Trigger_OnDamageCalculation : Trigger<Trigger_OnDamageCalculation> { }
public class Trigger_OnHit : Trigger<Trigger_OnHit> { }
public class Trigger_OnDamageDealt : Trigger<Trigger_OnDamageDealt> { }
public class Trigger_WhenHit : Trigger<Trigger_WhenHit> { }
public class Trigger_OnDamageRecieved : Trigger<Trigger_OnDamageRecieved> { }
public class Trigger_OnDie : Trigger<Trigger_OnDie> { }
public class Trigger_OnProjectilePierce : Trigger<Trigger_OnProjectilePierce> { }
public class Trigger_OnPersistantEffectApplied : Trigger<Trigger_OnPersistantEffectApplied> { }
public class Trigger_OnSpellEffectApplied : Trigger<Trigger_OnSpellEffectApplied> { }
public class Trigger_OnRuneUsed : Trigger<Trigger_OnRuneUsed> { }
public class Trigger_OnActionStart : Trigger<Trigger_OnActionStart> { }