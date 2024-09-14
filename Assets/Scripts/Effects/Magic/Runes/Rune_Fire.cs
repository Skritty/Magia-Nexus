using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rune_Fire : Rune
{
    public DamageInstance damageEffect;
    [SerializeReference]
    public Targeting_Radial targeting;
    [SerializeReference]
    public PersistentEffect AoEBuff;
    [SerializeReference]
    public PersistentEffect effectMultiBuff;

    public override void SpellEffect(Spell spell)
    {

    }

    public override Rune EffectFormula(Spell spell, Rune combiningRune)
    {
        string name = combiningRune == null ? "" : combiningRune.GetType().Name;
        switch (name)
        {
            case nameof(Rune_Fire):
                {
                    effectMultiBuff.Create(spell, spell.entity);
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
                    spell.spellEffect = damageEffect;
                    return this;
                }
        }
    }

    public override Rune TargetingFormula(Spell spell, Rune combiningRune)
    {
        string name = combiningRune == null ? "" : combiningRune.GetType().Name;
        switch (name)
        {
            case nameof(Rune_Fire):
                {
                    AoEBuff.Create(spell, spell.entity);
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
                    spell.spellEffect.targetSelector = targeting;
                    return this;
                }
        }
    }
}
