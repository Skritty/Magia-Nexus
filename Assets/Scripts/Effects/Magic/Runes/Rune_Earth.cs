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

    [Header("Spell Shape")]
    [SerializeReference]
    public Targeting shape;

    public override void MagicEffect(DamageInstance damage)
    {
        damage.onHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(DamageInstance damage, int currentRuneIndex)
    {
        PE_RuneCrystal crystal = new PE_RuneCrystal();
        crystal.rune = damage.runes[currentRuneIndex - 1]; // TODO: add stacks instead of new crystal
        damage.onHitEffects.Add(crystal);
    }

    public override void Shape(Spell spell)
    {
        spell.shape = SpellShape.Projectile;
        spell.castSpell.projectileFanAngle = 30f;
        spell.SetToProjectile(shape, 6f);
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    (spell.effect.targetSelector as Targeting_Radial).radius += 1;
                    break;
                }
            case SpellShape.Conjuration:
                {
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
            case SpellShape.Summon:
                {
                    break;
                }
            case SpellShape.Curse:
                {
                    break;
                }
        }
    }
}