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

    public override void MagicEffect(DamageInstance damage)
    {
        damage.onHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(DamageInstance damage, int currentRuneIndex)
    {
        // TODO
    }

    public override void Shape(Spell spell)
    {
        spell.shape = SpellShape.Curse;
        spell.effect.targetSelector = shape;
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    if (!spell.castingOnChannel)
                    {
                        spell.castingOnChannel = true;
                        spell.cleanup = Trigger_OnSpellStageIncrement.Subscribe(x => CastOnStageGained(spell, x.entity), 5);
                    }
                    spell.entity.Stat<Stat_Magic>().maxStages++;
                    break;
                }
            case SpellShape.Conjuration:
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
                    if(!(spell.entity.Stat<Stat_Movement>().movementSelector is Movement_HomeToTarget))
                    {
                        spell.entity.Stat<Stat_Movement>().movementSelector = new Movement_HomeToTarget();
                    }
                    (spell.entity.Stat<Stat_Movement>().movementSelector as Movement_HomeToTarget).homingRateDegreesPerSecond += 30f;
                    
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

    private void CastOnStageGained(Spell spell, Entity entity)
    {
        if (spell != entity.Stat<Stat_Magic>().originSpell) return;
        spell.spellAction.OnStart(spell.entity);
    }
}
