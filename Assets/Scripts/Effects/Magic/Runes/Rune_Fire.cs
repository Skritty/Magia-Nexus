using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TwitchLib.Api.Helix.Models.Common;

public class Rune_Fire : Rune
{
    public DamageInstance damageEffect;
    [SerializeReference]
    public Targeting targeting;
    [SerializeReference]
    public PE_EffectModifer expandingAoEBuff;
    [SerializeReference]
    public float spellEffectIncrease;
    public int damageTicksPerTurn;

    public override void SpellModifier(Spell spell)
    {
        spell.AddGenericSpellEffect(spellEffectIncrease, EffectModifierCalculationType.Additive);
    }

    public override Rune EffectFormula(Spell spell, Rune combiningRune) { return this; }

    public override void FinalizeEffectFormula(Spell spell)
    {
        spell.effect = damageEffect.Clone();
        Tally<EffectTag> damageTypeTally = new Tally<EffectTag>();
        foreach(Rune effectRune in spell.effectModifiers)
        {
            damageTypeTally.Add(effectRune.damageTags);
        }
        List<EffectTag> damageType = damageTypeTally.GetHighest(out _);
        if(damageType.Count >= 3)
        {
            spell.effect.effectTags.Add(EffectTag.Magical, 1);
        }
        else if(damageType.Count == 2)
        {
            float damage = 1f;
            EffectTag tag = damageType[0] | damageType[1];
            switch (tag)
            {
                case EffectTag.Chaos | EffectTag.Healing:
                    {
                        spell.effect.effectTags.Add(EffectTag.Damage | EffectTag.Spell, damage);
                        break;
                    }
                case EffectTag.Physical | EffectTag.Cold:
                    {
                        spell.effect.effectTags.Add(EffectTag.Bludgeoning | EffectTag.Spell, damage);
                        break;
                    }
                case EffectTag.Physical | EffectTag.Lightning:
                    {
                        spell.effect.effectTags.Add(EffectTag.Slashing | EffectTag.Spell, damage);
                        break;
                    }
                case EffectTag.Cold | EffectTag.Lightning:
                    {
                        spell.effect.effectTags.Add(EffectTag.Piercing | EffectTag.Spell, damage);
                        break;
                    }
                default:
                    {
                        damage /= damageType.Count;
                        foreach (EffectTag t in damageType)
                        {
                            spell.effect.effectTags.TryAdd(t | EffectTag.Spell, damage);
                        }
                        break;
                    }
            }
        }
        else
        {
            spell.effect.effectTags.Add(damageType[0] | EffectTag.Spell, 1);
        }
    }

    public override Rune TargetingFormula(Spell spell, Rune combiningRune)
    {
        switch (TypeName(combiningRune))
        {
            case nameof(Rune_Fire):
                {
                    spell.lifetime += GameManager.Instance.ticksPerTurn;
                    return this;
                }
            case nameof(Rune_Water):
                {
                    spell.AddAoESize(-1f, EffectModifierCalculationType.Additive);
                    expandingAoEBuff.Create(spell.entity);
                    spell.castSpell.spawnOnTarget = false;
                    spell.entity.Stat<Stat_Movement>().baseMovementSpeed += 2;
                    return this;
                }
            case nameof(Rune_Wind):
                {
                    spell.castSpell.spawnOnTarget = false;
                    spell.entity.Stat<Stat_Movement>().baseMovementSpeed += 4;
                    spell.entity.Stat<Stat_Actions>().AddAction(spell.spellAction);
                    spell.entity.Stat<Stat_Actions>().AddAction(spell.spellAction);
                    spell.entity.Stat<Stat_Actions>().AddAction(spell.spellAction);
                    return this;
                }
            case nameof(Rune_Earth):
                {
                    spell.AddAoESize(2f, EffectModifierCalculationType.Additive);
                    spell.entity.Stat<Stat_EffectModifiers>().AddModifier(EffectTag.MovementSpeed, 0, EffectModifierCalculationType.Multiplicative, 1, spell.effect);
                    return this;
                }
            case nameof(Rune_Order):
                {
                    spell.AddAoESize(1f, EffectModifierCalculationType.Additive);
                    spell.multiplier *= 2;
                    spell.castSpell.movementTarget = MovementTarget.Owner;
                    spell.castSpell.spawnOnTarget = false;
                    spell.entity.Stat<Stat_Movement>().baseMovementSpeed += 10;
                    spell.entity.Stat<Stat_Movement>().movementSelector = new Movement_DistanceFromTarget();
                    return this;
                }
            case nameof(Rune_Chaos):
                {
                    spell.entity.Stat<Stat_Movement>().baseMovementSpeed += 2;
                    spell.entity.Stat<Stat_Movement>().movementSelector = new Movement_DistanceFromTarget();
                    SetToDoT(spell);
                    return this;
                }
            default:
                {
                    spell.spellAction.timing = ActionEventTiming.OnEnd;
                    spell.lifetime = GameManager.Instance.ticksPerTurn;
                    (spell.effect as DamageInstance).ignoreFrames = GameManager.Instance.ticksPerTurn;
                    spell.effect.targetSelector = targeting;
                    spell.castSpell.spawnOnTarget = true;
                    return this;
                }
        }
    }

    private void SetToDoT(Spell spell)
    {
        (spell.effect as DamageInstance).ignoreFrames = GameManager.Instance.ticksPerTurn / damageTicksPerTurn;
        //(spell.effect as DamageInstance).preventTriggers = true;
        //(spell.effect as DamageInstance).skipFlatDamageReduction = true;
        spell.multiplier /= damageTicksPerTurn;
        spell.spellAction.timing = ActionEventTiming.OnTick;
    }
}
