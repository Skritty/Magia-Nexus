using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Rune_Order : Rune
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
        // TODO
    }

    public override void Shape(Spell spell)
    {
        spell.shape = SpellShape.Circle;
        spell.effect.targetSelector = shape;
    }

    public override void ShapeModifier(Spell spell)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    spell.castSpell.movementTarget = MovementTarget.Owner;
                    spell.castSpell.spawnOnTarget = false;
                    spell.entity.Stat<Stat_Movement>().baseMovementSpeed += 10;
                    spell.entity.Stat<Stat_Movement>().movementSelector = new Movement_DistanceFromTarget();
                    break;
                }
            case SpellShape.Cone:
                {
                    break;
                }
            case SpellShape.Line:
                {
                    break;
                }
            case SpellShape.Projectile:
                {
                    spell.lifetime += GameManager.Instance.ticksPerTurn * 2;
                    spell.castSpell.projectileFanAngle = 180f;
                    spell.castSpell.movementTarget = MovementTarget.Owner;
                    Movement_Orbit orbit = new Movement_Orbit();
                    orbit.orbitDistance = 1.75f;
                    spell.entity.Stat<Stat_Movement>().movementSelector = orbit;
                    spell.entity.Stat<Stat_Movement>().baseMovementSpeed = 4;
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
