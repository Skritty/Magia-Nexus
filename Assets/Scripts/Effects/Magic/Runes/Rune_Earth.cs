using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Rune_Earth : Rune
{
    [Header("Magic Effects")]
    [SerializeReference]
    public PersistentEffect buff;
    [SerializeReference]
    public Effect debuff;
    [SerializeReference]
    public PersistentEffect magicEffectModifier;

    [Header("Spell Shape")]
    [SerializeReference]
    public Targeting shape;

    public override void MagicEffect(Spell spell)
    {
        spell.effect.onHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(Spell spell)
    {
        spell.effect.onHitEffects.Add(magicEffectModifier);
    }

    public override void Shape(Spell spell)
    {
        spell.shape = SpellShape.Projectile;
        spell.castSpell.projectileFanAngle = 30f;
        spell.SetToProjectile(shape, 6f);
    }

    public override void ShapeModifier(Spell spell)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    spell.lifetime += GameManager.Instance.ticksPerTurn;
                    break;
                }
            case SpellShape.Cone:
                {
                    (spell.effect.targetSelector as Targeting_Radial).angle += 15f;
                    break;
                }
            case SpellShape.Line:
                {
                    break;
                }
            case SpellShape.Projectile:
                {
                    spell.castSpell.numberOfProjectiles += 2;
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