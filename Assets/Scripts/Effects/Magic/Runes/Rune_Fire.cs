using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TwitchLib.Api.Helix.Models.Common;
using Unity.VisualScripting;

public class Rune_Fire : Rune
{
    [Header("Magic Effects")]
    [SerializeReference]
    public PersistentEffect buff;
    [SerializeReference]
    public PersistentEffect debuff;
    [SerializeReference]
    public DamageInstance magicEffectModifier;

    [Header("Spell Shape")]
    [SerializeReference]
    public Targeting shape;

    public override void MagicEffect(Spell spell)
    {
        spell.effect.onHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(Spell spell)
    {
        DamageInstance explosion = magicEffectModifier.Clone();
        explosion.effectTags.AddRange(spell.effect.effectTags);
        spell.effect.onHitEffects.Add(explosion);
        // TODO: Add delay
    }

    public override void Shape(Spell spell)
    {
        spell.shape = SpellShape.Circle;
        spell.effect.targetSelector = shape;
        spell.castSpell.spawnOnTarget = true;
        spell.spellAction.timing = ActionEventTiming.OnEnd;
        spell.lifetime = GameManager.Instance.ticksPerTurn;
        //(spell.effect as DamageInstance).ignoreFrames = GameManager.Instance.ticksPerTurn;
    }

    public override void ShapeModifier(Spell spell)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    spell.actionsPerTurn += 1;
                    break;
                }
            case SpellShape.Cone:
                {
                    
                    break;
                }
            case SpellShape.Line:
                {
                    spell.aoeTargetsModifier += 1;
                    break;
                }
            case SpellShape.Projectile:
                {
                    spell.entity.Stat<Stat_Projectile>().piercesRemaining += 2;
                    break;
                }
            case SpellShape.Direct:
                {
                    spell.castTargets += 1;
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