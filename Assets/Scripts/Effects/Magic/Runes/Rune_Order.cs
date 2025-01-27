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

    [Header("Spell Shape")]
    [SerializeReference]
    public Targeting shape;

    public override void MagicEffect(DamageInstance damage)
    {
        damage.onHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(DamageInstance damage, int currentRuneIndex)
    {
        RemovePersistantEffect removeEffect = new RemovePersistantEffect();
        removeEffect.alignmentRemoved = damage.Owner.Stat<Stat_Team>().team == damage.Target.Stat<Stat_Team>().team ? PersistentEffect.Alignment.Debuff : PersistentEffect.Alignment.Buff;
        damage.onHitEffects.Add(removeEffect);
    }

    public override void Shape(Spell spell)
    {
        spell.shape = SpellShape.Circle;
        spell.effect.targetSelector = shape;
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    spell.entity.Stat<Stat_Magic>().maxStages += 2;
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
                    spell.lifetime += GameManager.Instance.ticksPerTurn * 2;
                    spell.castSpell.projectileFanAngle = 180f;
                    spell.castSpell.movementTarget = MovementTarget.Owner;
                    Movement_Orbit orbit = new Movement_Orbit();
                    orbit.orbitDistance = 1.75f;
                    spell.entity.Stat<Stat_Movement>().movementSelector = orbit;
                    spell.entity.Stat<Stat_Movement>().baseMovementSpeed = 4;
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
