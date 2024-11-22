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
    public PersistentEffect AoEBuff;
    [SerializeReference]
    public PersistentEffect effectMultiBuff;

    public override void SpellModifier(Spell spell)
    {
        effectMultiBuff.Create(spell.entity);
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
                        spell.effect.effectTags.Add(EffectTag.Damage, damage);
                        break;
                    }
                case EffectTag.Physical | EffectTag.Cold:
                    {
                        spell.effect.effectTags.Add(EffectTag.Bludgeoning, damage);
                        break;
                    }
                case EffectTag.Physical | EffectTag.Lightning:
                    {
                        spell.effect.effectTags.Add(EffectTag.Slashing, damage);
                        break;
                    }
                case EffectTag.Cold | EffectTag.Lightning:
                    {
                        spell.effect.effectTags.Add(EffectTag.Piercing, damage);
                        break;
                    }
                default:
                    {
                        damage /= damageType.Count;
                        foreach (EffectTag t in damageType)
                        {
                            spell.effect.effectTags.Add(tag, damage);
                        }
                        break;
                    }
            }
            
        }
        else
        {
            spell.effect.effectTags.Add(damageType[0], 1);
        }
    }

    public override Rune TargetingFormula(Spell spell, Rune combiningRune)
    {
        string name = combiningRune == null ? "" : combiningRune.GetType().Name;
        switch (name)
        {
            case nameof(Rune_Fire):
                {
                    //AoEBuff.Create(spell, spell.entity);
                    return this;
                }
            case nameof(Rune_Water):
                {

                    return this;
                }
            case nameof(Rune_Wind):
                {

                    return this;
                }
            case nameof(Rune_Earth):
                {

                    return this;
                }
            case nameof(Rune_Order):
                {

                    return this;
                }
            case nameof(Rune_Chaos):
                {

                    return this;
                }
            default:
                {
                    spell.effect.targetSelector = targeting;
                    spell.castSpell.spawnOnTarget = true;
                    return this;
                }
        }
    }
}
