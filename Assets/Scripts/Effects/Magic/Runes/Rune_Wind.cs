using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rune_Wind : Rune
{
    [SerializeReference]
    public Targeting targeting;
    [SerializeReference]
    public PersistentEffect movementSpeedDebuff;
    [SerializeReference]
    public PersistentEffect movementSpeedBuff;
    [SerializeReference]
    public PersistentEffect invisibility;
    public override void SpellEffect(Spell spell)
    {
        spell.targets++;
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
                    spell.effect = movementSpeedDebuff.Clone();
                    //new TriggeredEffect(new Trigger_OnSpellEffectApplied(), movementSpeedBuff).Create(spell, owner);
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
