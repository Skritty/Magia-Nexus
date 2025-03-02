using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[Serializable]
public class DamageInstance : Effect
{
    [FoldoutGroup("@GetType()")]
    public List<EffectModifier> damageModifiers = new List<EffectModifier>();
    [FoldoutGroup("@GetType()")]
    public List<Rune> runes = new List<Rune>(); 
    [FoldoutGroup("@GetType()")]
    public bool skipFlatDamageReduction;
    [FoldoutGroup("@GetType()")]
    public bool preventTriggers;
    
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<Effect> ownerEffects = new();
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<Effect> targetEffects = new();
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<Effect> onHitEffects = new();
    [SerializeReference, FoldoutGroup("@GetType()"), InfoBox("These are in effect until the end of damage calculation")]
    public List<PE_Trigger> temporaryTriggeredEffects = new();

    public override void Activate()
    {
        if (Target.Stat<Stat_Life>().maxLife <= 0) return;
        if (!preventTriggers)
        {
            foreach (PE_Trigger effect in temporaryTriggeredEffects)
            {
                Owner.Stat<Stat_PersistentEffects>().AddOrRemoveSimilarEffect(effect, effect.stacks, Owner);
            }
            new Trigger_PreHit(this, this, Owner, Target, Source);
            if (Owner.Stat<Stat_Magic>().useRunesToEnchantAttacks)
            {
                runes.AddRange(Owner.Stat<Stat_Magic>().runes);
                if (Owner.Stat<Stat_Magic>().consumeRunesOnEnchant)
                {
                    Owner.Stat<Stat_Magic>().runes.Clear();
                }
            }
            foreach (Effect effect in targetEffects)
            {
                effect.Create(Source, Owner, Target);
            }
            foreach (Effect effect in ownerEffects)
            {
                effect.Create(Source, Owner, Owner);
            }
            foreach (Effect effect in onHitEffects)
            {
                effect.Create(this);
            }
            foreach (PE_RuneCrystal crystal in Target.Stat<Stat_PersistentEffects>().GetEffects<PE_RuneCrystal>())
            {
                runes.Add(crystal.rune);
                Target.Stat<Stat_PersistentEffects>().AddOrRemoveSimilarEffect(crystal, -1);
            }
            GenerateMagicEffect();
            new Trigger_Hit(this, this, Owner, Target, Source);
        }

        CalculateDamageType();
        Target.Stat<Stat_Life>().TakeDamage(this);

        foreach (PE_Trigger effect in temporaryTriggeredEffects)
        {
            Owner.Stat<Stat_PersistentEffects>().AddOrRemoveSimilarEffect(effect, -effect.stacks, Owner);
        }
    }

    public void AddModifiers(EffectModifier calculation)
    {
        foreach(EffectModifier modifier in damageModifiers)
        {
            calculation.AddModifier(modifier);
        }
    }

    private void GenerateMagicEffect()
    {
        if (runes.Count == 0) return;
        int spellPhase = (int)Owner.Stat<Stat_EffectModifiers>().CalculateModifier(EffectTag.SpellPhase) - 1;
        spellPhase %= runes.Count;
        for (int i = spellPhase; i < runes.Count + spellPhase; i++)
        {
            if (i == spellPhase)
                runes[i].MagicEffect(this);
            else
                runes[i % runes.Count].MagicEffectModifier(this, i);// TODO pass in rune list here
        }
    }

    private void AddToDamage(float damage, DamageType tag)
    {
        damageModifiers.Add(new EffectModifier(tag, EffectTag.None, damage, EffectModifierCalculationType.Flat, 1, null));
        // TODO: contributing effect
    }

    public void CalculateDamageType()
    {
        if (runes.Count == 0) return;
        float addedFlatDamage = 1;
        Tally<DamageType> damageTypeTally = new Tally<DamageType>();
        foreach (Rune effectRune in runes)
        {
            addedFlatDamage += effectRune.magicEffectFlatDamage;
            damageTypeTally.Add(effectRune.damageType);
        }
        List<DamageType> damageTypes = damageTypeTally.GetHighest(out _);
        if (damageTypes.Count >= 3)
        {
            AddToDamage(addedFlatDamage, DamageType.Magical | DamageType.Spell);
        }
        else if (damageTypes.Count == 2)
        {
            DamageType tag = damageTypes[0] | damageTypes[1];
            switch (tag)
            {
                case DamageType.Chaos | DamageType.Order:
                    {
                        AddToDamage(addedFlatDamage, DamageType.Damage | DamageType.Spell);
                        break;
                    }
                case DamageType.Physical | DamageType.Cold:
                    {
                        AddToDamage(addedFlatDamage, DamageType.Bludgeoning | DamageType.Spell);
                        break;
                    }
                case DamageType.Physical | DamageType.Lightning:
                    {
                        AddToDamage(addedFlatDamage, DamageType.Slashing | DamageType.Spell);
                        break;
                    }
                case DamageType.Cold | DamageType.Lightning:
                    {
                        AddToDamage(addedFlatDamage, DamageType.Piercing | DamageType.Spell);
                        break;
                    }
                default:
                    {
                        foreach (DamageType t in damageTypes)
                        {
                            AddToDamage(addedFlatDamage / damageTypes.Count, t | DamageType.Spell);
                        }
                        break;
                    }
            }
        }
        else
        {
            AddToDamage(addedFlatDamage, damageTypes[0] | DamageType.Spell);
        }
    }

    public override Effect Clone()
    {
        DamageInstance clone = (DamageInstance)base.Clone();
        clone.damageModifiers = new List<EffectModifier>(damageModifiers);
        clone.runes = new List<Rune>(runes);
        clone.ownerEffects = new List<Effect>(ownerEffects);
        clone.targetEffects = new List<Effect>(targetEffects);
        clone.onHitEffects = new List<Effect>(onHitEffects);
        clone.temporaryTriggeredEffects = new List<PE_Trigger>(temporaryTriggeredEffects);
        return clone;
    }
}