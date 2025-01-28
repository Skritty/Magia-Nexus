public abstract class Trigger_Effect<T> : Trigger<T> where T : Trigger_Effect<T>
{
    public Effect effect;
    public Trigger_Effect() { }
    protected Trigger_Effect(Effect effect)
    {
        this.effect = effect;
        Invoke();
    }
}
public abstract class Trigger_Effect : Trigger_Effect<Trigger_Effect>
{
    public Trigger_Effect() { }
    protected Trigger_Effect(Effect effect) : base(effect) { }
}
public abstract class Trigger_DamageInstance<T> : Trigger_Effect<T> where T : Trigger_Effect<T>
{
    public Trigger_DamageInstance() { }
    protected Trigger_DamageInstance(DamageInstance damageInstance) : base(damageInstance) { }
    public DamageInstance damage => effect as DamageInstance;
}
public abstract class Trigger_DamageInstance : Trigger_DamageInstance<Trigger_DamageInstance>
{
    public Trigger_DamageInstance() { }
    public Trigger_DamageInstance(DamageInstance damageInstance) : base(damageInstance) { }
}
public class Trigger_OnActivateEffect : Trigger_Effect<Trigger_OnActivateEffect>
{
    public Trigger_OnActivateEffect() { }
    public Trigger_OnActivateEffect(Effect effect) : base(effect) { }
}
public class Trigger_MovementDirectionCalc : Trigger_Effect<Trigger_MovementDirectionCalc>
{
    public Trigger_MovementDirectionCalc() { }
    public Trigger_MovementDirectionCalc(Effect effect) : base(effect) { }
}
public class Trigger_OnHit : Trigger_DamageInstance<Trigger_OnHit>
{
    public Trigger_OnHit() { }
    public Trigger_OnHit(DamageInstance damageInstance) : base(damageInstance) { }
}
public class Trigger_OnDamageDealt : Trigger_DamageInstance<Trigger_OnDamageDealt>
{
    public Trigger_OnDamageDealt() { }
    public Trigger_OnDamageDealt(DamageInstance damageInstance) : base(damageInstance) { }
}
public class Trigger_WhenHit : Trigger_DamageInstance<Trigger_WhenHit>
{
    public Trigger_WhenHit() { }
    public Trigger_WhenHit(DamageInstance damageInstance) : base(damageInstance) { }
}
public class Trigger_OnDamageRecieved : Trigger_DamageInstance<Trigger_OnDamageRecieved>
{
    public Trigger_OnDamageRecieved() { }
    public Trigger_OnDamageRecieved(DamageInstance damageInstance) : base(damageInstance) { }
}
public class Trigger_OnDie : Trigger_DamageInstance<Trigger_OnDie>
{
    public Trigger_OnDie() { }
    public Trigger_OnDie(DamageInstance damageInstance) : base(damageInstance) { }
}
public class Trigger_OnRuneUsed : Trigger_Effect<Trigger_OnRuneUsed>
{
    public Trigger_OnRuneUsed() { }
    public Trigger_OnRuneUsed(Rune rune) : base(rune) { }
}
public class Trigger_OnPersistantEffectApplied : Trigger_Effect<Trigger_OnPersistantEffectApplied> 
{
    public Trigger_OnPersistantEffectApplied() { }
    protected Trigger_OnPersistantEffectApplied(PersistentEffect persistentEffect) : base(persistentEffect) { }
    public PersistentEffect persistentEffect => effect as PersistentEffect;
}
public class Trigger_OnProjectilePierce : Trigger_DamageInstance<Trigger_OnProjectilePierce>
{
    public Trigger_OnProjectilePierce() { }
    public Trigger_OnProjectilePierce(DamageInstance damage) : base(damage) { }
}