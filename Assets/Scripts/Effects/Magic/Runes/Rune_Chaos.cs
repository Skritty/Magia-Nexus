using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rune_Chaos : Rune
{
    [Header("Magic Effects")]
    [SerializeReference]
    public PersistentEffect buff;
    [SerializeReference]
    public PersistentEffect debuff;

    [Header("Spell Shape")]
    [SerializeReference]
    public Targeting shape;

    public override void MagicEffect(Spell spell)
    {
        spell.effect.onHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(Spell spell)
    {
        spell.lifetime += GameManager.Instance.ticksPerTurn / 2;
    }

    public override void Shape(Spell spell)
    {
        spell.shape = SpellShape.Direct;
        spell.effect.targetSelector = shape;
    }

    public override void ShapeModifier(Spell spell)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    spell.entity.Stat<Stat_Movement>().baseMovementSpeed += 2;
                    spell.entity.Stat<Stat_Movement>().movementSelector = new Movement_DistanceFromTarget();
                    //spell.SetHitRate(damageTicksPerTurn);
                    break;
                }
            case SpellShape.Cone:
                {
                    break;
                }
            case SpellShape.Line:
                {
                    spell.castTargets += 1;
                    break;
                }
            case SpellShape.Projectile:
                {
                    Movement_HomeToTarget homing = new Movement_HomeToTarget();
                    homing.homingRateDegreesPerSecond += 30f;
                    spell.entity.Stat<Stat_Movement>().movementSelector = homing;
                    break;
                }
            case SpellShape.Direct:
                {
                    // TODO
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
