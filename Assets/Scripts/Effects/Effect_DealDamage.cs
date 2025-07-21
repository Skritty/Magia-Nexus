using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Effect_DealDamage : Effect_DoHit
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
        damageInstance.EffectMultiplier = multiplier;
        damageInstance.Source = this;
        damageInstance.Owner = Owner;
        damageInstance.Target = Target;

        if (!triggered) damageInstance.PreHitTriggers();
        damageInstance.GenerateMagicEffect();
        if (!triggered) damageInstance.PostHitTriggers();
        damageInstance.CalculateDamageType();
        Target.GetMechanic<Mechanic_Damageable>().TakeDamage(damageInstance);
    }

    public new Effect_DealDamage Clone()
    {
        Effect_DealDamage clone = base.Clone() as Effect_DealDamage;
        clone.hit = hit;
        return clone;
    }
}