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

    public override void MagicEffect(DamageInstance damage)
    {
        damage.onHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(DamageInstance damage, int currentRuneIndex)
    {
        damage.onHitEffects.Add(magicEffectModifier);
    }

    public override void Shape(Spell spell)
    {
        spell.shape = SpellShape.Line;
        spell.effect.targetSelector = shape;
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    Trigger_OnSpellCast.Subscribe(x => StageIncrease(spell, x.entity));
                    break;
                }
            case SpellShape.Conjuration:
                {
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

    private void StageIncrease(Spell spell, Entity entity)
    {
        if (spell != entity.Stat<Stat_Magic>().originSpell) return;
        spell.entity.Stat<Stat_Magic>().Stage++;
    }
}
