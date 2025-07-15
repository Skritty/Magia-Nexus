using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Effect_DealDamage : EffectTask
{
    [FoldoutGroup("@GetType()")]
    public List<DamageSolver> damageModifiers = new List<DamageSolver>();
    [FoldoutGroup("@GetType()")]
    public List<Rune> runes = new List<Rune>();
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<TriggerTask> onHitEffects = new();
    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        if (Target.Stat<Stat_MaxLife>().Value <= 0) return;

        DamageInstance damageInstance = new DamageInstance(damageModifiers, runes, onHitEffects);

        new Trigger_PreHit(this, damageInstance, this, Owner, Target, SourceID);

        // Set up and generate magic effect
        foreach (Rune rune in Target.Stat<Stat_RuneCrystals>().Value)
        {
            runes.Add(rune);
            Target.GetMechanic<Stat_PersistentEffects>().AddOrRemoveSimilarEffect(crystal, -1);
        }
        if (Owner.GetMechanic<Mechanic_Magic>().enchantedAttacks.Count > 0)
        {
            damageInstance.runes.AddRange(Owner.GetMechanic<Mechanic_Magic>().enchantedAttacks.Dequeue());
        }
        damageInstance.GenerateMagicEffect();

        if (!triggered) damageInstance.DoTriggers(this, Owner, Target);

        damageInstance.CalculateDamageType();
        Target.GetMechanic<Mechanic_Damageable>().TakeDamage(damageInstance);
    }
}