using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Rune_Wind : Rune
{
    [Header("Magic Effects")]
    [SerializeReference]
    public PersistentEffect buff;
    [SerializeReference]
    public PersistentEffect debuff;
    [SerializeReference]
    public Effect magicEffectModifier;

    [Header("Spell Shape")]
    [SerializeReference]
    public Targeting shape;

    public override void MagicEffect(Spell spell)
    {
        spell.effect.onHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(Spell spell)
    {
        //spell.effect.onHitEffects.Add(magicEffectModifier);
    }

    public override void Shape(Spell spell)
    {
        spell.shape = SpellShape.Line;
        spell.effect.targetSelector = shape;
    }

    public override void ShapeModifier(Spell spell)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    spell.castSpell.spawnOnTarget = false;
                    spell.entity.Stat<Stat_Movement>().baseMovementSpeed += 2;
                    spell.actionsPerTurn += 1;
                    break;
                }
            case SpellShape.Cone:
                {
                    (spell.effect.targetSelector as Targeting_Radial).radius += 1;
                    break;
                }
            case SpellShape.Line:
                {
                    spell.entity.Stat<Stat_Projectile>().splitsRemaining += 1;
                    spell.aoeTargetsModifier -= 1;
                    break;
                }
            case SpellShape.Projectile:
                {
                    spell.entity.Stat<Stat_Projectile>().splitsRemaining += 1;
                    spell.entity.Stat<Stat_Projectile>().piercesRemaining -= 1;
                    break;
                }
            case SpellShape.Direct:
                {
                    break;
                }
            case SpellShape.Totem:
                {
                    break;
                }
            case SpellShape.Minion:
                {
                    break;
                }
            case SpellShape.Conjuration:
                {
                    break;
                }
        }
    }
}
