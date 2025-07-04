using System;

public class Trigger_Expire : Trigger<Trigger_Expire>, IDataContainer_Player, IDataContainer_OwnerEntity
{
    private Entity _owner;
    public Entity Entity => _owner;
    public Viewer Player => _owner.GetMechanic<Stat_PlayerOwner>().player;
    public Trigger_Expire() { }
    public Trigger_Expire(Entity entity, params object[] bindingObjects)
    {
        _owner = entity;
        Invoke(bindingObjects);
    }
}
public class Trigger_ProjectileCreated : Trigger<Trigger_ProjectileCreated>, IDataContainer_OwnerEntity
{
    private Entity _owner;
    public Entity Entity => _owner;
    public Trigger_ProjectileCreated() { }
    public Trigger_ProjectileCreated(Entity entity, params object[] bindingObjects)
    {
        _owner = entity;
        Invoke(bindingObjects);
    }
}
public class Trigger_SummonCreated : Trigger<Trigger_SummonCreated>, IDataContainer_OwnerEntity
{
    private Entity _owner;
    public Entity Entity => _owner;
    public Trigger_SummonCreated() { }
    public Trigger_SummonCreated(Entity entity, params object[] bindingObjects)
    {
        _owner = entity;
        Invoke(bindingObjects);
    }
}
public class Trigger_Channel : Trigger<Trigger_Channel>, IDataContainer_OwnerEntity
{
    private Entity _owner;
    public Entity Entity => _owner;

    public Trigger_Channel() { }
    public Trigger_Channel(Entity entity, params object[] bindingObjects)
    {
        _owner = entity;
        Invoke(bindingObjects);
    }
}
public class Trigger_SpellCast : Trigger<Trigger_SpellCast>, IDataContainer_OwnerEntity, IDataContainer_Spell
{
    private Entity _owner;
    public Entity Entity => _owner;
    public Spell _spell;
    public Spell Spell => _spell;

    public Trigger_SpellCast() { }
    public Trigger_SpellCast(Entity entity, Spell spell, params object[] bindingObjects)
    {
        _owner = entity;
        _spell = spell;
        Invoke(bindingObjects);
    }
}
public class Trigger_SpellStageIncrement : Trigger<Trigger_SpellStageIncrement>, IDataContainer_OwnerEntity, IDataContainer_Spell
{
    private Entity _owner;
    public Entity Entity => _owner;
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
public class Trigger_SpellMaxStage : Trigger<Trigger_SpellMaxStage>, IDataContainer_OwnerEntity, IDataContainer_Spell
{
    private Entity _owner;
    public Entity Entity => _owner;
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
public class Trigger_SpellEffectApplied : Trigger<Trigger_SpellEffectApplied>, IDataContainer_OwnerEntity, IDataContainer_Effect, IDataContainer_Spell
{
    private Effect _effect;
    public Effect Effect => _effect;
    private Entity _owner;
    public Entity Entity => _owner;
    private Spell _spell;
    public Spell Spell => _spell;
    public Trigger_SpellEffectApplied() { }
    public Trigger_SpellEffectApplied(Effect effect, Entity entity, Spell spell, params object[] bindingObjects)
    {
        _effect = effect;
        _owner = entity;
        _spell = spell;
        Invoke(bindingObjects);
    }
}

public class Trigger_SpellFinished : Trigger<Trigger_SpellFinished>, IDataContainer_OwnerEntity, IDataContainer_Spell
{
    private Entity _owner;
    public Entity Entity => _owner;
    private Spell _spell;
    public Spell Spell => _spell;
    public Trigger_SpellFinished() { }
    public Trigger_SpellFinished(Entity entity, Spell spell, params object[] bindingObjects)
    {
        _owner = entity;
        _spell = spell;
        Invoke(bindingObjects);
    }
}

public class Trigger_RuneGained : Trigger<Trigger_RuneGained>, IDataContainer_OwnerEntity, IDataContainer_Rune
{
    private Rune _rune;
    public Rune Rune => _rune;
    private Entity _owner;
    public Entity Entity => _owner;

    public Trigger_RuneGained() { }
    public Trigger_RuneGained(Entity entity, Rune rune, params object[] bindingObjects)
    {
        _owner = entity;
        _rune = rune;
        Invoke(bindingObjects);
    }
}