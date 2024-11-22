using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rune_Order : Rune
{
    [SerializeReference]
    public Targeting targeting;
    [SerializeReference]
    public PersistentEffect defensiveBuff;
    [SerializeReference]
    public PersistentEffect AoEBuff;
    public override void SpellModifier(Spell spell)
    {
        AoEBuff.Create(spell.entity);
    }

    public override Rune EffectFormula(Spell spell, Rune combiningRune)
    {
        string name = combiningRune == null ? "" : combiningRune.GetType().Name;
        switch (name)
        {
            case nameof(Rune_Fire):
                {

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
                    spell.effect = defensiveBuff.Clone();
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
                    return this;
                }
        }
    }
}
