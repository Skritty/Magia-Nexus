using System.Collections;
using System.Collections.Generic;
using Skritty.Tools.Saving;
using UnityEngine;

public class Rune_Water : Rune
{
    [Header("Magic Effects")]
    [SerializeReference]
    public PersistentEffect buff;
    [SerializeReference]
    public Effect debuff;

    [Header("Spell Shape")]
    [SerializeReference]
    public Targeting shape;
    [SerializeReference]
    public PE_EffectModifer expandingAoEBuff;
    [SerializeReference]
    public Targeting multicastConeTargeting;

    public override void MagicEffect(Spell spell)
    {
        spell.effect.onHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(Spell spell)
    {
        // TODO
    }

    public override void Shape(Spell spell)
    {
        spell.shape = SpellShape.Cone;
        spell.effect.targetSelector = shape;
    }

    public override void ShapeModifier(Spell spell)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    spell.AddAoESize(-1f, EffectModifierCalculationType.Additive);
                    expandingAoEBuff.Create(spell.entity);
                    spell.castSpell.spawnOnTarget = false;
                    spell.entity.Stat<Stat_Movement>().baseMovementSpeed += 2;
                    break;
                }
            case SpellShape.Cone:
                {
                    break;
                }
            case SpellShape.Line:
                {
                    spell.castSpell.targetSelector = multicastConeTargeting;
                    spell.castTargets += 2;
                    break;
                }
            case SpellShape.Projectile:
                {
                    spell.castSpell.numberOfProjectiles += 4;
                    spell.multiplier = 1f / spell.castSpell.numberOfProjectiles;
                    spell.castSpell.projectileFanType = CreateEntity.ProjectileFanType.Shotgun;
                    spell.castSpell.projectileFanAngle += 30f;
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