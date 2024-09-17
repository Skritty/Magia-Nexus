using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Rune_Fire : Rune
{
    public DamageInstance damageEffect;
    [SerializeReference]
    public Targeting targeting;
    [SerializeReference]
    public PersistentEffect AoEBuff;
    [SerializeReference]
    public PersistentEffect effectMultiBuff;

    public override void SpellEffect(Spell spell)
    {
        effectMultiBuff.Create(spell.entity);
    }

    public override Rune EffectFormula(Spell spell, Rune combiningRune)
    {
        /*if(combiningRune == null)
        {
            spell.effect = damageEffect.Clone();
            return this;
        }*/

        string name = combiningRune == null ? "" : combiningRune.GetType().Name;
        switch (name)
        {
            case nameof(Rune_Fire):
                {
                    //(spell.effect as DamageInstance).damageTypes |= DamageType.Fire;
                    return this;
                }
            case nameof(Rune_Water):
                {
                    //(spell.effect as DamageInstance).damageTypes |= DamageType.Cold;
                    return this;
                }
            case nameof(Rune_Wind):
                {
                    //(spell.effect as DamageInstance).damageTypes |= DamageType.Lightning;
                    return this;
                }
            case nameof(Rune_Earth):
                {
                    //(spell.effect as DamageInstance).damageTypes |=  DamageType.Slashing;
                    return this;
                }
            case nameof(Rune_Order):
                {
                    //(spell.effect as DamageInstance).damageTypes |= DamageType.Bludgeoning;
                    return this;
                }
            case nameof(Rune_Chaos):
                {
                    //(spell.effect as DamageInstance).damageTypes |= DamageType.Piercing;
                    return this;
                }
            default:
                {
                    spell.effect = damageEffect.Clone();
                    return this;
                }
        }
    }

    public override void FinalizeEffectFormula(Spell spell)
    {

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
