public class Trigger_MovementDirectionCalc : Trigger<Trigger_MovementDirectionCalc>, IDataContainer_OwnerEntity
{
    private Entity _entity;
    public Entity Entity => _entity;

    public Trigger_MovementDirectionCalc() { }
    public Trigger_MovementDirectionCalc(Entity owner, params object[] bindingObjects)
    {
        _entity = owner;
        Invoke(bindingObjects);
    }
}
public class Trigger_PreHit : Trigger<Trigger_PreHit>, IDataContainer_DamageInstance
{
    private DamageInstance _damageInstance;
    public DamageInstance Damage => _damageInstance;
    public Effect Effect => _damageInstance;
    public Trigger_PreHit() { }
    public Trigger_PreHit(DamageInstance damageInstance, params object[] bindingObjects)
    {
        _damageInstance = damageInstance;
        Invoke(bindingObjects);
    }
}
public class Trigger_Hit : Trigger<Trigger_Hit>, IDataContainer_DamageInstance
{
    private DamageInstance _damageInstance;
    public DamageInstance Damage => _damageInstance;
    public Effect Effect => _damageInstance;
    public Trigger_Hit() { }
    public Trigger_Hit(DamageInstance damageInstance, params object[] bindingObjects)
    {
        _damageInstance = damageInstance;
        Invoke(bindingObjects);
    }
}
public class Trigger_Damage : Trigger<Trigger_Damage>, IDataContainer_DamageInstance
{
    private DamageInstance _damageInstance;
    public DamageInstance Damage => _damageInstance;
    public Effect Effect => _damageInstance;
    public Trigger_Damage() { }
    public Trigger_Damage(DamageInstance damageInstance, params object[] bindingObjects)
    {
        _damageInstance = damageInstance;
        Invoke(bindingObjects);
    }
}
public class Trigger_Die : Trigger<Trigger_Die>, IDataContainer_OwnerEntity, IDataContainer_DamageInstance
{
    private DamageInstance _damageInstance;
    public DamageInstance Damage => _damageInstance;
    public Effect Effect => _damageInstance;
    private Entity _owner;
    public Entity Entity => _owner;

    public Trigger_Die() { }
    public Trigger_Die(DamageInstance damageInstance, params object[] bindingObjects)
    {
        _owner = damageInstance.Target;
        _damageInstance = damageInstance;
        Invoke(bindingObjects);
    }
}

public class Trigger_PersistantEffectApplied : Trigger<Trigger_PersistantEffectApplied>, IDataContainer_Effect
{
    private Effect _effect;
    public Effect Effect => _effect;
    public PersistentEffect PersistentEffect => _effect as PersistentEffect;
    public Trigger_PersistantEffectApplied() { }
    protected Trigger_PersistantEffectApplied(PersistentEffect persistentEffect, params object[] bindingObjects)
    {
        _effect = persistentEffect;
        Invoke(bindingObjects);
    }
    
}
public class Trigger_ProjectilePierce : Trigger<Trigger_ProjectilePierce>, IDataContainer_Effect
{
    private Effect _effect;
    public Effect Effect => _effect;
    public Trigger_ProjectilePierce() { }
    public Trigger_ProjectilePierce(Effect effect, params object[] bindingObjects)
    {
        _effect = effect;
        Invoke(bindingObjects);
    }
}

public class Trigger_PersistentEffectGained : Trigger<Trigger_PersistentEffectGained>, IDataContainer_Effect, IDataContainer_PersistentEffect
{
    private Effect _effect;
    public Effect Effect => _effect;
    public PersistentEffect PersistentEffect => _effect as PersistentEffect;
    public Trigger_PersistentEffectGained() { }
    public Trigger_PersistentEffectGained(Effect effect, params object[] bindingObjects)
    {
        _effect = effect;
        Invoke(bindingObjects);
    }
}

public class Trigger_PersistentEffectLost : Trigger<Trigger_PersistentEffectLost>, IDataContainer_Effect, IDataContainer_PersistentEffect
{
    private Effect _effect;
    public Effect Effect => _effect;
    public PersistentEffect PersistentEffect => _effect as PersistentEffect;
    public Trigger_PersistentEffectLost() { }
    public Trigger_PersistentEffectLost(Effect effect, params object[] bindingObjects)
    {
        _effect = effect;
        Invoke(bindingObjects);
    }
}