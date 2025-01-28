public abstract class Trigger_Entity<T> : Trigger<T> where T : Trigger_Entity<T>
{
    public Entity entity;
    public Trigger_Entity() { }
    protected Trigger_Entity(Entity entity)
    {
        this.entity = entity;
        Invoke();
    }
}

public abstract class Trigger_Entity : Trigger_Entity<Trigger_Entity>
{
    public Trigger_Entity() { }
    public Trigger_Entity(Entity entity) : base(entity) { }
}

public class Trigger_Expire : Trigger_Entity<Trigger_Expire>
{
    public Trigger_Expire() { }
    public Trigger_Expire(Entity entity) : base(entity) { }
}
public class Trigger_ProjectileCreated : Trigger_Entity<Trigger_ProjectileCreated>
{
    public Trigger_ProjectileCreated() { }
    public Trigger_ProjectileCreated(Entity entity) : base(entity) { }
}
public class Trigger_OnSpellCast : Trigger_Entity
{
    public Trigger_OnSpellCast() { }
    public Trigger_OnSpellCast(Entity entity) : base(entity) { }
}
public class Trigger_OnSpellStageIncrement : Trigger_Entity
{
    public Trigger_OnSpellStageIncrement() { }
    public Trigger_OnSpellStageIncrement(Entity entity) : base(entity) { }
}
public class Trigger_OnSpellMaxStage : Trigger_Entity
{
    public Trigger_OnSpellMaxStage() { }
    public Trigger_OnSpellMaxStage(Entity entity) : base(entity) { }
}
public class Trigger_OnSpellEffectApplied : Trigger_Entity
{
    public Trigger_OnSpellEffectApplied() { }
    public Trigger_OnSpellEffectApplied(Entity entity) : base(entity) { }
}