using System.Collections.Generic;
using Sirenix.OdinInspector;
public class DamageInstance : Hit
{
    [FoldoutGroup("@GetType()")]
    public float finalDamage;
    [FoldoutGroup("@GetType()")]
    public List<DamageSolver> damageModifiers = new();
    [FoldoutGroup("@GetType()")]
    public List<Rune> runes = new();
    [FoldoutGroup("@GetType()")]
    public bool skipFlatDamageReduction;

    public void GenerateMagicEffect()
    {
        foreach (Rune rune in Target.Stat<Stat_RuneCrystals>().Value)
        {
            runes.Add(rune);
            Target.GetMechanic<Stat_PersistentEffects>().AddOrRemoveSimilarEffect(crystal, -1);
        }
        if (Owner.GetMechanic<Mechanic_Magic>().enchantedAttacks.Count > 0)
        {
            damageInstance.runes.AddRange(Owner.GetMechanic<Mechanic_Magic>().enchantedAttacks.Dequeue());
        }

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

    private void AddToDamage(float damage, DamageType tag)
    {
        damageModifiers.Add(new DamageSolver(damage, CalculationStep.Flat, tag, DamageType.True));
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

    public new DamageInstance Clone()
    {
        DamageInstance clone = (DamageInstance)base.Clone();
        clone.damageModifiers = new List<DamageSolver>(damageModifiers);
        clone.runes = new List<Rune>(runes);
        return clone;
    }
}