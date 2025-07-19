using Unity.Burst.CompilerServices;

public class Trigger_MovementDirectionCalc : Trigger<Entity>
{
    public Trigger_MovementDirectionCalc() { }
    public Trigger_MovementDirectionCalc(Entity owner, params object[] bindingObjects)
    {
        _value = owner;
        Invoke(bindingObjects);
    }
}
public class Trigger_PreHit : Trigger<Hit>
{
    public Trigger_PreHit() { }
    public Trigger_PreHit(Hit hit, params object[] bindingObjects)
    {
        _value = hit;
        Invoke(bindingObjects);
    }
}
public class Trigger_Hit : Trigger<Hit>
{
    public Trigger_Hit() { }
    public Trigger_Hit(Hit hit, params object[] bindingObjects)
    {
        _value = hit;
        Invoke(bindingObjects);
    }
}
public class Trigger_Damage : Trigger<DamageInstance>
{
    public Trigger_Damage() { }
    public Trigger_Damage(DamageInstance damageInstance, params object[] bindingObjects)
    {
        _value = damageInstance;
        Invoke(bindingObjects);
    }
}
public class Trigger_Die : Trigger<DamageInstance>
{
    public Trigger_Die() { }
    public Trigger_Die(DamageInstance damageInstance, params object[] bindingObjects)
    {
        _value = damageInstance;
        Invoke(bindingObjects);
    }
}
public class Trigger_ProjectilePierce : Trigger<Entity>
{
    public Trigger_ProjectilePierce() { }
    public Trigger_ProjectilePierce(Entity entity, params object[] bindingObjects)
    {
        _value = entity;
        Invoke(bindingObjects);
    }
}
public class Trigger_ModifierGained : Trigger<IModifier>
{
    public Trigger_ModifierGained() { }
    public Trigger_ModifierGained(IModifier modifier, params object[] bindingObjects)
    {
        _value = modifier;
        Invoke(bindingObjects);
    }
}
public class Trigger_ModifierLost : Trigger<IModifier>
{
    public Trigger_ModifierLost() { }
    public Trigger_ModifierLost(IModifier modifier, params object[] bindingObjects)
    {
        _value = modifier;
        Invoke(bindingObjects);
    }
}
public class Trigger_BuffGained : Trigger<IModifier>
{
    public Trigger_BuffGained() { }
    public Trigger_BuffGained(IModifier modifier, params object[] bindingObjects)
    {
        _value = modifier;
        Invoke(bindingObjects);
    }
}
public class Trigger_BuffLost : Trigger<IModifier>
{
    public Trigger_BuffLost() { }
    public Trigger_BuffLost(IModifier modifier, params object[] bindingObjects)
    {
        _value = modifier;
        Invoke(bindingObjects);
    }
}
public class Trigger_DebuffGained : Trigger<IModifier>
{
    public Trigger_DebuffGained() { }
    public Trigger_DebuffGained(IModifier modifier, params object[] bindingObjects)
    {
        _value = modifier;
        Invoke(bindingObjects);
    }
}
public class Trigger_DebuffLost : Trigger<IModifier>
{
    public Trigger_DebuffLost() { }
    public Trigger_DebuffLost(IModifier modifier, params object[] bindingObjects)
    {
        _value = modifier;
        Invoke(bindingObjects);
    }
}
