public class Effect_Damage : Effect_Hit
{
    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        if (Target.GetMechanic<Mechanic_Damageable>() == null)
        {
            base.DoEffect(Owner, Target, multiplier, triggered);
            return;
        }

        DamageInstance damageInstance = (hit as DamageInstance)?.Clone();
        if (damageInstance == null) return;
        damageInstance.EffectMultiplier *= multiplier;
        damageInstance.Owner = Owner;
        damageInstance.Target = Target;

        if (!triggered) damageInstance.PreHitTriggers();
        damageInstance.GenerateMagicEffect();
        if (!triggered) damageInstance.PostHitTriggers();
        damageInstance.CalculateDamageType();
        UnityEngine.Debug.Log($"{Target.gameObject.name}: {Target.GetMechanic<Mechanic_Damageable>()}");
        Target.GetMechanic<Mechanic_Damageable>().TakeDamage(damageInstance, triggered);
    }

    public new Effect_Damage Clone()
    {
        Effect_Damage clone = base.Clone() as Effect_Damage;
        clone.hit = hit.Clone();
        return clone;
    }
}