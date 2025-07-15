public class Trigger_MovementDirectionCalc : Trigger<Entity>
{
    public Trigger_MovementDirectionCalc() { }
    public Trigger_MovementDirectionCalc(Entity owner, params object[] bindingObjects)
    {
        Value = owner;
        Invoke(bindingObjects);
    }
}
public class Trigger_PreHit : Trigger<EffectTask>
{
    public Trigger_PreHit() { }
    public Trigger_PreHit(EffectTask damageInstance, params object[] bindingObjects)
    {
        Value = damageInstance;
        Invoke(bindingObjects);
    }
}
public class Trigger_Hit : Trigger<EffectTask>
{
    public Trigger_Hit() { }
    public Trigger_Hit(EffectTask damageInstance, params object[] bindingObjects)
    {
        Value = damageInstance;
        Invoke(bindingObjects);
    }
}
public class Trigger_Damage : Trigger<DamageInstanceOLD>
{
    public Trigger_Damage() { }
    public Trigger_Damage(DamageInstanceOLD damageInstance, params object[] bindingObjects)
    {
        Value = damageInstance;
        Invoke(bindingObjects);
    }
}
public class Trigger_Die : Trigger<DamageInstanceOLD>
{
    public Trigger_Die() { }
    public Trigger_Die(DamageInstanceOLD damageInstance, params object[] bindingObjects)
    {
        Value = damageInstance;
        Invoke(bindingObjects);
    }
}

public class Trigger_PersistantEffectApplied : Trigger<PersistentEffect>
{
    public Trigger_PersistantEffectApplied() { }
    protected Trigger_PersistantEffectApplied(PersistentEffect persistentEffect, params object[] bindingObjects)
    {
        Value = persistentEffect;
        Invoke(bindingObjects);
    }
    
}
public class Trigger_ProjectilePierce : Trigger<Effect>
{
    public Trigger_ProjectilePierce() { }
    public Trigger_ProjectilePierce(Effect effect, params object[] bindingObjects)
    {
        Value = effect;
        Invoke(bindingObjects);
    }
}

public class Trigger_PersistentEffectGained : Trigger<PersistentEffect>
{
    public Trigger_PersistentEffectGained() { }
    public Trigger_PersistentEffectGained(PersistentEffect effect, params object[] bindingObjects)
    {
        Value = effect;
        Invoke(bindingObjects);
    }
}

public class Trigger_PersistentEffectLost : Trigger<PersistentEffect>
{
    public Trigger_PersistentEffectLost() { }
    public Trigger_PersistentEffectLost(PersistentEffect effect, params object[] bindingObjects)
    {
        Value = effect;
        Invoke(bindingObjects);
    }
}