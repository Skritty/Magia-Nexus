using System;

public class Trigger_Expire : Trigger<Trigger_Expire>, ITriggerData_OwnerEntity
{
    private Entity _owner;
    public Entity Owner => _owner;
    public Trigger_Expire() { }
    public Trigger_Expire(Entity entity, params object[] bindingObjects)
    {
        _owner = entity;
        Invoke(bindingObjects);
    }
}
public class Trigger_ProjectileCreated : Trigger<Trigger_ProjectileCreated>, ITriggerData_OwnerEntity
{
    private Entity _owner;
    public Entity Owner => _owner;
    public Trigger_ProjectileCreated() { }
    public Trigger_ProjectileCreated(Entity entity, params object[] bindingObjects)
    {
        _owner = entity;
        Invoke(bindingObjects);
    }
}
public class Trigger_SpellCast : Trigger<Trigger_SpellCast>, ITriggerData_OwnerEntity, ITriggerData_Spell
{
    private Entity _owner;
    public Entity Owner => _owner;
    public Spell _spell;
    public Spell Spell => _spell;

    public Trigger_SpellCast() { }
    public Trigger_SpellCast(Entity entity, params object[] bindingObjects)
    {
        _owner = entity;
        Invoke(bindingObjects);
    }
}
public class Trigger_SpellStageIncrement : Trigger<Trigger_SpellStageIncrement>, ITriggerData_OwnerEntity, ITriggerData_Spell
{
    private Entity _owner;
    public Entity Owner => _owner;
    private Spell _spell;
    public Spell Spell => _spell;
    public Trigger_SpellStageIncrement() { }
    public Trigger_SpellStageIncrement(Entity entity, Spell spell, params object[] bindingObjects)
    {
        _owner = entity;
        _spell = spell;
        Invoke(bindingObjects);
    }
}
public class Trigger_SpellMaxStage : Trigger<Trigger_SpellMaxStage>, ITriggerData_OwnerEntity, ITriggerData_Spell
{
    private Entity _owner;
    public Entity Owner => _owner;
    private Spell _spell;
    public Spell Spell => _spell;
    public Trigger_SpellMaxStage() { }
    public Trigger_SpellMaxStage(Entity entity, Spell spell, params object[] bindingObjects)
    {
        _owner = entity;
        _spell = spell;
        Invoke(bindingObjects);
    }
}
public class Trigger_SpellEffectApplied : Trigger<Trigger_SpellEffectApplied>, ITriggerData_OwnerEntity, ITriggerData_Spell
{
    private Entity _owner;
    public Entity Owner => _owner;
    private Spell _spell;
    public Spell Spell => _spell;
    public Trigger_SpellEffectApplied() { }
    public Trigger_SpellEffectApplied(Entity entity, Spell spell, params object[] bindingObjects)
    {
        _owner = entity;
        _spell = spell;
        Invoke(bindingObjects);
    }
}