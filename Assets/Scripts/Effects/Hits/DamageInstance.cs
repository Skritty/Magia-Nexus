using System.Collections.Generic;
using Sirenix.OdinInspector;
public class DamageInstance : Hit
{
    [FoldoutGroup("@GetType()")]
    public float finalDamage;
    [FoldoutGroup("@GetType()")]
    public List<DamageSolver> damageModifiers = new();
    [FoldoutGroup("@GetType()")]
    public bool skipFlatDamageReduction;

    public void GenerateMagicEffect()
    {
        // Check enchanted attack queue of the owner
        bool isAttack = false;
        foreach (DamageSolver solver in damageModifiers)
        {
            if (solver.damageType.HasFlag(DamageType.Attack))
            {
                isAttack = true;
                break;
            }
        }
        if (isAttack && Owner.Stat<Stat_Enchantments>().Value.Count > 0)
        {
            runes.AddRange(Owner.Stat<Stat_Enchantments>().Value.Dequeue());
        }

        // Check rune crystals on the target
        foreach (Rune rune in Target.Stat<Stat_RuneCrystals>().Value)
        {
            runes.Add(rune);
        }
        Target.Stat<Stat_RuneCrystals>().Value.Clear();

        if (runes.Count == 0) return;

        // Generate the magic effect
        int spellPhase = (int)Owner.Stat<Stat_PlayerCharacter>().Value.Stat<Stat_SpellPhase>().Value % runes.Count;
        runes[spellPhase].MagicEffect(this, spellPhase);
        for (int i = 1; i < runes.Count; i++)
        {
            runes[(i + spellPhase) % runes.Count].MagicEffectModifier(this, i);
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
        return clone;
    }
}