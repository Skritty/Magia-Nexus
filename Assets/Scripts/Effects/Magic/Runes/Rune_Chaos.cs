using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rune_Chaos : Rune
{
    [SerializeReference]
    public PersistentEffect debuff;
    [SerializeReference]
    public PersistentEffect effectMultiBuff;

    public override void SpellEffect(Spell spell)
    {
        effectMultiBuff.Create(spell, spell.entity);
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
                    spell.effect = debuff.Clone();
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
                    spell.effect.targetSelector = spell.owner.Stat<Stat_PlayerOwner>().player.targetType;
                    return this;
                }
        }
    }
}
