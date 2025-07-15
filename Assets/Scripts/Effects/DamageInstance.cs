using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class DamageInstance
{
    [HideInInspector]
    public float calculatedDamage;
    [FoldoutGroup("@GetType()")]
    public List<DamageSolver> damageModifiers = new();
    [FoldoutGroup("@GetType()")]
    public List<Rune> runes = new();
    [FoldoutGroup("@GetType()")]
    public bool skipFlatDamageReduction;
    [FoldoutGroup("@GetType()")]
    public bool triggerPlayerOwner;
    [FoldoutGroup("@GetType()")]
    public bool preventTriggers;

    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<TriggerTask> onHitEffects = new();
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<TriggerTask> postOnHitEffects = new();

    public DamageInstance() { }
    public DamageInstance(List<DamageSolver> damageModifiers, List<Rune> runes, List<TriggerTask> tasks)
    {
        damageModifiers.AddRange(damageModifiers);
        runes.AddRange(runes);
        tasks.AddRange(tasks);
    }

    public void GenerateMagicEffect()
    {
        if (runes.Count == 0) return;
        int spellPhase = 0;
        Owner.GetMechanic<Mechanic_PlayerOwner>().Proxy(x => spellPhase += (int)x.GetMechanic<Stat_EffectModifiers>().CalculateModifier(EffectTag.SpellPhase));
        spellPhase %= runes.Count;
        for (int i = spellPhase; i < runes.Count + spellPhase; i++)
        {
            if (i == spellPhase)
                runes[i].MagicEffect(this, i);
            else
                runes[i % runes.Count].MagicEffectModifier(this, i);// TODO pass in rune list here
        }
    }

    public void DoTriggers(EffectTask Source, Entity Owner, Entity Target)
    {
        foreach (TriggerTask task in onHitEffects)
        {
            if (!task.DoTask(null, Owner)) break;
        }

        new Trigger_Hit(Source, this, Owner, Target, Source.SourceID);

        foreach (TriggerTask task in postOnHitEffects)
        {
            if (!task.DoTask(null, Owner)) break;
        }
    }

    private void AddToDamage(float damage, DamageType tag)
    {
        damageModifiers.Add(new DamageSolver(damage, NumericalModifierCalculationMethod.Flat, tag, DamageType.True));
        // TODO: contributing effect
    }

    public void CalculateDamageType()
    {
        if (runes.Count == 0) return;
        float addedFlatDamage = 0;
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
}