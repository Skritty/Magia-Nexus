using System;

public class Trigger_Expire : Trigger<Entity>
{
    public Trigger_Expire() { }
    public Trigger_Expire(Entity entity, params object[] bindingObjects)
    {
        _value = entity;
        Invoke(bindingObjects);
    }
}
public class Trigger_ProjectileCreated : Trigger<Entity>
{
    public Trigger_ProjectileCreated() { }
    public Trigger_ProjectileCreated(Entity entity, params object[] bindingObjects)
    {
        _value = entity;
        Invoke(bindingObjects);
    }
}
public class Trigger_SummonCreated : Trigger<Entity>
{
    public Trigger_SummonCreated() { }
    public Trigger_SummonCreated(Entity entity, params object[] bindingObjects)
    {
        _value = entity;
        Invoke(bindingObjects);
    }
}
public class Trigger_Channel : Trigger<Entity>
{
    public Trigger_Channel() { }
    public Trigger_Channel(Entity entity, params object[] bindingObjects)
    {
        _value = entity;
        Invoke(bindingObjects);
    }
}
public class Trigger_SpellCast : Trigger<Spell>
{
    public Trigger_SpellCast() { }
    public Trigger_SpellCast(Spell spell, params object[] bindingObjects)
    {
        _value = spell;
        Invoke(bindingObjects);
    }
}
public class Trigger_SpellStageIncrement : Trigger<Spell>
{
    public Trigger_SpellStageIncrement() { }
    public Trigger_SpellStageIncrement(Spell spell, params object[] bindingObjects)
    {
        _value = spell;
        Invoke(bindingObjects);
    }
}
public class Trigger_SpellMaxStage : Trigger<Spell>
{
    public Trigger_SpellMaxStage() { }
    public Trigger_SpellMaxStage(Spell spell, params object[] bindingObjects)
    {
        _value = spell;
        Invoke(bindingObjects);
    }
}
public class Trigger_SpellEffectApplied : Trigger<Effect>
{
    public Trigger_SpellEffectApplied() { }
    public Trigger_SpellEffectApplied(Effect effect, params object[] bindingObjects)
    {
        _value = effect;
        Invoke(bindingObjects);
    }
}

public class Trigger_SpellFinished : Trigger<Spell>
{
    public Trigger_SpellFinished() { }
    public Trigger_SpellFinished(Spell spell, params object[] bindingObjects)
    {
        _value = spell;
        Invoke(bindingObjects);
    }
}

public class Trigger_RuneGained : Trigger<Rune>
{
    public Trigger_RuneGained() { }
    public Trigger_RuneGained(Rune rune, params object[] bindingObjects)
    {
        _value = rune;
        Invoke(bindingObjects);
    }
}