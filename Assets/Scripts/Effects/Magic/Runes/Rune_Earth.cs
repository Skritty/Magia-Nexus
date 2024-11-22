using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rune_Earth : Rune
{
    public DamageInstance damageEffect;
    [SerializeReference]
    public PersistentEffect AoEBuff;
    [SerializeReference]
    public Targeting targeting;
    public Action orbitHoming;
    public Action trackingHoming;

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
                    spell.effect = damageEffect.Clone();
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
                    spell.castSpell.entityType = CreateEntity.EntityType.Projectile;
                    spell.entity.Stat<Stat_Movement>().baseMovementSpeed = 10f;
                    return this;
                }
        }
    }
}
